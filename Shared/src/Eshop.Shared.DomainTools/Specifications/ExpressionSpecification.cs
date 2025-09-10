namespace EShop.Shared.DomainTools.Specifications;

public class ExpressionSpecification<T> : CompositeSpecification<T>
{
    private readonly Func<T, bool> expression;

    public ExpressionSpecification(Func<T, bool> expression)
    {
        this.expression = expression ?? throw new ArgumentNullException(nameof(expression));
    }

    public override bool IsSatisfiedBy(T o)
    {
        return this.expression(o);
    }
}
