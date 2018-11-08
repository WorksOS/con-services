using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using LandfillService.Common.Contracts;

#pragma warning disable 1591
namespace LandfillService.Common
{
    public class InternalServerErrorResult : IHttpActionResult
    {
        public InternalServerErrorResult(
            string content, 
            Encoding encoding, 
            HttpRequestMessage request)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }

            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }

            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            this.Content = content;
            this.Encoding = encoding;
            this.Request = request;
        }

        public string Content { get; private set; }

        public Encoding Encoding { get; private set; }

        public HttpRequestMessage Request { get; private set; }

        public ContractExecutionResult ExecutionResult { get
        {
            return new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, this.Content);
        } }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        private HttpResponseMessage Execute()
        {
            var response = this.Request.CreateResponse(HttpStatusCode.InternalServerError);
            response.Content = new StringContent(JsonConvert.SerializeObject(this.ExecutionResult));
            return response;
        }
    }
}