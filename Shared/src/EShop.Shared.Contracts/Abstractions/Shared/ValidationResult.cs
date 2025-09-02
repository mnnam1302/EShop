namespace EShop.Shared.Contracts.Abstractions.Shared;

public class ValidationResult : Result, IValidationResult
{
    private ValidationResult(Error[] errors)
        : base(false, IValidationResult.ValidationError)
    {
        Errors = errors;
    }

    public Error[] Errors { get; set; }

    public static ValidationResult WithErrors(Error[] errors) => new(errors);
}