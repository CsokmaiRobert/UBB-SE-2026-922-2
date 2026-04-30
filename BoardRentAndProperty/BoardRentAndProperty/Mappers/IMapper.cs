namespace BoardRentAndProperty.Mappers
{
    public interface IMapper<TDomainModel, TDTO, TId>
        where TDomainModel : IEntity<TId>
        where TDTO : IDTO<TDomainModel, TId>
    {
        TDTO ToDTO(TDomainModel sourceModel);

        TDomainModel ToModel(TDTO sourceDataTransferObject);
    }
}