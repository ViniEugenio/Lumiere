using FluentValidation;
using Lumiere.Application.Interfaces;
using MediatR;

namespace Lumiere.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResultDto, new()
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        var validationErrors = await ValidateRequestAsync(request, cancellationToken);

        if (validationErrors.Count > 0)
        {
            var result = new TResponse();
            result.AddErrors(validationErrors);

            return result;
        }

        return await next(cancellationToken);
    }

    private async Task<List<string>> ValidateRequestAsync(TRequest request, CancellationToken cancellationToken)
    {
        var validationContext = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(validator => validator.ValidateAsync(validationContext, cancellationToken)));

        return [.. validationResults
            .SelectMany(validationResult => validationResult.Errors)
            .Select(validationFailure => validationFailure.ErrorMessage)];
    }
}
