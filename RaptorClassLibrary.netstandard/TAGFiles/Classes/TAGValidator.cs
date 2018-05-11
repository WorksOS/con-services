using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using log4net;
using VSS.TRex.TAGFiles.Classes;
using VSS.TRex.TAGFiles.Models;
using VSS.VisionLink.Raptor;
using VSS.VisionLink.Raptor.Events;
using VSS.VisionLink.Raptor.Machines;
using VSS.VisionLink.Raptor.SiteModels;
using VSS.VisionLink.Raptor.SubGridTrees.Server;
using VSS.VisionLink.Raptor.TAGFiles.Classes;
using VSS.VisionLink.Raptor.TAGFiles.Classes.Sinks;
using VSS.VisionLink.Raptor.TAGFiles.Types;

/// <summary>
/// This class is only a placeholder for refactoring later. Possibly in its own library
/// </summary>
namespace VSS.TRex.TAGFiles.Classes
{
    public static class TAGValidator
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static bool assetIsJohnDoe = false;
        private static bool overrideProjectID = false;
        private static bool overrideAssetID = false;
        private static double seedLatitude = 0.0;
        private static double seedLongitude = 0.0;
        private static bool projectIsLandfill = false;
        private static ChooseMachineFromFileResult chooseMachineFromFileResult;
        private static string kTorchTagFileLabel = "torch";

    public class GetProjectIDRequest
        {
            public long assetId;
            public double latitude;
            public double longitude;
            public double height;
            public string timeOfPosition;
            public string tccOrgUid;
        }

        public class TAGDetail
        {
            public long projectId;
            public Guid assetId;
            public string tagFileName;
            public byte[] tagFileContent;
            public string tccOrgId;
        }


        public enum ValidationCode
        {
            None,
            OK,
            TagFileInvalid,
            InputInvalid,
            NotProcessible
        }


        public enum ChooseMachineFromFileResult
        {
            cmSuccess,
            cmUnknown,
            cmUnknownMachine,
            cmInvalidTagFile,
            cmInvalidSubscriptions,
            cmUnableToDetermineMachine,
            cmTFAError
        }



        public enum MachineLevel
        {
            Unknown = 0, // (No machine control subscription at level 3 or 4 or expired)
            Essentials = 1,
            ManualMaintenanceLog = 2,
            CATHealth = 3,
            StandardHealth = 4,
            CATUtilization = 5,
            StandardUtilization = 6,
            CATMAINT = 7,
            VLMAINT = 8,
            RealTimeDigitalSwitchAlerts = 9,
            e1minuteUpdateRateUpgrade = 10,
            Unused1 = 11, // Introduced to ensure continuity of Delphi enum thereby enabling RTTI generation for improved logging
            Unused2 = 12, // Introduced to ensure continuity of Delphi enum thereby enabling RTTI generation for improved logging
            Unused3 = 13, // Introduced to ensure continuity of Delphi enum thereby enabling RTTI generation for improved logging
            ConnectedSiteGateway = 14,
            BasicProduction = 15, // Basic Production (Level 3)
            ProductionAndCompaction = 16, // Production and Compaction (Level 4)
            Unused4 = 17, // Introduced to ensure continuity of Delphi enum thereby enabling RTTI generation for improved logging
            Manual3DProjectMonitoring = 18 // Manual 3DPM import only subscription
        }



        public enum DeviceType
        {
            ManualDevice = 0,
            PL121 = 1,
            PL321 = 2,
            Series522 = 3,
            Series523 = 4,
            Series521 = 5,
            SNM940 = 6,
            CrossCheck = 7,
            TrimTrac = 8,
            TM3000 = 9
        }


