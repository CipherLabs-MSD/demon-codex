using System.Linq;
using DemonCodex.Rules;
using UnityEngine;

namespace DemonCodex.Presentation
{
    // Original radial prototype layout. Screen coordinates only; never decides legality.
    public static class BoardLayout
    {
        public static Vector2 Track(int index, int length) => Radial(index * 360f / length, 292);
        private static Vector2 Radial(float degrees, float radius)
        {
            float angle = (degrees - 90) * Mathf.Deg2Rad;
            return new Vector2(410 + Mathf.Cos(angle) * radius, 440 + Mathf.Sin(angle) * radius);
        }
        public static Vector2 Path(PlayerId owner, int step) => Radial((int)owner * 90, 280 - step * 32);
        public static Vector2 Abyss(PlayerId owner) => new[] {
            new Vector2(90, 170), new Vector2(730, 170), new Vector2(730, 720), new Vector2(90, 720)
        }[(int)owner];
        public static Vector2 Home(PlayerId owner) => Radial((int)owner * 90, 65);
        public static Vector2 Slot(Vector2 center, int index) => center + new Vector2((index % 2) * 30 - 15, (index / 2) * 30 - 15);
        public static Vector2 Piece(MatchState state, PieceState piece)
        {
            switch (piece.Position.Kind)
            {
                case PieceKind.OnMainTrack: return Track(state.Board.TrackIndex(piece.Owner, piece.Position.Step), state.Board.MainTrackLength);
                case PieceKind.OnAscensionPath: return Path(piece.Owner, piece.Position.Step);
                case PieceKind.Completed: return Slot(Home(piece.Owner), piece.Id.Index);
                default: return Slot(Abyss(piece.Owner), piece.Id.Index);
            }
        }
    }

