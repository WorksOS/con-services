namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class RequestResultCode
  {
    public static readonly int ExecutedSuccessfully = 0;
    public static readonly int IncorrectRequestedData = -1;
    public static readonly int ValidationError = -2;
    public static readonly int InternalProcessingError = -3;
    public static readonly int FailedToGetResults = -4;
  }
}
