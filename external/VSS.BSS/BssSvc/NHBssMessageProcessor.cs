using EasyHttp.Http;
using log4net;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core;
using System.Linq;
using System.Threading;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.Hosted.VLCommon.Services.Interfaces;
using VSS.Nighthawk.NHBssSvc.DataAccess;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Implementations.Helpers;
using VssJsonMessageSerializer = VSS.Nighthawk.MassTransit.JsonMessageSerializer;

namespace VSS.Nighthawk.NHBssSvc
{
  /// <summary>
  /// The primary implementation of the BSS service Message Processing.
  /// </summary>
  public partial class NHBssMessageProcessor
  {
    private static Timer _processTimer;
    private static readonly TimeSpan ProcessingDelay = BssSvcSettings.Default.BSSMessageProcessingDelay;
    private static readonly TimeSpan StuckMessageTimeout = BssSvcSettings.Default.BssFeedbackRunTimeout;
    //construct the failed count and corresponding time delays dictionary
    //the delays are (failedcount: delayseconds): 1:15, 2:30, 3:60, 4:120, 5:240
    private static readonly Dictionary<int, double> RetryDelays =
        (from delay in BssSvcSettings.Default.BSSMessageRetryDelay.Split('|')
         let keyvalue = delay.Split(',')
         select new
         {
           key = Convert.ToInt32(keyvalue[0]),
           value = Convert.ToDouble(keyvalue[1])
         }).ToDictionary(t => t.key, t => t.value);
    
    private static readonly Processors Processor;
    private static readonly IBssReference AddBssReference;
    private static IServiceBus _serviceBus;
    private static bool _enablePublishingToServiceBus;

    private static long _endPointId;
    private static Dictionary<string, long> _sendToEndpoints; 
    
    private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    #region constructor

    static NHBssMessageProcessor()
    {
      VerifyServiceBusConfiguration();
      AddBssReference =
        new BssReference(
          new ReferenceLookupClient(
            new QueryHelper(new HttpClientWrapper(new HttpClient {ThrowExceptionOnHttpError = true}))),
          new ReferenceLookupClient(
            new QueryHelper(new HttpClientWrapper(new HttpClient { ThrowExceptionOnHttpError = true }))));
      Processor = new Processors();
    }

    #endregion constructor

    #region Service Start/Stop
    /// <summary>
    /// Starts the hosting service for the BSS service.
    /// </summary>
    internal static void Start()
    {
      DeviceConfig.ResetEnvironmentFlag();
      WorkflowRunner.ResetTimeoutValue();
      ServiceViewAPI.ResetServiceViewBlockSizeValue();
      _processTimer = new Timer(ProcessRow);
      _processTimer.Change(ProcessingDelay, TimeSpan.FromMilliseconds(-1));
    }

    /// <summary>
    /// Stops the hosting service.
    /// </summary>
    internal static void Stop()
    {
      if (_processTimer != null)
      {
        _processTimer.Change(Timeout.Infinite, Timeout.Infinite);
      }
    }

    #endregion

    #region Implementation

    private static void ProcessRow(object sender)
    {
      try
      {
        var doProcessing = true;
        while (doProcessing)
        {
          //get row if you can else reset timer
          var message = GetMessage();

          if (message != null)
          {
            //figure out which message type and send to correct processor
            //create new received message object
            //and fill object with received xml
            var response = Processor.Process(message.BSSMessageType, message.MessageXML);
            
            //update row based on Pass/Fail of processing

            UpdateRow(message, response);

            if (response == null)
            {
              doProcessing = false;
            }
          }
          else
          {
            doProcessing = false;
          }
        }
      }
      catch (Exception e)
      {
        Log.IfError("Error Processing BSS Message", e);
      }
      finally
      {
        if(_processTimer != null)
          _processTimer.Change(ProcessingDelay, TimeSpan.FromMilliseconds(-1));
      }
    }

    private static void UpdateRow(BSSProvisioningMsg bssMessage, Response response)
    {
      try
      {
        byte? failure = bssMessage.FailedCount;
        var status = BSSStatusEnum.Complete;

        if (response == null)
        {
          failure++;

          if (failure == BssSvcSettings.Default.MaxFailures)
          {
            status = BSSStatusEnum.Failed;
            Log.IfErrorFormat("Processing Failures has Reached limit for bss message Sequence Number {0} will not try again", bssMessage.SequenceNumber);
          }
          else
          {
            status = BSSStatusEnum.RetryPending;
            Log.WarnFormat("Processing Failed for bss message SequenceNumber {0} will try again", bssMessage.SequenceNumber);
          }
        }
        using (var ctx = ObjectContextFactory.NewNHContext<INH_OP>())
        {
          //I get the message again because by this point bssMessage is not connected to a context and I do not want the context hanging around while the actual processing is occuring
          var msg = (from b in ctx.BSSProvisioningMsg
                                    where b.SequenceNumber == bssMessage.SequenceNumber
                                    select b).SingleOrDefault();

          API.BssProvisioningMsg.Update(ctx, msg, status, DateTime.UtcNow, failure);

          //add a row to the response table if things completed successfully
          if (response != null)
            API.BssResponseMsg.Create(ctx, bssMessage.SequenceNumber, BssCommon.WriteXML(response), _endPointId, Environment.MachineName);
        }
      }
      catch (Exception e)
      {
        Log.IfErrorFormat(e, "Unexpected Error updating Provisioning Message SequenceNumber: {0} Request has been processed and this needs to be investigated", bssMessage.SequenceNumber);
      }

    }

