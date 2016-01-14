using System.Net;

namespace LandfillService.AcceptanceTests.Helpers
{
    /// <summary>
    /// Represent an HTTP request response
    /// </summary>
    public class ServiceResponse
    {
        public WebHeaderCollection ResponseHeader { get; set; }
        public HttpStatusCode HttpCode { get; set; }
        public string ResponseBody { get; set; }
    }
}
