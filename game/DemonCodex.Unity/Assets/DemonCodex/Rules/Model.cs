using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DemonCodex.Rules.Tests")]

namespace DemonCodex.Rules
{
    public enum PlayerId { P0, P1, P2, P3 }
    public enum ControlType { Human, Bot }
    public enum MatchPhase { AwaitingRoll, AwaitingSelection, Finished }
    public enum PieceKind { InAbyss, OnMainTrack, OnAscensionPath, Completed }
    public enum MoveKind { Summon, Advance }

    public readonly struct PieceId : IEquatable<PieceId>
    {
        public PlayerId Owner { get; }
        public int Index { get; }
        public PieceId(PlayerId owner, int index)
        {
            if (!Enum.IsDefined(typeof(PlayerId), owner)) throw new ArgumentOutOfRangeException(nameof(owner));
            if (index < 0 || index >= 4) throw new ArgumentOutOfRangeException(nameof(index));
            Owner = owner; Index = index;
        }
        public bool Equals(PieceId other) => Owner == other.Owner && Index == other.Index;
        public override bool Equals(object obj) => obj is PieceId other && Equals(other);
        public override int GetHashCode() => (int)Owner * 4 + Index;
        public override string ToString() => Owner + ":" + Index;
    }

    public readonly struct PiecePosition : IEquatable<PiecePosition>
    {
        public PieceKind Kind { get; }
        // Track progress p, private-path j, or zero for non-occupiable states.
        public int Step { get; }
        private PiecePosition(PieceKind kind, int step) { Kind = kind; Step = step; }
        public static PiecePosition Abyss => new PiecePosition(PieceKind.InAbyss, 0);
        public static PiecePosition Completed => new PiecePosition(PieceKind.Completed, 0);
        public static PiecePosition Track(int progress)
        {
            if (progress < 0) throw new ArgumentOutOfRangeException(nameof(progress));
            return new PiecePosition(PieceKind.OnMainTrack, progress);
        }
        public static PiecePosition Path(int index)
        {
            if (index < 1) throw new ArgumentOutOfRangeException(nameof(index));
            return new PiecePosition(PieceKind.OnAscensionPath, index);
        }
        public bool Equals(PiecePosition other) => Kind == other.Kind && Step == other.Step;
        public override bool Equals(object obj) => obj is PiecePosition other && Equals(other);
        public override int GetHashCode() => unchecked((int)Kind * 397 ^ Step);
        public override string ToString() => Kind + ":" + Step;
    }

    public sealed class PlayerState
    {
        public PlayerId Id { get; }
        public ControlType Control { get; }
        public PlayerState(PlayerId id, ControlType control) { Id = id; Control = control; }
    }

    public sealed class PieceState
    {
        public PieceId Id { get; }
        public PlayerId Owner => Id.Owner;
        public PiecePosition Position { get; }
        public PieceState(PieceId id, PiecePosition position) { Id = id; Position = position; }
    }

    public sealed class BoardConfiguration
    {
        public int MainTrackLength { get; }
        public IReadOnlyDictionary<PlayerId, int> StartIndex { get; }
        public IReadOnlyDictionary<PlayerId, int> FinalPathLength { get; }
        public IReadOnlyList<int> SafeSpaceIndices { get; }
        public IReadOnlyList<PlayerId> PlayerOrder { get; }
        public PlayerId InitialPlayer { get; }
        public IReadOnlyList<PlayerState> Players { get; }
        private readonly HashSet<int> safe;

