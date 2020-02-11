using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors
{
  /// <summary>
  /// The executor which gets the project id of the project for the requested asset location and date time.
  /// </summary>
  public class ProjectAndAssetUidsExecutor : RequestExecutorContainer
  {

    ///  <summary>
    ///  There are 2 modes this may be called in:
    ///  a) Manual Import
    ///     a projectUid is provided, for which we determine if
    ///          an appropriate subscription is available:
    ///            whose endDate is after the project
    ///            either the projects customer has a man3d
    ///              or the asset (if provided) Customer havs a man3d AND the project is owned by the same customer
    ///              or the asset has a 3dPm sub
    ///          and the location is inside the project
    ///             the time of location is not limited to the project start/end time
    ///          PM (aka Civil) are not valid
    ///          Deleted projects are not valid
    ///     if a radioSerial/dtype is proovided and can be resolved, the assetUid will also be returned.
    /// 
    ///  b) Auto Import
    ///     a device and/or tccOrgId provided.
    ///     One of these must be resolveable and it's customer used to identify appropriate project.
    ///     A customers projects cannot overlap spatially at the same point-in-time
    ///                  therefore this should legitimately retrieve max of ONE match
    ///    
    ///     A standard (aka construction) project is only fair game if
    ///          an assetID is provided
    ///            and it as a 3dpm sub for the time
    ///            and location is within it
    ///     A landfill project is fair game
    ///          if a TCCOrgID is resolvable
    ///          and has a project at that time
    ///          with a landfill subscription
    ///          and location is within it
    ///     Similar for PM project which requires a PM sub
    ///     Deleted projects are not considered
    /// 
    ///  </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as GetProjectAndAssetUidsRequest;
      if (request == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          GetProjectAndAssetUidsResult.FormatResult(uniqueCode: TagFileAuth.Models.ContractExecutionStatesEnum.SerializationError));
      }

      var projectCustomerSubs = new List<Subscriptions>();

      var assetUid = string.Empty;
      var assetSubs = new List<Subscriptions>();
      string assetOwningCustomerUid = string.Empty;

      Project.Abstractions.Models.DatabaseModels.Project project = null;

      if (!string.IsNullOrEmpty(request.ProjectUid))
      {
        project = await dataRepository.LoadProject(request.ProjectUid);
        log.LogDebug($"{nameof(ProjectAndAssetUidsExecutor)}: Loaded project? {JsonConvert.SerializeObject(project)}");

        if (project != null)
        {
          if (project.IsDeleted)
          {
            return GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 43);
          }

          if (project.ProjectType == ProjectType.ProjectMonitoring)
          {
            return GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 44);
          }

          projectCustomerSubs =
            (await dataRepository.LoadManual3DCustomerBasedSubs(project.CustomerUID, DateTime.UtcNow));
          log.LogDebug($"{nameof(ProjectAndAssetUidsExecutor)}: Loaded ProjectCustomerSubs? {JsonConvert.SerializeObject(projectCustomerSubs)}");
        }
        else
        {
          return GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 38);
        }
      }

      // if a SNM device is provided, try to use it, over any EC520
      if (request.DeviceType != (int) DeviceTypeEnum.MANUALDEVICE && !string.IsNullOrEmpty(request.RadioSerial))
      {
        var assetResult = await ProjectUidHelper.GetSNMAsset(log, dataRepository, request.RadioSerial, request.DeviceType, 
          (string.IsNullOrEmpty(request.ProjectUid) || project?.ProjectType == ProjectType.Standard));

        assetUid = assetResult.assetUid;
        assetOwningCustomerUid = assetResult.assetOwningCustomerUid;
        assetSubs = assetResult.assetSubs;
      }

      // if a SNM device was not provided, or did not have any subs,
      //     then see if we can get a relevant sub from any EC520 device 
      if (assetSubs.Count == 0 && !string.IsNullOrEmpty(request.Ec520Serial))
      {
        var assetResult = await ProjectUidHelper.GetEMAsset(log, dataRepository, request.Ec520Serial, (int) DeviceTypeEnum.EC520,
          (string.IsNullOrEmpty(request.ProjectUid) || project?.ProjectType == ProjectType.Standard));

        assetUid = assetResult.assetUid;
        assetOwningCustomerUid = assetResult.assetOwningCustomerUid;
        assetSubs = assetResult.assetSubs;
      }

      if (!string.IsNullOrEmpty(request.ProjectUid))
      {
        return await HandleManualImport(request, project, projectCustomerSubs, assetUid, assetOwningCustomerUid,
          assetSubs);
      }

      return await HandleAutoImport(request, assetUid, assetOwningCustomerUid, assetSubs);
    }


    private async Task<GetProjectAndAssetUidsResult> HandleManualImport(GetProjectAndAssetUidsRequest request,
      Project.Abstractions.Models.DatabaseModels.Project project, List<Subscriptions> projectCustomerSubs, string assetUid, string assetOwningCustomerUid,
      List<Subscriptions> assetSubs)
    {
      // by this stage...
      //  got a project,
      //  note that for manual, any projectTimeRange is ok. sub endDate is significant for Landfill.
      // Can manually import tag files regardless if tag file time outside projectTime
      //   Must have current a)  Manual Sub for standard or b) Landfill/Civil (for those projects)
      //   i.e. Can only view and therefore manuallyImport a LandfillProject IF you have a current Landfill sub

      var intersectingProjects = await dataRepository.GetIntersectingProjects(project.CustomerUID, request.Latitude,
        request.Longitude, new int[] {(int) project.ProjectType}, null);
      log.LogDebug(
        $"{nameof(HandleManualImport)}: Projects which intersect with manually imported project {JsonConvert.SerializeObject(intersectingProjects)}");

      if (!intersectingProjects.Any())
      {
        return GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 41);
      }

      if (project.ProjectType == ProjectType.Standard)
      {
        if (!projectCustomerSubs.Any())
        {
          // see if we can use the asset assetCustomerSub Man3d
          if (!string.IsNullOrEmpty(assetUid))
          {
            // we need assetOwningCustomer to determine if assetCustomer == projectCustomer 
            var assetCustomerSubs =
              (await dataRepository.LoadManual3DCustomerBasedSubs(assetOwningCustomerUid, DateTime.UtcNow));
            log.LogDebug(
              $"{nameof(HandleManualImport)}: Loaded assetsCustomerSubs? {JsonConvert.SerializeObject(assetCustomerSubs)}");

            // if only a 3dpm sub avail then this will be tossed out if customers !=
            var mostSignificantServiceType =
              GetMostSignificantServiceType(assetUid, project.CustomerUID, projectCustomerSubs,
                assetCustomerSubs, assetSubs);
            log.LogDebug(
              $"{nameof(HandleManualImport)}: after GetMostSignificantServiceType(). mostSignificantServiceType: {mostSignificantServiceType}");

            if (mostSignificantServiceType == serviceTypeMappings.serviceTypes.Find(st => st.name == "Unknown").NGEnum)
            {
              return GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 39);
            }
          }
          else
          {
            return GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 40);
          }
        }

        //  got a valid subscription by here
        if (intersectingProjects.Select(p =>
            string.Compare(p.ProjectUID, project.ProjectUID, StringComparison.OrdinalIgnoreCase))
          .Any())
        {
          return GetProjectAndAssetUidsResult.FormatResult(project.ProjectUID, assetUid);
        }

        return GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 50);
      }


      if (project.ProjectType == ProjectType.LandFill)
      {
        if (intersectingProjects
          .Any(p =>
            (string.Compare(p.ProjectUID, project.ProjectUID, StringComparison.OrdinalIgnoreCase) == 0)
            && p.ServiceTypeID == (int) ServiceTypeEnum.Landfill
            && p.SubscriptionEndDate.HasValue
            && request.TimeOfPosition <= p.SubscriptionEndDate))
        {
          return GetProjectAndAssetUidsResult.FormatResult(project.ProjectUID, assetUid);
        }

        return GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 45);
      }

      // pm prevented from getting here, all types should be handled already
      throw new ServiceException(HttpStatusCode.BadRequest,
        GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 46));
    }


    private async Task<GetProjectAndAssetUidsResult> HandleAutoImport(GetProjectAndAssetUidsRequest request,
      string assetUid, string assetOwningCustomerUid, List<Subscriptions> assetSubs)
    {
      // must be able to identify one or other customer for a) tccOrgUid b) radioSerial
      string tccCustomerUid = null;
      if (!string.IsNullOrEmpty(request.TccOrgUid))
      {
        var customerTccOrg = await dataRepository.LoadCustomerByTccOrgId(request.TccOrgUid);
        log.LogDebug(
          $"{nameof(HandleAutoImport)}: Loaded CustomerByTccOrgId? {JsonConvert.SerializeObject(customerTccOrg)}");
        tccCustomerUid = customerTccOrg?.CustomerUID ?? string.Empty;
      }

      if (string.IsNullOrEmpty(tccCustomerUid) && string.IsNullOrEmpty(assetOwningCustomerUid))
      {
        return GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 47);
      }

      var potentialProjects = await GetPotentialProjects(assetOwningCustomerUid, assetSubs, tccCustomerUid, request);
      log.LogDebug(
        $"{nameof(HandleAutoImport)}: GotPotentialProjects: {JsonConvert.SerializeObject(potentialProjects)}");

      if (!potentialProjects.Any())
      {
        return GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 48);
      }

      if (potentialProjects.Count > 1)
      {
        return GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 49);
      }

      return GetProjectAndAssetUidsResult.FormatResult(potentialProjects[0].ProjectUID, assetUid);
    }

    private int GetMostSignificantServiceType(string assetUid, string projectCustomerUid,
      List<Subscriptions> projectCustomerSubs, List<Subscriptions> assetCustomerSubs, List<Subscriptions> assetSubs)
    {
      var serviceType = serviceTypeMappings.serviceTypes.Find(st => st.name == "Unknown").NGEnum;

      var subs = new List<Subscriptions>();
      if (projectCustomerSubs != null && projectCustomerSubs.Any()) subs.AddRange(projectCustomerSubs.Select(s => s));
      if (assetCustomerSubs != null && assetCustomerSubs.Any()) subs.AddRange(assetCustomerSubs.Select(s => s));
      if (assetSubs != null && assetSubs.Any()) subs.AddRange(assetSubs.Select(s => s));
      
      log.LogDebug($"{nameof(GetMostSignificantServiceType)}: assetUid: {assetUid} projectCustomer: {projectCustomerUid}, subs: {JsonConvert.SerializeObject(subs)})");

      if (subs.Any())
      {
        //Look for highest level machine subscription which is current
        foreach (var sub in subs)
        {
          // Manual3d is least significant
          if (sub.serviceTypeId == (int) ServiceTypeEnum.Manual3DProjectMonitoring)
          {
            if (serviceType != (int) ServiceTypeEnum.ThreeDProjectMonitoring)
            {
              log.LogDebug($"{nameof(GetMostSignificantServiceType)}: found Manual3DProjectMonitoring for assetUid {assetUid}");
              serviceType = (int) ServiceTypeEnum.Manual3DProjectMonitoring;
            }
          }

          // 3D PM is most significant
          // if 3D asset-based, the assets customer must be the same as the Projects customer 
          if (sub.serviceTypeId == (int) ServiceTypeEnum.ThreeDProjectMonitoring)
          {
            if (serviceType != (int) ServiceTypeEnum.ThreeDProjectMonitoring)
            {
              //Allow manual tag file import for customer who has the 3D subscription for the asset
              //and allow automatic tag file processing in all cases (can't tell customer for automatic)
              log.LogDebug($"{nameof(GetMostSignificantServiceType)}: found e3DProjectMonitoring for assetUid {assetUid} sub.customerUid {sub.customerUid}");
              if (string.IsNullOrEmpty(projectCustomerUid) || sub.customerUid == projectCustomerUid)
              {
                serviceType = (int) ServiceTypeEnum.ThreeDProjectMonitoring;
                break;
              }
            }
          }
        }
      }

      log.LogDebug($"{nameof(GetMostSignificantServiceType)}:for assetUid {assetUid}, returning serviceTypeNG {serviceType}.");
      return serviceType;
    }

    private async Task<List<Project.Abstractions.Models.DatabaseModels.Project>> GetPotentialProjects
    (string assetOwningCustomerUid, List<Subscriptions> assetSubs, string tccCustomerUid,
      GetProjectAndAssetUidsRequest request)
    {
      //  Look for projects with location inside their boundary
      //     and belonging to asset or tccOrgId customers
      //     where customer or project (depending on project type) has the correct sub.
      //
      //  Note: In VL multiple customers can have subscriptions
      //    for an asset but only the owner gets the tag file data.

      var potentialProjects = new List<Project.Abstractions.Models.DatabaseModels.Project>();

      //  standard 2d / 3d project 
      //    IGNORE any tccOrgID
      //    must have valid assetID, which must have a 3d sub.
      if (!string.IsNullOrEmpty(assetOwningCustomerUid) && assetSubs.Any())
      {
        potentialProjects.AddRange((await dataRepository.GetIntersectingProjects(assetOwningCustomerUid,
          request.Latitude, request.Longitude, new[] {(int) ProjectType.Standard}, request.TimeOfPosition)));
      }

      if (!string.IsNullOrEmpty(tccCustomerUid))
      {
        // ProjectMonitoring and Landfill projects
        //  MUST have a TCCOrgID
        //  project must have a PM/LF sub 
        //  does not require an asset has been identified
        potentialProjects.AddRange(
          (await dataRepository.GetIntersectingProjects(tccCustomerUid, request.Latitude,
            request.Longitude, new[] {(int) ProjectType.ProjectMonitoring, (int) ProjectType.LandFill},
            request.TimeOfPosition))
          .Where(pm => (pm.ServiceTypeID == (int) ServiceTypeEnum.ProjectMonitoring
                        || pm.ServiceTypeID == (int) ServiceTypeEnum.Landfill)));
      }

      return potentialProjects;
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new System.NotImplementedException();
    }
  }
}
