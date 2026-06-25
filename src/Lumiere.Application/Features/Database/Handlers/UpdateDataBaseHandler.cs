using Lumiere.Application.DTOs;
using Lumiere.Application.Features.Database.Commands;
using Lumiere.Domain.Interfaces;
using MediatR;

namespace Lumiere.Application.Features.Database.Handlers
{
    public class UpdateDataBaseHandler(IDataBaseRepository dataBaseRepository) : IRequestHandler<UpdateDataBaseCommand, ResultDto<object>>
    {
        public async Task<ResultDto<object>> Handle(UpdateDataBaseCommand request, CancellationToken cancellationToken)
        {
            var result = new ResultDto<object>();

            try
            {
                await dataBaseRepository.ApplyMigrations(cancellationToken);
                return result;
            }
            catch (Exception ex)
            {
                result.AddError(ex);
            }

            return result;
        }
    }
}
