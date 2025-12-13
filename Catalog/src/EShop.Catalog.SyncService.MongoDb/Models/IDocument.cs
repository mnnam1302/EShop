using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;

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
    [Attr]
    public Guid DocumentId { get; set; }

    public ulong Version { get; set; }
}