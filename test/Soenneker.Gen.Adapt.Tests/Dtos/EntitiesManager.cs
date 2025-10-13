using System;

namespace Soenneker.Gen.Adapt.Tests.Dtos;

public class EntitiesManager
{
    
    public virtual CustomerEntity Create(CustomerEntity entity)
    {
        entity.CreatedAt = DateTime.UtcNow;

        var document = entity.Adapt<CustomerDocument>();

        document.DocumentId = Guid.NewGuid().ToString();
        document.PartitionKey = document.DocumentId;

        return entity;
    }
}
