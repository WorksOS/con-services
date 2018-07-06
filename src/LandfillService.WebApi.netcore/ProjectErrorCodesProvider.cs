using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.MasterData.Landfill.WebAPI.Common.ResultsHandling
{
  public class ProjectErrorCodesProvider : ContractExecutionStatesEnum
  {
    public ProjectErrorCodesProvider()
    {
      DynamicAddwithOffset("No access to the project for a customer or the project does not exist.", 1);
      DynamicAddwithOffset(
        "Supplied CoordinateSystem filename is not valid. Exceeds the length limit of 256, is empty, or contains illegal characters.",
        2);
      DynamicAddwithOffset("Unable to connect to the Project database repository.", 3);
      DynamicAddwithOffset("Missing ActionUTC.", 4);
      DynamicAddwithOffset("Missing ProjectUID.", 5);
      DynamicAddwithOffset("Project already exists.", 6);
      DynamicAddwithOffset("Project does not exist.", 7);
      DynamicAddwithOffset("Missing ProjectBoundary.", 8);
      DynamicAddwithOffset("Missing ProjectTimezone.", 9);
      DynamicAddwithOffset("Invalid ProjectTimezone.", 10);
      DynamicAddwithOffset("Missing ProjectName.", 11);
      DynamicAddwithOffset("ProjectName is longer than the 255 characters allowed.", 12);
      DynamicAddwithOffset("Description is longer than the 2000 characters allowed.", 13);
      DynamicAddwithOffset("Missing ProjectStartDate.", 14);
      DynamicAddwithOffset("Missing ProjectEndDate.", 15);
      DynamicAddwithOffset("Project start date must be earlier than end date.", 16);
      DynamicAddwithOffset("Project timezone cannot be updated.", 17);
      DynamicAddwithOffset(
        "CustomerUid parameter differs to the requesting CustomerUid. Impersonation is not supported.", 18);
      DynamicAddwithOffset("Missing CustomerUID.", 19);
      DynamicAddwithOffset("Project already associated with a customer.", 20);
      DynamicAddwithOffset("Dissociating projects from customers is not supported.", 21);
      DynamicAddwithOffset("Missing GeofenceUID.", 22);
      DynamicAddwithOffset("Missing project boundary.", 23);
      DynamicAddwithOffset("Invalid project boundary as it should contain at least 3 points.", 24);
      DynamicAddwithOffset("Invalid project boundary.", 25);
      DynamicAddwithOffset("Filespace Id, filespace name, path and file name are all required.", 26);
      DynamicAddwithOffset("CreateImportedFileV4.The file was not imported successfully.", 27);
      DynamicAddwithOffset(
        "CreateImportedFileV4. Supplied filename is not valid. Either exceeds the length limit of 256 is empty or contains illegal characters.",
        28);
      DynamicAddwithOffset(
        "CreateImportedFileV4. Supplied path is not valid. Either is empty or contains illegal characters.", 29);
      DynamicAddwithOffset("CreateImportedFileV4. ImportedFileType is an unrecognized type.", 30);
      DynamicAddwithOffset("CreateImportedFileV4. ImportedFileType is not supported at present.", 31);
      DynamicAddwithOffset("CreateImportedFileV4. ImportedFileType does not match the file extension.", 32);
      DynamicAddwithOffset("The fileCreatedUtc is over 30 years old or >2 days in the future.", 33);
      DynamicAddwithOffset("The fileUpdatedUtc is over 30 years old or >2 days in the future.", 34);
      DynamicAddwithOffset("The ImportedBy is not available from authentication.", 35);
      DynamicAddwithOffset("The SurveyedUtc is not available.", 36);
      DynamicAddwithOffset("There are no available subscriptions for the selected customer.", 37);
      DynamicAddwithOffset("Legacy CustomerID must be provided.", 38);
      DynamicAddwithOffset("Missing CreateProjectRequest.", 39);
      DynamicAddwithOffset("Missing UpdateProjectRequest.", 40);
      DynamicAddwithOffset("Unable to create/update CoordinateSystem in RaptorServices. returned: {0} {1}.", 41);
      DynamicAddwithOffset("LegacyProjectId has not been generated.", 42);
      DynamicAddwithOffset("Project boundary overlaps another project, for this customer and time span.", 43);
      DynamicAddwithOffset("Missing legacyProjectId.", 44);
      DynamicAddwithOffset("Landfill is missing its CoordinateSystem.", 45);
      DynamicAddwithOffset("Invalid CoordinateSystem.", 46);
      DynamicAddwithOffset("Unable to validate CoordinateSystem in RaptorServices. returned: {0} {1}.", 47);
      DynamicAddwithOffset("Unable to obtain TCC fileSpaceId.", 48);
      DynamicAddwithOffset("CreateImportedFileV4. Unable to store Imported File event to database.", 49);
      DynamicAddwithOffset("LegacyImportedFileId has not been generated.", 50);
      DynamicAddwithOffset("DeleteImportedFileV4. Unable to set Imported File event to deleted.", 51);
      DynamicAddwithOffset("CreateImportedFileV4. Unable to store updated Imported File event to database.", 52);
      DynamicAddwithOffset("WriteFileToRepository: Unable to write file to TCC.", 53);
      DynamicAddwithOffset("Unable to put delete fileDescriptor from TCC. TCC code {0} message {1}", 54);
      DynamicAddwithOffset("CreateImportedFileV4. The uploaded file is not accessible.", 55);
      DynamicAddwithOffset("DeleteImportedFileV4. The importedFileUid doesn't exist under this project.", 56);
      DynamicAddwithOffset("A problem occurred at the {0} endpoint. Exception: {1}", 57);
      DynamicAddwithOffset("CreateImportedFileV4. The file has already been created.", 58);
      DynamicAddwithOffset("GeofenceService CreateGeofence failed. No geofenceUid returned.", 59);
      DynamicAddwithOffset("Application calling context supports only HttpGet endpoints.", 60);
      DynamicAddwithOffset("Unable to create project.", 61);
      DynamicAddwithOffset("Unable to update project.", 62);
      DynamicAddwithOffset("Unable to associate project with customer.", 63);
      DynamicAddwithOffset("Unable to disassociate project from customer.", 64);
      DynamicAddwithOffset("Unable to store project-geofence associate in database.", 65);
      DynamicAddwithOffset("Unable to delete project.", 66);
      DynamicAddwithOffset("FileImport AddFile in RaptorServices failed. Reason: {0} {1}.", 67);
      DynamicAddwithOffset("Invalid parameters.", 68);
      DynamicAddwithOffset("Unable to retrieve project settings from repository. Reason: {0} {1}.", 69);
      DynamicAddwithOffset("Unable to validate project settings with raptor. Reason: {0} {1}.", 70);
      DynamicAddwithOffset("Unable to update project settings with raptor. Reason: {0} {1}.", 71);
      DynamicAddwithOffset("Unable to create Kafka event. Reason: {0}.", 72);
      DynamicAddwithOffset("Invalid geofence Types.", 73);
      DynamicAddwithOffset("Landfill projects are not supported.", 74);
      DynamicAddwithOffset("CreateImportedFileV4. DxfUnitsType is an unrecognized type.", 75);
      DynamicAddwithOffset("CreateImportedFileV4. DxfUnitsType is not supported at present.", 76);
      DynamicAddwithOffset("Unsupported ProjectSettings type.", 77);
      DynamicAddwithOffset("GetCoordinateSystemFromFileRepo: Folder {0} doesn't exist.", 78);
      DynamicAddwithOffset("GetCoordinateSystemFromFileRepo: Exception reading file {0}.", 79);
      DynamicAddwithOffset("GetCoordinateSystemFromFileRepo: Returned file invalid {0}.", 80);
      DynamicAddwithOffset("CreateProjectV2: Missing CreateProjectRequest.", 81);
      DynamicAddwithOffset("CreateProjectV2: Missing BusinessCentreFile.", 82);
      DynamicAddwithOffset("CreateProjectV2: Invalid businessCentreFile path.", 83);
      DynamicAddwithOffset("CreateProjectV2: Invalid businessCentreFile fileSpaceId.", 84);
      DynamicAddwithOffset("UpdateProjectV4: Invalid ProjectType. Can ony be changed from Standard to Landfill/Civil.",
        85);
      DynamicAddwithOffset("ValidateTCCAuthorization: Invalid Request: {0}.", 86);
      DynamicAddwithOffset("ValidateTCCAuthorization: Exception getting list of organizations from TCC: {0}.", 87);
      DynamicAddwithOffset("ValidateTCCAuthorization: Unable to locate orgShortName in TCC {0}.", 88);
      DynamicAddwithOffset("ValidateTCCAuthorization: This orgShortName is missing a filespaceId or orgId in TCC {0}.",
        89);
      DynamicAddwithOffset("ValidateTCCAuthorization: Exception getting customerTccOrg from database: {0.}", 90);
      DynamicAddwithOffset("ValidateTCCAuthorization: Unable to match customer with tccOrg in database.", 91);
      DynamicAddwithOffset("CopyTccImportedFile: Unable to copy file {0}.", 92);
      DynamicAddwithOffset("CopyTccImportedFile: Unable to read file {0}.", 93);
      DynamicAddwithOffset("CopyTccImportedFile: Unable to obtain file properties {0}.", 94);
      DynamicAddwithOffset("UpsertImportedFileV2: Alignment type is missing detail parameter.", 95);
      DynamicAddwithOffset("UpdateGeofenceInGeofenceService: Unable to find the project-geofence association.", 96);
      DynamicAddwithOffset("UpdateGeofenceInGeofenceService: Unable to find the projects Geofence.", 97);
      DynamicAddwithOffset("UpdateGeofenceInGeofenceService: Unable to find the projects Geofence. Exception: {0}", 98);
      DynamicAddwithOffset("UpdateGeofenceInGeofenceService: Unable update the projects Geofence.", 99);
      DynamicAddwithOffset("UpdateGeofenceInGeofenceService: Unable update the projects Geofence. Exception: {0}", 100);
      DynamicAddwithOffset(
        "UpdateGeofenceInGeofenceService: Unable to find the project-geofence association. Exception: {0}", 101);
      DynamicAddwithOffset("ProjectGeofenceAssociation: Only landfill project and site types are supported at present.",
        102);
      DynamicAddwithOffset("ProjectGeofenceAssociation: Invalid GeofenceUid list.", 103);
      DynamicAddwithOffset("ProjectGeofenceAssociation: Unable to find GeofenceUid/s.", 104);
      DynamicAddwithOffset("ProjectGeofenceAssociation: Geofence is assigned to a different project.", 105);
      DynamicAddwithOffset("ProjectGeofenceAssociation: Error accessing database. Exception: {0}", 106);
      DynamicAddwithOffset("ProjectGeofenceAssociation: Already associated geofence is missing from request.", 107);
      DynamicAddwithOffset("FileImport DeleteFile in RaptorServices failed. Reason: {0} {1}.", 108);
      DynamicAddwithOffset("UpsertProject Not allowed duplicate, active projectnames: {0}.", 109);
      DynamicAddwithOffset("ProjectGeofenceAssociation: Duplicate GeofenceUids appear in the request body.", 110);
    }
  }
}