using GdNet.Domain.Entity;
using System;

namespace GdNet.Data.EF.Strategies
{
    /// <summary>
    /// Just change the IsAvailable of entity to False
    /// </summary>
    public class ChangeAvailabilityOnDeletionStrategy<T> : ChangeAvailabilityOnDeletionStrategyT<T, Guid>, IDeletionStrategy<T>
        where T : class, IEditableEntity
    {
    }
}