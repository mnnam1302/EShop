namespace EShop.Shared.DomainTools.Specifications;

public interface ISpecification<T>
{
    bool IsSatisfiedBy(T o);
    IEnumerable<string> WhyIsNotSatisfiedBy(T obj);
}
