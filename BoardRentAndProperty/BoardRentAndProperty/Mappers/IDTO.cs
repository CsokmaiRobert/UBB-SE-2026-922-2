namespace BoardRentAndProperty.Mappers
{
    public interface IDTO<TDomainModel>
        where TDomainModel : IEntity
    {
        int Id { get; set; }
    }
}