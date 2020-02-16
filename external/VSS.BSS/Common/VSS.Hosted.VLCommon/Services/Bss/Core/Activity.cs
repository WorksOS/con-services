using MassTransit;
using System;
using System.Collections.Generic;
using VSS.Hosted.VLCommon.Events;

namespace VSS.Hosted.VLCommon.Bss
{
  public abstract class Activity : IActivity
  {
    private readonly IList<string> _summary = new List<string>();
    
    public abstract ActivityResult Execute(Inputs inputs);

    protected ActivityResult Cancelled(string summary = null, params object[] values)
    {
      if (!string.IsNullOrWhiteSpace(summary))
        AddSummary(string.Format("Cancelled: {0}", summary), values);
      
      return new ActivityResult { Summary = _summary.ToNewLineString() };
    }

    protected WarningResult Warning(string summary = null, params object[] values)
    {
      if (!string.IsNullOrWhiteSpace(summary))
        AddSummary(string.Format("Warning: {0}", summary), values);
      
      return new WarningResult{Summary = _summary.ToNewLineString()};
    }

    protected ErrorResult Error(string summary = null, params object[] values)
    {
      if (!string.IsNullOrWhiteSpace(summary))
        AddSummary(string.Format("Error: {0}", summary), values);
      
      return new ErrorResult { Summary = _summary.ToNewLineString()};
    }

    protected ExceptionResult Exception(Exception exception, string summary = null, params object[] values)
    {
      if (!string.IsNullOrWhiteSpace(summary))
        AddSummary(string.Format("Exception: {0}", summary), values);
      
      return new ExceptionResult { Summary = _summary.ToNewLineString(), Exception = exception };
    }

    protected NotifyResult Notify(Exception exception, string summary = null, params object[] values)
    {
      if (!string.IsNullOrWhiteSpace(summary))
        AddSummary(string.Format("Exception: {0}", summary), values);

      return new NotifyResult { Summary = _summary.ToNewLineString(), Exception = exception };
    }

    protected ActivityResult Success(string summary = null, params object[] values)
    {
      if(!string.IsNullOrWhiteSpace(summary))
        AddSummary(string.Format("Success: {0}", summary), values);
      
      return new ActivityResult {Summary = _summary.ToNewLineString()};
    }

    protected void AddWarning(string warning = null, params object[] values)
    {
      if (!string.IsNullOrWhiteSpace(warning))
        AddSummary(string.Format("Warning: {0}", warning), values);
    }

    protected void AddSummary(string summary = null, params object[] values)
    {
      if(summary == null)
        return;
      
      _summary.Add(string.Format(summary, values));
    }

    protected void AddEventMessage<T>(Inputs inputs, T message) where T : CorrelatedBy<Guid>
    {
      if (inputs.ContainsKey<EventMessageSequence>())
        inputs.Get<EventMessageSequence>().Add(message);
    }
  }
}
