using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.TagFileGateway.Common.Models.Executors
{
    public class TagFileProcessExecutor : RequestExecutorContainer
    {
        protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
        {
            if (!(item is CompactionTagFileRequest request))
            {
                Logger.LogWarning("Empty request passed");
                return ContractExecutionResult.ErrorResult("Empty Request");
            }

            request.Validate();

            Logger.LogInformation($"Received Tag File with filename: {request.FileName}. TCC Org: {request.OrgId}. Data Length: {request.Data.Length}");

            var result = await TagFileForwarder.SendTagFileDirect(request);

            await using (var data = new MemoryStream(request.Data))
            {
                Logger.LogInformation($"Uploading Tag File {request.FileName}");
                var path = GetS3Key(request.FileName);

                TransferProxy.Upload(data, path);
                Logger.LogInformation($"Successfully uploaded Tag File {request.FileName}");
            }

            return result;
        }

        public static string GetS3Key(string tagFileName)
        {
            //Example tagfile name: 0415J010SW--HOUK IR 29 16--170731225438.tag
            //Format: <display or ECM serial>--<machine name>--yyMMddhhmmss.tag
            //Required folder structure is /<serial>--<machine name>/<serial--machine name--date>/<tagfile>
            //e.g. 0415J010SW--HOUK IR 29 16/Production-Data (Archived)/0415J010SW--HOUK IR 29 16--170731/0415J010SW--HOUK IR 29 16--170731225438.tag
            const string separator = "--";
            var parts = tagFileName.Split(new string[] { separator }, StringSplitOptions.None);
            if (parts.Length < 3)
            {
                return $"invalid/{tagFileName}";
            }
            var nameWithoutTime = tagFileName.Substring(0, tagFileName.Length - 10);
            //TCC org ID is not provided with direct submission from machines
            return $"{parts[0]}{separator}{parts[1]}/{nameWithoutTime}/{tagFileName}";
        }
    }
}
