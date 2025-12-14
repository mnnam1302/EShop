namespace EShop.Shared.DomainTools.Specifications;

public class AllSpecification<T> : Specification<T>
{
    private readonly IReadOnlyList<ISpecification<T>> _specifications;

    public AllSpecification(IEnumerable<ISpecification<T>> specifications)
    {
        var specificationList = (specifications ?? Enumerable.Empty<ISpecification<T>>()).ToList();

        if (!specificationList.Any())
        {
            throw new ArgumentException("At least one specification is required", nameof(specifications));
        }

        _specifications = specificationList;
    }

    protected override IEnumerable<string> IsNotSatisfiedBecause(T obj)
    {
        return _specifications.SelectMany(spec => spec.WhyIsNotSatisfiedBy(obj));
    }
}
