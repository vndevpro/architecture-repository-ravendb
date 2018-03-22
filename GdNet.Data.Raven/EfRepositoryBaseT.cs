using GdNet.Common;
using GdNet.Data.EF.Strategies;
using GdNet.Domain.Entity;
using GdNet.Domain.Exceptions;
using GdNet.Domain.Repository;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GdNet.Data.EF
{
    public abstract class EfRepositoryBaseT<T, TId> : IRepositoryBaseT<T, TId>
        where T : class, IAggregateRootT<TId>
        where TId : new()
    {
        protected readonly IDocumentSession DocumentSession;
        protected readonly ISavingStrategy SavingStrategy;
        protected readonly IFilterStrategy<T, TId> FilterStrategy;

        private readonly IDeletionStrategyT<T, TId> _deletionStrategy;

        /// <summary>
        /// By default, this constructor uses EmptySavingStrategy and ChangeAvailabilityOnDeletionStrategy
        /// </summary>
        protected EfRepositoryBaseT(IDocumentSession entities)
            : this(entities,
                new EmptySavingStrategy(),
                new ChangeAvailabilityOnDeletionStrategyT<T, TId>(),
                new EntityIsAvailableFilterStrategy<T, TId>())
        {
        }

        /// <summary>
        /// Set deletion & saving strategies explicitly
        /// </summary>
        protected EfRepositoryBaseT(IDocumentSession documentSession,
            ISavingStrategy savingStrategy,
            IDeletionStrategyT<T, TId> deletionStrategy,
            IFilterStrategy<T, TId> filterStrategy)
        {
            DocumentSession = documentSession;
            SavingStrategy = savingStrategy;
            _deletionStrategy = deletionStrategy;
            FilterStrategy = filterStrategy;
        }

        /// <summary>
        /// Count all entities in the system
        /// </summary>
        /// <returns></returns>
        public long Count()
        {
            return DocumentSession.Query<T>().LongCount(FilterStrategy.Predicate);
        }

        /// <summary>
        /// Count all entities in the system which match the filter
        /// </summary>
        public long Count(Func<T, bool> filter)
        {
            return DocumentSession.Query<T>().LongCount(filter);
        }

        public void Delete(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                Delete(entity);
            }
        }

        public void Delete(T entity)
        {
            _deletionStrategy.Execute(entity);
        }

        public void Delete(TId id)
        {
            var entity = GetById(id);
            Delete(entity);
        }

        public T GetById(TId id)
        {
            var entity = DocumentSession.Load<T>(id as string);

            if (entity == null)
            {
                throw new EntityNotFoundException<TId>(id);
            }

            return entity;
        }

        /// <summary>
        /// Get page of entities with default filter from filterStrategy.Predicate
        /// </summary>
        public Result<T> Get(Page page)
        {
            return OnGet(DocumentSession.Query<T>().OrderByDescending(x => x.LastModifiedAt), page);
        }

        /// <summary>
        /// Get page of entities with custom filter (not include the predicate from filterStrategy)
        /// </summary>
        public Result<T> Get(Page page, Func<T, bool> filter)
        {
            return OnGet(DocumentSession.Query<T>().OrderByDescending(x => x.LastModifiedAt), page, filter);
        }

        /// <summary>
        /// Get an entity by given filter
        /// </summary>
        public T GetByFilter(Func<T, bool> filter)
        {
            return DocumentSession.Query<T>().FirstOrDefault(filter);
        }

        /// <summary>
        /// Save a collection of entities
        /// </summary>
        public IEnumerable<T> Save(IEnumerable<T> entities)
        {
            return entities.Select(Save).ToList();
        }

        /// <summary>
        /// Save one entity
        /// </summary>
        public T Save(T entity)
        {
            SavingStrategy.OnSaving();

            DocumentSession.Store(entity, entity.Id.ToString());

            SavingStrategy.OnSaved();

            return entity;
        }

        /// <summary>
        /// Get entities with applying default filter from filterStrategy.Predicate
        /// </summary>
        protected Result<T> OnGet(IEnumerable<T> entities, Page page)
        {
            return OnGet(entities, page, FilterStrategy.Predicate);
        }

        /// <summary>
        /// Get entities with applying custom filter (not include the predicate from filterStrategy)
        /// </summary>
        protected Result<T> OnGet(IEnumerable<T> entities, Page page, Func<T, bool> filter)
        {
            var offset = page.PageIndex * page.ItemsPerPage;
            var pagedEntities = entities.Where(filter).Skip(offset).Take(page.ItemsPerPage);

            return new Result<T>(pagedEntities)
            {
                Total = Count(filter)
            };
        }
    }
}