# Evidence strategy

Foundation: run `python tools/check_foundation.py` for local links, required files,
schema structure, task IDs and milestone exclusion checks. CI runs the same command.
These are structural checks, not a substitute for human narrative/consistency review.

M001: test approved legal/illegal transitions, safe/home boundaries, knockout,
turn sequencing, win and restart. Use deterministic seeds and state invariants;
record simulation timeouts and failure seeds. Pair automated evidence with an
actual human match and fresh-checkout setup. No gameplay tests exist yet.
