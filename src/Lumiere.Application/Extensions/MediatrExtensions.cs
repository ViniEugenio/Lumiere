using FluentValidation;
using FluentValidation.Results;
using Lumiere.Application.DTOs.Results;
using Lumiere.Application.Features.Users.CreateUser;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Lumiere.Application.Extensions;

public static class MediatrExtensions
{
    public static void AddMediatr(this IServiceCollection services)
    {
        services.AddMediatR(config =>
        {

            config.RegisterServicesFromAssembly(typeof(ApplicationExtensions).Assembly);
            config.AddOpenBehavior(typeof(ValidatitorBehavior<,>));

        });
    }

    private class ValidatitorBehavior<TRequest, TResponse>(IValidator<TRequest> validator) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : ResultDto, new()
    {

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {

            ValidationContext<TRequest> validationContext = new ValidationContext<TRequest>(request);
            ValidationResult validationResult = await validator.ValidateAsync(validationContext, cancellationToken);

            if(!validationResult.IsValid)
            {
                return BuildInvalidResponse(validationResult);
            }

            return await next(cancellationToken);

        }

        private static TResponse BuildInvalidResponse(ValidationResult validationResult)
        {

            List<string> errors = [..

                validationResult
                    .Errors
                    .Select(error =>  error.ErrorMessage)

            ];

            TResponse response = new();
            response.AddErrors(errors);

            return response;

        }

    }
}
