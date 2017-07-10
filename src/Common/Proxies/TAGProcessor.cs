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
    /// <returns></returns>
    public TAGProcessorClient ProjectDataServerTAGProcessorClient()
    {
      IPAddress PDSAddress;
      try
      {
        log.LogInformation("PDS server location  {0}:{1}", VLPDServiceLocations.__Global.VLPDSvcLocations().VLPDTAGProcServiceIPAddress,
            (short)VLPDServiceLocations.__Global.VLPDSvcLocations().VLPDTAGProcServiceIPPort);

        string hostIdentifier = VLPDServiceLocations.__Global.VLPDSvcLocations().VLPDTAGProcServiceIPAddress;

        if (hostIdentifier.Length == 0) {
          throw new ArgumentNullException("Missing PDS TAG Processor IPAddress in configuration file");
        }


        if (!IPAddress.TryParse(hostIdentifier, out PDSAddress)) {
          IPHostEntry host = Dns.GetHostEntry(VLPDServiceLocations.__Global.VLPDSvcLocations().VLPDTAGProcServiceIPAddress);
          log.LogInformation("Have got {0} addresses for PDS server", host.AddressList.Length);

          foreach (IPAddress address in host.AddressList) {
            log.LogInformation("Checking PDS Server address {0}", address);
            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
              PDSAddress = address;
              break;
            }
          }

        }

        log.LogInformation("PDS Server address ok, creating TAG Processor Client with address {0}", PDSAddress);
        return new TAGProcessorClient(PDSAddress, (short)VLPDServiceLocations.__Global.VLPDSvcLocations().VLPDTAGProcServiceIPPort);

        //return null;
      }
      catch (System.Exception ex)
      {
        log.LogWarning("EXCEPTION {0}", ex.ToString());
        log.LogWarning("Exception creating PDS TAG processor client. {0}", ex.Message);
        return null;
      }
    }

  }
}
