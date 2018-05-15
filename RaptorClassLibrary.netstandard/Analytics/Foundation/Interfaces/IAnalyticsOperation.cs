using VSS.TRex.Analytics.Models;

namespace VSS.TRex.Analytics.Foundation.Interfaces
{
    public interface IAnalyticsOperation<TArgument, TResult>
        where TResult : AnalyticsResult, new()
    {
        /// <summary>
        /// Execute the analytics operation with the supplied argument
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        TResult Execute(TArgument arg);
    }
}
