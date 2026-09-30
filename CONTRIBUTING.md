# Contributing

1. Read [agent policy](AGENTS.md) and select an authorized, READY task.
2. Inspect status; branch from current main (`codex/<topic>` by default;
   this bootstrap uses the requested `foundation/v0.1`). Preserve unrelated work.
3. Agree acceptance criteria and dependencies before implementation. Keep scope small.
4. Update docs with CANON / WORKING DECISION / PROPOSAL / UNKNOWN / RESTRICTED labels.
5. Run relevant checks and record commands, results and limitations in the PR.
6. Review for secrets, rights issues, broken links and unintended product decisions.
7. Request review; the owner merges. No force push or published-history rewriting.

Use focused commits such as `docs: clarify safe-square rules`. A proposal may be
documented without being accepted. Approval-gated work must name its reviewer.
See [task template](.github/ISSUE_TEMPLATE/task.yml) and
[ADR template](docs/adr/template.md). No dependencies beyond Python are currently needed.
