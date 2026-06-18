using Lumiere.Application.DTOs;

namespace Lumiere.Application.Services.Interfaces
{
    public interface IDataBaseService
    {
        Task<ResultDto<object>> UpdateBase(CancellationToken cancellationToken = default);
    }
}
