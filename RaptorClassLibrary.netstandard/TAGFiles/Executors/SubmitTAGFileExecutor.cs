using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using log4net;
using VSS.TRex.TAGFiles.Classes.Queues;
using VSS.TRex.TAGFiles.GridFabric.Requests;
using VSS.TRex.TAGFiles.GridFabric.Responses;
using VSS.VisionLink.Raptor.TAGFiles.GridFabric.Arguments;
using VSS.VisionLink.Raptor.TAGFiles.GridFabric.Responses;

namespace VSS.TRex.TAGFiles.Executors
{
    /// <summary>
    /// Execute internal business logic to handle submission of a TAG file to TRex
    /// </summary>
    public class SubmitTAGFileExecutor
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Receive a TAG file to be processed, validate TAG File Authorisation for the file, and add it to the 
        /// queue to be processed.
        /// </summary>
        /// <param name="ProjectID">Project ID to be used as an override to any project ID that may be determined via TAG file authorization</param>
        /// <param name="AssetID">Asset ID to be used as an override to any Asste ID that may be determined via TAG file authorization</param>
        /// <param name="TAGFileContent">The content of the TAG file to be processed, expressed as a byte array</param>
        /// <returns></returns>
        public static SubmitTAGFileResponse Execute(long ProjectID, long AssetID, string TAGFileName, byte [] TAGFileContent)
        {
      // Execute TFA based business logic along with override IDs to determine final project and asset
      // identities to be used for processing the TAG file
      // ...

      /*
       * unit TAGProcServiceRPCExecute;
       *
       *
       *       function ProcessTAGFile(const FileName: String;
                              var FileContents: TStream;
                              const ADataModelID : Int64;
                              const AForceProcessingIntoDesignatedDataModel : Boolean;
                              const AMachineID : TICMachineID;
                              AMachineLevel : MachineLevelEnum;
                              AAssetIsAJohnDoe : Boolean;
                              const AAddedAsReplicatedTagFile: Boolean;
                              const SeedLatitude, SeedLongitude : Double;
                              const ConvertToCSV, ConvertToDXF, ConvertToGPX : Boolean): TTAGProcServerProcessResult;
       */


            Log.Info($"#In# SubmitTAGFileResponse. Processing {TAGFileName} TAG files into project {ProjectID}, asset {AssetID}");

            SubmitTAGFileResponse response = new SubmitTAGFileResponse();
            response.FileName = TAGFileName;
            response.Success = false;
            response.Exception = "Unknown";

            // validate input
            // Check file already processing??? 
            // check count of submitted tags is not to high else pause 1 sec
            // xml file ???


            // Place the validated TAG file content and processing meta data (project ID, asset ID, etc) into
            // the TAG file processing queue cache.
            // ...

              Guid projectUID = Guid.NewGuid(); // todo convert to use GUID
            Guid assetUID = Guid.NewGuid(); // todo convert to use GUID

            TAGFileBufferQueue queue = new TAGFileBufferQueue();

              TAGFileBufferQueueKey tagKey = new TAGFileBufferQueueKey(TAGFileName, projectUID, assetUID);
              // todo AssetID is now GUID
              TAGFileBufferQueueItem tagItem = new TAGFileBufferQueueItem
                                               {
                                                   InsertUTC = DateTime.Now,
                                                   ProjectUID = projectUID,
                                                   AssetUID = Guid.NewGuid(),
                                                   FileName = TAGFileName,
                                                   Content = TAGFileContent
              };

              if (queue.Add(tagKey, tagItem))
              {
                response.Success = true;
                response.Exception = "";
              }
              else
              {
                response.Success = false;
                response.Exception = "Failed to submit to tagfile processing queue";
              }


          Log.Info($"#Out# SubmitTAGFileResponse. Processed {TAGFileName} Result. Success:{response.Success}, Exception:{response.Exception}");

          return response;
        }
    }
}
