# Security Policy

JasonQuery welcomes responsible reports of security vulnerabilities that affect the application, its official release packages, or JasonQuery-owned source code.

Please do not disclose security-sensitive details in a public GitHub issue, discussion, pull request, or other public channel.

## Supported Versions

JasonQuery uses Production and Test release channels.

| Release | Security support |
| --- | --- |
| Latest Production release | Supported |
| Earlier Production releases | Not normally supported; users should update to the latest Production release |
| Test releases | Best effort; Test releases may be replaced quickly |
| Unreleased code on `main` | Reports are accepted, but `main` is not an end-user support channel |

A security fix may be backported to an older release when the maintainer determines that doing so is necessary, but backports are not guaranteed.

## Reporting a Vulnerability

Use GitHub's private vulnerability reporting for this repository.

From the repository's **Security** tab, choose **Report a vulnerability** and submit the report privately.

Do not open a public issue containing vulnerability details, proof-of-concept code, credentials, recovery material, database contents, or other sensitive information.

A useful report should include, when applicable:

- the affected JasonQuery version or commit;
- the Windows version and architecture;
- the database platform and version, if the issue is database-specific;
- a clear description of the security impact;
- the minimum steps required to reproduce the issue;
- proof-of-concept material that uses synthetic or non-sensitive data;
- any known prerequisites, mitigations, or workarounds; and
- whether the issue reproduces on the latest Production release.

Please provide the smallest amount of sensitive material necessary to demonstrate the issue.

Unless specifically requested through the private reporting channel, do not send real database passwords, connection credentials, recovery keys, private keys, customer data, SQL history, production logs containing sensitive information, or a production `JasonQuery.db`.

## Security Scope

Examples of issues that are appropriate to report include:

- arbitrary code execution or command execution caused by JasonQuery;
- path traversal, unsafe archive extraction, or unsafe file replacement;
- bypasses of package integrity or update verification;
- vulnerabilities in the update, backup, rollback, or package-installation workflow;
- unintended disclosure or bypass of connection-credential protection;
- bypasses of `JasonQuery.db` security, key protection, password protection, recovery controls, or storage-migration safety checks;
- vulnerabilities that expose recovery keys or other security-sensitive material;
- privilege-boundary or trust-boundary violations in JasonQuery-owned code;
- unsafe handling of untrusted database metadata, files, or external input when it creates a security impact;
- official release-package integrity problems;
- secrets or credentials accidentally committed to the JasonQuery repository or included in an official release package; and
- vulnerabilities in a third-party component when JasonQuery's use or integration of that component creates an exploitable security issue.

This list is illustrative rather than exhaustive.

## Generally Out of Scope

The following are generally not considered JasonQuery security vulnerabilities by themselves:

- SQL statements intentionally entered or executed by a user with that user's existing database privileges;
- database permissions, authentication policy, network policy, or server configuration controlled by the database administrator rather than JasonQuery;
- a user who already has permission to modify the JasonQuery installation directory replacing JasonQuery executables or other files, unless the report demonstrates an additional JasonQuery protection-boundary bypass;
- vulnerabilities that exist only in an unsupported older JasonQuery release and do not affect the latest Production release;
- vulnerabilities solely in a third-party product or service with no JasonQuery-specific exploit path; and
- purely theoretical findings that do not demonstrate a meaningful security impact or violated trust boundary.

If a third-party dependency is affected, reporting the issue to the upstream project is usually appropriate. You may also report it privately to JasonQuery when an official JasonQuery release is affected or mitigation is required in JasonQuery.

## Response Process

JasonQuery is maintained by a single maintainer. Security reports are handled as promptly as practical, but response and remediation times can vary with severity, reproducibility, complexity, and upstream dependencies.

The current response targets are:

- acknowledgment within 5 business days;
- an initial assessment within 10 business days when sufficient reproduction information is available; and
- periodic updates for accepted reports when remediation requires additional time.

These are response targets, not guaranteed service-level agreements.

After a report is received, the maintainer may:

1. request additional reproduction information;
2. confirm the affected versions and security impact;
3. determine whether the issue belongs in JasonQuery or an upstream dependency;
4. prepare and validate a fix;
5. coordinate release timing and disclosure with the reporter; and
6. publish an advisory or release note when appropriate.

High-impact issues that are reproducible in a supported Production release receive priority.

## Coordinated Disclosure

Please keep vulnerability details private until a fix, mitigation, or coordinated disclosure date has been agreed upon.

When practical, JasonQuery will coordinate disclosure with the reporter after affected users have a reasonable opportunity to update.

A public advisory may include affected versions, impact, remediation, and credit to the reporter. Reporter credit is optional and will be omitted on request.

A CVE may be requested when appropriate, but not every accepted security issue requires a CVE.

## Security Research Guidelines

When testing JasonQuery:

- test only systems, accounts, databases, and data that you own or are authorized to use;
- use synthetic data whenever possible;
- do not access, modify, or delete another person's data;
- avoid destructive testing against production systems;
- stop testing once the security impact has been demonstrated sufficiently;
- do not use social engineering, phishing, spam, or denial-of-service attacks;
- do not attempt to compromise GitHub, the JasonQuery website host, database vendors, or other third-party infrastructure; and
- do not publish sensitive details before coordinated disclosure.

These guidelines do not authorize testing of systems owned or operated by third parties.

## Security Updates

Security fixes are normally delivered through a new Production release.

Users should install the latest Production release rather than relying on an older release after a security fix becomes available.

Official JasonQuery publishing and update workflows include package-integrity checks. Security-sensitive release changes are subject to the project's normal build, test, and package-validation process before publication.

## Bug Bounty

JasonQuery does not currently operate a paid bug bounty program.

Responsible reports are nevertheless appreciated and may be credited in a security advisory or release note when the reporter wishes to be identified.
