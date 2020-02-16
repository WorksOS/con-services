namespace VSS.Hosted.VLCommon.Bss
{
  public class NotificationActivity : IActivity
  {
    private readonly ResultType _resultType;
    private readonly string _summary;

    public NotificationActivity(string summary) : this(ResultType.Debug, summary){}
    public NotificationActivity(ResultType resultType, string summary)
    {
      _resultType = resultType;
      _summary = summary;
    }

    public ActivityResult Execute(Inputs inpus)
    {
      switch (_resultType)
      {
        case ResultType.Information:
          return new ActivityResult {Summary = _summary};
        case ResultType.Warning:
          return new WarningResult {Summary = _summary};
      }

      return new DebugResult {Summary = _summary};
    }
  }
}