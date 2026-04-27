using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace BoardRent.Data
{
    public interface IUnitOfWork : IDisposable
    {
        SqlConnection Connection { get; }
        Task OpenAsync();
    }
}
