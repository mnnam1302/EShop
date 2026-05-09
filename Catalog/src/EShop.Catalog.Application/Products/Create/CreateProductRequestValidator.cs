using EShop.Shared.Contracts.Shared;
using FluentValidation;

namespace EShop.Catalog.Application.Products.Create;

public sealed class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(ModelConstants.MediumText);

        RuleFor(x => x.Description)
            .MaximumLength(ModelConstants.LongText);

        RuleFor(x => x.CategoryId)
            .NotEmpty();

        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(ModelConstants.MediumLongText);

        RuleForEach(x => x.Tags)
            .MaximumLength(50);

        RuleForEach(x => x.Images)
            .Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute));
    }
}