using System;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Gen.Adapt.Tests.Dtos;

public class EntitiesManager<TEntity, TDocument> where TEntity : CustomerEntity, new() where TDocument : CustomerDocument
{
    
    public virtual TEntity Create(TEntity entity)
    {
        entity.CreatedAt = DateTime.UtcNow;

        var document = entity.Adapt<TDocument>();

        document.DocumentId = Guid.NewGuid().ToString();
        document.PartitionKey = document.DocumentId;

        return entity;
    }
}
