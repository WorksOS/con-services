using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using log4net;
using TAGProcServiceDecls;
using VSS.Productivity3D.TagFileHarvester.Interfaces;
using VSS.Velociraptor.PDSInterface.Client.TAGProcessor;

namespace VSS.Productivity3D.TagFileHarvester.Implementation
{
  public class TagFileProcessingRaptor : ITAGProcessorClient
  {

    public static ILog Log;
    private static TAGProcessorClient client;
    private static readonly object lockObject = new object();

    /// <summary>
    /// PDSClient implementation
    /// </summary>
    /// <returns></returns>
    private static TAGProcessorClient ProjectDataServerTAGProcessorClient()
    {
      try
      {
        Log.Debug(String.Format("PDS server location  {0}", VLPDServiceLocations.__Global.VLPDSvcLocations().VLPDTAGProcServiceIPAddress));

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
        Log.Warn(String.Format("Exception creating PDS TAG processor client. {0}", ex.Message));
        return null;
      }
    }


    public TTAGProcServerProcessResult SubmitTAGFileToTAGFileProcessor(string orgId, string TagFilename, Stream File)
    {
      lock (lockObject)
      {
        if (client == null)
          client = ProjectDataServerTAGProcessorClient();
      }
      VLPDDecls.TWGS84FenceContainer NullBoundary = new VLPDDecls.TWGS84FenceContainer();
      NullBoundary.Init();
      return client.SubmitTAGFileToTAGFileProcessor(TagFilename, File, -1, 0, 0, NullBoundary, orgId);
    }
  }
}
