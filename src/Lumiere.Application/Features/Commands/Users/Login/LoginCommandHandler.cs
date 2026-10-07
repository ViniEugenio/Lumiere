using Lumiere.Application.DTOs.Results;
using Lumiere.Application.DTOs.User;
using Lumiere.Application.Interfaces.Repositories;
using Lumiere.Application.Interfaces.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lumiere.Application.Features.Commands.Users.Login
{
    public class LoginCommandHandler(
        IUserRepository userRepository,
        IJWTService jwtService) : IRequestHandler<LoginCommand, ResultDto>
    {

        public async Task<ResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {

            JWTUserDataDto jwtUserData = await GetJWTUserData(request, cancellationToken);
            string jwt = jwtService.GenerateJWT(jwtUserData);

            ResultDto result = new();

            LoginResultDto loginResult = new(jwt, DateTime.UtcNow);
            result.SetData(loginResult);

            return result;

        }

        private async Task<JWTUserDataDto> GetJWTUserData(LoginCommand request, CancellationToken cancellationToken)
        {

            return await userRepository
                .Get(user => user.Email == request.Email)
                .Select(user => new JWTUserDataDto(
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user
                        .Roles
                        .Select(role => role.Id)
                        .ToList()
                ))
                .SingleAsync(cancellationToken);

        }

    }
}
