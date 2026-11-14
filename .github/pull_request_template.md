<!--
JasonQuery PR guide
- Keep this concise for low-risk changes; remove optional sections that do not apply.
- For maintainer-authored release PRs, prefer a title ending in (vX.YY); automation-generated PRs may use their own titles.
- Explain significant design/safety decisions for medium- or high-risk changes.
- Check validation items only after they PASS. Explain N/A or not-run items below.
- Never include secrets, connection strings, private evidence or raw credentials.
-->

## Summary

<!-- What problem does this PR solve? What changes for users or maintainers? -->

## Scope & Key Changes

<!-- Briefly describe the affected components, providers, and operations. Keep unrelated work out. -->

- 

## Design Decisions & Trade-offs (if applicable)

<!-- For nontrivial changes, record: chosen approach, why, significant rejected/deferred alternatives, and the trade-off. Omit for straightforward low-risk fixes. -->

- **Chosen / why:**
- **Rejected or deferred / why:**
- **Trade-off:**

## Safety Boundaries & Compatibility

<!-- State what must remain true: target/SQL identity, transactions, reconnect/retry rules, data loss risks, backwards compatibility, or N/A. Describe only risks relevant to this PR. -->

- **Preserved invariants:**
- **Failure / recovery behavior:**
- **Compatibility impact:**

## Verification & Evidence

<!-- Check only items actually passed. For N/A or not-run items, explain why in the Evidence notes. -->

- [ ] `Build\Validate-Repository.ps1` passed (local or equivalent qualification)
- [ ] GitHub Actions `Repository Guard` passed
- [ ] GitHub Actions `Secret Scan` passed
- [ ] `Release | x64` rebuild passed
- [ ] Relevant automated tests passed
- [ ] Relevant database integration tests passed
- [ ] Manual GUI / runtime / packaging smoke passed, when applicable
- [ ] `git diff --check` passed

**Evidence / results:**

<!-- Give actual counts, environments, relevant Golden-test groups and real GUI/DB outcomes. Distinguish simulated tests from observed DB execution. Link non-sensitive evidence where appropriate; do not commit VM ZIPs/reports solely as PR evidence. -->

**Not run / N/A (with reason):**

## Repository Safety & Scope Check

- [ ] No credentials, private/licensed dependencies, confidential data, or unintended database/release artifacts committed
- [ ] No unintended generated/build outputs (`bin`, `obj`, DLL, PDB, ZIP, etc.); intentionally versioned generated files such as localization XML are allowed
- [ ] No unrelated changes or formatting/refactoring mixed into the PR
- [ ] User-visible behavior and compatibility risks reviewed

## Known Limitations & Follow-ups (if applicable)

<!-- Explicitly state unsupported cases, non-goals, residual risks and work deferred to another PR. -->

## Engineering Phase Glossary (if applicable)

<!-- Explain internal identifiers such as F7-U3, C4C2, or Golden stages when needed for review. -->

## Reviewer Focus (if applicable)

<!-- Point reviewers to the most important files, invariants or regression risks. -->

## Release Notes

<!-- Choose: New Feature / Enhancement / Bug Fix / Maintenance / Not required -->

**Category:**

**Summary:**
