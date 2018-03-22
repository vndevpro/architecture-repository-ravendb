using GdNet.Data.EF.Strategies;
using GdNet.Domain.Entity;
using GdNet.Domain.Repository;
using Raven.Client.Documents.Session;
using System;

namespace GdNet.Data.EF
{
    public abstract class EfRepositoryBase<T> : EfRepositoryBaseT<T, Guid>, IRepositoryBase<T>
        where T : class, IAggregateRoot
    {
        protected EfRepositoryBase(IDocumentSession documentSession)
            : base(documentSession)
        {
        }

        protected EfRepositoryBase(IDocumentSession documentSession,
            ISavingStrategy savingStrategy,
            IDeletionStrategy<T> deletionStrategy,
            IFilterStrategy<T, Guid> filterStrategy)
            : base(documentSession, savingStrategy, deletionStrategy, filterStrategy)
        {
        }
    }
}
