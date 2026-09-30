using Lumiere.Application.DTOs.Results;
using Lumiere.Application.Interfaces.Repositories;
using Lumiere.Application.Interfaces.Services;
using Lumiere.Domain.Entities;
using MediatR;

namespace Lumiere.Application.Features.Commands.Users.CreateUser
{
    public class CreateUserCommandHandler(
        IUserRepository userRepository, 
        IPasswordHasher passwordHasher) : IRequestHandler<CreateUserCommand, ResultDto>
    {

        private readonly IUserRepository _userRepository = userRepository;

        public async Task<ResultDto> Handle(CreateUserCommand command, CancellationToken cancellationToken)
        {

            string passwordHash = passwordHasher.Hash(command.Password);

            User user = User.Create(command.FirstName, command.LastName, command.Email, passwordHash);
            await _userRepository.AddAsync(user, cancellationToken);

            ResultDto result = new();
            CreateUserResultDto data = new(user.Id, user.FirstName, user.LastName);

            result.SetData(data);

            return result;

        }

    }
}
