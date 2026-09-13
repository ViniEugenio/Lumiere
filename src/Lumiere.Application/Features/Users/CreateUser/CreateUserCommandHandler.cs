using Lumiere.Application.DTOs.Results;
using Lumiere.Domain.Entities;
using Lumiere.Domain.Interfaces;
using MediatR;

namespace Lumiere.Application.Features.Users.CreateUser
{
    public class CreateUserCommandHandler(IUserRepository userRepository) : IRequestHandler<CreateUserCommand, ResultDto>
    {

        private readonly IUserRepository _userRepository = userRepository;

        public async Task<ResultDto> Handle(CreateUserCommand command, CancellationToken cancellationToken)
        {

            User user = User.Create(command.FirstName, command.LastName, command.Email);
            await _userRepository.AddAsync(user, cancellationToken);

            ResultDto result = new();
            CreateUserResultDto data = new(user.Id, user.FirstName, user.LastName);

            result.SetData(data);

            return result;

        }

    }
}
