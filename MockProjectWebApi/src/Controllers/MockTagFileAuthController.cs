using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MockProjectWebApi.Common;
using MockProjectWebApi.Utils;
using Newtonsoft.Json;

// Mocking the Tagfile Auth Service
// Uses json files to provide a dynamic response for testing

namespace MockProjectWebApi.Controllers
{

  // Mock GetProjectID
  public class MockTagFileAuthController : Controller
  {


    [Route("api/v1/project/getId")]
    [HttpPost]
    public async Task<GetProjectIdResult> GetProjectId([FromBody] GetProjectIdRequest request)
    {
      request.Validate();
      TagFileUtilsHelper.Init();
      // Return values are stored in Json files e.g. GetProjectId.json
      return TagFileUtilsHelper.LookupProjectId(request.assetId, request.tccOrgUid);
    }


    [Route("api/v1/asset/getId")]
    [HttpPost]
    public async Task<GetAssetIdResult> GetAssetId([FromBody]GetAssetIdRequest request)
    {
      request.Validate();
      TagFileUtilsHelper.Init();
      return TagFileUtilsHelper.LookupAssetId(request.projectId, request.radioSerial);
    }



    [Route("api/v1/project/getBoundary")]
    [HttpPost]
    public async Task<GetProjectBoundaryAtDateResult> PostProjectBoundary([FromBody]GetProjectBoundaryAtDateRequest request)
    {
      request.Validate();
      return TagFileUtilsHelper.LookupBoundary(request.projectId);
    }

   
    [Route("api/v1/project/getBoundaries")]
    [HttpPost]
    public async Task<GetProjectBoundariesAtDateResult> PostProjectBoundaries([FromBody]GetProjectBoundariesAtDateRequest request)
    {
      request.Validate();
      return TagFileUtilsHelper.LookupBoundaries(request.assetId);
    }



    [Route("api/v2/notification/tagFileProcessingError")]
    [HttpPost]
    public async Task<TagFileProcessingErrorResult> PostTagFileProcessingError([FromBody] TagFileProcessingErrorV2Request request)
    {
      TagFileUtilsHelper.Init();
      request.Validate();
      return TagFileUtilsHelper.ReportError();
    }


    /// <summary>
    /// Posts the application alarm.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns></returns>
    [Route("api/v1/notification/appAlarm")]
    [HttpPost]
    public ContractExecutionResult PostAppAlarm([FromBody] AppAlarmMessage request)
    {
      request.Validate();
      return new ContractExecutionResult();
    }

  

  }








  /*



      public class TagFileProcessingErrorV1Request
      {

        /// <summary>
        /// The id of the asset whose tag file has the error. 
        /// </summary>
        [JsonProperty(PropertyName = "assetId", Required = Required.Always)]
        public long assetId { get; set; }

        /// <summary>
        /// The name of the tag file with the error.
        /// </summary>
        [JsonProperty(PropertyName = "tagFileName", Required = Required.Always)]
        public string tagFileName { get; set; } = String.Empty;

        [JsonProperty(PropertyName = "error", Required = Required.Always)]
        public TagFileErrorsEnum error { get; set; }

        /// <summary>
        /// Private constructor
        /// </summary>
        private TagFileProcessingErrorV1Request()
        {
        }

        /// <summary>
        /// Create instance of TagFileProcessingErrorRequest
        /// </summary>
        public static TagFileProcessingErrorV1Request CreateTagFileProcessingErrorRequest(long assetId, string tagFileName,
          int error)
        {
          return new TagFileProcessingErrorV1Request
                 {
                   assetId = assetId,
                   tagFileName = tagFileName,
                   error = (TagFileErrorsEnum)Enum.ToObject(typeof(TagFileErrorsEnum), error)
                 };
        }


        /// <summary>
        /// Validates all properties
        /// </summary>
        public void Validate()
        {
          if (assetId <= 0)
          {
          //  throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
           //   TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            //    ResultHandling.ContractExecutionStatesEnum.ValidationError, 9));
          }

          if (string.IsNullOrEmpty(tagFileName))
          {
            throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
              TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
                ContractExecutionStatesEnum.ValidationError, 5));
          }

          if (Enum.IsDefined(typeof(TagFileErrorsEnum), error) == false)
          {
            throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
              TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
                ContractExecutionStatesEnum.ValidationError, 4));
          }
        }
      }





      [Route("api/v1/notification/tagFileProcessingError")]
      [HttpPost]
      public TagFileProcessingErrorResult PostTagFileProcessingError([FromBody] TagFileProcessingErrorV1Request request)
      {
     //   log.LogDebug("PostTagFileProcessingErrorV1: request:{0}", JsonConvert.SerializeObject(request));
    //    request.Validate();

       // var result = RequestExecutorContainer.Build<TagFileProcessingErrorV1Executor>(log, configStore, assetRepository, deviceRepository, customerRepository, projectRepository, subscriptionsRepository)
         //   .Process(request) as TagFileProcessingErrorResult;

        ///log.LogDebug("PostTagFileProcessingErrorV2: result:{0}", JsonConvert.SerializeObject(result));
        return TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(true, 0, 0, "", "");
      }



    */









}
