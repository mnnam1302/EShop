---
name: creating-pull-requests
description: Creates GitHub pull requests or generates PR descriptions with titles, descriptions, and checklist responses for EShop .NET microservices. Use when creating PRs, updating PRs, writing PR descriptions, filling PR checklists, or when user mentions "create pull request", "create PR", "update pull request", "update PR", "PR description". Analyzes git changes and formats according to repository standards.
---

# Creating Pull Requests

Generate complete pull request content following EShop repository standards.

## Workflow

Copy this checklist and track your progress:

```
PR Generation:
- [ ] Step 1: Gather required information
- [ ] Step 2: Analyze branch changes
- [ ] Step 3: Generate PR description
- [ ] Step 4: Generate checklist responses
- [ ] Step 5: Format PR title
- [ ] Step 6: Create PR or output description
```

## Step 1: Gather required information

Prompt user for:

- **JIRA issue key** (e.g., EShop-xxxxx) - required
- **Review ticket key** (e.g., EShop-yyyyy) - required
- **Solutioning ticket key** (e.g., EShop-zzzzz) - optional
- **PR title** - required
- **Action**: Create PR directly or generate description only? - required

## Step 2: Analyze branch changes

Retrieve git changes between current branch and master branch. Use changes as context for description and checklist.

## Step 3: Generate PR description

Use [pull_request_template.md](/pull_request_template.md) as structure template.

**Fill template sections:**

- **JIRA links**: Format as `https://EShopinsure.atlassian.net/browse/EShop-xxxxx`
- **Solution link**: Include only if solutioning ticket provided; otherwise delete the line
- **All services**: Include if changes affect Shared project or multiple services; otherwise delete section
- **Service sections**: Summarize changes per microservice (Compliance, Configuration, Documents, Finance, Migration, Motor Registry, Pricing, Reinsurance, Sales, Tenancy, Underwriting, Users)
- Delete service sections with no changes
- Delete TODO markers for completed sections
- Preserve `## AC verification` section unchanged with its TODO

**Analysis priority:**

1. Analyze actual code changes (source of truth)
2. Use git commit messages as hints only
3. When conflicts exist between commits and actual changes, trust the changes

## Step 4: Generate checklist responses

Reference the `pr-checklist.md` reference file of the `reviewing-pull-requests` skill for item definitions.

**Checklist formatting:**

- Answer each item from [pull_request_template.md](../../../.github/pull_request_template.md)
- Provide brief, specific explanations (e.g., "CPS001 - no schema changes, no domain event changes, service runs side-by-side")
- Keep all items **unchecked** (for reviewer evaluation)
- **No blank lines** between items (maintains single checklist block)

## Step 5: Format PR title

Add `[EShop-xxxxx]` prefix if not already present. Use the provided PR title text.

## Step 6: Create PR or output description

Determine action based on user input from Step 1:

**Option A: Create PR directly**

Run GitHub CLI command to create pull request:

```bash
gh pr create --title "[PR Title]" --body "[Full PR body with description and checklist]" --base master
```

Confirm successful creation and provide PR URL.

**Option B: Generate description only**

Display in markdown code block for manual copying:

````markdown
```markdown
[PR Title]

[PR Description]

[Checklist responses]
```
````
