using System;
using System.Collections.Generic;
using VSS.Hosted.VLCommon;
namespace VSS.Hosted.VLCommon
{
  public interface IEmailAPI
  {
    void AddToQueue(string emailFrom, string emailTo, string emailSubject, string emailBody, bool isSMS, bool isOrbComm, string origin,EmailPriorityEnum emailPriority = EmailPriorityEnum.Alert, long? alertIncidentId = null);
    void AddToQueue(INH_OP ctx, EmailQueue emItem, EmailPriorityEnum emailPriority = EmailPriorityEnum.Alert);
    List<EmailQueue> GetEmailDetailsForEmailID(string emailID, DateTime fromDate, DateTime toDate);
  }
}



