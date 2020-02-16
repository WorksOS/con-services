﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon
{
  internal class EmailAPI:IEmailAPI
  {
    public void AddToQueue(string emailFrom, string emailTo, string emailSubject, string emailBody, bool isSMS, bool isOrbComm, string origin,EmailPriorityEnum emailPriority = EmailPriorityEnum.Alert , long? alertIncidentId = null)
    {
      EmailQueue emItem = new EmailQueue();
      emItem.MailFrom = emailFrom ?? "";
      emItem.MailTo = emailTo ?? "";
      emItem.EmailSubject = emailSubject ?? "";
      emItem.EmailBody = emailBody ?? "";
      emItem.IsSMS = isSMS;
      emItem.IsOrbComm = isOrbComm;
      emItem.EmailOrigin = origin ?? "";
      emItem.fk_EmailPriorityID = (int)emailPriority;
      DateTime now = DateTime.UtcNow;
      emItem.InsertUTC = now;
      emItem.UpdateUTC = now;
      emItem.ifk_AlertIncidentID = alertIncidentId;

      using (INH_OP opCtxWriteable = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        opCtxWriteable.EmailQueue.AddObject(emItem);
        int result = opCtxWriteable.SaveChanges();
      }

    }

    public void AddToQueue(INH_OP ctx, EmailQueue emItem, EmailPriorityEnum emailPriority = EmailPriorityEnum.Alert)
    {
      emItem.fk_EmailPriorityID = (int)emailPriority; 
      ctx.EmailQueue.AddObject(emItem);
      //Dont make SaveChanges here
    }

    public List<EmailQueue> GetEmailDetailsForEmailID(string emailID, DateTime fromDate, DateTime toDate)
    {
      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        List<EmailQueue> result = (from a in opCtx.EmailQueue                                 
                                 where a.MailTo == emailID && 
                                 a.InsertUTC >= fromDate && a.InsertUTC <= toDate                                 
                                 select a).ToList();

        return result;
      }     
    }
  }
}