    [RequireComponent(typeof(LocalMatchController))]
    public sealed class LocalMatchView : MonoBehaviour
    {
        private LocalMatchController controller;
        private string seedText = "2026";
        private bool debug;
        private GUIStyle title, label, small, button, pieceLabel;
        private static readonly Color[] Colors = {
            new Color(.40f, .86f, 1), new Color(1, .60f, .38f), new Color(.76f, .60f, 1), new Color(.48f, .92f, .64f)
        };
        private void Awake() { controller = GetComponent<LocalMatchController>(); }
        private void Styles()
        {
            if (title != null) return;
            title = new GUIStyle(GUI.skin.label) { fontSize = 27, fontStyle = FontStyle.Bold };
            label = new GUIStyle(GUI.skin.label) { fontSize = 18, wordWrap = true };
            small = new GUIStyle(GUI.skin.label) { fontSize = 14, wordWrap = true };
            button = new GUIStyle(GUI.skin.button) { fontSize = 18, fontStyle = FontStyle.Bold };
            pieceLabel = new GUIStyle(GUI.skin.label) { fontSize = 15, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            pieceLabel.normal.textColor = new Color(.035f, .045f, .07f);
        }
        private static Rect Around(Vector2 point, float size) => new Rect(point.x - size / 2, point.y - size / 2, size, size);
        private static void Fill(Rect rect, Color color)
        { var previous = GUI.color; GUI.color = color; GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = previous; }
        private void OnGUI()
        {
            Styles();
            float scale = Mathf.Min(Screen.width / 1200f, Screen.height / 850f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1200 * scale) / 2, (Screen.height - 850 * scale) / 2), Quaternion.identity, new Vector3(scale, scale, 1));
            Fill(new Rect(0, 0, 1200, 850), new Color(.035f, .043f, .065f));
            string heading = "DEMON CODEX  /  LOCAL MATCH";
            string subtitle = "M001 prototype  ·  One human + three bots  ·  Clockwise travel";
#if UNITY_EDITOR
            if (controller.DevelopmentScenario != null)
            {
                heading = "DEVELOPMENT TEST SCENARIO";
                subtitle = controller.DevelopmentScenario;
            }
#endif
            GUI.Label(new Rect(30, 20, 720, 38), heading, title);
            GUI.Label(new Rect(32, 62, 740, 25), subtitle, small);
            var session = controller.Session;
            var state = controller.PresentedState;
            for (int i = 0; i < state.Board.MainTrackLength; i++)
            {
                Vector2 point = BoardLayout.Track(i, state.Board.MainTrackLength);
                Fill(Around(point, 36), state.Board.IsSafe(i) ? new Color(.46f, .44f, .28f) : new Color(.16f, .20f, .26f));
                GUI.Label(Around(point, 30), i.ToString(), small);
            }
            for (int p = 0; p < 4; p++)
            {
                var owner = (PlayerId)p;
                var abyss = BoardLayout.Abyss(owner);
                Fill(new Rect(abyss.x - 72, abyss.y - 57, 144, 105), new Color(.09f, .11f, .16f));
                GUI.color = Colors[p];
                GUI.Label(new Rect(abyss.x - 72, abyss.y - 88, 170, 24), LocalMatchController.Name(owner) + " / ABYSS", small);
                GUI.color = Color.white;
                for (int j = 1; j < state.Board.FinalPathLength[owner]; j++)
                {
                    Vector2 point = BoardLayout.Path(owner, j);
                    Fill(Around(point, 27), Colors[p] * .6f);
                    GUI.Label(Around(point, 25), j.ToString(), small);
                }
                Vector2 start = BoardLayout.Track(state.Board.StartIndex[owner], state.Board.MainTrackLength);
                GUI.color = Colors[p];
                GUI.Label(new Rect(start.x - 48, start.y - 46, 110, 24), "P" + p + " START", small);
                Vector2 home = BoardLayout.Home(owner);
                Fill(Around(home, 63), Colors[p] * .32f);
                GUI.Label(new Rect(home.x - 30, home.y - 49, 90, 20), "P" + p + " HOME", small);
                GUI.color = Color.white;
            }
            foreach (var piece in state.Pieces)
            {
                var move = session.SelectableMoves.FirstOrDefault(m => m.Piece.Equals(piece.Id));
                Rect rect = Around(BoardLayout.Piece(state, piece), 26);
                if (move != null) Fill(new Rect(rect.x - 4, rect.y - 4, 34, 34), Color.white);
                Fill(rect, Colors[(int)piece.Owner]);
                if (move != null)
                {
                    if (GUI.Button(rect, (piece.Id.Index + 1).ToString(), pieceLabel)) controller.Select(move);
                }
                else GUI.Label(rect, (piece.Id.Index + 1).ToString(), pieceLabel);
            }
            GUI.Label(new Rect(35, 790, 740, 44), "Gold = safe (opponent blocks entry)  ·  White border = legal human piece\nColored spokes = private Ascension Paths  ·  Exact roll to reach home", small);
            DrawPanel();
            GUI.matrix = Matrix4x4.identity;
        }
        private void DrawPanel()
        {
            Fill(new Rect(805, 20, 375, 810), new Color(.075f, .086f, .12f));
            var session = controller.Session;
            var state = session.State;
            bool seedValid = uint.TryParse(seedText, out uint seed);
            bool scenarioPaused = false;
#if UNITY_EDITOR
            scenarioPaused = controller.DevelopmentScenarioAwaitingExit;
#endif
            string status = !session.Started ? "Ready to descend?" : state.Winner.HasValue
                ? LocalMatchController.Name(state.Winner.Value) + " WINS!" : LocalMatchController.Name(state.ActivePlayer) + " turn";
            GUI.Label(new Rect(825, 40, 340, 42), status, title);
            string instruction = !session.Started ? "Summon on six. Race all four pieces home." : state.Winner.HasValue
                ? "All four pieces are home. Play another match." : session.IsHumanTurn
                ? state.Phase == MatchPhase.AwaitingRoll ? "Roll to begin your move." : "Choose a white-bordered piece or a button below."
                : "Bots are playing automatically...";
#if UNITY_EDITOR
            if (scenarioPaused && !state.Winner.HasValue)
                instruction = "Test result shown; bots are paused. PLAY AGAIN starts a normal match.";
#endif
            GUI.Label(new Rect(825, 88, 332, 58), instruction, label);
            GUI.Label(new Rect(825, 152, 332, 36), "DIE  " + (controller.LatestRoll?.ToString() ?? "—"), title);
            if (!session.Started || state.Winner.HasValue || scenarioPaused)
            {
                GUI.Label(new Rect(825, 200, 68, 28), "Seed", label);
                seedText = GUI.TextField(new Rect(895, 200, 257, 30), seedText, 10);
                GUI.enabled = seedValid;
                if (GUI.Button(new Rect(825, 246, 332, 45), session.Started ? "PLAY AGAIN" : "START MATCH", button))
                { if (session.Started) controller.PlayAgain(seed); else controller.StartMatch(seed); }
                GUI.enabled = true;
                if (!seedValid) GUI.Label(new Rect(825, 293, 332, 25), "Enter an integer from 0 to 4294967295.", small);
            }
            else
            {
                GUI.enabled = session.IsHumanTurn && state.Phase == MatchPhase.AwaitingRoll;
                if (GUI.Button(new Rect(825, 200, 332, 42), "ROLL", button)) controller.Roll();
                GUI.enabled = true;
                var moves = session.SelectableMoves;
                for (int i = 0; i < moves.Count; i++)
                {
                    var move = moves[i];
                    if (GUI.Button(new Rect(825 + (i % 2) * 171, 253 + (i / 2) * 40, 161, 34), "Piece " + (move.Piece.Index + 1), button)) controller.Select(move);
                }
            }
            GUI.Label(new Rect(825, 350, 332, 26), "MATCH LOG  ·  latest at bottom", small);
            for (int i = 0; i < controller.Log.Count; i++) GUI.Label(new Rect(825, 380 + i * 22, 332, 24), controller.Log[i], small);
            debug = GUI.Toggle(new Rect(825, 665, 170, 26), debug, "Development panel");
            if (debug) GUI.Label(new Rect(825, 695, 332, 120),
                "Seed: " + session.Seed + "   Revision: " + state.Revision + "\n" + state.ActivePlayer + " / " + state.Phase +
                "\nPending die: " + (state.PendingRoll?.ToString() ?? "—") + "   Legal moves: " + session.LegalMoves.Count +
                "\nWinner: " + (state.Winner?.ToString() ?? "—"), small);
        }
    }
}
