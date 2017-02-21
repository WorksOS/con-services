
using VSS.Raptor.Service.Common.Contracts;

namespace VSS.Raptor.Service.WebApiModels.TagfileProcessing.ResultHandling
{
    /// <summary>
    /// REpresents response from the service after TAG file POST request
    /// </summary>
    public class TAGFilePostResult : ContractExecutionResult
    {
        /// <summary>
        /// Private constructor
        /// </summary>
        private TAGFilePostResult()
        {}

        /// <summary>
        /// Create instance of TAGFilePostResult
        /// </summary>
        public static TAGFilePostResult CreateTAGFilePostResult()
        {
          return new TAGFilePostResult();
        }

        /// <summary>
        /// TAGFile instance
        /// </summary>
        public static TAGFilePostResult HelpSample
        {
            get { return new TAGFilePostResult(); }
        }

    }
}