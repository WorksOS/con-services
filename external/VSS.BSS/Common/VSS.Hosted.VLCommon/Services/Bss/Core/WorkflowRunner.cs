using log4net;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Transactions;
using VSS.Hosted.VLCommon.Events;

namespace VSS.Hosted.VLCommon.Bss
{
  public class WorkflowRunner : IWorkflowRunner
  {
    private readonly IServiceBus _serviceBus;
    private readonly bool _enablePublishingToServiceBus;
    private static TimeSpan? _transactionTimeoutValue;
    private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    protected WorkflowResult WorkflowResult = new WorkflowResult();
    public Inputs Inputs { get; protected set; }

    public WorkflowRunner()
    {
    }

    public WorkflowRunner(IServiceBus serviceBus, bool enablePublishingToServiceBus)
    {
      _serviceBus = serviceBus;
      _enablePublishingToServiceBus = enablePublishingToServiceBus;
    }

    public WorkflowResult Run(IWorkflow workflow)
    {
      
      if(workflow.ActivitySequences.Count == 0)
      {
        WorkflowResult.Summary = CoreConstants.WORKFLOW_HAS_NO_ACTIVITY_SEQUENCES;
        return WorkflowResult;
      }
      
      Inputs = workflow.Inputs;

      using (Data.Context)
      {
        ExecuteActivitySequences(workflow.ActivitySequences);
      }

      WorkflowResult.Summary = CoreConstants.WORKFLOW_COMPLETED_SUCCESSFULLY;
      return WorkflowResult;
    }

    protected virtual void ExecuteActivitySequences(IList<IActivitySequence> activitySequences)
    {
      for (int i = 0; i < activitySequences.Count; i++)
      {

        if (activitySequences[i] is TransactionStart)
        {

        /*
         * A TransactionStart ActivitySequence intructs 
         * the runner to execute the following sequences
         * inside of a transaction until either it finds an
         * TransactionCommit ActivitySequence or all
         * ActivitySequences have been executed.
         */

          bool success = ExecuteActivitySequencesInTransaction(activitySequences, ref i);
          if (!success) return; // ErrorResult or ExceptionResult encountered

        }
        else
        {

          bool success = ExecuteActivities(activitySequences[i].Activities);
          if (!success) return; // ErrorResult or ExceptionResult encountered

        }
      }

      //publish the message to the service bus
      if (_enablePublishingToServiceBus)
      {
        try
        {
          Inputs.Get<EventMessageSequence>().ForEach(message => _serviceBus.Publish(message, message.GetType()));
        }
        catch (Exception ex)
        {
          Log.IfErrorFormat(ex, "Error publishing to service bus.");
        }

        Log.IfInfoFormat("Published {0} bss messages to service bus", Inputs.Get<EventMessageSequence>().Count);
      }
    }

    protected virtual bool ExecuteActivitySequencesInTransaction(IList<IActivitySequence> activitySequences, ref int i)
    {
      using (var transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted, Timeout = GetTransactionTimeoutValue() }))
      {
        while (i < activitySequences.Count)
        {
          
          if (activitySequences[i] is TransactionCommit)
          {

          /*
           * An ActivitySequence of type TransactionCommit
           * instructs the runner to Commit the Activities
           * that have been executed so far and exits the
           * TransactionScope.
           */

            break;
          }

          bool success = ExecuteActivities(activitySequences[i].Activities);
          if (!success)
          {

            /*
             * The activity execution has encountered an ErrorResult or 
             * ExceptionResult and will roll back the transaction.
             */

            WorkflowResult.ActivityResults.Add(new DebugResult { Summary = CoreConstants.TRANSACTION_ROLLED_BACK });
            return false;
          }

          i++;

        } // end of while

        /*
         * The runner has either run a TransactionCommit ActivitySequence
         * or has come to the end of the ActivitySequences without
         * an encountering an ErrorResult or ExceptionResult.
         */

        WorkflowResult.ActivityResults.Add(new DebugResult { Summary = CoreConstants.TRANSACTION_COMMITED });
        transaction.Complete();

      } // end of using - transaction

      return true;
    }

    protected virtual bool ExecuteActivities(IEnumerable<IActivity> activities)
    {
      foreach (var activity in activities)
      {

        try
        {
          var result = activity.Execute(Inputs);
          WorkflowResult.ActivityResults.Add(result);

          if (result is ErrorResult || result is ExceptionResult)
            return false;
        }
        catch (Exception ex)
        {
          var exceptionResult = new ExceptionResult { Exception = ex };
          WorkflowResult.ActivityResults.Add(exceptionResult);
          return false;
        }

      }

      return true;
    }

    public static void ResetTimeoutValue()
    {
      _transactionTimeoutValue = null;
      GetTransactionTimeoutValue();
    }

    public static TimeSpan GetTransactionTimeoutValue()
    {
      if (!_transactionTimeoutValue.HasValue)
      {
        TimeSpan timeout;
        string timeoutConfig = ConfigurationManager.AppSettings["BSSTransactionTimeout"];
        if (TimeSpan.TryParse(timeoutConfig, out timeout))
        {
          _transactionTimeoutValue = timeout;
          Log.IfInfoFormat("WorkflowRunner.GetTransactionTimeoutValue: BSSTransactionTimeout is {0}.",
            _transactionTimeoutValue);
        }
        else
        {
          // default transaction timeout to five minutes
          _transactionTimeoutValue = new TimeSpan(0, 5, 0);
          Log.IfInfoFormat(
            "WorkflowRunner.GetTransactionTimeoutValue: Could not parse BSSTransactionTimeout. Value set to default of {0}.",
            _transactionTimeoutValue);
        }
      }

      Log.IfDebugFormat("WorkflowRunner.GetTransactionTimeoutValue: Returning {0}", _transactionTimeoutValue.Value);
      return _transactionTimeoutValue.Value;
    }
  }
}