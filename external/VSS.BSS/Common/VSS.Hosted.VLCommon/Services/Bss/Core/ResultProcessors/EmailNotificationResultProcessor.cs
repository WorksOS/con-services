using log4net;
using System;
using System.Linq;

namespace VSS.Hosted.VLCommon.Bss
{
  public class EmailNotificationResultProcessor : IWorkflowResultProcessor
  {
    private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    private readonly IBssConfiguration _config;
    private readonly IBssEmailService _emailService;

    public EmailNotificationResultProcessor() : this(new BssConfiguration(), new BssEmailService()){ }
    public EmailNotificationResultProcessor(IBssConfiguration config, IBssEmailService emailService)
    {
      _config = config;
      _emailService = emailService;
    }

    public void Process<TMessage>(TMessage sourceMessage, WorkflowResult workflowResult)
    {
      if (!EmailNeedsToBeSent(workflowResult))
        return;

      if (workflowResult.ActivityResults.Any(x => x.Type == ResultType.Exception))
        if (!MessageHasReachedFailedCountMaximum(sourceMessage))
          return;

      Log.Debug("Sending email notification of BSS processing result.");

      if (_config.ToEmailAddress.IsNotDefined() || _config.FromEmailAddress.IsNotDefined())
      {
        Log.Warn("Email Notification can not be sent. Either to or from address is missing.");
        return;
      }

      const string subject = "BSS Message Processing Issue";

      string body = workflowResult.ActivityResults.ToText(sourceMessage);

      // Send email to Support with complete log;
      _emailService.Send(_config.FromEmailAddress, _config.ToEmailAddress, subject, body);

    }

    private bool EmailNeedsToBeSent(WorkflowResult workflowResult)
    {
      return workflowResult.ActivityResults.Any(x =>
        x.Type == ResultType.Notify ||
        x.Type == ResultType.Exception);
    }

    private bool MessageHasReachedFailedCountMaximum(object sourceMessage)
    {
      long sequenceNumber;
      if (!long.TryParse(sourceMessage.PropertyValueByName("SequenceNumber").ToString(), out sequenceNumber))
        throw new InvalidOperationException("Unable to get ");

      int failedCount;
      using (var ctx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        failedCount = ctx.BSSProvisioningMsgReadOnly
          .Where(x => x.SequenceNumber == sequenceNumber)
          .Select(x => x.FailedCount).First();
      }

      Log.DebugFormat("Current failure count: {0} vs. Max failure count: {1}", failedCount + 1, _config.MessageQueueFailedCountMaximum);
      return failedCount + 1 >= _config.MessageQueueFailedCountMaximum;
    }
  }
}
