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
        log.LogInformation($"{nameof(ProjectDataServerTAGProcessorClient)}: PDS server location  {VLPDServiceLocations.__Global.VLPDSvcLocations().VLPDTAGProcServiceIPAddress}:{(short)VLPDServiceLocations.__Global.VLPDSvcLocations().VLPDTAGProcServiceIPPort}");

        var hostIdentifier = VLPDServiceLocations.__Global.VLPDSvcLocations().VLPDTAGProcServiceIPAddress;

        if (hostIdentifier.Length == 0)
        {
          throw new ArgumentNullException($"Missing PDS TAG Processor IPAddress in configuration file");
        }

        if (!IPAddress.TryParse(hostIdentifier, out var PDSAddress))
        {
          var host = Dns.GetHostEntry(VLPDServiceLocations.__Global.VLPDSvcLocations().VLPDTAGProcServiceIPAddress);
          log.LogInformation($"{nameof(ProjectDataServerTAGProcessorClient)}: Have got {host.AddressList.Length} addresses for PDS server");

          foreach (var address in host.AddressList)
          {
            log.LogInformation($"{nameof(ProjectDataServerTAGProcessorClient)}: Checking PDS Server address {address}");
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

        log.LogInformation($"{nameof(ProjectDataServerTAGProcessorClient)}: PDS Server address ok, creating TAG Processor Client with address {PDSAddress}");

        return new TAGProcessorClient(PDSAddress, (short)VLPDServiceLocations.__Global.VLPDSvcLocations().VLPDTAGProcServiceIPPort);
      }
      catch (Exception ex)
      {
        log.LogWarning(ex, $"{nameof(ProjectDataServerTAGProcessorClient)}: Exception creating PDS TAG processor client");
        return null;
      }
    }
  }
}
