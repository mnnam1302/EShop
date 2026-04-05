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
        Configure(ProductState.Draft)
            .PermitReentry(ProductAction.Update)
            .Permit(ProductAction.Publish, ProductState.Published)
            .Permit(ProductAction.Delete, ProductState.Deleted);

        Configure(ProductState.Published)
            .PermitReentry(ProductAction.Update)
            .Permit(ProductAction.Unpublish, ProductState.Unpublished);

        Configure(ProductState.Unpublished)
            .PermitReentry(ProductAction.Update)
            .Permit(ProductAction.Publish, ProductState.Published)
            .Permit(ProductAction.Delete, ProductState.Deleted);
    }
}

public enum ProductState
{
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
    Delete = 4
}