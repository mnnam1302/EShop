namespace EShop.Shared.DomainTools.Specifications;

public abstract class Specification<T> : ISpecification<T>
{
    public bool IsSatisfiedBy(T o)
    {
        return !IsNotSatisfiedBecause(o).Any();
    }

    public IEnumerable<string> WhyIsNotSatisfiedBy(T obj)
    {
        return IsNotSatisfiedBecause(obj);
    }

    protected abstract IEnumerable<string> IsNotSatisfiedBecause(T obj);
}