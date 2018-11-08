namespace VSS.MasterData.Repositories.Extensions
{
    public static class IntegerExtensions
    {
      public static int CalculateUpsertCount(this int upsertedCount)
      {
        return upsertedCount == 2
          ? 1
          : upsertedCount; // 2=1RowUpdated; 1=1RowInserted; 0=noRowsInserted
      }
  }
}