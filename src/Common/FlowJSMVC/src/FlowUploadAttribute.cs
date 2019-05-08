using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace VSS.FlowJSHandler
{
        public class FlowUploadAttribute : ActionFilterAttribute
        {

            public FlowUploadAttribute(params string[] extensions)
            {
                Extensions = extensions;
                Size = 5000000;
            }
            public int Size { get; set; }
            public string[] Extensions { get; set; }
            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {
                var request = filterContext.HttpContext.Request;
                FlowJsPostChunkResponse status = null;
                //For reference surface files there is nothing to upload
                if (request.Query.ContainsKey("importedFileType"))
                {
                  var fileType = request.Query["importedFileType"];
                  if (fileType == "ReferenceSurface")
                  {
                    if (request.Query.ContainsKey("filename"))
                    {
                      var filename = request.Query["filename"];
                      status = new FlowJsPostChunkResponse { FileName = filename, Status = PostChunkStatus.Done };
                    }
                    else
                    {
                      status = new FlowJsPostChunkResponse { Status = PostChunkStatus.Error, ErrorMessages = new List<string>{"Missing filename for reference surface"}};
                    }
                  }
                  else
                  {
                    var flowJs = new FlowJsRepo();
                    var validationRules = new FlowValidationRules();
                    validationRules.AcceptedExtensions.AddRange(Extensions);
                    validationRules.MaxFileSize = Size;
                    status = flowJs.PostChunk(request, Path.GetTempPath(), validationRules);
                  }
                }

                if (status.Status == PostChunkStatus.Done)
                {
                    var filepath = Path.Combine(Path.GetTempPath(), status.FileName);
                    var p = filterContext.ActionDescriptor.Parameters
                        .FirstOrDefault(x => x.ParameterType == typeof (FlowFile));

                    if (filepath != null)
                    {
                        filterContext.ActionArguments[p.Name] = new FlowFile
                        {
                            flowFilename = status.FileName,
                            path = filepath
                        };
                        return;
                    }
                }

                filterContext.Result = new AcceptedResult();

                base.OnActionExecuting(filterContext);
            }
        }
}
