<TODO: **Important!** Consider splitting a large user story into multiple pull requests (PRs) for easier review>

## Summary of changes
<TODO: Please summarize the changes>

- Jira: link to JIRA ticket, e.g.
- Review: link to JIRA code review sub-task, e.g.
- Solution: link to JIRA solutioning sub-task or indicate current state of pending discussions

<TODO: Please link back from the PR Links section of the JIRA ticket's to this PR>

### All services
- <TODO: changes for all services if any>

### Compliance/Configuration/Documents/Finance/Pricing/Sales/Tenancy/Users/Motor Registry/Migration/Reinsurance
- <TODO: service changes if any; mention key files; consider adding a comment to highlight important changes>

## AC verification
<TODO: Please describe how the changes are fulfilling the Acceptance Criteria. Provide screen recordings (or a link to them),
relevant log entries, BDD scenarios or simply a short explanation on how we can confirm everything is working as expected.>

## Please consider [checklist v24 (Updated 17/06/25)](Docs/PullRequest-CheckList.md):
(Please add notes on **how** each aspect has been addressed to help reviewer accept the item)
- [ ] CPS001 Backward-compatible data access (DB Schema/Domain Events/background jobs/side-by-side) - TODO
- [ ] CPS002 Safe EF migrations (separate PR; max length and indexes) - TODO
- [ ] CPS003 Public contract backward-compatible for ALL Clients (API and Integration Events) - TODO
- [ ] CPS005 Multi-service deployment (Specific Order/Downtime/Independent) - TODO
- [ ] CPS006 Safe release of new Features (Feature Switch/Additive not active by default)/Emergency Off) - TODO
- [ ] CPS007 Tests - TODO
- [ ] CPS009 Exceptions and validations - TODO
- [ ] CPS010 Logging - TODO
- [ ] CPS012 Documentation (including feature recordings) - TODO
- [ ] CPS019 Shared Product Generator - TODO
- [ ] CPS020 Secure code (user input, deserialization, RegEx) - TODO
- [ ] CPS021 Data Classification - TODO
- [ ] CPS022 Live data (data patching/configuration/permissions) - TODO
- [ ] CPS023 No personal data usage (code, tests and logs) - TODO
- [ ] CPS024 Resilient communications (retry, idempotent messages etc) - TODO
- [ ] CPS025 Self-review - TODO

## Just before completing please double-check
- [ ] PR Title, Merges, Components, outdated CI and SemVer