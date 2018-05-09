namespace VSS.MasterData.Models.ResultHandling.Abstractions
{
  public class FilterValidationErrorCodesProvider : ContractExecutionStatesEnum
  {
    public FilterValidationErrorCodesProvider()
    {
      DynamicAddwithOffset("Invalid Date Range. StartUTC must be earlier than EndUTC.", 29);
      DynamicAddwithOffset("Invalid Date Range. If using a date range both dates must be provided.", 30);
      DynamicAddwithOffset("Invalid designUid.", 31);
      DynamicAddwithOffset("Invalid spatial filter boundary. Too few points for filter polygon", 35);
      DynamicAddwithOffset("Invalid alignment filter. alignment File Uid is invalid.", 64);
      DynamicAddwithOffset("Invalid alignment filter. Start or end station are invalid.", 65);
      DynamicAddwithOffset("Invalid alignment filter. Left or right offset are invalid.", 66);
      DynamicAddwithOffset("Invalid alignment filter. Parameters are incomplete.", 67);
      DynamicAddwithOffset("Invalid Date Filter. Either EndUTC or DateRangeType must be provided for an as-at date filter.", 68);
    }
  }
}
