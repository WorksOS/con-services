using VSS.Common.ResultsHandling;

namespace VSS.MasterData.Models.ResultHandling
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
      DynamicAddwithOffset("Invalid Date Range. StartUTC must be earlier than EndUTC.", 29);
      DynamicAddwithOffset("Invalid Date Range. If using a date range both dates must be provided.", 30);
      DynamicAddwithOffset("Invalid designUid.", 31);
      DynamicAddwithOffset("Invalid layer type. The layer type should be one of the following types: None, TagFileNumber, MapReset.", 32);
      DynamicAddwithOffset("Layer type error. If using a tag file layer filter, layer number must be provided", 33);
      DynamicAddwithOffset("Layer number error. To use the layer number filter, layer type must be specified", 34);
      DynamicAddwithOffset("Invalid spatial filter boundary. Too few points for filter polygon", 35);
    }
  }
}
