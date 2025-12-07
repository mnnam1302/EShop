using EShop.Catalog.Application.Categories.Create;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.DomainTools.Entities;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;

namespace EShop.Catalog.Application.Categories;

public sealed class CategoryAggregate : Aggregate, IScoped
{
    public string Name { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public CategoryStateMachine StateMachine { get; set; } = new CategoryStateMachine();

    public static CategoryAggregate Create(CreateCategoryCommand command, IUserDetailsProvider userDetailsProvider)
    {
        var category = new CategoryAggregate();
        category.RaiseEvent(new CategoryCreatedEvent
        {
            CategoryId = Guid.NewGuid(),
            Name = command.Name,
            Reference = command.Reference,
            Slug = command.Slug,
            ParentId = command.ParentId,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            TenantId = userDetailsProvider.AuthenticatedUser.TenantId,
            Scope = userDetailsProvider.AuthenticatedUser.TenantId
        });

        return category;
    }

    public void Apply(CategoryCreatedEvent @event)
    {
        Id = @event.CategoryId;
        Name = @event.Name;
        Reference = @event.Reference;
        Slug = @event.Slug;
        ParentId = @event.ParentId;
        CreatedAtUtc = @event.CreatedAtUtc;
        UpdatedAtUtc = @event.UpdatedAtUtc;
        TenantId = @event.TenantId;
        Scope = @event.Scope;
    }
}