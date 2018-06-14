namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  public class PatchResult
  {
    public byte[] PatchData { get; private set; }

    private PatchResult()
    { }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static PatchResult CreatePatchResult(byte[] data)
    {
      return new PatchResult
      {
        PatchData = data
      };
    }
  }
}
