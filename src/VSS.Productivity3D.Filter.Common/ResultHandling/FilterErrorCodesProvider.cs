using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Filter.Common.ResultHandling
{
  public class FilterErrorCodesProvider : FilterValidationErrorCodesProvider
  {
    public FilterErrorCodesProvider()
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
      DynamicAddwithOffset("DeleteFilter failed. The requested filter does not exist, or does not belong to the requesting customer; project or user.", 11);
      DynamicAddwithOffset("DeleteFilter failed. Unable to delete filterUid.", 12);
      DynamicAddwithOffset("DeleteFilter failed. Unable to delete filterUid. Exception: {0}.", 13);
      DynamicAddwithOffset("DeleteFilter failed. Unable to write to Kafka. Exception: {0}.", 14);
      DynamicAddwithOffset("UpsertFilter failed. Unable to read filters for project. Exception: {0}.", 15);
      DynamicAddwithOffset("UpsertFilter failed. Transient filter not updateable, should not have filterUid provided.", 16);
      DynamicAddwithOffset("UpsertFilter failed. Unable to update persistent filter.", 17);
      DynamicAddwithOffset("UpsertFilter failed. Unable to update persistent filter. Exception: {0}.", 18);
      DynamicAddwithOffset("UpsertFilter failed. Unable to create transient filter.", 19);
      DynamicAddwithOffset("UpsertFilter failed. Unable to create transient filter. Exception: {0}.", 20);
      DynamicAddwithOffset("UpsertFilter failed. Unable to find persistent filterUid provided.", 21);
      DynamicAddwithOffset("UpsertFilter failed. Unable to delete persistent filter.", 22);
      //DynamicAddwithOffset("UpsertFilter failed. Unable to delete persistent filter. Exception: {0}.", 23);
      DynamicAddwithOffset("UpsertFilter failed. Unable to create persistent filter.", 24);
      DynamicAddwithOffset("UpsertFilter failed. Unable to create persistent filter. Exception: {0}.", 25);
      DynamicAddwithOffset("UpsertFilter failed. Unable to write to Kafka. Exception: {0}.", 26);
      DynamicAddwithOffset("Invalid customerUid.", 27);
      DynamicAddwithOffset("Invalid userUid.", 28);
      //29: FilterValidationErrorCodesProvider
      //30: FilterValidationErrorCodesProvider
      //31: FilterValidationErrorCodesProvider
      //DynamicAddwithOffset("Invalid layer type. The layer type should be one of the following types: None, TagFileNumber, MapReset.", 32);
      //DynamicAddwithOffset("Layer type error. If using a tag file layer filter, layer number must be provided", 33);
      //DynamicAddwithOffset("Layer number error. To use the layer number filter, layer type must be specified", 34);
      //35: FilterValidationErrorCodesProvider
      DynamicAddwithOffset("GetFilter By filterUid. The requested filter does not exist, or does not belong to the requesting customer; project or user.", 36);
      DynamicAddwithOffset("DeleteFilter. Invalid parameters.", 37);
      DynamicAddwithOffset("UpsertFilter. Invalid parameters.", 38);
      DynamicAddwithOffset("UpsertFilter failed. Unable to add persistent filter as Name already exists.", 39);
      DynamicAddwithOffset("Validation of Project/Boundary failed. Not allowed.", 40);
      DynamicAddwithOffset("Validation of Project/Boundary failed. Exception: {0}.", 41);
      DynamicAddwithOffset("Invalid filterJson. Exception: {0}.", 42);
      DynamicAddwithOffset("Hydration of filterJson with boundary failed. Exception: {0}.", 43);
      DynamicAddwithOffset("Hydration of filterJson with boundary failed.", 44);
      DynamicAddwithOffset("Invalid spatial filter boundary. One or more polygon components are missing.", 45);
      DynamicAddwithOffset("Invalid object type, cast failed.", 46);
      DynamicAddwithOffset("GetBoundary By BoundaryUid. Invalid parameters.", 47);
      DynamicAddwithOffset("GetBoundary By BoundaryUid. Unable to retrieve boundary. Exception: {0}.", 48);
      DynamicAddwithOffset("GetBoundary By BoundaryUid. The requested Boundary does not exist, or does not belong to the requesting project or filter.", 49);
      DynamicAddwithOffset("GetBoundaries By projectUid. Unable to retrieve Boundaries. Exception: {0}.", 50);
      DynamicAddwithOffset("GetBoundary. Invalid parameters.", 51);
      DynamicAddwithOffset("GetBoundaries. Invalid parameters.", 52);
      DynamicAddwithOffset("DeleteBoundary. Invalid parameters.", 53);
      DynamicAddwithOffset("UpsertBoundary. Invalid parameters.", 54);
      DynamicAddwithOffset("UpsertBoundary failed. Unable to create association to project.", 55);
      DynamicAddwithOffset("UpsertBoundary failed. Unable to write to Kafka. Exception: {0}.", 56);
      DynamicAddwithOffset("UpsertBoundary failed. Unable to create boundary. Exception: {0}.", 57);
      DynamicAddwithOffset("UpsertBoundary failed. Unable to create boundary.", 58);
      DynamicAddwithOffset("Invalid boundaryUid.", 59);
      DynamicAddwithOffset("Missing boundary parameters.", 60);
      DynamicAddwithOffset("UpsertBoundary. Update not supported", 61);
      DynamicAddwithOffset("Duplicate boundary name", 62);
      //DynamicAddwithOffset("Invalid boundary polygon WKT. Should not be null.", 63);
      //DynamicAddwithOffset("Invalid boundary polygon WKT. Should be > 3 points.", 64);
      //DynamicAddwithOffset("Invalid boundary polygon WKT. Invalid format.", 65);
      //66: FilterValidationErrorCodesProvider
      //67: FilterValidationErrorCodesProvider
      //68: FilterValidationErrorCodesProvider
    }
  }
}
