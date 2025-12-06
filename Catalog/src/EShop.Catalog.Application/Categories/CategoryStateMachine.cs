using Stateless;

namespace EShop.Catalog.Application.Categories;

public sealed class CategoryStateMachine : StateMachine<CategoryState, CategoryAction>
{
    public CategoryStateMachine(Func<CategoryState> stateAccessor, Action<CategoryState> stateMutator) 
        : base(stateAccessor, stateMutator)
    {
    }

    public CategoryStateMachine() : base(CategoryState.Unpublished)
    {
        Configure();
    }

    public void Configure()
    {
        Configure(CategoryState.Unpublished)
            .Permit(CategoryAction.Publish, CategoryState.Published);
        Configure(CategoryState.Published)
            .Permit(CategoryAction.Unpublish, CategoryState.Unpublished);
    }
}

public enum CategoryAction
{
    Create = 0,
    Publish = 1,
    Unpublish = 2
}

public enum CategoryState
{
    Unpublished = 0,
    Published = 1
}
