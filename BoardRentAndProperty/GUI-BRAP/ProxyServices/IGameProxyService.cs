using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace GUI_BRAP.ProxyServices
{
    public interface IGameProxyService
    {
        Task<IReadOnlyList<GameDTO>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