        private static ChooseMachineFromFileResult ChooseMachineFromFile(TAGDetail tagDetail, TAGProcessor processor, out long projectIdOverride, out Guid assetId, out DeviceType radioType, out MachineLevel machineLevel, out bool isJohnDoe, ref string tccOrgId)
        {
            assetId = Guid.Empty;
            radioType = DeviceType.ManualDevice;
            machineLevel = MachineLevel.Manual3DProjectMonitoring;
            isJohnDoe = false;
            tccOrgId = String.Empty;
            projectIdOverride = 0;

            /*
const FileName : String;
                               const UseXMLFile : Boolean;
                               const DataModelID : Int64;
                               const AssetIDOverride : Int64; //const FileToProcess : TICSnippetFileToProcess;
                               const Processor: TSVOICSnailTrailProcessorStateBase;
                               out ProjectIDOverride : Int64;
                               out AssetID : Int64;
                               out RadioType: DeviceTypeEnum;
                               out MachineLevel : MachineLevel;
                               out IsJohnDoe : Boolean;
                               var TCCOrgID : String) : TChooseMachineFromFileResult;             
             */


            if (tagDetail.assetId != Guid.Empty) // If overriding AssetID
            {
                Log.Info($"ChooseMachineFromFile. Setting AssetID to override value {tagDetail.assetId} for TAG file {tagDetail.tagFileName}");
                assetId = tagDetail.assetId;
                machineLevel = MachineLevel.ProductionAndCompaction;
            }
            else
            {
                // XML file read ???   
                
                if (processor.RadioType == kTorchTagFileLabel)
                   radioType = DeviceType.SNM940; // torch is converted to type SMN940

                if (processor.RadioSerial != string.Empty)
                    Log.Info($"RadioSerial:{processor.RadioSerial} located in tag file {tagDetail.tagFileName}"); 
                else
                    Log.Warn($"No RadioSerial found in tag file {tagDetail.tagFileName}");


                if (processor.RadioSerial == String.Empty && tagDetail.projectId < 1)
                    Log.Warn($"Not calling GetAssetID as DataModel and RadioSerial not present");
                else
                {
                    var tfaRequest = new TFAProxy();
                    var res = tfaRequest.GetAssetId(tagDetail.projectId, radioType, processor.RadioSerial, out assetId,out machineLevel);
                    if (res == RequestResult.Ok)
                    {
                        Log.Info($"GetAssetId returned.  AssetId:{assetId}, MachineLevel:{machineLevel}, for Project:{tagDetail.projectId}, RadioType:{radioType} in tag file {tagDetail.tagFileName}");
                    }
                    else
                    {
                        Log.Warn($"GetAssetID failed due to error using TFA service for tag file {tagDetail.tagFileName}");
                        return ChooseMachineFromFileResult.cmTFAError;
                    }
                }

            }

            if (machineLevel != MachineLevel.ProductionAndCompaction ||
                machineLevel != MachineLevel.Manual3DProjectMonitoring)
            {
                Log.Warn($"Unable to determine valid AssetID for TAG file {tagDetail.tagFileName} - No valid subscription");
                return ChooseMachineFromFileResult.cmInvalidSubscriptions;
            }
            else if (assetId == Guid.Empty)
            {
                Log.Warn($"Unable to determine valid AssetID for TAG file {tagDetail.tagFileName} - No matchind Asset");
                return ChooseMachineFromFileResult.cmUnableToDetermineMachine;

            }

            return ChooseMachineFromFileResult.cmSuccess;
        }


        private static bool CheckFileIsProcessible(TAGDetail tagDetail, TAGProcessor processor)
        {
            // CheckFileIsProcessible
            /*
             *
function CheckFileIsProcessible(const SubmittedMachineID : TICMachineID;
                                const SubmittedDataModelID : Int64;
                                const SubmittedFileName : String;
                                const SubmittedWGS84ProjectBoundary : TWGS84FenceContainer;
                                var TCCOrgID :String;
                                const Processor : TSVOICSnailTrailProcessorStateBase;
                                var   AssetID : TICMachineID;
                                var   MachineLevel              :MachineLevelEnum;
                                var   AssetIsAJohnDoe : Boolean;
                                const DataModelID : Int64;
                                const ForceDataModelIDOverride : Boolean;
                                var   SeedLatitude, SeedLongitude : Double;
                                var   ServerResult : TTAGProcServerProcessResult) :Boolean;
             
             *
             */
            //   Log.Debug($"#In# CheckFileIsProcessible. Processing {TAGFileName} TAG file into project {ProjectID}, asset {AssetID}");

            //     if (tagDetail.assetID == -1)

            //            chooseMachineResult = ChooseMachineFromFile(tagDetail, SubmittedFileName, False, SubmittedDataModelID, SubmittedMachineID, Processor, ProjectIDOverride, AssetID, RadioType, MachineLevel, AssetIsAJohnDoe, TCCOrgID)

            long ProjectIDOverride;
            Guid AssetID = Guid.Empty;
            TAGValidator.DeviceType RadioType;
            TAGValidator.MachineLevel MachineLevel;
            bool AssetIsAJohnDoe;
            string TCCOrgID = tagDetail.tccOrgId;

            TAGValidator.ChooseMachineFromFileResult chooseMachineResult = ChooseMachineFromFile(tagDetail, processor, out ProjectIDOverride, out AssetID,
                    out RadioType, out MachineLevel, out AssetIsAJohnDoe, ref TCCOrgID);

         //   else // The asset ID is being overridden - don't bother to check it
         //   ChooseMachineResult:= cmSuccess;

            if (chooseMachineResult == TAGValidator.ChooseMachineFromFileResult.cmTFAError)
            {
            //    Result:= False;
            //    ServerResult:= TTAGProcServerProcessResult.tpsprTFAServiceError;
             //   Exit;
                return false;
            }
            

            return true;
        }


    

