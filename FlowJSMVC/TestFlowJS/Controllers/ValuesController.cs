using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FlowUploadFilter;
using Microsoft.AspNetCore.Mvc;

namespace TestFlowJS.Controllers
{
    [Route("api/test")]
    public class ValuesController : Controller
    {

        [HttpGet]
        public ActionResult Upload()
        {
            return new NoContentResult();
        }


        [HttpPost]
        [ActionName("Upload")]
        [FlowUpload("exe")]
        public async Task<IActionResult> Index(FlowFile file)
        {
            return Json(new {derp = file.flowFilename});
        }

    }
}

