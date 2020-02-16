using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using VSS.Hosted.VLCommon;

namespace VSS.Nighthawk.NHDataSvc.DataAccess
{
  public class DocumentStore<T> : IDocumentStore<T> where T : class
  {
    private readonly string _connectionString = ObjectContextFactory.ConnectionString("NH_TCA");
    private readonly MongoDatabase _documentDatabase;
    private readonly WriteConcern _writeConcern;

    private readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private MongoCollection<T> _documentCollection;

    public DocumentStore()
    {
      _log.IfInfoFormat("Initializing mongo connection for database NH_TCA.");
      _log.IfDebugFormat("Mongo database connection string is {0}", _connectionString);
      
      var urlBuilder = new MongoUrlBuilder(_connectionString);

      _writeConcern = urlBuilder.GetWriteConcern(true);

      var dbClient = new MongoClient(_connectionString);

      var dbServer = dbClient.GetServer();

      _documentDatabase = dbServer.GetDatabase(urlBuilder.DatabaseName, _writeConcern);

      var collectionName = typeof(T).Name;

      if (_documentDatabase != null)
        SetupCollection(collectionName);
    }

    public MongoCollection<T> MongoDocumentCollection
    {
      get { return _documentCollection; }
    }

    public IQueryable<T> DocumentCollection
    {
      get { return _documentCollection.AsQueryable<T>(); }
    }

    public bool IsOnline
    {
      get { return _documentDatabase != null && DocumentCollection != null; }
    }

    public bool Store(IEnumerable<T> messages)
    {
      try
      {
        foreach (var message in messages)
          Store(message);

        return true;
      }
      catch (WriteConcernException ex)
      {
        LogWriteConcernException(ex, "BatchInsert");
      }
      catch (Exception exception)
      {
        _log.IfError("Unable to store the documents in database.", exception);
      }

      return false;
    }

    public bool Store(T message)
    {
      try
      {
        _documentCollection.Save(message, _writeConcern);
        return true;
      }
      catch (WriteConcernException ex)
      {
        LogWriteConcernException(ex, "Insert");
      }
      catch (Exception exception)
      {
        _log.IfError("Unable to store the documents in database", exception);
      }

      return false;
    }

    public WriteConcernResult Upsert(IMongoQuery query, IMongoUpdate update)
    {
      return _documentCollection.Update(query, update, UpdateFlags.Upsert);
    }

    public WriteConcernResult Update(IMongoQuery query, IMongoUpdate update)
    {
      return _documentCollection.Update(query, update, UpdateFlags.None);
    }

    public WriteConcernResult Delete(IMongoQuery query)
    {
      return _documentCollection.Remove(query);
    }

    private void SetupCollection(string collectionName, Func<BsonClassMap<T>> classMap = null)
    {
      _log.IfDebug("Registering class map...");

      if (!BsonClassMap.IsClassMapRegistered(typeof(T)))
      {
        BsonClassMap.RegisterClassMap<T>(x =>
                                           {
                                             x.AutoMap();
                                             x.SetIgnoreExtraElements(true);
                                             if (typeof(T).Name.EndsWith("Event"))
                                             {
                                               x.IdMemberMap.SetIdGenerator(StringObjectIdGenerator.Instance);  //(BsonObjectIdGenerator.Instance);
                                               if (typeof(T).GetProperties().Any(p => p.Name == "_id"))
                                               {
                                                 x.GetMemberMap("_id").SetRepresentation(BsonType.ObjectId);
                                                 x.SetIdMember(x.GetMemberMap("_id"));
                                               }
                                             }
                                             //if (typeof (T).GetProperties().Any(p => p.Name == "CorrelationId"))
                                             //  x.SetIdMember(x.GetMemberMap("CorrelationId"));
                                           });
      }

      _log.IfDebug("Custom class map for polymorphic types...");

      try
      {
        _log.IfDebugFormat("Getting the collection '{0}'", collectionName);

        _documentCollection = _documentDatabase.GetCollection<T>(collectionName, _writeConcern);

        _log.IfDebugFormat("Applying indexes...");

        ApplyIndexes();
      }
      catch (Exception exception)
      {
        _log.IfError("Error occurred while setting up database.", exception);
        _documentCollection = null;
      }
    }

    private void ApplyIndexes()
    {
      var indexes = FindIndexAttributes(typeof(T));

      if (indexes.Count == 0)
        return;

      if (!_documentCollection.IndexExists(indexes.ToArray()))
        _documentCollection.CreateIndex(indexes.ToArray());

      _log.IfInfoFormat("Created index for collection '{0}'", typeof(T).Name);

      foreach (var index in indexes)
        _log.IfInfoFormat("Index created: {0}", index);
    }

    private static IList<string> FindIndexAttributes(Type t)
    {
      var propertiesInfo = t.GetProperties();

      return (from propertyInfo in propertiesInfo
              let attrib = (IndexedAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(IndexedAttribute))
              where attrib != null
              select propertyInfo.Name).ToList();
    }

    private void LogWriteConcernException(MongoCommandException ex, string documentAction)
    {
      _log.ErrorFormat("Database {1} Error -> {0}", ex.Message, documentAction);
      _log.ErrorFormat("Result -> {0}", ex.CommandResult.ToJson());
      _log.ErrorFormat("Data -> {0}", ex.Data.ToJson());
    }
  }
}