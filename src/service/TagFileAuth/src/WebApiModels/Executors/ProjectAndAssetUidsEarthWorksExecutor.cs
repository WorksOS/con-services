﻿using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors
{
  /// <summary>
  /// The executor which tries to identify a project for the location,
  ///      for use by CTCT EarthWorks devices to obtain cutfill map from 3dp.
  /// The customer, for which projects are fair game, can be determined from
  ///     1) SNM serialNumber
  ///     2) EC520 serialNumber
  ///     3) tccOrgId  
  /// The commercial model re servicePlans has not been established,
  ///      it MAY be that if an asset found but has no service plan,
  ///                      then only surveyedSurface ground is provided (no productionData)
  ///                      else production data AND SS is provided
  ///      don't know what it would be for landfills and civil project using a TCCOrgId
  /// </summary>
  public class ProjectAndAssetUidsEarthWorksExecutor : RequestExecutorContainer
  {
    ///  <summary>
    ///  Processes the get project Uid request and finds the Uid of the project corresponding to the given location and devices Customer and relavant subscriptions.
    ///  </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as GetProjectAndAssetUidsEarthWorksRequest;
      if (request == null)
        throw new ServiceException(HttpStatusCode.BadRequest,
          GetProjectAndAssetUidsEarthWorksResult.FormatResult(uniqueCode: TagFileAuth.Models.ContractExecutionStatesEnum.SerializationError));
      
      var assetUid = string.Empty;
      var assetSubs = new List<Subscriptions>();
      string assetOwningCustomerUid = string.Empty;

      // if a SNM device is provided, try to use it, over any EC520
      // get the owningCustomer of the SNM
      if (!string.IsNullOrEmpty(request.RadioSerial))
      {
        var assetResult = await ProjectUidHelper.GetSNMAsset(log, dataRepository, request.RadioSerial, (int)DeviceTypeEnum.SNM940, true);
        if (!string.IsNullOrEmpty(assetResult.assetUid))
        {
          assetUid = assetResult.assetUid;
          assetOwningCustomerUid = assetResult.assetOwningCustomerUid;
          assetSubs = assetResult.assetSubs;
        }
      }

      // if a SNM device was not provided, or did not have any subs,
      //     then see if we can get a relevant sub from any EC520 device 
      // Note: will always have a EC520 serial
      if (assetSubs.Count == 0 && !string.IsNullOrEmpty(request.Ec520Serial))
      {
        var assetResult = await ProjectUidHelper.GetEMAsset(log, dataRepository, request.Ec520Serial, (int) DeviceTypeEnum.EC520, true);
        if (!string.IsNullOrEmpty(assetResult.assetUid))
        {
          assetUid = assetResult.assetUid;
          assetOwningCustomerUid = assetResult.assetOwningCustomerUid;
          assetSubs = assetResult.assetSubs;
        }

        // Unable to identify the EC in the 3dPM system, and no tccOrgId provided
        if (string.IsNullOrEmpty(request.TccOrgUid) && string.IsNullOrEmpty(assetOwningCustomerUid))
          return GetProjectAndAssetUidsEarthWorksResult.FormatResult(uniqueCode: 33);
      }

      return await LocateProjectsInProximity(request, assetUid, assetOwningCustomerUid, assetSubs);
    }

    /// <summary>
    /// EarthWorks cut/fill doesn't necessarily REQUIRE a subscription, or of the type required for 3dp tagFiles
    /// Must be able to identify one or other customer for a) serialNumber b) EM520 c) tccOrgUid
    /// </summary>
    private async Task<GetProjectAndAssetUidsEarthWorksResult> LocateProjectsInProximity(GetProjectAndAssetUidsEarthWorksRequest request,
      string assetUid, string assetOwningCustomerUid, List<Subscriptions> assetSubs)
    {
      string tccCustomerUid = null;
      if (!string.IsNullOrEmpty(request.TccOrgUid))
      {
        var customerTccOrg = await dataRepository.LoadCustomerByTccOrgId(request.TccOrgUid);
        log.LogDebug($"{nameof(LocateProjectsInProximity)}: Loaded CustomerByTccOrgId? {JsonConvert.SerializeObject(customerTccOrg)}");
        tccCustomerUid = customerTccOrg?.CustomerUID ?? string.Empty;
      }

      if (string.IsNullOrEmpty(tccCustomerUid) && string.IsNullOrEmpty(assetOwningCustomerUid))
        return GetProjectAndAssetUidsEarthWorksResult.FormatResult(assetUid: assetUid, uniqueCode: 47);

      var potentialProjects = await GetPotentialProjects(assetOwningCustomerUid, tccCustomerUid, request);
      log.LogDebug($"{nameof(LocateProjectsInProximity)}: GotPotentialProjects: {JsonConvert.SerializeObject(potentialProjects)}");

      if (!potentialProjects.Any())
      {
        // with EarthWorks we allow assetCustomerUid LF/PM sub for landfill/pm projects
        if (string.IsNullOrEmpty(tccCustomerUid) && !string.IsNullOrEmpty(assetOwningCustomerUid) && !assetSubs.Any())
          return GetProjectAndAssetUidsEarthWorksResult.FormatResult(assetUid: assetUid, customerUid: assetOwningCustomerUid, uniqueCode: 52);

        return GetProjectAndAssetUidsEarthWorksResult.FormatResult(assetUid: assetUid, customerUid: assetOwningCustomerUid, uniqueCode: 48);
      }

      if (potentialProjects.Count > 1)
        return GetProjectAndAssetUidsEarthWorksResult.FormatResult(assetUid: assetUid, customerUid: potentialProjects[0].CustomerUID, hasValidSub: true, uniqueCode: 49);

      return GetProjectAndAssetUidsEarthWorksResult.FormatResult(
        potentialProjects[0].ProjectUID, assetUid,
        potentialProjects[0].CustomerUID,
        ((potentialProjects[0].ProjectType == ProjectType.Standard && assetSubs.Any())
          || (potentialProjects[0].ProjectType != ProjectType.Standard && !string.IsNullOrEmpty(potentialProjects[0].SubscriptionUID))));
    }

    /// <summary>
    /// Look for projects with location inside their boundary
    ///     and belonging to asset or tccOrgId customers
    /// EarthWorks cutfill doesn't necessarily need a traditional tagfile sub
    /// </summary>
    private async Task<List<Project.Abstractions.Models.DatabaseModels.Project>> GetPotentialProjects
      (string assetOwningCustomerUid, string tccCustomerUid, GetProjectAndAssetUidsEarthWorksRequest request)
    {
      var potentialProjects = new List<Project.Abstractions.Models.DatabaseModels.Project>();

      if (!string.IsNullOrEmpty(assetOwningCustomerUid) ) 
        potentialProjects.AddRange((await dataRepository.GetIntersectingProjects(assetOwningCustomerUid,
          request.Latitude, request.Longitude, new[] { (int)ProjectType.Standard }, request.TimeOfPosition)));

      if (!string.IsNullOrEmpty(assetOwningCustomerUid))
        potentialProjects.AddRange((await dataRepository.GetIntersectingProjects(assetOwningCustomerUid,
          request.Latitude, request.Longitude, new[] { (int)ProjectType.ProjectMonitoring, (int)ProjectType.LandFill },
          request.TimeOfPosition)));


      // in case asset owner and tccOrg resolve to same customer, don't add project twice
      if (!string.IsNullOrEmpty(tccCustomerUid))
      {
        var projects = await dataRepository.GetIntersectingProjects(tccCustomerUid, request.Latitude,
            request.Longitude, new[] {(int) ProjectType.ProjectMonitoring, (int) ProjectType.LandFill},
            request.TimeOfPosition);
        foreach (var project in projects)
        {
          if (!potentialProjects.Any(p => p.ProjectUID == project.ProjectUID))
            potentialProjects.Add(project);
        }
      }

      return potentialProjects;
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new System.NotImplementedException();
    }
  }
}
