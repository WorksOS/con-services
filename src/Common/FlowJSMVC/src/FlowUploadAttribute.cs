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
      var flowJs = new FlowJsRepo();
      var request = filterContext.HttpContext.Request;
      var validationRules = new FlowValidationRules();
      validationRules.AcceptedExtensions.AddRange(Extensions);
      validationRules.MaxFileSize = Size;
      var status = flowJs.PostChunk(request, Path.GetTempPath(), validationRules);

      if (status.Status == PostChunkStatus.Done)
      {
        var parameterDescriptor = filterContext.ActionDescriptor
                                               .Parameters
                                               .FirstOrDefault(x => x.ParameterType == typeof(FlowFile));

        filterContext.ActionArguments[parameterDescriptor.Name] = new FlowFile
        {
          flowFilename = status.FileName,
          path = Path.Combine(Path.GetTempPath(), status.FileName)
        };

        return;
      }

      if (status.Status == PostChunkStatus.Error)
        //TODO: Figure out how we can return the flow errors to the client
        filterContext.Result = new BadRequestResult();
      else
        filterContext.Result = new AcceptedResult();

      base.OnActionExecuting(filterContext);
    }
  }
}
