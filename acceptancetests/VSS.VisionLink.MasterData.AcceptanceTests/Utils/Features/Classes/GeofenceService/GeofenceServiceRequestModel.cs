using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.MasterData.AcceptanceTests.Utils.Features.Classes
{

  #region Valid GeofenceServiceCreateRequest

  public class CreateGeofenceModel
  {
    public CreateGeofenceEvent CreateGeofenceEvent;
  }


  public class CreateGeofenceEvent
  {
    public string GeofenceName { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Description { get; set; }


    public int GeofenceType { get; set; }


    public string GeometryWKT { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? FillColor { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public bool? IsTransparent { get; set; }


    public Guid CustomerUID { get; set; }


    public Guid UserUID { get; set; }


    public Guid GeofenceUID { get; set; }


    public DateTime ActionUTC { get; set; }

    public DateTime ReceivedUTC { get; set; }
  }
  #endregion

  #region Valid GeofenceServiceUpdateRequest

  public class UpdateGeofenceModel
  {
    public UpdateGeofenceEvent UpdateGeofenceEvent;
  }
  public class UpdateGeofenceEvent
  {
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string GeofenceName { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Description { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? GeofenceType { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string GeometryWKT { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? FillColor { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public bool? IsTransparent { get; set; }

    public Guid UserUID { get; set; }


    public Guid GeofenceUID { get; set; }


    public DateTime ActionUTC { get; set; }

    public DateTime ReceivedUTC { get; set; }
  }
  #endregion

  #region Valid GeofenceServiceDeleteRequest

  public class DeleteGeofenceModel
  {
    public DeleteGeofenceEvent DeleteGeofenceEvent;
  }

  public class DeleteGeofenceEvent 
  {

    public Guid GeofenceUID { get; set; }

    public Guid UserUID { get; set; }


    public DateTime ActionUTC { get; set; }

    public DateTime ReceivedUTC { get; set; }

  }
    #endregion

  #region Invalid GeofenceServiceCreateRequest

  public class InvalidCreateGeofenceEvent
  {
    public string GeofenceName { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Description { get; set; }


    public string GeofenceType { get; set; }


    public string GeometryWKT { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string FillColor { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string IsTransparent { get; set; }


    public string CustomerUID { get; set; }


    public string UserUID { get; set; }


    public string GeofenceUID { get; set; }


    public string ActionUTC { get; set; }

    public string ReceivedUTC { get; set; }
  }
  #endregion

  #region Invalid GeofenceServiceUpdateRequest
  public class InvalidUpdateGeofenceEvent
  {

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string GeofenceName { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Description { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string GeofenceType { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string GeometryWKT { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string FillColor { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string IsTransparent { get; set; }

    public string UserUID { get; set; }


    public string GeofenceUID { get; set; }


    public string ActionUTC { get; set; }

    public string ReceivedUTC { get; set; }
  }
    #endregion

  #region Invalid GeofenceServiceDeleteRequest


  public class InvalidDeleteGeofenceEvent
  {

    public string GeofenceUID { get; set; }

    public string UserUID { get; set; }


    public string ActionUTC { get; set; }

    public string ReceivedUTC { get; set; }

  }
  #endregion
}

