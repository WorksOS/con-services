using Microsoft.AspNetCore.Mvc;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.Models;
using VSS.Productivity3D.WebApiModels.TagfileProcessing.Models;
using VSS.Productivity3D.WebApiModels.TagfileProcessing.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.TagfileProcessing.Contracts
{
    public interface ITagFileContract
    {
        TAGFilePostResult Post([FromBody]TagFileRequest request);
    }
}