        public BoardConfiguration(int mainTrackLength,
            IDictionary<PlayerId, int> startIndex, IEnumerable<int> safeSpaceIndices,
            IDictionary<PlayerId, int> finalPathLength, IEnumerable<PlayerId> playerOrder,
            PlayerId initialPlayer, PlayerId humanPlayer)
        {
            if (mainTrackLength < 4) throw new ArgumentOutOfRangeException(nameof(mainTrackLength));
            if (startIndex == null || safeSpaceIndices == null || finalPathLength == null || playerOrder == null)
                throw new ArgumentNullException("All configuration collections are required.");
            var order = playerOrder.ToArray();
            var ids = (PlayerId[])Enum.GetValues(typeof(PlayerId));
            if (order.Length != 4 || !order.OrderBy(x => x).SequenceEqual(ids))
                throw new ArgumentException("playerOrder must be a permutation of all four players.");
            if (!ids.Contains(initialPlayer) || !ids.Contains(humanPlayer))
                throw new ArgumentException("Initial and human player must belong to the match.");
            var starts = new Dictionary<PlayerId, int>(startIndex);
            var paths = new Dictionary<PlayerId, int>(finalPathLength);
            if (!starts.Keys.OrderBy(x => x).SequenceEqual(ids) || !paths.Keys.OrderBy(x => x).SequenceEqual(ids))
                throw new ArgumentException("Each player needs exactly one start and final-path length.");
            if (starts.Values.Any(x => x < 0 || x >= mainTrackLength) || starts.Values.Distinct().Count() != 4)
                throw new ArgumentException("Start indices must be distinct and on the main track.");
            if (paths.Values.Any(x => x < 1)) throw new ArgumentException("Final paths must have positive lengths.");
            safe = new HashSet<int>(safeSpaceIndices);
            if (safe.Any(x => x < 0 || x >= mainTrackLength) || starts.Values.Any(x => !safe.Contains(x)))
                throw new ArgumentException("Safe indices must be on the track and include every start.");
            MainTrackLength = mainTrackLength;
            StartIndex = new ReadOnlyDictionary<PlayerId, int>(starts);
            FinalPathLength = new ReadOnlyDictionary<PlayerId, int>(paths);
            SafeSpaceIndices = Array.AsReadOnly(safe.OrderBy(x => x).ToArray());
            PlayerOrder = Array.AsReadOnly(order);
            InitialPlayer = initialPlayer;
            Players = Array.AsReadOnly(ids.Select(x => new PlayerState(x, x == humanPlayer ? ControlType.Human : ControlType.Bot)).ToArray());
        }

        public bool IsSafe(int index) => safe.Contains(index);
        public int TrackIndex(PlayerId owner, int progress) => (int)(((long)StartIndex[owner] + progress) % MainTrackLength);
        public PlayerId NextPlayer(PlayerId player)
        {
            for (int i = 0; i < 4; i++) if (PlayerOrder[i] == player) return PlayerOrder[(i + 1) % 4];
            throw new ArgumentOutOfRangeException(nameof(player));
        }
    }

    public sealed class MatchState
    {
        public BoardConfiguration Board { get; }
        public IReadOnlyList<PlayerState> Players => Board.Players;
        public IReadOnlyList<PieceState> Pieces { get; }
        public MatchPhase Phase { get; }
        public PlayerId ActivePlayer { get; }
        public int? PendingRoll { get; }
        public PlayerId? Winner { get; }
        public long Revision { get; }

        // Only the reducer creates snapshots. Internal visibility supports deliberate
        // invariant-checked test fixtures; arbitrary loaded state is not a public API.
        internal MatchState(BoardConfiguration board, IEnumerable<PieceState> pieces,
            MatchPhase phase, PlayerId activePlayer, int? pendingRoll, PlayerId? winner, long revision)
        {
            Board = board ?? throw new ArgumentNullException(nameof(board));
            Pieces = Array.AsReadOnly(pieces.ToArray());
            Phase = phase; ActivePlayer = activePlayer; PendingRoll = pendingRoll; Winner = winner; Revision = revision;
        }
        public static MatchState Create(BoardConfiguration board) => Initial(board, 0);
        internal static MatchState Initial(BoardConfiguration board, long revision)
        {
            if (board == null) throw new ArgumentNullException(nameof(board));
            return new MatchState(board, board.Players.SelectMany(p => Enumerable.Range(0, 4)
                .Select(i => new PieceState(new PieceId(p.Id, i), PiecePosition.Abyss))),
                MatchPhase.AwaitingRoll, board.InitialPlayer, null, null, revision);
        }
        public PieceState Piece(PieceId id) => Pieces.Single(p => p.Id.Equals(id));
    }

    public sealed class LegalMove : IEquatable<LegalMove>
    {
        public PieceId Piece { get; }
        public MoveKind Kind { get; }
        public PiecePosition Source { get; }
        public PiecePosition Target { get; }
        public int Roll { get; }
        public long Revision { get; }
        public LegalMove(PieceId piece, MoveKind kind, PiecePosition source, PiecePosition target, int roll, long revision)
        { Piece = piece; Kind = kind; Source = source; Target = target; Roll = roll; Revision = revision; }
        public bool Equals(LegalMove other) => other != null && Piece.Equals(other.Piece) && Kind == other.Kind &&
            Source.Equals(other.Source) && Target.Equals(other.Target) && Roll == other.Roll && Revision == other.Revision;
        public override bool Equals(object obj) => Equals(obj as LegalMove);
        public override int GetHashCode() => unchecked(Piece.GetHashCode() * 397 ^ Revision.GetHashCode());
    }
}
