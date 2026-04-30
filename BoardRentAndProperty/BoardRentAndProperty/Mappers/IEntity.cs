namespace BoardRentAndProperty.Mappers
{
    public interface IEntity<TId>
    {
        TId Id { get; set; }
    }
}