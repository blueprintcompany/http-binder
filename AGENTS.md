# AGENTS.md

## Repository Instructions

- When you generate GitHub PR summaries, use the full branch changes, not just the last commit.
- Keep package identity stable as `BlueprintSoftware.HttpBinder` unless a maintainer explicitly approves a breaking rename.
- Treat these as public contract: package ID, namespace, attribute names, enum values, analyzer diagnostic IDs, and generated `BindAsync` shape.
- Before release-related changes, run restore, tests, pack, and inspect the package contents.
