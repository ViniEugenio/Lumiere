using FluentValidation;
using Lumiere.Application.Features.Common;
using Lumiere.Application.Resources;

namespace Lumiere.Application.Validators;

public class BasePaginationValidator : AbstractValidator<BasePaginatedQuery>
{
    public BasePaginationValidator()
    {
        int validMinPage = 1;

        RuleFor(basePagination => basePagination.Page)
            .GreaterThanOrEqualTo(validMinPage)
            .WithMessage(Errors.InvalidPage);

        int validMinPageAmount = 1;
        int validMaxPageAmount = 100;

        RuleFor(basePagination => basePagination.PageAmount)
            .InclusiveBetween(validMinPageAmount, validMaxPageAmount)
            .WithMessage(Errors.InvalidPageAmount);
    }
}
