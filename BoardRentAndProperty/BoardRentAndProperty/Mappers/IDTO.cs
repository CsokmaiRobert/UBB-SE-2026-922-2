using System;

namespace BoardRentAndProperty.Mappers
{
    public interface IDTO<TDomainModel, TId>
        where TDomainModel : IEntity<TId>
    {
        TId Id { get; set; }
    }
}