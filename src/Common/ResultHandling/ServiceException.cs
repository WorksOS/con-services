using System;
using System.Net;
using Newtonsoft.Json;
using VSS.Productivity3D.Common.Contracts;

namespace VSS.Productivity3D.Common.ResultHandling
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
        {
            GetResult = result;
            GetContent = JsonConvert.SerializeObject(result);
            Code = code;
        }

        /// <summary>
        /// 
        /// </summary>
        public string GetContent { get; }

        /// <summary>
        /// The result causing the exception
        /// </summary>
        public ContractExecutionResult GetResult { get; }

        /// <summary>
        /// Gets the code.
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
        public HttpStatusCode Code { get; set; }
    }
}