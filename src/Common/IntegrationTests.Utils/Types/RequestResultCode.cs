namespace CCSS.IntegrationTests.Utils.Types
{
  public enum RequestResultCode
  {
    ExecutedSuccessfully = 0,
    IncorrectRequestedData = -1,
    ValidationError = -2,
    InternalProcessingError = -3,
    FailedToGetResults = -4
  }
}
