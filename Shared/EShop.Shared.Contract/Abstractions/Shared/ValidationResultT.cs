namespace EShop.Shared.Contract.Abstractions.Shared;

public class ValidationResult<TValue> : Result<TValue>, IValidationResult
{
    private ValidationResult(Error[] errors)
        : base(default, false, IValidationResult.ValidationError)
    {
        Errors = errors;
    }

    public Error[] Errors { get; set; }

    public static ValidationResult<TValue> WithErrors(Error[] errors)
        => new(errors);
}