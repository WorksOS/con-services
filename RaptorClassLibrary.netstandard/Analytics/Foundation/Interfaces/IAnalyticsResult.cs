namespace RaptorClassLibrary.netstandard.Analytics.Foundation.Interfaces
{
    public interface IAnalyticsResult
    {
        void PopulateFromClusterComputeResponse(object response);
    }
}
