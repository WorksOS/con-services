using System;
using MongoDB.Bson;

namespace VSS.Nighthawk.NHDataSvc.Helpers
{
  public class VehicleMapping
  {
    public BsonObjectId _id { get; set; }
    public long OwnerId { get; set; }
    public long CustomerId { get; set; }
    public long AssetId { get; set; }
    public string TcaId { get; set; }
    public string LifeState { get; set; }
    public bool IsActive { get; set; }
    public DateTime InsertUtc { get; set; }
    public DateTime UpdateUtc { get; set; }
  }
}