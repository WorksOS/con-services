namespace VSS.Productivity3D.TagFileAuth.Models
{
  /// <summary>
  /// Endpoint called via 3dp to identify customer/project for a device and location
  /// </summary>
  public class GetProjectUidsEarthWorksRequest : GetProjectUidsBaseRequest
  {
    public GetProjectUidsEarthWorksRequest() { }


    public GetProjectUidsEarthWorksRequest
      (string platformSerial,
        double latitude, double longitude) 
      : base(platformSerial, latitude, longitude)
    { }

    public new void Validate()
    {
      base.Validate();
    }
  }
}
