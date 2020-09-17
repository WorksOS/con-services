using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling
{
  public class ProjectErrorCodesProvider : ContractExecutionStatesEnum
  {
    public ProjectErrorCodesProvider()
    {
      this.DynamicAddwithOffset("No access to the project for a customer or the project does not exist.", 1);
      this.DynamicAddwithOffset("Missing ProjectUID.", 5);
      this.DynamicAddwithOffset("Project already exists.", 6);
      this.DynamicAddwithOffset("Project does not exist.", 7);
      this.DynamicAddwithOffset("Missing ProjectBoundary.", 8);
      this.DynamicAddwithOffset("Missing ProjectTimezone.", 9);
      this.DynamicAddwithOffset("Invalid ProjectTimezone.", 10);
      this.DynamicAddwithOffset("Missing ProjectName.", 11);
      this.DynamicAddwithOffset("CustomerUID parameter differs to the requesting CustomerUID. Impersonation is not supported.", 18);
      this.DynamicAddwithOffset("Missing CustomerUID.", 19);
      this.DynamicAddwithOffset("shortRaptorProjectId has not been generated. {0}", 42);
      this.DynamicAddwithOffset("Project boundary overlaps another project, for this customer and time span.", 43);
      this.DynamicAddwithOffset("Missing legacyProjectId.", 44);
      this.DynamicAddwithOffset("Landfill is missing its CoordinateSystem.", 45);
      this.DynamicAddwithOffset("Invalid CoordinateSystem.", 46);
      this.DynamicAddwithOffset("Unable to validate CoordinateSystem in RaptorServices. returned: {0} {1}.", 47);
      this.DynamicAddwithOffset("Unable to obtain TCC fileSpaceId.", 48);
      this.DynamicAddwithOffset("CreateImportedFile. Unable to store Imported File event to database.", 49);
      this.DynamicAddwithOffset("LegacyImportedFileId has not been generated.", 50);
      this.DynamicAddwithOffset("DeleteImportedFile. Unable to set Imported File event to deleted.", 51);
      this.DynamicAddwithOffset("CreateImportedFile. Unable to store updated Imported File event to database.", 52);
      this.DynamicAddwithOffset("CreateImportedFile. The uploaded file is not accessible.", 55);
      this.DynamicAddwithOffset("DeleteImportedFile. The importedFileUid doesn't exist under this project.", 56);
      this.DynamicAddwithOffset("A problem occurred at the {0} endpoint. Exception: {1}", 57);
      this.DynamicAddwithOffset("CreateImportedFile. The file has already been created.", 58);
      this.DynamicAddwithOffset("GeofenceService CreateGeofence failed. No geofenceUid returned. {0}", 59);
      this.DynamicAddwithOffset("Unable to create project. {0}", 61);
      this.DynamicAddwithOffset("Invalid parameters.", 68);
      this.DynamicAddwithOffset("Unable to retrieve project settings from repository. Reason: {0} {1}.", 69);
      this.DynamicAddwithOffset("Unable to validate project settings with raptor. Reason: {0} {1}.", 70);
      this.DynamicAddwithOffset("Unsupported ProjectSettings type.", 77);
      this.DynamicAddwithOffset("GetCoordinateSystemFromFileRepo: Exception reading file {0}.", 79);
      this.DynamicAddwithOffset("GetCoordinateSystemFromFileRepo: Returned file invalid {0}.", 80);
      this.DynamicAddwithOffset("CreateProject: Missing CreateProjectRequest.", 81);
      this.DynamicAddwithOffset("ValidateTCCAuthorization: Invalid Request: {0}.", 86);
      this.DynamicAddwithOffset("CopyTccImportedFile: Unable to copy file {0}.", 92);
      this.DynamicAddwithOffset("CopyTccImportedFile: Unable to obtain file properties {0}.", 94);
      this.DynamicAddwithOffset("UpsertProject Not allowed duplicate, active projectnames.", 109);
      this.DynamicAddwithOffset("Invalid project boundary points.Latitudes should be -90 through 90 and Longitude -180 through 180. Points around 0,0 are invalid", 111);
      this.DynamicAddwithOffset("Cannot delete a design, surveyed surface, or alignment file which is used in a filter.", 112);
      this.DynamicAddwithOffset("FileImport AddFile in TRex failed. Reason: {0} {1}.", 114);
      this.DynamicAddwithOffset("Unable to obtain DataOcean root folder.", 115);
      this.DynamicAddwithOffset("Unable to write file to DataOcean.", 116);
      this.DynamicAddwithOffset("Unable to delete file from DataOcean", 117);
      this.DynamicAddwithOffset("A reference surface must have a parent design surface and offset", 118);
      this.DynamicAddwithOffset("Cannot delete a design that has reference surfaces", 119);
      this.DynamicAddwithOffset("Missing parent design for reference surface", 120);
      this.DynamicAddwithOffset("Reference surface already exists", 121);
      this.DynamicAddwithOffset("This endpoint does not support importing reference surfaces", 122);
      this.DynamicAddwithOffset("Unable to retrieve existing file for the update", 123);
      this.DynamicAddwithOffset("Device cws service exception: {0}", 124);
      this.DynamicAddwithOffset("Self-intersecting project boundary.", 129);
      this.DynamicAddwithOffset("Missing project type.", 130);
      this.DynamicAddwithOffset("A problem occurred downloading the calibration file. Exception: {0}", 131);
      this.DynamicAddwithOffset("Missing coordinate system file name.", 132);
      this.DynamicAddwithOffset("Missing coordinate system file contents.", 133);
      this.DynamicAddwithOffset("Both coordinate system file name and contents must be provided.", 134);
      this.DynamicAddwithOffset("Mismatched customerUid.", 135);
      this.DynamicAddwithOffset("Unknown update type in project validation.", 136);
      this.DynamicAddwithOffset("Invalid ProjectUid", 137);
      this.DynamicAddwithOffset("Invalid earliestOfInterestUtc", 138);
      this.DynamicAddwithOffset("More than 1 project matches the TBC project Id requested.", 139);
      this.DynamicAddwithOffset("Unable to extract shortProjectId.", 140);
      this.DynamicAddwithOffset("Cannot delete a project that has 3D production (tag file) data", 141);
    }
  }
}

