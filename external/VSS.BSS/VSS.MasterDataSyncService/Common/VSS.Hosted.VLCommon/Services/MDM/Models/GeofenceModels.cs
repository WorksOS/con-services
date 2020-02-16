using System;

namespace VSS.Hosted.VLCommon.Services.MDM.Models
{
  public class CreateGeofenceEvent 
  {

    public string GeofenceName { get; set; }

    public string Description { get; set; }

    public int? GeofenceType { get; set; }

    public string GeometryWKT { get; set; }

    public int? FillColor { get; set; }

    public bool? IsTransparent { get; set; }

    public Guid CustomerUID { get; set; }

    public Guid UserUID { get; set; }

    public Guid GeofenceUID { get; set; }

    public DateTime ActionUTC { get; set; }

    public DateTime ReceivedUTC { get; set; }

  }

  public class UpdateGeofenceEvent
  {

    public string GeofenceName { get; set; }

    public string Description { get; set; }

    public int? GeofenceType { get; set; }

    public string GeometryWKT { get; set; }

    public int? FillColor { get; set; }

    public bool? IsTransparent { get; set; }

    public Guid UserUID { get; set; }

    public Guid GeofenceUID { get; set; }

    public DateTime ActionUTC { get; set; }

    public DateTime ReceivedUTC { get; set; }

  }

  public class DeleteGeofenceEvent
  {
    public Guid GeofenceUID { get; set; }

    public Guid UserUID { get; set; }

    public DateTime ActionUTC { get; set; }

    public DateTime ReceivedUTC { get; set; }

  }

  public class FavoriteGeofenceEvent 
  {
      public Guid GeofenceUID { get; set; }
      public Guid UserUID { get; set; }
      public Guid CustomerUID { get; set; }
      public DateTime ActionUTC { get; set; }
      public DateTime ReceivedUTC { get; set; }
  }

  public class UnfavoriteGeofenceEvent 
  {
      public Guid GeofenceUID { get; set; }
      public Guid UserUID { get; set; }
      public Guid CustomerUID { get; set; }
      public DateTime ActionUTC { get; set; }
      public DateTime ReceivedUTC { get; set; }
  }


}
