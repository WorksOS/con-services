using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Velociraptor.PDSInterface.Client.TAGProcessor;

namespace VSS.Productivity3D.Common.Proxies
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
    public TAGProcessorClient ProjectDataServerTAGProcessorClient()
    {
      try
      {
        log.LogInformation("PDS server location  {0}:{1}", VLPDServiceLocations.__Global.VLPDSvcLocations().VLPDTAGProcServiceIPAddress,
            (short)VLPDServiceLocations.__Global.VLPDSvcLocations().VLPDTAGProcServiceIPPort);

        var hostIdentifier = VLPDServiceLocations.__Global.VLPDSvcLocations().VLPDTAGProcServiceIPAddress;

        if (hostIdentifier.Length == 0)
        {
          throw new ArgumentNullException("Missing PDS TAG Processor IPAddress in configuration file");
        }

        if (!IPAddress.TryParse(hostIdentifier, out var PDSAddress))
        {
          var host = Dns.GetHostEntry(VLPDServiceLocations.__Global.VLPDSvcLocations().VLPDTAGProcServiceIPAddress);
          log.LogInformation("Have got {0} addresses for PDS server", host.AddressList.Length);

          foreach (var address in host.AddressList)
          {
            log.LogInformation("Checking PDS Server address {0}", address);
            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
              PDSAddress = address;
              break;
            }
          }
        }

        if (PDSAddress == null)
        {
          throw new ArgumentException("Invalid PDS TAG Processor IPAddress in configuration file");
        }

        log.LogInformation("PDS Server address ok, creating TAG Processor Client with address {0}", PDSAddress);

        return new TAGProcessorClient(PDSAddress, (short)VLPDServiceLocations.__Global.VLPDSvcLocations().VLPDTAGProcServiceIPPort);
      }
      catch (Exception ex)
      {
        log.LogWarning(ex, "Exception creating PDS TAG processor client");
        return null;
      }
    }
  }
}
