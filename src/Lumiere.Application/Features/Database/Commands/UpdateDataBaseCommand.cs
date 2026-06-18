using Lumiere.Application.DTOs;
using MediatR;

namespace Lumiere.Application.Features.Database.Commands
{
    public class UpdateDataBaseCommand : IRequest<ResultDto<object>>
    {
    }
}