        public static ValidationCode ValidSubmission(TAGDetail tagDetail)
        {
        // Do the TFA thing here

        // Check file already processing??? 
        // check count of submitted tags is not to high else pause 1 sec
        // xml file ???

            TAGValidator.ValidationCode result = TAGValidator.ValidationCode.None;

            // Perform some Validation Checks
        
            if (tagDetail.tagFileContent.Length <= RaptorConfig.MinTAGFileLength)
                return TAGValidator.ValidationCode.TagFileInvalid;

            if (!(tagDetail.projectId > 0 || tagDetail.projectId == -1))
                return TAGValidator.ValidationCode.InputInvalid;

           // if (!(tagDetail.assetId < 0))
             //   return TAGValidator.ValidationCode.InputInvalid;
            
            if (!RaptorConfig.EnableTFAService)
            {
                Log.Debug($"SubmitTAGFileResponse.ValidSubmission. EnableTFAService disabled. Bypassing validation checks");
                return TAGValidator.ValidationCode.OK;
            }

            // Now open tagfile and validate
            var siteModel = new SiteModel(Guid.Empty);
            var machine = new Machine()
                      {
                              TargetValueChanges = new ProductionEventLists(siteModel, 0)
                       };

            var siteModelGridAggregator = new ServerSubGridTree(siteModel);
            var machineTargetValueChangesAggregator = new ProductionEventLists(siteModel, long.MaxValue);
            TAGProcessor processor = new TAGProcessor(siteModel, machine, siteModelGridAggregator, machineTargetValueChangesAggregator);
            TAGValueSink sink = new TAGVisionLinkPrerequisitesValueSink(processor);
            TAGReader reader = new TAGReader(new MemoryStream(tagDetail.tagFileContent));
            TAGFile tagFile = new TAGFile();
            TAGReadResult readResult = tagFile.Read(reader, sink);
            if (readResult != TAGReadResult.NoError)
                return TAGValidator.ValidationCode.TagFileInvalid;


            return CheckFileIsProcessible(tagDetail, processor) ? TAGValidator.ValidationCode.OK : TAGValidator.ValidationCode.NotProcessible;


            /*
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(req);

        // Update port # in the following line.
        string URL = RaptorConfig.TFAServiceURL + RaptorConfig.TFAServiceGetProjectID;
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
        request.Method = "POST";
        request.ContentType = "application/json";
        request.ContentLength = json.Length;
        StreamWriter requestWriter = new StreamWriter(request.GetRequestStream(), System.Text.Encoding.ASCII);
        requestWriter.Write(json);
        requestWriter.Close();

        try
        {
          WebResponse webResponse = request.GetResponse();
          Stream webStream = webResponse.GetResponseStream();
          StreamReader responseReader = new StreamReader(webStream);
          string response = responseReader.ReadToEnd();
          Console.Out.WriteLine(response);
          responseReader.Close();
        }
        catch (Exception e)
        {
          Console.Out.WriteLine("-----------------");
          Console.Out.WriteLine(e.Message);
        }
        */

        
      }

  }
}
