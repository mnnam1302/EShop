using EShop.Catalog.SyncService.MongoDb.Infrastructure.Attributes;
using EShop.Catalog.SyncService.MongoDb.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace EShop.Catalog.SyncService.MongoDb.Infrastructure.Repository;

public sealed class MongoRepository<TDocument> : IMongoRepository<TDocument>
    where TDocument : IDocument
{
    private readonly IMongoCollection<TDocument> _collection;

    static MongoRepository()
    {
        // Configure GUID serialization globally for MongoDB
        BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
    }

    public MongoRepository(IMongoClient mongoClient, IMongoDbSettings mongoDbSettings)
    {
        var database = mongoClient.GetDatabase(mongoDbSettings.DatabaseName);
        _collection = database.GetCollection<TDocument>(GetCollectionName(typeof(TDocument)));
    }

    private static string GetCollectionName(Type documentType)
    {
        return Attribute.GetCustomAttribute(documentType, typeof(MongoCollectionAttribute)) is MongoCollectionAttribute attribute
            ? attribute.CollectionName
            : documentType.Name;
    }

    public IEnumerable<TDocument> FilterBy(Expression<Func<TDocument, bool>> filterExpression)
    {
        return _collection
            .Find(filterExpression)
            .ToEnumerable();
    }

    public IEnumerable<TProjected> FilterBy<TProjected>(
        Expression<Func<TDocument, bool>> filterExpression,
        Expression<Func<TDocument, TProjected>> projectedExpression)
    {
        return _collection
            .Find(filterExpression)
            .Project(projectedExpression)
            .ToEnumerable();
    }

    public TDocument FindById(string id)
    {
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, id);

        return _collection
            .Find(filter)
            .SingleOrDefault();
    }

    public async Task<TDocument> FindByIdAsync(string id, CancellationToken cancellationToken)
    {
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, id);

        return await _collection
            .Find(filter)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public TDocument FindOne(Expression<Func<TDocument, bool>> filterExpression)
    {
        return _collection
            .Find(filterExpression)
            .FirstOrDefault();
    }

    public async Task<TDocument> FindOneAsync(Expression<Func<TDocument, bool>> filterExpression, CancellationToken cancellationToken)
    {
        return await _collection
            .Find(filterExpression)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public void InsertOne(TDocument document)
    {
        _collection.InsertOne(document);
    }

    public async Task InsertOneAsync(TDocument document, CancellationToken cancellationToken)
    {
        await _collection.InsertOneAsync(document, options: null, cancellationToken);
    }

    public void InsertMany(ICollection<TDocument> documents)
    {
        _collection.InsertMany(documents);
    }

    public async Task InsertManyAsync(ICollection<TDocument> documents, CancellationToken cancellationToken)
    {
        await _collection.InsertManyAsync(documents, cancellationToken: cancellationToken);
    }

    public void ReplaceOne(TDocument document)
    {
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
        _collection.FindOneAndReplace(filter, document);
    }

    public async Task ReplaceOneAsync(TDocument document, CancellationToken cancellationToken)
    {
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
        await _collection.FindOneAndReplaceAsync(filter, document, cancellationToken: cancellationToken);
    }

    public void DeleteById(string id)
    {
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, id);

        _collection.FindOneAndDelete(filter);
    }

    public async Task DeleteByIdAsync(string id, CancellationToken cancellationToken)
    {
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, id);

        await _collection.FindOneAndDeleteAsync(filter, cancellationToken: cancellationToken);
    }

    public void DeleteMany(Expression<Func<TDocument, bool>> filterExpression)
    {
        _collection.DeleteMany(filterExpression);
    }

    public async Task DeleteManyAsync(Expression<Func<TDocument, bool>> filterExpression, CancellationToken cancellationToken)
    {
        await _collection.DeleteManyAsync(filterExpression, cancellationToken);
    }

    public void DeleteOne(Expression<Func<TDocument, bool>> filterExpression)
    {
        _collection.FindOneAndDelete(filterExpression);
    }

    public async Task DeleteOneAsync(Expression<Func<TDocument, bool>> filterExpression, CancellationToken cancellationToken)
    {
        await _collection.FindOneAndDeleteAsync(filterExpression, cancellationToken: cancellationToken);
    }
}