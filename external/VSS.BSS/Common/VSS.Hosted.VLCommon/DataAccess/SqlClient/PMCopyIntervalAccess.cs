namespace VSS.Hosted.VLCommon
{
  public class PMCopyIntervalAccess
  {
    public static void CopyIntervals(long sourceAssetID, long targetAssetID, bool copyCheckListAndParts, bool copyServiceLevelIntervals, bool copyIndependentIntervals, bool copyMajorComponentIntervals)
    {
      StoredProcDefinition sp = new StoredProcDefinition("NH_OP", "uspPub_PMInterval_Copy");
      sp.AddInput("@sourceAssetID", sourceAssetID);
      sp.AddInput("@targetAssetID", targetAssetID);
      sp.AddInput("@copyChecklistAndParts", copyCheckListAndParts);
      sp.AddInput("@copyServiceLevelIntervals", copyServiceLevelIntervals);
      sp.AddInput("@copyIndependentIntervals", copyIndependentIntervals);
      sp.AddInput("@copyMajorComponentIntervals", copyMajorComponentIntervals);      
      SqlAccessMethods.ExecuteNonQuery(sp);
    }
  }
}
