using Microsoft.AspNetCore.Mvc;
using VSS.Raptor.Service.WebApiModels.TagfileProcessing.Models;
using VSS.Raptor.Service.WebApiModels.TagfileProcessing.ResultHandling;


namespace VSS.Raptor.Service.WebApiModels.TagfileProcessing.Contracts
{
    public interface ITagFileContract
    {
        TAGFilePostResult Post([FromBody]TagFileRequest request);
    }
}