using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.MasterData.AcceptanceTests.Utils.Features.Classes.SubscriptionService
{
  public class CreateSubscriptionModel
  {
    public CreateSubscriptionEvent CreateSubscriptionEvent;
  }

  public class CreateSubscriptionEvent
  {
    public Guid SubscriptionUID { get; set; }
    public Guid CustomerUID { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Guid? AssetUID { get; set; }
    public SubscriptionType SubscriptionTypeID { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }

  public class UpdateSubscriptionModel
  {
    public UpdateSubscriptionEvent UpdateSubscriptionEvent;
  }

  public class UpdateSubscriptionEvent
  {
    public Guid SubscriptionUID { get; set; }
    public Guid CustomerUID { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Guid? AssetUID { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public SubscriptionType? SubscriptionTypeID { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? StartDate { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public DateTime EndDate { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }

  public class InvalidCreateSubscriptionEvent
  {
    public string SubscriptionUID { get; set; }
    public string CustomerUID { get; set; }
    public string AssetUID { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string SubscriptionTypeID { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public string ActionUTC { get; set; }
    public string ReceivedUTC { get; set; }
  }

  public class InvalidUpdateSubscriptionEvent
  {
    public string SubscriptionUID { get; set; }
    public string CustomerUID { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string AssetUID { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string SubscriptionTypeID { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string StartDate { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string EndDate { get; set; }
    public string ActionUTC { get; set; }
    public string ReceivedUTC { get; set; }
  }
  public enum SubscriptionType
  {
    Unknown = 0,
    Essentials = 1,
    ManualMaintenanceLog = 2,
    CATHealth = 3,
    StandardHealth = 4,
    CATUtilization = 5,
    StandardUtilization = 6,
    CATMAINT = 7,
    VLMAINT = 8,
    RealTimeDigitalSwitchAlerts = 9,
    e1minuteUpdateRateUpgrade = 10,
    ConnectedSiteGateway = 14,
    e2DProjectMonitoring = 15,
    e3DProjectMonitoring = 16,
    VisionLinkRFID = 17,
    Manual3DProjectMonitoring = 18,
    VehicleConnect = 19,
    UnifiedFleet = 21,
    AdvancedProductivity = 22,
    Landfill = 23,
    ProjectMonitoring = 24
  }


}
