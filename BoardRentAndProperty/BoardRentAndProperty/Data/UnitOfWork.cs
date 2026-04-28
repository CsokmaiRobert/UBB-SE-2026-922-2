namespace BoardRentAndProperty.Data
{
    using System.Threading.Tasks;
    using Microsoft.Data.SqlClient;

    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext dbContext;
        private SqlConnection connection;

        public UnitOfWork(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public SqlConnection Connection
        {
            get
            {
                if (this.connection == null)
                {
                    this.connection = this.dbContext.CreateConnection();
                }

                return this.connection;
            }
        }

        public async Task OpenAsync()
        {
            if (this.Connection.State != System.Data.ConnectionState.Open)
            {
                await this.Connection.OpenAsync();
            }
        }

        public void Dispose()
        {
            if (this.connection != null)
            {
                this.connection.Dispose();
                this.connection = null;
            }
        }
    }
}
