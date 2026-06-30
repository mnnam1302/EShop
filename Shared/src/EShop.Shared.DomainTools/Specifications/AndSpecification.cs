namespace EShop.Shared.DomainTools.Specifications;

public class AndSpecification<T> : Specification<T>
{
    private readonly ISpecification<T> _specification1;
    private readonly ISpecification<T> _specification2;

    public AndSpecification(ISpecification<T> leftSpecification, ISpecification<T> rightSpecification)
    {
        _specification1 = leftSpecification;
        _specification2 = rightSpecification;
    }

    protected override IEnumerable<string> IsNotSatisfiedBecause(T obj)
    {
        return _specification1.WhyIsNotSatisfiedBy(obj)
            .Concat(_specification2.WhyIsNotSatisfiedBy(obj));
    }
}