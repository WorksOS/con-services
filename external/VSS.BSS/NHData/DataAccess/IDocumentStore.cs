using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;

namespace VSS.Nighthawk.NHDataSvc.DataAccess
{
  public interface IDocumentStore<T> where T : class
  {
    MongoCollection<T> MongoDocumentCollection { get; }
    IQueryable<T> DocumentCollection { get; }
    bool IsOnline { get; }
    bool Store(IEnumerable<T> items);
    bool Store(T item);
    WriteConcernResult Update(IMongoQuery query, IMongoUpdate update);
    WriteConcernResult Upsert(IMongoQuery query, IMongoUpdate update);
    WriteConcernResult Delete(IMongoQuery query);
  }
}
