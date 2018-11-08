namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class ResponseBase : IResponseBase
  {
    public int Code { get; set; }
    public string Message { get; set; }

    public ResponseBase()
    { }

    protected ResponseBase(int code, string message = "")
    {
      Code = code;
      Message = message;
    }

    protected ResponseBase(string message)
        : this(RequestResultCode.ExecutedSuccessfully, message)
    { }
  }
}
