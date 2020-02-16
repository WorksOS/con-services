using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Threading;
using log4net;

using VSS.Nighthawk.NHBssSvc.BSSResponseWS;

using VSS.Hosted.VLCommon;
using processRequest = VSS.Nighthawk.NHBssSvc.BSSResponseWS.processRequest;
using processResponse = VSS.Nighthawk.NHBssSvc.BSSResponseWS.processResponse;

namespace VSS.Nighthawk.NHBssSvc
{
  public class NHBSSResponseProcessor
  {
    private static Timer processTimer;
    private static readonly TimeSpan processingDelay = BssSvcSettings.Default.BSSMessageProcessingDelay;
    private static readonly TimeSpan stuckMessageTimeout = BssSvcSettings.Default.BssFeedbackRunTimeout;
    //construct the failed count and corresponding time delays dictionary
    //the delays are (failedcount: delayseconds): 1:15, 2:30, 3:60, 4:120, 5:240
    private static readonly Dictionary<int, double> retryDelays =
        (from delay in BssSvcSettings.Default.BSSMessageRetryDelay.Split('|')
         let keyvalue = delay.Split(',')
         select new
         {
           key = Convert.ToInt32(keyvalue[0]),
           value = Convert.ToDouble(keyvalue[1])
         }).ToDictionary(t => t.key, t => t.value);

    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);

    #region Service Start/Stop
    /// <summary>
    /// Starts the hosting service for the BSS service.
    /// </summary>
    internal static void Start()
    {
      processTimer = new Timer(new TimerCallback(SendToBss));
      processTimer.Change(processingDelay, TimeSpan.FromSeconds(30));
    }

    /// <summary>
    /// Stops the hosting service.
    /// </summary>
    internal static void Stop()
    {
      if (processTimer != null)
      {
        processTimer.Change(Timeout.Infinite, Timeout.Infinite);
      }
    }

    #endregion

    #region Implementation

    private static void SendToBss(object sender)
    {
      try
      {
        bool doProcessing = true;
        using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
        {
          while (doProcessing)
          {
            bool failure = false;

            BSSResponseMsg message = GetNextMessageToSend(ctx);
            //get next message to send if you can't get one 
            if (message != null)
            {
              try
              {
                //try to send message to destination url
                if (message.fk_BSSResponseEndPointID > 0)
                {
                  BSSResponseEndPoint responseEndpoint = message.BSSResponseEndPoint;
                  //do not attempt to send a response if DestinationURI is not specified
                  if (string.IsNullOrEmpty(responseEndpoint.DestinationURI))
                  {
                    log.IfDebugFormat("No destination URI specified for endpoint {0}. Not Sending message for Sequence Number {0}", message.BSSResponseEndPoint.SenderIP, message.fk_BSSProvisioningMsgID);
                  }
                  else
                  {
                    log.IfInfoFormat("Sending Response message for Sequence Number {0} to {1}", message.fk_BSSProvisioningMsgID, responseEndpoint.DestinationURI);                    
                    SendMessage(message, responseEndpoint.DestinationURI, responseEndpoint.UserName, responseEndpoint.Password);
                  }
                }
                else
                {
                  log.IfWarnFormat("Not Sending message for Sequence Number {0} because of empty destination", message.fk_BSSProvisioningMsgID);
                }
              }
              catch (Exception e)
              {
                failure = true;
                log.IfErrorFormat(e, "Failed to Send Response Message for Sequence Number {0} to {1}", message.fk_BSSProvisioningMsgID, message.BSSResponseEndPoint == null ? string.Empty : message.BSSResponseEndPoint.DestinationURI);
              }

              UpdateBssResponseMsg(ctx, message, failure);
            }
            else
            {
              doProcessing = false;
            }
          }
        }
      }
      catch (Exception e)
      {
        log.IfError("Error retrieving message from BSSResponseMsg Table", e);
      }
      finally
      {
        if (processTimer != null)
          processTimer.Change(processingDelay, TimeSpan.FromMilliseconds(-1));
      }
    }

    private static void UpdateBssResponseMsg(INH_OP ctx, BSSResponseMsg message, bool failedToSend)
    {
      byte? failed = null;
      BSSStatusEnum status = BSSStatusEnum.Complete;
      if (failedToSend)
      {
        
        status = BSSStatusEnum.RetryPending;
        failed = (byte)(message.FailedCount + 1);
        if (failed == BssSvcSettings.Default.MaxFailures)
        {
          status = BSSStatusEnum.Failed;
        }
      }
      API.BssResponseMsg.Update(ctx, message, status, DateTime.UtcNow, failed);
    }

