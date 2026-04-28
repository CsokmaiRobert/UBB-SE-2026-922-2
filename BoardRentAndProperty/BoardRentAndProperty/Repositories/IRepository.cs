using System.Collections.Generic;
using System.Collections.Immutable;
using BoardRentAndProperty.Mappers;

namespace BoardRentAndProperty.Repositories
{
    public interface IRepository<TRepositoryItem>
        where TRepositoryItem : notnull, IEntity
    {
        ImmutableList<TRepositoryItem> GetAll();

        void Add(TRepositoryItem itemToAdd);

        TRepositoryItem Delete(int itemIdentifierToRemove);

        void Update(int itemIdentifierToUpdate, TRepositoryItem replacementItem);

        TRepositoryItem Get(int itemIdentifier);
    }
}