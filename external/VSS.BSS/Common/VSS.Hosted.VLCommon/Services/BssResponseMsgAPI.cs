using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VSS.Hosted.VLCommon;
using log4net;

namespace VSS.Hosted.VLCommon
{
  internal class BssResponseMsgAPI : IBssResponseMsgAPI
  {
    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);
    public BSSResponseMsg Create(INH_OP ctx, long bssSequenceNumber, string responseXML, long responseEndpointID, string machineName, BSSStatusEnum status = BSSStatusEnum.Pending, DateTime? responseSentUTC = null)
    {
      BSSResponseMsg response = new BSSResponseMsg();
      response.fk_BSSStatusID = (int)status;
      response.fk_BSSProvisioningMsgID = bssSequenceNumber;
      response.FailedCount = 0;
      response.InsertUTC = DateTime.UtcNow;
      response.UpdateUTC = DateTime.UtcNow;
      response.MachineName = machineName;
      response.ResponseSentUTC = responseSentUTC;
      response.ResponseXML = responseXML;
      response.fk_BSSResponseEndPointID = responseEndpointID;
      response.UpdateUTC = DateTime.UtcNow;
      ctx.BSSResponseMsg.AddObject(response);
      ctx.SaveChanges();

      return response;
    }

    public bool Update(INH_OP ctx, BSSResponseMsg message, BSSStatusEnum status, DateTime sentUTC, byte? failedCount = null)
    {
      message.ResponseSentUTC = sentUTC;
      message.UpdateUTC = DateTime.UtcNow;
      message.fk_BSSStatusID = (int)status;
      if (failedCount != null)
      {
        message.FailedCount = failedCount.Value;
      }

      return ctx.SaveChanges() > 0;
    }

    public BSSResponseMsg GetNextMessageToSend(INH_OP ctx, DateTime currentTime, DateTime stuckMsgTimeout, Dictionary<int, double> retryDelays, int maxFailures)
    {
      int pending = (int)BSSStatusEnum.Pending;
      int retry = (int)BSSStatusEnum.RetryPending;
      int inProgress = (int)BSSStatusEnum.InProgress;

      log.DebugFormat("Checking for any response message currently in progress..");
      BSSResponseMsg msgInProgress = MessageCurrentlyInProgress(ctx, inProgress);

      //A message in progress?
      if (msgInProgress != null)
      {
        //Message not stuck? Then we do not need to process anything
        if (msgInProgress.UpdateUTC < stuckMsgTimeout)
        {
          log.DebugFormat("Response Message with ID {0} is 'In Progress' but is not stuck.", msgInProgress.ID);
          return null;
        }
        else   //Unstick it..
        {
          log.DebugFormat("Response Message with ID {0} is 'In Progress'and is stuck. Unsticking it..", msgInProgress.ID);
          //If processing has not failed, set it to 'pending', else set it to 'Retry Pending'
          if (msgInProgress.FailedCount == 0)
            msgInProgress.fk_BSSStatusID = pending;
          else
            msgInProgress.fk_BSSStatusID = retry;

          msgInProgress.UpdateUTC = DateTime.UtcNow;
          ctx.SaveChanges();
          return null;
        }
      }

      log.DebugFormat("Checking for response message to process..");
      //Get the first message that meets the following criteria
      //-Message is waiting to be Retried
      //-Message is Pending and has never been tried      
      BSSResponseMsg message = (from b in ctx.BSSResponseMsg.Include("BSSResponseEndpoint")
                                where b.InsertUTC <= currentTime
                                && ((b.FailedCount == 0 && b.fk_BSSStatusID == pending)
                                  || (b.ResponseSentUTC == null)
                                  || (b.fk_BSSStatusID == retry && b.FailedCount < maxFailures))
                                orderby b.ID
                                select b).FirstOrDefault();

      if (message != null)
      {
        //Return the message if it is 'Pending'
        if (message.fk_BSSStatusID == pending)
        {
          log.DebugFormat("Found response message with sequence number {0} in 'Pending' state", message.ID);
          return message;
        }

        //If the message is in 'ReTry' state, check to see if it has hit the max wait time limit (based off the failed count)
        //before returning it.
        if ((message.ResponseSentUTC.HasValue && message.ResponseSentUTC.Value.AddSeconds(retryDelays[message.FailedCount]) <= DateTime.UtcNow)
              && message.fk_BSSStatusID == retry)
        {
          log.DebugFormat("Found provisioning message with ID {0} in 'Retry-Pending' state and Failed Count = {1}", message.ID, message.FailedCount);
          return message;
        }
      }

      return null;
    }

    private BSSResponseMsg MessageCurrentlyInProgress(INH_OP ctx, int statusInProgress)
    {
      return (from b in ctx.BSSResponseMsg
              where b.fk_BSSStatusID == statusInProgress
              orderby b.ID
              select b).FirstOrDefault();
    }


    # region VLSupportTool

    public BSSResponseMsg GetBssResponseMsg(long sequenceNo)
    {
      BSSResponseMsg result = new BSSResponseMsg();
      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>(true))
      {
        result = (from resMsg in opCtx.BSSResponseMsgReadOnly
                  where resMsg.fk_BSSProvisioningMsgID == sequenceNo
                  select resMsg).FirstOrDefault();
      }

      return result;
    }

    # endregion 
  }
}
