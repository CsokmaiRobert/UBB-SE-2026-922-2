using System.Collections.Immutable;
using BoardRentAndProperty.Mappers;
using BoardRentAndProperty.Models;

namespace BoardRentAndProperty.Repositories
{
    public interface IGameRepository : IRepository<Game>
    {
        ImmutableList<Game> GetGamesByOwner(int ownerUserId);
    }
}