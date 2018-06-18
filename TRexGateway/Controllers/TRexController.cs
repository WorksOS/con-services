using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TRexGateway.Controllers
{
    [Produces("application/json")]
    [Route("api/TRex")]
    public class TRexController : Controller
    {

        [HttpGet]
        public string Get()
        {
            return "Hello from TRex";
        }
        
    }
}
