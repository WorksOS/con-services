using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Swashbuckle.Swagger.Model;

namespace ProjectWebApi.Models
{
  public class ServiceException : HttpResponseException
  {
    /// <summary>
    ///   ServiceException class constructor.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    public ServiceException(HttpStatusCode code, string message)
      : base(new HttpResponseMessage(code))
    {
      Response.Content = new StringContent(message);
    }

  }
}
