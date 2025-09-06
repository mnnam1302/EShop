namespace EShop.Shared.DomainTools.Specifications;

public class AndSpecification<T> : CompositeSpecification<T>
{
    ISpecification<T> leftSpecification;
    ISpecification<T> rightSpecification;

    public AndSpecification(ISpecification<T> leftSpecification, ISpecification<T> rightSpecification)
    {
        this.leftSpecification = leftSpecification;
        this.rightSpecification = rightSpecification;
    }

    public override bool IsSatisfiedBy(T o)
    {
        return leftSpecification.IsSatisfiedBy(o) && rightSpecification.IsSatisfiedBy(o);
    }
}
