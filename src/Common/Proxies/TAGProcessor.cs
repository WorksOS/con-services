using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Velociraptor.PDSInterface.Client.TAGProcessor;

namespace VSS.Raptor.Service.Common.Proxies
{
  /// <summary>
  /// Production Data Server client implementation
  /// </summary>
  public class TagProcessor : ITagProcessor
  {
    private readonly ILogger log;

    public TagProcessor(ILoggerFactory logger)
    {
      log = logger.CreateLogger<TagProcessor>();
    }
    /// <summary>
    /// PDSClient implementation
    /// </summary>
    /// <returns></returns>
    public TAGProcessorClient ProjectDataServerTAGProcessorClient()
    {
      try
      {
        log.LogInformation("PDS server location  {0}", VLPDServiceLocations.__Global.VLPDSvcLocations().VLPDTAGProcServiceIPAddress);

        IPHostEntry host = Dns.GetHostEntry(VLPDServiceLocations.__Global.VLPDSvcLocations().VLPDTAGProcServiceIPAddress);
        if (host == null)
          throw new ArgumentNullException("Missing PDS TAG Processor IPAddress in configuration file");

        foreach (IPAddress address in host.AddressList)
        {
          if (address.AddressFamily == AddressFamily.InterNetwork)
          {
            TAGProcessorClient client = new TAGProcessorClient(address, (short)VLPDServiceLocations.__Global.VLPDSvcLocations().VLPDTAGProcServiceIPPort);
            return client;
          }
        }

        return null;
      }
      catch (System.Exception ex)
      {
        log.LogWarning("Exception creating PDS TAG processor client. {0}", ex.Message);
        return null;
      }
    }

  }
}
