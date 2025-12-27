# GitHub Copilot Custom Instructions

This file provides guidance for GitHub Copilot when assisting with the Komodo solution. For comprehensive agent guidelines, see [AGENTS.md](/AGENTS.md).

## Repository Context

This is a .NET monorepo for the Seamless SaaS multi-tenant insurance platform hosted in AWS, containing multiple microservices including:
- **Tenancy**: Tenant and feature management and settings
- **Users**: User, application, and authentication management

## C# Development Standards

### Language & Framework
- **C#** with nullable reference types enabled
- **.NET 10** microservices architecture
- Follow `.editorconfig` formatting rules strictly

### Code Style
- Use **file-scoped namespace declarations**
- Single-line `using` directives
- Newline before opening curly braces
- Pattern matching and switch expressions preferred
- Use `nameof()` instead of string literals for member names
- Declare variables non-nullable by default
- Use `is null` or `is not null` for null comparisons
- XML documentation comments for all public APIs

### Key Dependencies & Patterns
- **EventFlow**: CQRS and Event Sourcing framework
- **JsonApiDotNetCore**: JSON:API compliant APIs
- **MassTransit**: Integration events and messaging
- **Hangfire**: Background job processing
- **XUnit**: Unit testing with FakeItEasy 8.x for mocking
- **Reqnroll.XUnit**: BDD testing with Cucumber expressions

## Common Patterns

### API Request Processing (EventFlow)
1. Validate and sanitize input data
2. Check feature and user permissions
3. Convert request to command and publish using `ICommandBus`
4. Command handler calls a single method on aggregate root
5. Aggregate root checks specification and emits domain events
6. Read/write models apply domain events
7. Domain event subscribers publish business logs, audit logs, or integration events
8. Return appropriate response

### Integration Event Consumption
1. Validate and sanitize integration event data
2. Check feature switches and configuration
3. Perform idempotency check
4. Convert to command and publish using `ICommandBus`
5. Follow standard command processing flow

## Anti-Patterns to Avoid

❌ **DO NOT** add extra action methods to `JsonApiController` derivatives
- ✅ Instead: Create separate `Operations` controllers derived from `ControllerBase`

❌ **DO NOT** modify multiple aggregates in a single command
- ✅ Instead: Each command modifies only one aggregate; use integration events for coordination

❌ **DO NOT** publish commands from EventFlow domain event subscribers
- ✅ Instead: Use subscribers only for logging and publishing integration events

❌ **DO NOT** use a single DbContext for operations across multiple tenants for any `IScoped` or `IRingFenced` entity
- ✅ Instead: Ensure each DbContext instance is scoped to a single tenant to maintain proper data isolation

❌ **DO NOT** change `[EventVersion]` name or version number without implementing `IEventUpgrader`
- ✅ EventFlow domain events are persisted in the event stream. Changing event names or versions breaks deserialization of existing data.

## Testing Standards

### Unit Tests (NUnit)
- Include `// Arrange`, `// Act`, `// Assert` comments
- Use FakeItEasy for mocking
- Follow existing naming patterns (underscore-separated words for new test classes)
- Execute with `dotnet test`

### BDD Tests (Reqnroll.NUnit)
- Prefer Cucumber expressions in step definitions
- Execute with `dotnet test`

## Security & Performance

### Security
- Validate and sanitize all external inputs
- Encrypt sensitive data at rest
- Avoid `TypeNameHandling.Auto` in JSON.NET
- Prevent ReDoS attacks in regular expressions
- Follow security best practices for public APIs
- Never use a single DbContext for operations across multiple tenants for any `IScoped` or `IRingFenced` entity

### Performance
- Design for high concurrent user load
- Optimize memory allocation patterns
- Avoid querying large datasets in memory

## Development Workflow

1. **Analyze existing patterns** before making changes
2. **Make incremental changes** one file at a time
3. **Maintain test coverage** for all new code
4. **Follow existing conventions** in the codebase
5. **Respect backward compatibility** for:
   - Public APIs
   - Integration events
   - Background jobs
   - Existing data

## File Restrictions

⚠️ **NEVER modify** unless explicitly requested:
- `global.json`
- `package.json`
- `NuGet.config`

## Additional Resources

For comprehensive guidelines including planning phases, quality standards, and detailed patterns, refer to [AGENTS.md](/AGENTS.md).