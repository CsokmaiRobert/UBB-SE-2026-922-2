using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace BoardRentAndProperty.Data
{
    public interface IUnitOfWork : IDisposable
    {
        SqlConnection Connection { get; }
        Task OpenAsync();
    }
}
