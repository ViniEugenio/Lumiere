using Lumiere.Application.DTOs;
using Lumiere.Application.Services.Interfaces;
using Lumiere.Domain.Interfaces;

namespace Lumiere.Application.Services.Implementations
{
    public class DataBaseService(IDataBaseRepository dataBaseRepository) : IDataBaseService
    {
        public async Task<ResultDto<object>> UpdateBase(CancellationToken cancellationToken = default)
        {

            ResultDto<object> result = new();

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
