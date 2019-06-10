namespace VSS.TRex.Gateway.Common.Proxy
{
  public enum GatewayType
  {
    /// <summary>
    /// Default value, will not include a gateway portion in URL
    /// </summary>
    None,
    Immutable,
    Mutable,
    ConnectedSite
  }
}