    private static BSSProvisioningMsg GetMessage()
    {
      Log.IfInfo("Checking for new ProvisioningMessage to Process");

      try
      {
        using (var ctx = ObjectContextFactory.NewNHContext<INH_OP>())
        {
          BSSProvisioningMsg message = GetMessageFromTable(ctx);

          if (message != null)
          {
            //if retrieved a row update the row to in progress
            Log.IfInfoFormat("Found new ProvisioningMessage Row SequenceNumber: {0} updating Status To InProgress", message.SequenceNumber);

            //Check if the message is coming from an IP that is supported
            _endPointId = SupportedIP(ctx, message.SenderIP);

            if (_endPointId <= 0)
            {
              Log.IfInfoFormat("Will not process message SequenceNumber: {0} was received from invalid IP: {1}; setting status to Blocked", message.SequenceNumber, message.SenderIP);
              //set status to blocked
              API.BssProvisioningMsg.Update(ctx, message, BSSStatusEnum.Blocked, DateTime.UtcNow);
              return null;
            }
            //Status = pending set to inProgress
            if (API.BssProvisioningMsg.Update(ctx, message, BSSStatusEnum.InProgress, null, null, Environment.MachineName))
              return message;
            return null;
          }
        }
      }
      catch (OptimisticConcurrencyException o)
      {
        Log.IfWarnFormat(o, "Tried to update a row that has already been updated will not process message");
      }
      return null;
    }

    private static BSSProvisioningMsg GetMessageFromTable(INH_OP ctx)
    {
      var currentTime = DateTime.UtcNow.Subtract(ProcessingDelay);

      //This specifies the time we need to wait before we deem a message 'Stuck' in the 'In Progress' state
      var stuckMsgTimeout = DateTime.UtcNow.Add(StuckMessageTimeout);

      var maxFailures = BssSvcSettings.Default.MaxFailures;

      return API.BssProvisioningMsg.GetNextMessageToProcess(ctx, currentTime, stuckMsgTimeout, RetryDelays, maxFailures);
    }

    private static long SupportedIP(INH_OP ctx, string ipAddress)
    {
      var octet = ipAddress.Contains(".") ? ipAddress.Substring(0, ipAddress.LastIndexOf('.')) : ipAddress;
      if(_sendToEndpoints == null || _sendToEndpoints.Count == 0)
      {
        if(_sendToEndpoints == null)
          _sendToEndpoints = new Dictionary<string,long>();
        _sendToEndpoints = (from c in ctx.BSSResponseEndPointReadOnly
                           group c by c.SenderIP into g
                           select g).ToDictionary(f => f.Key, z =>
                           {
                             var bssResponseEndPoint = z.SingleOrDefault();
                             return bssResponseEndPoint != null ? bssResponseEndPoint.ID : 0;
                           });
      }
      if(_sendToEndpoints.ContainsKey(octet))
        return _sendToEndpoints[octet];

      return 0;
    }

    private static void VerifyServiceBusConfiguration()
    {
      _enablePublishingToServiceBus = !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["NHBssSvc.EnablePublishingToServiceBus"]) && Convert.ToBoolean(ConfigurationManager.AppSettings["NHBssSvc.EnablePublishingToServiceBus"]);

      Log.IfInfoFormat("EnablePublishingToServiceBus is {0}", _enablePublishingToServiceBus);

      if (!(_enablePublishingToServiceBus && (_serviceBus == null)))
        return;

      IConnectionConfig connectionConfig = new RabbitMqConnectionConfig();
      var connectionString = connectionConfig.ConnectionString();
      Log.Info("Connection string : " + connectionString);

      if (connectionString != string.Empty)
      {
        try
        {
          _serviceBus = ServiceBusFactory.New(sbc =>
          {
            sbc.UseRabbitMq(
              cnfg => cnfg.ConfigureHost(new Uri(connectionString),
                configurator =>
                {
                  configurator.SetUsername(
                    connectionConfig.GetUserName());
                  configurator.SetPassword(
                    connectionConfig.GetPassword());
                  configurator.SetRequestedHeartbeat(connectionConfig.GetHeartbeatSeconds());
                }));
            sbc.ReceiveFrom(connectionString);
            sbc.Validate();
            sbc.SetDefaultSerializer<VssJsonMessageSerializer>();
          });
        }
        catch (Exception ex)
        {
          Log.IfErrorFormat(ex, "Service bus creation error. Service bus uri is {0}", connectionString);
          _enablePublishingToServiceBus = false;
        }
      }
      else
      {
        Log.IfErrorFormat("ServiceBusURI is blank.");
        _enablePublishingToServiceBus = false;
      }
    }

    #endregion

    private class Processors
    {
      private static readonly BssMessageProcessor processor = new BssMessageProcessor(_serviceBus, _enablePublishingToServiceBus, AddBssReference);

      public Response Process(string messageType, string xml)
      {
        Response response = null;
        try
        {
          if (!string.IsNullOrEmpty(xml))
          {

            switch (messageType)
            {
              case "AccountHierarchy":
                response = processor.Process(BssCommon.ReadXML<AccountHierarchy>(xml));
                break;
              case "InstallBase":
                response = processor.Process(BssCommon.ReadXML<InstallBase>(xml));
                break;
              case "ServicePlan":
                response = processor.Process(BssCommon.ReadXML<ServicePlan>(xml));
                break;
              case "DeviceReplacement":
                response = processor.Process(BssCommon.ReadXML<DeviceReplacement>(xml));
                break;
              case "DeviceRegistration":
                response = processor.Process(BssCommon.ReadXML<DeviceRegistration>(xml));
                break;
              default:
                Log.IfErrorFormat("Invalid Message Type");
                throw new InvalidOperationException("Invalid Message Type");
            }
          }
        }
        catch (Exception e)
        {
          Log.IfError("Unexpected Error Processing Message", e);
          response = null;
        }
        return response;
      }
    }
  }

}
