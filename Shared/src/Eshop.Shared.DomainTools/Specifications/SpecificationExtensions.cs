using EShop.Shared.DomainTools.Exceptions;
using System.Linq.Expressions;

namespace EShop.Shared.DomainTools.Specifications;

public static class SpecificationExtensions
{
    public static void ThrowDomainErrorIfNotSatisfied<T>(this ISpecification<T> specification, T obj)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var reasons = specification.WhyIsNotSatisfiedBy(obj).ToList();
        if (reasons.Any())
        {
            var specificationName = specification.GetType().Name;
            var prettyMessage = $"'{specificationName}' is not satisfied because of {string.Join(" and ", reasons)}";

            throw new DomainException(specificationName, prettyMessage);
        }
    }

    public static ISpecification<T> And<T>(this ISpecification<T> specification1, ISpecification<T> specification2)
    {
        return new AndSpecification<T>(specification1, specification2);
    }

    public static ISpecification<T> And<T>(this ISpecification<T> specification, Expression<Func<T, bool>> expression)
    {
        return specification.And(new ExpressionSpecification<T>(expression));
    }

    public static ISpecification<T> Or<T>(this ISpecification<T> specification1, ISpecification<T> specification2)
    {
        return new OrSpecification<T>(specification1, specification2);
    }
    public static ISpecification<T> Or<T>(this ISpecification<T> specification, Expression<Func<T, bool>> expression)
    {
        return specification.Or(new ExpressionSpecification<T>(expression));
    }

    public static ISpecification<T> Not<T>(this ISpecification<T> specification)
    {
        return new NotSpecification<T>(specification);
    }
}