namespace CommonModel.Error
{
	public interface IErrorInfo
    {
        string Message { get; set; }
        int ErrorCode { get; set; }
        bool IsInvalid { get; set; }
	}
}