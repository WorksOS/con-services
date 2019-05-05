namespace VSS.TRex.Types
{
  /// <summary>
  /// LiftDetectionType defines the method by which the server builds cell pass profiles
  /// </summary>
  public enum LiftDetectionType : byte
  {
       Automatic,
       MapReset,
       AutoMapReset,
       Tagfile,
       None
    }
}
