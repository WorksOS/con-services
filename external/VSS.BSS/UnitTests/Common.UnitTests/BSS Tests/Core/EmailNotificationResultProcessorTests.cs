using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class EmailNotificationResultProcessorTests : UnitTestBase
  {

    [TestMethod]
    public void Process_WorkflowNoExceptions_EmailNotSent()
    {
      var config = new FakeBssConfig
      {
        MessageQueueFailedCountMaximum = 5,
        FromEmailAddress = "email@domain.com",
        ToEmailAddress = "email@domain.com"
      };
      var emailSvc = new FakeEmailSvc();

      var message = BSS.AHCreated.Build();
      var result = new WorkflowResult();
      result.ActivityResults.Add(new ActivityResult());

      var processor = new EmailNotificationResultProcessor(config, emailSvc);
      processor.Process(message, result);

      Assert.IsFalse(emailSvc.EmailWasSent, "Email was sent.");
    }

    [TestMethod]
    public void Process_ExceptionEncountered_FailedCountBelowMax_EmailSent()
    {
      var config = new FakeBssConfig
      {
        MessageQueueFailedCountMaximum = 5,
        FromEmailAddress = "email@domain.com",
        ToEmailAddress = "email@domain.com"
      };
      var emailSvc = new FakeEmailSvc();

      var message = BSS.AHCreated.Build();
      var result = new WorkflowResult();
      result.ActivityResults.Add(new ExceptionResult { Exception = new Exception("TEST") });

      var bssRecord = GetBssProvisionsingMsg();
      bssRecord.SequenceNumber = message.SequenceNumber;
      bssRecord.FailedCount = 3;
      Ctx.OpContext.BSSProvisioningMsg.AddObject(bssRecord);

      var processor = new EmailNotificationResultProcessor(config, emailSvc);
      processor.Process(message, result);

      Assert.IsFalse(emailSvc.EmailWasSent, "Email was sent.");
    }

    [TestMethod]
    public void Process_NotifyEncountered_FailedCountBelowMax_EmailSent()
    {
      var config = new FakeBssConfig
      {
        MessageQueueFailedCountMaximum = 5,
        FromEmailAddress = "email@domain.com",
        ToEmailAddress = "email@domain.com"
      };
      var emailSvc = new FakeEmailSvc();

      var message = BSS.AHCreated.Build();
      var result = new WorkflowResult();
      result.ActivityResults.Add(new NotifyResult { Exception = new Exception("TEST") });

      var bssRecord = GetBssProvisionsingMsg();
      bssRecord.SequenceNumber = message.SequenceNumber;
      bssRecord.FailedCount = 3;
      Ctx.OpContext.BSSProvisioningMsg.AddObject(bssRecord);

      var processor = new EmailNotificationResultProcessor(config, emailSvc);
      processor.Process(message, result);

      Assert.IsTrue(emailSvc.EmailWasSent, "Email was not sent.");
    }

    [TestMethod]
    public void Process_ExceptionEncountered_FailedCountEqualsMax_EmailsPopulated_EmailSent()
    {
      var config = new FakeBssConfig
      {
        MessageQueueFailedCountMaximum = 5,
        FromEmailAddress = "email@domain.com",
        ToEmailAddress = "email@domain.com"
      };
      var emailSvc = new FakeEmailSvc();

      var message = BSS.AHCreated.Build();
      var result = new WorkflowResult();
      result.ActivityResults.Add(new ExceptionResult { Exception = new Exception("TEST") });

      var bssRecord = GetBssProvisionsingMsg();
      bssRecord.SequenceNumber = message.SequenceNumber;
      bssRecord.FailedCount = 4;
      Ctx.OpContext.BSSProvisioningMsg.AddObject(bssRecord);

      var processor = new EmailNotificationResultProcessor(config, emailSvc);
      processor.Process(message, result);

      Assert.IsTrue(emailSvc.EmailWasSent, "Email was not sent.");
    }

    [TestMethod]
    public void Process_NotifyEncountered_FailedCountEqualsMax_EmailsNotPopulated_EmailNotSent()
    {
      var config = new FakeBssConfig
      {
        MessageQueueFailedCountMaximum = 5,
        FromEmailAddress = "email@domain.com",
        ToEmailAddress = "email@domain.com"
      };
      var emailSvc = new FakeEmailSvc();

      var message = BSS.AHCreated.Build();
      var result = new WorkflowResult();
      result.ActivityResults.Add(new NotifyResult { Exception = new Exception("TEST") });

      var bssRecord = GetBssProvisionsingMsg();
      bssRecord.SequenceNumber = message.SequenceNumber;
      bssRecord.FailedCount = 4;
      Ctx.OpContext.BSSProvisioningMsg.AddObject(bssRecord);

      var processor = new EmailNotificationResultProcessor(config, emailSvc);
      processor.Process(message, result);

      Assert.IsTrue(emailSvc.EmailWasSent, "Email was not sent.");
    }

    [TestMethod]
    public void Process_ExceptionEncountered_FailedCountEqualsMax_EmailsArePopulated_EmailSent()
    {
      var config = new FakeBssConfig
      {
        MessageQueueFailedCountMaximum = 5,
        FromEmailAddress = "email@domain.com",
        ToEmailAddress = "email@domain.com"
      };
      var emailSvc = new FakeEmailSvc();

      var message = BSS.AHCreated.Build();
      var result = new WorkflowResult();
      result.ActivityResults.Add(new ExceptionResult{Exception = new Exception("TEST")});

      var bssRecord = GetBssProvisionsingMsg();
      bssRecord.SequenceNumber = message.SequenceNumber;
      bssRecord.FailedCount = 4;
      Ctx.OpContext.BSSProvisioningMsg.AddObject(bssRecord);

      var processor = new EmailNotificationResultProcessor(config, emailSvc);
      processor.Process(message, result);

      Assert.IsTrue(emailSvc.EmailWasSent, "Email not sent.");
    }

    [TestMethod]
    public void Process_NotifyEncountered_FailedCountEqualsMax_EmailsArePopulated_EmailSent()
    {
      var config = new FakeBssConfig
      {
        MessageQueueFailedCountMaximum = 5,
        FromEmailAddress = "email@domain.com",
        ToEmailAddress = "email@domain.com"
      };
      var emailSvc = new FakeEmailSvc();

      var message = BSS.AHCreated.Build();
      var result = new WorkflowResult();
      result.ActivityResults.Add(new NotifyResult { Exception = new Exception("TEST") });

      var bssRecord = GetBssProvisionsingMsg();
      bssRecord.SequenceNumber = message.SequenceNumber;
      bssRecord.FailedCount = 4;
      Ctx.OpContext.BSSProvisioningMsg.AddObject(bssRecord);

      var processor = new EmailNotificationResultProcessor(config, emailSvc);
      processor.Process(message, result);

      Assert.IsTrue(emailSvc.EmailWasSent, "Email not sent.");
    }

    private BSSProvisioningMsg GetBssProvisionsingMsg()
    {
      return new BSSProvisioningMsg
      {
        BSSMessageType = "AccountHierarchy",
        FailedCount = 0,
      };
    }
  }

  public class FakeEmailSvc : IBssEmailService
  {
    public bool EmailWasSent { get; set; }

    public void Send(string fromAddress, string toAddress, string subject, string body)
    {
      EmailWasSent = true;
    }
  }

  public class FakeBssConfig : IBssConfiguration
  {
    public string ToEmailAddress { get; set; }
    public string FromEmailAddress { get; set; }
    public int MessageQueueFailedCountMaximum { get; set; }
  }
}
