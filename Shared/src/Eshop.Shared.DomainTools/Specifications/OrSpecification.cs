namespace EShop.Shared.DomainTools.Specifications;

public class OrSpecification<T> : Specification<T>
{
    private readonly ISpecification<T> _specification1;
    private readonly ISpecification<T> _specification2;

    public OrSpecification(ISpecification<T> leftSpecification, ISpecification<T> rightSpecification)
    {
        _specification1 = leftSpecification;
        _specification2 = rightSpecification;
    }

    protected override IEnumerable<string> IsNotSatisfiedBecause(T obj)
    {
        var reason1 = _specification1.WhyIsNotSatisfiedBy(obj);
        var reason2 = _specification2.WhyIsNotSatisfiedBy(obj);

        if (!reason1.Any() || !reason2.Any())
        {
            return Enumerable.Empty<string>();
        }

        return reason1.Concat(reason2);
    }
}