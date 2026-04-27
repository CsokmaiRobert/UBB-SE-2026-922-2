namespace BoardRent.Data
{
    public interface IUnitOfWorkFactory
    {
        IUnitOfWork Create();
    }
}
