namespace VSS.Productivity3D.Scheduler.WebAPI
{
  public class ScheduleConfig
  {
    public string Name { get; set; }
    public string JobId { get; set; }
    public bool IsEnabled { get; set; }
    public string CustomerUid { get; set; }

    public string[] Emails { get; set; }
    public string Schedule { get; set; }
  }
}
