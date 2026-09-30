# Security and information boundaries

WORKING DECISION: ordinary repository content is agent-readable and potentially
public. Never commit credentials, personal player data, private keys, full secret
lore, unreleased identities or discovery conditions. Git history retains deletions.
Ignore files and obscure names do not enforce access control.

RESTRICTED: sensitive narrative material must live in a separate access-controlled
store with least-privilege grants, audit trails and human-controlled release packets.
The repository documents interfaces and redacted placeholders only. Do not add
real secret references or reconstruct withheld lore. Clients, logs, test fixtures,
analytics payloads, issue bodies and PR descriptions share this boundary.

PROPOSAL: future trusted backend checks secret eligibility; return only permitted
outcomes. Do not ship rules or secret identifiers in downloadable bundles.
Human review is required before any restricted reveal. No automated secret scan
can establish narrative confidentiality; manual review remains necessary.

If exposure is suspected, stop dissemination, notify the repository owner privately,
revoke exposed credentials where applicable and coordinate containment. Do not
paste the payload into a public issue or rewrite shared history unilaterally.
UNKNOWN: private reporting channel, threat model, data retention and compliance
requirements. Establish these before collecting player data or accepting reports.
