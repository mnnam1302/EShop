namespace EShop.Shared.DomainTools.Specifications;

/// <summary>
/// Represents the Specification Pattern interface which allows for business rules to be 
/// defined in a declarative manner and combined using logical operators.
/// </summary>
/// <typeparam name="T">The type to which the specification applies</typeparam>
/// <remarks>
/// Usage example:
/// 
/// // Define specification classes
/// public class ProductInStockSpecification : ISpecification<Product>
/// {
///     public bool IsSatisfiedBy(Product product) => product.StockQuantity > 0;
///     
///     // Implement And, Or, Not methods...
/// }
/// 
/// public class ProductActivatedSpecification : ISpecification<Product>
/// {
///     public bool IsSatisfiedBy(Product product) => product.IsActive;
///     
///     // Implement And, Or, Not methods...
/// }
/// 
/// // Combine specifications
/// var inStockSpec = new ProductInStockSpecification();
/// var activatedSpec = new ProductActivatedSpecification();
/// var availableProductSpec = inStockSpec.And(activatedSpec);
/// 
/// // Use specification
/// bool isAvailable = availableProductSpec.IsSatisfiedBy(someProduct);
/// </remarks>
public interface ISpecification<T>
{
    bool IsSatisfiedBy(T o);
    ISpecification<T> And(ISpecification<T> specification);
    ISpecification<T> Or(ISpecification<T> specification);
    ISpecification<T> Not(ISpecification<T> specification);
}
