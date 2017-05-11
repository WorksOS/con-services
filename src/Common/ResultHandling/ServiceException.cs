using System;
using System.Net;
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Contracts;
using System.Net.Http;

namespace VSS.Raptor.Service.Common.ResultHandling
{
    /// <summary>
    ///   This is an expected exception and should be ignored by unit test failure methods.
    /// </summary>
    public class ServiceException : Exception
    {
        /// <summary>
        ///   ServiceException class constructor.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="result"></param>
        public ServiceException(HttpStatusCode code, ContractExecutionResult result)
            : base()
        {
            GetResult = result;
            GetContent = JsonConvert.SerializeObject(result);
            Code = code;
        }

        /// <summary>
        /// 
        /// </summary>
        public string GetContent { get; private set; }

        /// <summary>
        /// The result causing the exception
        /// </summary>
        public ContractExecutionResult GetResult { get; private set; }

        /// <summary>
        /// Gets the code.
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
        public HttpStatusCode Code { get; set; }

    }
}
