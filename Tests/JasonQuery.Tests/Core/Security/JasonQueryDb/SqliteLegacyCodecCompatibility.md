# SQLite Legacy Codec Compatibility Characterization

> **Historical characterization status**
>
> This document records the pre-Storage V2 investigation that established the
> historical SQLite compatibility boundary. It is retained as engineering
> evidence and should not be read as the current canonical JasonQuery.db
> architecture.
>
> JasonQuery v0.96 uses the later Storage V2 design with an isolated modern
> SQLCipher-compatible runtime while retaining a separate legacy migration
> boundary for supported historical databases. The characterization results
> below remain relevant to that historical compatibility boundary; statements
> about future provider or codec selection reflect the point in time when this
> investigation was performed.

## Purpose

This document records the SQLite provider and legacy encrypted database
compatibility findings established before the JasonQuery.db storage provider
is modernized.

This characterization does not change the production SQLite provider,
database format, encryption format, or migration behavior.

## Current JasonQuery Runtime Baseline

At the time of this characterization:

| Component | Version |
| --- | --- |
| System.Data.SQLite managed assembly | 1.0.112.0 |
| SQLite engine | 3.30.1 |
| SQLite.Interop | 1.0.112.0 |
| Process architecture | x64 |

Characterized SHA-256 values:

| File | SHA-256 |
| --- | --- |
| System.Data.SQLite.dll 1.0.112.0 | 2229240874CB86363B01BFB117E75FCB5E1A3884F59D7ECAC013DAD1D36CB730 |
| SQLite.Interop.dll 1.0.112.0 | 2ABB21507A37592DD97C11A035E3450D4D4E3438BEC7B62C27D096C3952DD2DB |

## Legacy Runtime Candidate

System.Data.SQLite.Core package 1.0.112.2 was characterized and validated as
a legacy-codec reader candidate for the historical migration boundary.

The package contains binaries identifying themselves as version 1.0.112.1.

| File | SHA-256 |
| --- | --- |
| net40 System.Data.SQLite.dll 1.0.112.1 | 19AD18AD0A128F690667C7239DBAF89629ABE43A6BB365BAC295B72A8CC26318 |
| x64 SQLite.Interop.dll 1.0.112.1 | 96C952EFA25720EEC63437DF20E20B8959DDE5230C6F1D5C30BE68CF72665532 |

The 1.0.112.0 and 1.0.112.1 legacy runtimes were compatible in the
characterized create, read, password-validation, and rekey scenarios.

## Cross-Version Characterization

| Scenario | Result |
| --- | --- |
| 1.0.112.0 creates encrypted DB; 1.0.112.1 reads it | PASS |
| 1.0.112.1 creates encrypted DB; 1.0.112.0 reads it | PASS |
| 1.0.112.1 rekeys a 1.0.112.0 encrypted DB | PASS |
| 1.0.112.0 reads the DB rekeyed by 1.0.112.1 | PASS |
| Wrong password is rejected | PASS |
| No password is rejected | PASS |
| Original source DB remains unchanged during copy-based tests | PASS |

## Real Historical JasonQuery.db Characterization

Real historical databases were tested locally and are intentionally not
committed to the repository.

| Historical database | 1.0.112.1 legacy runtime |
| --- | --- |
| v0.91 Legacy Custom Password | PASS |
| v0.94 Legacy Default Password | PASS |

For both historical databases:

- The expected legacy password opened the database.
- A wrong password was rejected.
- No-password access was rejected.
- The SystemConfig table was readable.
- Read-only validation did not modify the database.
- No SQLite journal, WAL, or SHM sidecar was created.

Historical database files, their credentials, local paths, and content
fingerprints are intentionally excluded from the repository.

## Modern Ordinary SQLite Control

The modern control runtime used:

| Component | Version |
| --- | --- |
| System.Data.SQLite | 2.0.4 |
| Target asset | net471 |
| SourceGear.sqlite3 | 3.53.4 |
| Native architecture | Windows x64 |

Characterized SHA-256 values:

| File | SHA-256 |
| --- | --- |
| System.Data.SQLite.dll 2.0.4 net471 | 833F96850D8FAE7AA30398A2EC5A048F34AA2CCA0391469019A5CB64BFCFC7EF |
| e_sqlite3.dll 3.53.4 win-x64 | 6AD8E149F8CE3ED3716402B4B3A2268EBBDC7B64391B5FAFED747E03BB1B9418 |

The modern runtime successfully created and reopened an ordinary plaintext
SQLite database.

Observed runtime characteristics:

- SQLite engine version: 3.53.4.
- The ordinary native engine exposed no codec-, SEE-, or cipher-related
  compile options.
- System.Data.SQLite 2.0.4 still exposed SetPassword and ChangePassword
  managed APIs.
- API presence did not imply legacy codec support.

## Modern Runtime Negative Controls

| Historical database | Modern 2.0.4 + ordinary SQLite 3.53.4 |
| --- | --- |
| v0.91 Legacy Custom Password | NOT READABLE - expected |
| v0.94 Legacy Default Password | NOT READABLE - expected |

For both databases, SetPassword completed but Connection.Open failed before
the schema could be read.

The exact exception text is an observation only and must not be used as a
database-format detector or migration-routing signal.

Both database copies and both original historical databases remained
unchanged during the negative controls.

## Architectural Conclusions

1. System.Data.SQLite 2.x with an ordinary SQLite native engine cannot replace
   the historical JasonQuery legacy-codec reader.

2. A legacy 1.x SQLite runtime must remain available in an isolated migration
   boundary while JasonQuery continues to support historical encrypted
   JasonQuery.db formats.

3. Historical database routing must use persisted storage/security format
   state. It must not use exception text, API availability, or
   try-new-then-fallback-to-old heuristics.

4. Database Security V2 metadata and SQLite page/storage codec version are
   separate concepts and must remain separately versioned.

5. Migration to a future modern encrypted SQLite format must be treated as a
   database-format migration, not merely as ChangePassword.

6. Plaintext SQLite database staging is not an acceptable migration design.

7. The normal runtime provider and the historical migration provider must be
   treated as separate responsibilities.

8. Future releases must preserve direct migration from supported historical
   JasonQuery.db states to the current canonical state without requiring the
   user to run intermediate JasonQuery releases.

## Scope Boundary

This characterization does not select the future JasonQuery.db encryption
codec.

It does not:

- adopt System.Data.SQLite 2.x in production;
- adopt SEE, SQLCipher, or another encrypted SQLite implementation;
- change JasonQuery.db;
- change Database Security V2;
- change startup migration behavior;
- change release packaging;
- add user-facing SQLite datasource support.

Those decisions belong to subsequent migration and storage-format work.
