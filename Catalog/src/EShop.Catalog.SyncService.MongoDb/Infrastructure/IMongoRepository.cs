using EShop.Catalog.SyncService.MongoDb.Entities;
using System.Linq.Expressions;

namespace EShop.Catalog.SyncService.MongoDb.Infrastructure;

public interface IMongoRepository<TDocument>
    where TDocument : IDocument
{
    IEnumerable<TDocument> FilterBy(Expression<Func<TDocument, bool>> filterExpression);
    IEnumerable<TProjected> FilterBy<TProjected>(Expression<Func<TDocument, bool>> filterExpression, Expression<Func<TDocument, TProjected>> projectedExpression);

    TDocument FindById(string id);
    Task<TDocument> FindByIdAsync(string id, CancellationToken cancellationToken);
    TDocument FindOne(Expression<Func<TDocument, bool>> filterExpression);
    Task<TDocument> FindOneAsync(Expression<Func<TDocument, bool>> filterExpression, CancellationToken cancellationToken);

    void InsertOne(TDocument document);
    Task InsertOneAsync(TDocument document, CancellationToken cancellationToken);
    void InsertMany(ICollection<TDocument> documents);
    Task InsertManyAsync(ICollection<TDocument> documents, CancellationToken cancellationToken);

    void ReplaceOne(TDocument document);
    Task ReplaceOneAsync(TDocument document, CancellationToken cancellationToken);

    void DeleteOne(Expression<Func<TDocument, bool>> filterExpression);
    Task DeleteOneAsync(Expression<Func<TDocument, bool>> filterExpression, CancellationToken cancellationToken);
    void DeleteById(string id);
    Task DeleteByIdAsync(string id, CancellationToken cancellationToken);
    void DeleteMany(Expression<Func<TDocument, bool>> filterExpression);
    Task DeleteManyAsync(Expression<Func<TDocument, bool>> filterExpression, CancellationToken cancellationToken);
}
