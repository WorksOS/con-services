using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.TagfileProcessing.Executors
{
  public class TagFileConnectedSiteSubmissionExecutor : RequestExecutorContainer
  {
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as CompactionTagFileRequest;

      if (request == null)
        ThrowRequestTypeCastException<CompactionTagFileRequest>();

      var result = new ContractExecutionResult(ContractExecutionStatesEnum.ExecutedSuccessfully,
        "Connected Site Disabled");
      //Send the tagfile to the connected site gateway if enabled first, no project/subscription validation is required.
      bool.TryParse(configStore.GetValueString("ENABLE_CONNECTED_SITE_GATEWAY"), out var enableConnectedSiteGateway);
      if (enableConnectedSiteGateway)
      {
        result = new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
        "3dPm Unknown exception.");
        try
        {
          log.LogDebug("Sending tag file to connected site gateway");
          request.Validate();
          result = await CallConnectedSiteEndpoint(request).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
          //Something went wrong but we don't really care log and continue
          log.LogDebug($"PostTagFile(NonDirect TRex): Failed to send TAG file to connected site gateway exception occured {ex.ToString()} ");
        }

        if (result.Code == 0)
        {
          log.LogDebug($"PostTagFile (NonDirect TRex): Successfully sent TAG file to connected site gateay '{request.FileName}'.");
        }
        else
        {
          log.LogDebug(
            $"PostTagFile (NonDirect TRex): Failed to send TAG file to connected site gateway'{request.FileName}', {result.Message}");
        }
      }
      return result;
    }

    private async Task<ContractExecutionResult> CallConnectedSiteEndpoint(CompactionTagFileRequest request)
    {

      var connectedSiteResult = await tRexTagFileProxy.SendTagFileNonDirectToConnectedSite(request, customHeaders).ConfigureAwait(false);

      log.LogInformation($"PostTagFile (NonDirect TRex): result: {JsonConvert.SerializeObject(connectedSiteResult)}");

      return connectedSiteResult;
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
