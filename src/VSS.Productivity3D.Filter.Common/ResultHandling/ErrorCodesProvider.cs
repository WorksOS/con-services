using VSS.Common.ResultsHandling;

namespace VSS.Productivity3D.Filter.Common.ResultHandling
{
  public class ErrorCodesProvider : ContractExecutionStatesEnum
  {
    public ErrorCodesProvider()
    {
      DynamicAddwithOffset("Invalid projectUid.", 1);
      DynamicAddwithOffset("Invalid filterUid.", 2);
      DynamicAddwithOffset("Invalid name. Should not be null.", 3);
      DynamicAddwithOffset("Invalid filterJson. Should not be null.", 4);
      DynamicAddwithOffset("GetFilter By filterUid. Invalid parameters.", 5);
      DynamicAddwithOffset("GetFilter By filterUid. Unable to retrieve filters. Exception: {0}.", 6);
      DynamicAddwithOffset("Validation of Customer/Project failed. Exception: {0}.", 7);
      DynamicAddwithOffset("Validation of Customer/Project failed. Not allowed.", 8);
      DynamicAddwithOffset("GetFilters By projectUid. Invalid parameters.", 9);
      DynamicAddwithOffset("GetFilters By projectUid. Unable to retrieve filters. Exception: {0}.", 10);
      DynamicAddwithOffset("DeleteFilter failed. Unable to find filterUid.", 11);
      DynamicAddwithOffset("DeleteFilter failed. Unable to delete filterUid.", 12);
      DynamicAddwithOffset("DeleteFilter failed. Unable to delete filterUid. Exception: {0}.", 13);
      DynamicAddwithOffset("DeleteFilter failed. Unable to write to Kafka. Exception: {0}.", 14);
      DynamicAddwithOffset("UpsertFilter failed. Unable to read filters for project. Exception: {0}.", 15);
      DynamicAddwithOffset("UpsertFilter failed. Unable to find transient filterUid provided.", 16);
      DynamicAddwithOffset("UpsertFilter failed. Unable to update transient filter.", 17);
      DynamicAddwithOffset("UpsertFilter failed. Unable to update transient filter. Exception: {0}.", 18);
      DynamicAddwithOffset("UpsertFilter failed. Unable to create transient filter.", 19);
      DynamicAddwithOffset("UpsertFilter failed. Unable to create transient filter. Exception: {0}.", 20);
      DynamicAddwithOffset("UpsertFilter failed. Unable to find persistant filterUid provided.", 21);
      DynamicAddwithOffset("UpsertFilter failed. Unable to delete persistant filter.", 22);
      DynamicAddwithOffset("UpsertFilter failed. Unable to delete persistant filter. Exception: {0}.", 23);
      DynamicAddwithOffset("UpsertFilter failed. Unable to create persistant filter.", 24);
      DynamicAddwithOffset("UpsertFilter failed. Unable to create persistant filter. Exception: {0}.", 25);
      DynamicAddwithOffset("UpsertFilter failed. Unable to write to Kafka. Exception: {0}.", 26);
      DynamicAddwithOffset("Invalid customerUid.", 27);
      DynamicAddwithOffset("Invalid userUid.", 28);
      DynamicAddwithOffset("Unable to notify 3dp of filterUid change. Return code {0} message {1}", 29);
      DynamicAddwithOffset("Unable to notify 3dp of filterUid change. Endpoint: {0} message {1}.", 30);
    }

    protected override int executionResultOffset { get; } = 3000;
  }
}
