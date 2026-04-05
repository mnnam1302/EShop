using Stateless;

namespace EShop.Catalog.Application.Products;

public sealed class ProductStateMachine : StateMachine<ProductState, ProductAction>
{
    public ProductStateMachine(Func<ProductState> stateAccessor, Action<ProductState> stateMutator)
        : base(stateAccessor, stateMutator)
    {
        Configure();
    }

    public ProductStateMachine() : base(ProductState.Draft)
    {
        Configure();
    }

    public void Configure()
    {
        Configure(ProductState.Active)
            .InternalTransition(ProductAction.AddVariant, _ => { })
            .InternalTransition(ProductAction.UpdateVariant, _ => { })
            .InternalTransition(ProductAction.ChangeVariantPrice, _ => { })
            .InternalTransition(ProductAction.PublishVariant, _ => { })
            .InternalTransition(ProductAction.UnpublishVariant, _ => { })
            .InternalTransition(ProductAction.AddVariationDimension, _ => { })
            .InternalTransition(ProductAction.UpdateVariationDimension, _ => { })
            .InternalTransition(ProductAction.ChangeVariationDimensionValues, _ => { });

        Configure(ProductState.Draft)
            .SubstateOf(ProductState.Active)
            .PermitReentry(ProductAction.Update)
            .Permit(ProductAction.Publish, ProductState.Published)
            .Permit(ProductAction.Delete, ProductState.Deleted);

        Configure(ProductState.Published)
            .SubstateOf(ProductState.Active)
            .PermitReentry(ProductAction.Update)
            .Permit(ProductAction.Unpublish, ProductState.Unpublished);

        Configure(ProductState.Unpublished)
            .SubstateOf(ProductState.Active)
            .PermitReentry(ProductAction.Update)
            .Permit(ProductAction.Publish, ProductState.Published)
            .Permit(ProductAction.Delete, ProductState.Deleted);
    }
}

public enum ProductState
{
    Active = 0,
    Draft = 1,
    Published = 2,
    Unpublished = 3,
    Deleted = 4
}

public enum ProductAction
{
    Update = 1,
    Publish = 2,
    Unpublish = 3,
    Delete = 4,
    AddVariant = 5,
    UpdateVariant = 6,
    ChangeVariantPrice = 7,
    PublishVariant = 8,
    UnpublishVariant = 9,
    AddVariationDimension = 10,
    UpdateVariationDimension = 11,
    ChangeVariationDimensionValues = 12
}