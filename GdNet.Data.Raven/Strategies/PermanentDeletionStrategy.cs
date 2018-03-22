using GdNet.Domain.Entity;
using Raven.Client.Documents.Session;

namespace GdNet.Data.EF.Strategies
{
    /// <summary>
    /// Remove the entity from repository
    /// </summary>
    public class PermanentDeletionStrategy<T> : IDeletionStrategy<T> where T : class, IEditableEntity
    {
        private readonly IDocumentSession _entities;

        public PermanentDeletionStrategy(IDocumentSession entities)
        {
            _entities = entities;
        }

        public void Execute(T entity)
        {
            _entities.Delete(entity);
        }
    }
}