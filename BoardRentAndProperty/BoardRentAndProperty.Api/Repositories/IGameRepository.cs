using System;
using System.Collections.Immutable;
using BoardRentAndProperty.Api.Models;

namespace BoardRentAndProperty.Api.Repositories
{
    public interface IGameRepository
    {
        ImmutableList<Game> GetAll();
        void Add(Game game);
        Game Delete(int id);
        void Update(int id, Game updated);
        Game Get(int id);
        ImmutableList<Game> GetGamesByOwner(Guid ownerAccountId);
    }
}