    private static void SendMessage(BSSResponseMsg message, string destination, string userName, string password)
    {
      DateTime begin = DateTime.Now;

      log.IfDebugFormat("Sending BSS Response: {0}", message.ResponseXML);

      var schemaResponse = VSS.Hosted.VLCommon.Bss.Schema.V2.Response.ReadXML<VSS.Hosted.VLCommon.Bss.Schema.V2.Response>(message.ResponseXML);

      bpelVisionLinkResponseProcess sender = SendBSSResponse(destination, userName, password, null, null);

      ResponseType bssResponse = new ResponseType
      {
        TargetStack = schemaResponse.TargetStack,
        SequenceNumber = schemaResponse.SequenceNumber.ToString(),
        ControlNumber = schemaResponse.ControlNumber,
        EndPointName = message.BSSProvisioningMsg.BSSMessageType,
        ProcessedUTC = schemaResponse.ProcessedUTC,
        //Oracle BSS endpoint expects the value of Success to be in capital letters, 
        //otherwise the message will be considered as incorrect message
        Success = schemaResponse.Success.ToUpper(),
        ErrorCode = string.IsNullOrWhiteSpace(schemaResponse.ErrorCode) ? string.Empty : schemaResponse.ErrorCode,
        ErrorDescription = string.IsNullOrWhiteSpace(schemaResponse.ErrorDescription) ? string.Empty : schemaResponse.ErrorDescription,
      };

      processResponse response = sender.process(new processRequest(bssResponse));
      log.IfInfoFormat("Process Response = {0}", response.bpelVisionLinkResponseProcessProcessResponse.result);
      log.IfDebugFormat("BSS Response: took {0} ms to send", (DateTime.Now - begin).Milliseconds);
    }

    private static BSSResponseMsg GetNextMessageToSend(INH_OP ctx)
    {
      log.IfInfo("Checking for new ResponseMessage to Process");
      try
      {
        DateTime currentTime = DateTime.UtcNow.Subtract(BssSvcSettings.Default.BSSMessageProcessingDelay);
        int maxFailures = BssSvcSettings.Default.MaxFailures;
        DateTime stuckMsgTimeout = DateTime.UtcNow.Add(stuckMessageTimeout);

        var message = API.BssResponseMsg.GetNextMessageToSend(ctx, currentTime, stuckMsgTimeout, retryDelays, maxFailures);

        if (message != null)
        {
          //if retrieved a row update the row to in progress
          log.IfInfoFormat("Found new ResponseMessage Row SequenceNumber: {0} updating Status To InProgress", message.fk_BSSProvisioningMsgID);

          //Status = pending set to inProgress
          API.BssResponseMsg.Update(ctx, message, BSSStatusEnum.InProgress, DateTime.UtcNow);
        }
        return message;
      }
      catch (OptimisticConcurrencyException o)
      {
        log.IfWarnFormat(o, "Tried to update a row that has already been updated will not process message");
        return null;
      }
    }

    #endregion

    #region bpel

    /// <summary>
    /// Method that sets up the channel to send the info to the url that is in the service provider table
    /// HTTP uses WebChannelFactory with default setting and https needs to have a webhttpbinding with basic authentication
    /// </summary>
    internal static bpelVisionLinkResponseProcess SendBSSResponse(string uri, string username, string password, string sslSubject, string sslThumbprint)
    {
      try
      {
        ChannelFactory<bpelVisionLinkResponseProcess> cf = null;
        bpelVisionLinkResponseProcess proxy = null;

        if (uri.Contains("https:"))
        {
          BasicHttpBinding binding = new BasicHttpBinding();
          binding.Security.Mode = BasicHttpSecurityMode.Transport;
          binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
          EndpointAddress address = new EndpointAddress(uri);

          cf = new ChannelFactory<bpelVisionLinkResponseProcess>(binding, address);
          cf.Endpoint.Behaviors.Add(new CustomHttpEndpointBehavior(new CustomHttpMessageInspector { Password = password, UserName = username }));

          cf.Credentials.UserName.UserName = username;
          cf.Credentials.UserName.Password = password;

          subjectName = sslSubject;
          thumbprint = sslThumbprint;
          ServicePointManager.ServerCertificateValidationCallback += CustomXertificateValidation;

          cf.Open();

          proxy = cf.CreateChannel(new EndpointAddress(uri));

        }
        else
        {
          cf = new ChannelFactory<bpelVisionLinkResponseProcess>(new BasicHttpBinding(), new EndpointAddress(uri));
          proxy = cf.CreateChannel();
        }

        return proxy;
      }
      catch (Exception e)
      {
        log.IfError(string.Format("Could not Create Channel for URI {0}", uri), e);
      }

      return null;
    }

    /// <summary>
    /// this method checks to see if the certificate that is bound to the port is a trusted cert if not it will check the cert that is in the service Provider table for the task
    /// to validate the cert
    /// </summary>
    private static bool CustomXertificateValidation(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
    {
      //if the chainstatus length is 0 it is assumed that the cert is trusted
      if (chain.ChainStatus.Length != 0 && !string.IsNullOrEmpty(subjectName) && !string.IsNullOrEmpty(thumbprint))
      {
        X509Certificate2 certificate = (X509Certificate2)cert;
        if (certificate.Subject.ToUpper().Contains(string.Format("CN={0}", subjectName.ToUpper()))
          && !string.IsNullOrEmpty(certificate.Thumbprint)
          && certificate.Thumbprint.ToUpper() == thumbprint.ToUpper())
        {
          return true;
        }

        return false;
      }
      else if (chain.ChainStatus.Length != 0 && (string.IsNullOrEmpty(subjectName) || string.IsNullOrEmpty(thumbprint)))
      {
#if DEBUG
        return true;
#else
        return false;
#endif
      }

      return true;
    }

    private static string thumbprint;
    private static string subjectName;

    #endregion
  }
}
