---
name: reviewing-pull-requests
description: 'Perform comprehensive pull request reviews for the EShop .NET microservices codebase using GitHub MCP pull request tools. Use when asked to review a PR, inspect changed files or diff hunks, evaluate GitHub review comments or CI check runs, check merge readiness, or look for anti-patterns in pull request changes. Triggers: "review PR", "code review", "check this PR", "review my pull request", "review pull request", "review PR comments", "check CI on PR", "review for anti-patterns".'
---

# Reviewing Pull Requests

Perform thorough code reviews for the EShop .NET microservices following the team's PR checklist and quality standards.

Prefer authoritative pull request data from GitHub MCP tools over local workspace git state. `get_changed_files` is for local staged and unstaged changes, not for reviewing a GitHub pull request.

## When to Use This Skill

- User asks to "review a PR", "perform code review", or "check changes"
- User wants feedback before merging
- User asks to evaluate code quality or identify issues
- User mentions "PR checklist" or "review checklist"
- User asks to inspect PR comments, review threads, or CI/check-run failures
- User asks to look for anti-patterns or architectural issues

## Review Workflow

### Step 1: Gather Context

```text
Review Progress:
- [ ] Identify owner, repo, and pull request number
- [ ] Read PR title, description, labels, base branch, and head branch
- [ ] Get list of changed files
- [ ] Get patch context for the changed files
- [ ] Review existing comments, review threads, and check runs when relevant
- [ ] Understand the scope (which microservices and layers)
```

If the pull request number is not provided, locate it with `mcp_github_search_pull_requests` or `mcp_github_list_pull_requests` before starting the review.

If the pull request head is not checked out locally and you need exact file contents beyond the diff, use `mcp_github_get_file_contents` with `ref: "refs/pull/{pullNumber}/head"`.

### Step 2: Apply Checklist Categories

Work through each applicable category from [pr-checklist.md](./pull-request-checklist.md) based on the actual scope of the pull request. Focus on checklist items that are materially affected by the changed code and deployment shape.

At minimum, consider:

- Backward compatibility for APIs, integration events, data, and Hangfire jobs
- Multi-tenant safety and tenant-boundary authorization
- Independent deployment and rolling-update compatibility
- Test coverage and whether missing tests create regression risk
- Logging, exception translation, and resilience expectations
- Security, secrets, PII, and external-input validation

### Step 3: Review for Anti-Patterns

Review the changed code for EShop-specific anti-patterns using the anti-pattern guidance from the `refactor` skill.

Reference: the EShop Anti-Patterns in the `refactor` skill.
- Focus on anti-patterns that are present in the actual diff, not theoretical concerns
- When an anti-pattern is found, report:
  - anti-pattern name
  - why the changed code matches it
  - specific file/line references
  - recommended remedy aligned with the reference guide
- If no anti-patterns are found, state that explicitly in the review output

## PR Checklist Reference

See [pr-checklist.md](./pull-request-checklist.md)

## Anti-Pattern Reference

See the EShop Anti-Patterns in the `refactor` skill.

## Review Output Format

Structure your review as:

```markdown
### ❌ Issues Requiring Changes
- [Severity] [Short title]: [Concrete problem, impact, and file:line reference]

### ⚠️ Concerns / Suggestions
- [Item]: [Specific concern, risk, or improvement with file:line reference]

### Questions for Author
- [Any clarifying questions]

### Anti-Patterns Found
- [Anti-pattern name]: [Why it applies, with file:line reference and recommended remedy]

### Brief Summary
- Scope: [Services/areas modified]
- Risk Level: [Low/Medium/High]
- Checks Passed: [List passed items briefly]
- Residual Risks / Testing Gaps: [Any remaining uncertainty]
```

If no anti-patterns are found, add:

```markdown
### Anti-Patterns Found
- None identified in the changed code.
```

If no issues requiring changes are found, state that explicitly and still mention any residual risk or missing verification, for example missing test execution evidence or unverified deployment assumptions.

Review findings should be the primary focus of the output. Do not lead with summary text before the issues. Order findings by severity and keep summaries brief.

## Domain-Specific Patterns to Watch

### EventFlow/CQRS Patterns

- Command modifying multiple aggregates → Split or use integration events
- Commands published from domain event subscribers → Use only for logging/events
- Missing specification checks in aggregate methods

### JSON:API Controllers

- Extra action methods on `JsonApiController` → Move to separate `OperationsController`
- Missing permission checks on endpoints

### Multi-tenancy

- Single DbContext across tenants for `IScoped`/`IRingFenced` entities → Critical bug
- Missing TenantId in unique indexes

### Integration Events

- Breaking changes to event contracts
- Missing idempotency handling in consumers

## EShop Anti-Patterns to Check

Reviewers should explicitly check the changed code for EShop anti-patterns found in the reference guide. Report only anti-patterns that are actually introduced or reinforced by the pull request.

## Practical Guidance

- Prefer reviewing the actual diff before reading entire files.
- Escalate only on issues that are evidenced by the pull request, not on speculative future concerns.
- When citing a problem, explain the expected regression or risk, not just the style violation.
- If CI is failing, distinguish between failures caused by the PR and unrelated pre-existing failures when the evidence supports that distinction.
- If the pull request modifies shared code, explicitly consider downstream service impact and SemVer implications.

## Shortcuts

For quick partial reviews:

| Focus | Command |
| ----- | ------- |
| Security only | "Review for security issues" |
| Breaking changes | "Check for backwards compatibility" |
| Test coverage | "Review test coverage" |
| Existing review feedback | "Review PR comments and unresolved threads" |
| CI signal | "Check CI on this PR" |
| Architecture | "Review for anti-patterns" |
