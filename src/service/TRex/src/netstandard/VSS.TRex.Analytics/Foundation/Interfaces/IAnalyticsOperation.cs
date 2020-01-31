using System.Threading.Tasks;

namespace VSS.TRex.Analytics.Foundation.Interfaces
{
    public interface IAnalyticsOperation<TArgument, TResult>
    {
        /// <summary>
        /// Execute the analytics operation with the supplied argument asynchronously.
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        Task<TResult> ExecuteAsync(TArgument arg);
    }
}
