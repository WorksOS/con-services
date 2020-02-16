using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.Web;
using System.Xml.Linq;
using log4net;
using Microsoft.ServiceModel.Web;
using System.Xml;

using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.Hosted.VLCommon;

namespace VSS.Nighthawk.NHBssSvc.BSSEndPoints
{
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
  public class NHBssEndPointSvc : INHBSSEndpointSvc
  {
    #region Variables
    
    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);
    private static NHWebHost<NHBssEndPointSvc> m_host;
    private static readonly string serviceBaseUri = ConfigurationManager.AppSettings["BSSServiceBaseUri"];

    private static readonly string bssAllowedStack = 
      string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["BSSAllowedStack"]) ? "US01" : ConfigurationManager.AppSettings["BSSAllowedStack"];

    #endregion

    #region Service Start/Stop

    public static void Start()
    {
      string[] uris = serviceBaseUri.Split(',');
      List<Uri> serviceUris = uris.Select(uri => new Uri(uri)).ToList();

      var requestInterceptors = new List<RequestInterceptor> { new XMLInterceptor() };

      m_host = new NHWebHost<NHBssEndPointSvc>();
      m_host.StartHTTPService(serviceUris, typeof(INHBSSEndpointSvc), new BssUserNameValidator(), requestInterceptors);
      log.IfInfo("NHBssEndpointSvc started");
    }

    public static void Stop()
    {
      if (m_host == null) return;

      m_host.StopService();
      log.IfInfo("NHBssEndPointSvc stopped");
    }

    #endregion

    #region V2 Endpoint Implementation

    public void AccountHierarchiesV2(AccountHierarchy ah)
    {
      string xml = AccountHierarchy.WriteXML(ah);
      log.IfInfoFormat("Received account hierarchy payload:\n{0}", xml);

      VerifyTargetStack(ah.TargetStack);

      SaveMessage(typeof(AccountHierarchy).Name, ah.SequenceNumber, xml);
    }

    public void InstallBasesV2(InstallBase installBasePayload)
    {
      string xml = InstallBase.WriteXML(installBasePayload);
      log.IfInfoFormat("Received install base payload:\n{0}", xml);

      VerifyTargetStack(installBasePayload.TargetStack);

      SaveMessage(typeof(InstallBase).Name, installBasePayload.SequenceNumber, xml);
    }

    public void ServicePlansV2(ServicePlan servicePlan)
    {
      string xml = ServicePlan.WriteXML(servicePlan);
      log.IfInfoFormat("Received service plan payload:\n{0}", xml);

      VerifyTargetStack(servicePlan.TargetStack);
      SaveMessage(typeof(ServicePlan).Name, servicePlan.SequenceNumber, xml);
    }

    public void DeviceReplacementV2(DeviceReplacement deviceReplacement)
    {
      string xml = DeviceReplacement.WriteXML(deviceReplacement);
      log.IfInfoFormat("Received device replacement payload:\n{0}", xml);
      
      VerifyTargetStack(deviceReplacement.TargetStack);

      SaveMessage(typeof(DeviceReplacement).Name, deviceReplacement.SequenceNumber, xml);
    }

    public void DeviceRegistrationV2(DeviceRegistration deviceRegistration)
    {
      string xml = DeviceRegistration.WriteXML(deviceRegistration);
      log.IfInfoFormat("Received device registrationk payload:\n{0}", xml);

      VerifyTargetStack(deviceRegistration.TargetStack);

      SaveMessage(typeof(DeviceRegistration).Name, deviceRegistration.SequenceNumber, xml);
    }

    public void WebServiceAvailability(XElement serviceAvailability)
    {
      log.IfInfoFormat("Received service availability payload:\n{0}", serviceAvailability.ToString());
    }

    public AssetIDChanges AssetIDChanges(string bookMarkUTC)
    {
        log.IfInfoFormat("Requested Book Mark time stamp : {0}", bookMarkUTC);
        return GetAssetIDs(bookMarkUTC);
    }

    #endregion

    #region Private methods

    private static string GetSenderIP()
    {
      string ipAddress = string.Empty;
      if (OperationContext.Current != null)
      {
        log.IfDebugFormat("Found a non-null OperationContext with IncomingMessageProperties {0}", OperationContext.Current.IncomingMessageProperties.ToString());
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
      }

      return ipAddress;
    }

    private static void SaveMessage(string messageType, long sequenceNumber, string xml)
    {
      try
      {
        using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
        {
          API.BssProvisioningMsg.Create(ctx, messageType, sequenceNumber, GetSenderIP(), xml);
        }
      }
      catch(UpdateException updateException)
      {
        if(updateException.InnerException != null && updateException.InnerException is SqlException)
        {
          var innerEx = (SqlException) updateException.InnerException;
          if(innerEx.ErrorCode == -2146232060)
          {
            string errorMessage = string.Format("Duplicate SequenceNumber: {0} could not be saved to the Database.", sequenceNumber);
            log.Error(errorMessage);
            throw new WebProtocolException(HttpStatusCode.BadRequest, errorMessage, innerEx);
          }
        }

        log.Error(updateException);
        throw;
      }
      catch (Exception e)
      {
        log.IfError("Could not store data received from BSS", e);
        throw e;
      }
    }

    private static void VerifyTargetStack(string targetStack)
    {
      if (string.Compare(targetStack, bssAllowedStack, true) != 0)
      {
        log.IfWarnFormat("Invalid targetStack received {0} in message returning internal server error", targetStack);

        throw new HttpException(500, "Invalid Target Stack");
      }
    }

    private static AssetIDChanges GetAssetIDs(string bookMarkUTC) 
    {        
        DateTime bookMarkDate = new DateTime();
        if (!bookMarkUTC.isDateTimeISO8601("yyyy-MM-ddTHH:mm:ssZ",out bookMarkDate)) 
        {          
          throw new WebProtocolException(HttpStatusCode.BadRequest, "Invalid Date format. Expected format: yyyy-MM-ddTHH:mm:ssZ", null);
        }
        if (bookMarkDate > DateTime.UtcNow)
        {
          throw new WebProtocolException(HttpStatusCode.BadRequest, "Invalid Date", null);
        }
        return AssetIDChangesAPI.GetAssetIDChanges(bookMarkDate);
    }

    #endregion

  }
}
