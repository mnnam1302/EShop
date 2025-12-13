using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EShop.Catalog.SyncService.MongoDb.Abstractions;

public interface IDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    ObjectId Id { get; set; }

    ulong Version { get; }
}

public abstract class Document : IDocument
{
    public ObjectId Id { get; set; }
    public Guid DocumentId { get; set; }
    public ulong Version { get; set; }
}
