using System.Collections.Immutable;
using BoardRentAndProperty.Models;

namespace BoardRentAndProperty.Services
{
    public interface IGameRepository : IRepository<Game>
    {
        ImmutableList<Game> GetGamesByOwner(int ownerUserId);
    }
}