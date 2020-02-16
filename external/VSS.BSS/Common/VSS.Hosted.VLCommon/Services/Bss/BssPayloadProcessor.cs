using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.Configuration;
using System.ServiceModel;
using VSS.Hosted.VLCommon;
using System.ServiceModel.Channels;


namespace VSS.Hosted.VLCommon.Bss
{
  public class BssPayloadProcessor
  {
    protected const string CREATED = "CREATED";
    protected const string UPDATED = "UPDATED";
    protected const string DELETED = "DELETED";
    protected const string ACTIVATED = "ACTIVATED";
    protected const string CANCELLED = "CANCELLED";
    protected const string RESEND = "RESEND";
    protected const string successResult = "Success";
    protected const string failureResult = "Failure";
    protected static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);

    protected void AddBSSMessageToTable(INH_OP ctx, string ipAddress, string requestEndpoint, string globalControlNumber, DateTime actionUTC, DateTime processedUTC, string requestXML, string resultXML)
    {
      try
      {
        BSSMessages newBSSMessage = new BSSMessages
        {
          InsertUTC = DateTime.UtcNow,
          SenderIP = ipAddress,
          RequestEndPoint = requestEndpoint,
          ActionUTC = actionUTC,
          RequestXML = requestXML,
          ProcessedUTC = processedUTC,
          ResponseXML = resultXML,
          MachineName = Environment.MachineName
        };
        newBSSMessage.GlobalControlNumber = globalControlNumber;
        ctx.BSSMessages.AddObject(newBSSMessage);
        ctx.SaveChanges();
      }
      catch (Exception e)
      {
        log.IfErrorFormat(e, "Could not add record to BssMessage Table for Endpoint {0}, IPAddress: {1}", requestEndpoint, ipAddress);
      }
    }

    protected bool CheckIPForBSSProd(out string ipAddress, out string requestEndpoint)
    {
      ipAddress = string.Empty;
      requestEndpoint = string.Empty;
      bool isBSSProduction = false;

      if (OperationContext.Current != null)
      {
        log.IfDebugFormat("Found a non-null OperationContext with IncomingMessageProperties {0}", OperationContext.Current.IncomingMessageProperties.ToString());
        if (OperationContext.Current.IncomingMessageHeaders != null && OperationContext.Current.IncomingMessageHeaders.To != null)
        {
          requestEndpoint = OperationContext.Current.IncomingMessageHeaders.To.OriginalString.Split('/').Last<string>();
        }

        if (OperationContext.Current.IncomingMessageProperties != null)
        {
          if (OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] != null)
          {
            log.IfDebugFormat("Finding clientEndpoing for MessageProperty Name {0}", RemoteEndpointMessageProperty.Name);
            RemoteEndpointMessageProperty clientEndpoint = OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
            if (clientEndpoint != null)
            {
              ipAddress = clientEndpoint.Address;
            }
            log.IfInfoFormat("Received Request From {0}", ipAddress);
          }
        }

        isBSSProduction = AddressIsBSSProduction(ipAddress, ConfigurationManager.AppSettings["ProductionBss"]);
      }
      return isBSSProduction;
    }

    private bool AddressIsBSSProduction(string ipAddress, string productionBss)
    {
      bool isBSSProduction = false;

      if (!string.IsNullOrEmpty(ipAddress))
      {
        if (productionBss.Contains(ipAddress))
        {
          log.IfInfo("IP Address is BSS Production");
          isBSSProduction = true;
        }
        else
        {
          log.IfInfo("Full IP Address does not match BSS Production checking first 3 octets only");
          if (productionBss.Contains(ipAddress.Substring(0, ipAddress.LastIndexOf('.'))))
          {
            log.IfInfo("IP Address is BSS Production");
            isBSSProduction = true;
          }
          else
          {
            isBSSProduction = false;
            log.IfInfo("Request received from non production server");
          }
        }
      }
      return isBSSProduction;
    }
  }
}
