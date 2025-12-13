using JsonApiDotNetCore.MongoDb.Resources;

namespace EShop.Catalog.SyncService.MongoDb.Models;

public interface IDocument : IMongoIdentifiable
{
    Guid DocumentId { get; set; }
    ulong Version { get; }
}

public abstract class Document : HexStringMongoIdentifiable, IDocument
{
    /// <summary>
    /// Gets or sets the unique identifier for the document.
    /// This is an Aggregate's ID in write model.
    /// </summary>
    public Guid DocumentId { get; set; }

    public ulong Version { get; set; }
}