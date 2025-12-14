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
            .Permit(ProductAction.Publish, ProductState.Published)
            .Permit(ProductAction.Delete, ProductState.Deleted);

        Configure(ProductState.Published)
            .Permit(ProductAction.Unpublish, ProductState.Unpublished)
            .Permit(ProductAction.Delete, ProductState.Deleted);

        Configure(ProductState.Unpublished)
            .Permit(ProductAction.Publish, ProductState.Published)
            .Permit(ProductAction.Delete, ProductState.Deleted);

        Configure(ProductState.Deleted)
            .Ignore(ProductAction.Publish)
            .Ignore(ProductAction.Unpublish)
            .Ignore(ProductAction.Delete);
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
    Publish = 1,
    Unpublish = 2,
    Delete = 3
}