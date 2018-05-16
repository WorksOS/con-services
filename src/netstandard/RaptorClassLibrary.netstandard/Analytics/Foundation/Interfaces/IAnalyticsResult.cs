namespace VSS.TRex.Analytics.Foundation.Interfaces
{
    public interface IAnalyticsResult<TResponse>
    {
        void PopulateFromClusterComputeResponse(TResponse response);
    }
}
