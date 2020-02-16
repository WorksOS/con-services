using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Moq;

namespace UnitTests
{
  public class MongoMockHelper
  {

    public static Mock<MongoCursor<T>> GetMockMongoCursor<T>()
    {
      var mongoServerSettings = Mock.Of<MongoServerSettings>();
      var mongoServer = new MongoServer(mongoServerSettings);
     
      var mongoDatabaseSettings = Mock.Of<MongoDatabaseSettings>();
      mongoDatabaseSettings.GuidRepresentation = GuidRepresentation.Standard;
      mongoDatabaseSettings.ReadPreference = new ReadPreference();
      mongoDatabaseSettings.WriteEncoding = mongoDatabaseSettings.ReadEncoding = new UTF8Encoding();
      mongoDatabaseSettings.WriteConcern = new WriteConcern();

      // Mock objects
      var mongoDatabase = new Mock<MongoDatabase>(mongoServer, "UnitTestServer", mongoDatabaseSettings);
      string msg = String.Empty;
      mongoDatabase.Setup(db => db.Settings).Returns(mongoDatabaseSettings);
      mongoDatabase.Setup(db => db.IsCollectionNameValid(It.IsAny<string>(), out msg)).Returns(true);

      var mongoCollectionSettings = new MongoCollectionSettings();
      var mongoCollection = new Mock<MongoCollection<T>>(mongoDatabase.Object, "UnitTestDatabase", mongoCollectionSettings);
      mongoCollection.Setup(c => c.Database).Returns(mongoDatabase.Object);
      mongoCollection.Setup(c => c.Settings).Returns(mongoCollectionSettings);

      mongoDatabase.Setup(db => db.GetCollection<T>(typeof(T).Name)).Returns(mongoCollection.Object);

      var mongoQuery = Mock.Of<IMongoQuery>();
      var readPreference = Mock.Of<ReadPreference>(); // Default constructor
      var bsonSerializer = Mock.Of<IBsonSerializer>();
      var bsonSerializationOptions = Mock.Of<IBsonSerializationOptions>();

      
      var cursor = new Mock<MongoCursor<T>>(mongoCollection.Object, mongoQuery, readPreference, bsonSerializer, bsonSerializationOptions);
      cursor.SetupAllProperties();
      IEnumerable<T> enumerable = new List<T>();
      cursor.Setup(c => c.GetEnumerator()).Returns(enumerable.GetEnumerator());
      
      cursor.Setup(x => x.SetSortOrder(It.IsAny<IMongoSortBy>())).Returns(cursor.Object);

      cursor.Setup(x => x.SetLimit(It.IsAny<int>())).Returns(cursor.Object);
      cursor.Setup(x => x.SetFields(It.IsAny<IMongoFields>())).Returns(cursor.Object);
      cursor.Setup(x => x.SetFields(It.IsAny<string[]>())).Returns(cursor.Object);
      cursor.Setup(x => x.SetFields(It.IsAny<string>())).Returns(cursor.Object);
      cursor.Setup(x => x.SetSkip(It.IsAny<int>())).Returns(cursor.Object);


      return cursor;
    }

  }


}
