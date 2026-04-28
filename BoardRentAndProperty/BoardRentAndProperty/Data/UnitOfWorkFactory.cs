namespace BoardRentAndProperty.Data
{
    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly AppDbContext dbContext;

        public UnitOfWorkFactory(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IUnitOfWork Create()
        {
            return new UnitOfWork(this.dbContext);
        }
    }
}
