namespace TestUtility.Model.TestEvents
{
  /// <summary>
  /// This class to to cater for the incoming events
  /// </summary>
    public class EventTable
  {
    public string EventDate { get; set; } //this is a "special VSSDateString eg 1d+12:00
    public string EventType { get; set; }
    public string DayOffset { get; set; }
    public string Timestamp { get; set; }
    public string UtcOffsetHours { get; set; }
    public string ActionUTC { get; set; }
    public string ReceivedUTC { get; set; }
    public string ProjectUID { get; set; }
    public string CustomerUID { get; set; }
    public string UserUID { get; set; }
    public string SubscriptionUID { get; set; }
    public string GeofenceUID { get; set; }

    // Project
    public string ProjectEndDate { get; set; }
    public string ProjectStartDate { get; set; }
    public string ProjectName { get; set; }
    public string ProjectTimezone { get; set; }
    public string ProjectType { get; set; }
    public string ProjectID { get; set; }

    //Customer
    public string CustomerName { get; set; }
    public string CustomerType { get; set; }

    //Subscription
    public string SubscriptionType { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public string EffectiveDate { get; set; }

    // Geofence
    public string GeofenceName { get; set; }
    public string Description { get; set; }
    public string FillColor { get; set; }
    public string IsTransparent { get; set; }
    public string GeofenceType { get; set; }
    public string GeometryWKT { get; set; }

    //Asset details
    public string AssetName { get; set; }
    public string AssetType { get; set; }
    public string SerialNumber { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public string IconId { get; set; }

        public EventTable()
    {
      UtcOffsetHours = "0";
    }
  }
}
