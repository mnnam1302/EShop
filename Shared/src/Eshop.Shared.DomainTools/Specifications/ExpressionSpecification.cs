namespace EShop.Shared.DomainTools.Specifications;

public class ExpressionSpecification<T> : CompositeSpecification<T>
{
    private readonly Func<T, bool> _expression;

    public ExpressionSpecification(Func<T, bool> expression)
    {
        _expression = expression ?? throw new ArgumentNullException(nameof(expression));
    }

    public override bool IsSatisfiedBy(T o)
    {
        return _expression(o);
    }
}
