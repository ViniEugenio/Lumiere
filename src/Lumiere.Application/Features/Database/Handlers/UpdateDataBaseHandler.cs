using Lumiere.Application.DTOs;
using Lumiere.Application.Features.Database.Commands;
using Lumiere.Application.Services.Interfaces;
using MediatR;

namespace Lumiere.Application.Features.Database.Handlers
{
    public class UpdateDataBaseHandler(IDataBaseService dataBaseService) : IRequestHandler<UpdateDataBaseCommand, ResultDto<object>>
    {
        public async Task<ResultDto<object>> Handle(UpdateDataBaseCommand request, CancellationToken cancellationToken)
        {
            return await dataBaseService.UpdateBase(cancellationToken);
        }
    }
}
