﻿using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Repositories.DBModels;
using VSS.MasterData.Repositories.ExtendedModels;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using ContractExecutionStatesEnum = VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling.ContractExecutionStatesEnum;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors
{
  /// <summary>
  /// The executor which gets the project id of the project for the requested asset location and date time.
  /// </summary>
  public class ProjectAndAssetUidsExecutor : RequestExecutorContainer
  {

    ///  <summary>
    ///  Processes the request
    ///     and finds the project corresponding to the given location
    ///            and devices Customer and relavant subscriptions.
    ///
    ///   A device asset reports at a certain location, at a point in time -
    ///         which project should its data be accumulating into?
    ///     assumption1: A customers projects cannot overlap spatially at the same point-in-time
    ///                  this applies to construction and Landfill types
    ///                  therefore this should legitimately retrieve max of ONE match
    ///     assumption2: tag files are data type-generic at this level, so this function does not need to
    ///                  differentiate between the 3 subscription types.
    ///     assumption3: the customer must be identifiable by EITHER the AssetID, or TCCOrgID being supplied
    ///                  only projects for that customer are fair game.
    ///    
    ///     A construction project is only fair game if an assetID is provided
    ///     A landfill project is fair game for an aasetID or a TCCOrgID
    /// 
    ///     determine the union (ONE) of the following:
    ///     1) which projects were valid at this time?
    ///     2) which customers have a machineControl-type subscription for at this time? (for construction type projects)
    ///         a) for the asset provided OR
    ///         b) any assets if -1 is provided
    ///     3) which project.sites are these points are in?
    ///
    ///   note that since we don't return serviceTypes to Trex, we can just use NGen ones internally.
    ///  </summary>
    ///  <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a GetProjectAndAssetUidsResult if successful</returns>      
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as GetProjectAndAssetUidsRequest;
      if (request == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          GetProjectAndAssetUidsResult.CreateGetProjectAndAssetUidsResult("", "",
            ContractExecutionStatesEnum.SerializationError));
      }

      var projectUid = string.Empty;
      var projectOwningCustomer = string.Empty;
      var projectCustomerSubs = new List<Subscriptions>();

      var assetUid = string.Empty;
      var assetSubs = new List<Subscriptions>();
      string assetOwningCustomerUid = string.Empty;

      string tccCustomerUid = string.Empty;

      var isJohnDoeAsset = false;

      Project project = null;

      // presence of projectUid indicates a manual Import
      //     can exist with or without a radioSerial
      if (!string.IsNullOrEmpty(request.ProjectUid))
      {
        project = await dataRepository.LoadProject(request.ProjectUid);
        log.LogDebug($"ProjectAndAssetUidsExecutor: Loaded project? {JsonConvert.SerializeObject(project)}");

        if (project != null)
        {
          projectUid = project.ProjectUID;
          projectOwningCustomer = project.CustomerUID;
          projectCustomerSubs =
            (await dataRepository.LoadManual3DCustomerBasedSubs(projectOwningCustomer, DateTime.UtcNow)).ToList();
          log.LogDebug(
            $"ProjectAndAssetUidsExecutor: Loaded ProjectCustomerSubs? {JsonConvert.SerializeObject(projectCustomerSubs)}");
        }
        else
        {
          return GetProjectAndAssetUidsResult.CreateGetProjectAndAssetUidsResult(projectUid, "", 38);
        }
      }

      // setup AssetUid to return. Assemble ServicePlans for assetCustomer and/or TCCOrgID customer

      // For a manual import (projectUid provided),
      //    A manual device type (or where no asset-type parameters are available), only serviceType will be available.
      //    So if there are any projectCustomer subs, then set serviceType to manual
      if (request.DeviceType == (int) DeviceTypeEnum.MANUALDEVICE || string.IsNullOrEmpty(request.RadioSerial))
      {
        isJohnDoeAsset = IsJohnDoe(projectUid, projectCustomerSubs);
      }
      else
      {
        //Radio serial in tag file. Use it to map to asset in VL.
        var assetDevice =
          await dataRepository.LoadAssetDevice(request.RadioSerial, ((DeviceTypeEnum) request.DeviceType).ToString());

        // special case in CGen US36833 If fails on DT SNM940 try as again SNM941 
        if (assetDevice == null && (DeviceTypeEnum) request.DeviceType == DeviceTypeEnum.SNM940)
        {
          log.LogDebug("ProjectAndAssetUidsExecutor: Failed for SNM940 trying again as Device Type SNM941");
          assetDevice = await dataRepository.LoadAssetDevice(request.RadioSerial, DeviceTypeEnum.SNM941.ToString());
        }

        log.LogDebug("ProjectAndAssetUidsExecutor: Loaded assetDevice? {0}", JsonConvert.SerializeObject(assetDevice));

        if (assetDevice != null)
        {
          assetUid = assetDevice.AssetUID;
          assetOwningCustomerUid = assetDevice.OwningCustomerUID;
          log.LogDebug($"ProjectAndAssetUidsExecutor: Loaded assetDevice {JsonConvert.SerializeObject(assetDevice)}");

          // get any assetSubs ("3D Project Monitoring") 
          assetSubs = (await dataRepository.LoadAssetSubs(assetUid, DateTime.UtcNow)).ToList();
          log.LogDebug($"ProjectAndAssetUidsExecutor: Loaded assetSubs? {JsonConvert.SerializeObject(assetSubs)}");

        }
        else
        {
          isJohnDoeAsset = IsJohnDoe(request.ProjectUid, projectCustomerSubs);
        }
      }

      // manual is quite different e.g. any time prior to sub EndDate is ok
      //                                assetCustomer and projectCustomer subs may play a role
      if (!string.IsNullOrEmpty(request.ProjectUid))
      {
        return await HandleManualImport(request, project, projectCustomerSubs, assetUid, assetOwningCustomerUid,
          assetSubs);
      }

      return await HandleAutoImport(request, assetUid, assetOwningCustomerUid, assetSubs);
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new System.NotImplementedException();
    }

    private async Task<GetProjectAndAssetUidsResult> HandleManualImport(GetProjectAndAssetUidsRequest request,
      Project project, List<Subscriptions> projectCustomerSubs, string assetUid, string assetOwningCustomerUid,
      List<Subscriptions> assetSubs)
    {
      if (project.ProjectType == ProjectType.Standard)
      {
        if (!projectCustomerSubs.Any())
        {
          // see if we can use the assetCustomerSub Man3d
          if (!string.IsNullOrEmpty(assetUid))
          {
            // we need assetOwningCustomer for Manual import to determine if assetCustomer == projectCustomer 
            var assetCustomerSubs =
              (await dataRepository.LoadManual3DCustomerBasedSubs(assetOwningCustomerUid, DateTime.UtcNow)).ToList();
            log.LogDebug(
              $"ProjectAndAssetUidsExecutor: Loaded assetsCustomerSubs? {JsonConvert.SerializeObject(assetCustomerSubs)}");

            var mostSignificantServiceType =
              GetMostSignificantServiceType(assetUid, project.CustomerUID, projectCustomerSubs,
                assetCustomerSubs, assetSubs);
            log.LogDebug(
              $"ProjectAndAssetUidsExecutor: after GetMostSignificantServiceType(). mostSignificantServiceType: {mostSignificantServiceType}");

            if (mostSignificantServiceType == serviceTypeMappings.serviceTypes.Find(st => st.name == "Unknown").NGEnum)
            {
              return GetProjectAndAssetUidsResult.CreateGetProjectAndAssetUidsResult(string.Empty, assetUid, 39);
            }
          }
          else
          {
            return GetProjectAndAssetUidsResult.CreateGetProjectAndAssetUidsResult(string.Empty, assetUid, 40);
          }
        }


        // got some kind of subscription, and project by this stage...
        // for manual, any projectTimeRange is ok
        var intersectingProjects = (await dataRepository.GetIntersectingProjects(project.CustomerUID,
          new int[0], request.Latitude, request.Longitude, null)).ToList();

        log.LogDebug(
          $"ProjectAndAssetUidsExecutor: Projects which intersect with manually imported project {JsonConvert.SerializeObject(intersectingProjects)}");

        if (intersectingProjects.Count != 1)
        {
          return GetProjectAndAssetUidsResult.CreateGetProjectAndAssetUidsResult(string.Empty, assetUid, 41,
            $"{intersectingProjects.Count} found");
        }

        if (intersectingProjects
          .Select(p => string.Compare(p.ProjectUID, project.ProjectUID, StringComparison.OrdinalIgnoreCase)).Any())
        {
          return GetProjectAndAssetUidsResult.CreateGetProjectAndAssetUidsResult(project.ProjectUID, assetUid);
        }

        return GetProjectAndAssetUidsResult.CreateGetProjectAndAssetUidsResult(string.Empty, assetUid, 42);
      }
      else if (project.ProjectType == ProjectType.LandFill)
      {
        // project customer must have current landfill sub, time ok anytime prior to (sub expiration or now?) todo
        ///
        ///    // ProjectMonitoring project
        //  MUST have a TCCOrgID
        //  TccCustomerUid must have a PM sub (sql join in GetProjectMonitoringProject)
        //  allow johnDoe assets and valid assetIDs.
        //  Don't allow manually imported tagfiles i.e. ProjectUid provided. !--- todo
        //if (!string.IsNullOrEmpty(tccCustomerUid) && string.IsNullOrEmpty(request.ProjectUid))
        //{
          var pmProjects = (await dataRepository.GetProjectMonitoringProject(project.CustomerUID,
            request.Latitude, request.Longitude, request.TimeOfPosition,
            (int)ProjectType.ProjectMonitoring,
            (serviceTypeMappings.serviceTypes.Find(st => st.name == "Project Monitoring").NGEnum))).ToList();
          if (pmProjects.Any())
          {
            //potentialProjects.AddRange(pmProjects);
            log.LogDebug(
              "ProjectAndAssetUidsExecutor: Loaded pmProjects which lat/long is within {JsonConvert.SerializeObject(pmProjects)}");
          }
          else
          {
            log.LogDebug("ProjectAndAssetUidsExecutor: No pmProjects loaded");
          }
        //}

        
      }

      else if (project.ProjectType == ProjectType.ProjectMonitoring)
      {
        // project customer must have current PM sub, time ok anytime prior to (sub expiration or now?) todo
       
        // Landfill project
        //  MUST have a TCCOrgID
        //  TccCustomerUid must have a Landfill sub (sql join in GetProjectMonitoringProject)
        //  allow johnDoe assets, valid assetIDs and manual import i.e. ProjectUid provided.
        //if (!string.IsNullOrEmpty(request.TccOrgUid))
        //{
          var landfillProjects = (await dataRepository.GetProjectMonitoringProject(project.CustomerUID , // todo tccCustomerUid,
              request.Latitude, request.Longitude, request.TimeOfPosition,
              (int)ProjectType.LandFill, (serviceTypeMappings.serviceTypes.Find(st => st.name == "Landfill").NGEnum)))
            .ToList();
          if (landfillProjects.Any())
          {
            //potentialProjects.AddRange(landfillProjects);
            log.LogDebug(
              $"ProjectAndAssetUidsExecutor: Loaded landfillProjects which lat/long is within {JsonConvert.SerializeObject(landfillProjects)}");
          }
          else
          {
            log.LogDebug("ProjectAndAssetUidsExecutor: No landfillProjects loaded");
          }
        //}
        //
      }

      return GetProjectAndAssetUidsResult.CreateGetProjectAndAssetUidsResult(string.Empty, string.Empty,
        99); // todo unknown projectType
    }



    private async Task<GetProjectAndAssetUidsResult> HandleAutoImport(GetProjectAndAssetUidsRequest request,
      string assetUid, string assetOwningCustomerUid, List<Subscriptions> assetSubs)
    {
      // Auto Import, search for appropriate project

      var tccCustomerUid = string.Empty;

      // must be able to find one or other customer for a) tccOrgUid b) radioSerial
      if (!string.IsNullOrEmpty(request.TccOrgUid))
      {
        var customerTccOrg = await dataRepository.LoadCustomerByTccOrgId(request.TccOrgUid);
        log.LogDebug(
          $"ProjectAndAssetUidsExecutor: Loaded CustomerByTccOrgId? {JsonConvert.SerializeObject(customerTccOrg)}");
        tccCustomerUid = customerTccOrg?.CustomerUID ?? string.Empty;

        // todo information message that tccorg not found? this is only significant if the customer has landfill/PM type projects
      }

      var potentialProjects = new List<Project>();
      if (!string.IsNullOrEmpty(tccCustomerUid) || !string.IsNullOrEmpty(assetOwningCustomerUid))
      {
        potentialProjects = await GetPotentialProjects(assetOwningCustomerUid, assetSubs, tccCustomerUid, request);
        log.LogDebug(
          $"ProjectAndAssetUidsExecutor: GetPotentialProjects: {JsonConvert.SerializeObject(potentialProjects)}");

      }

      //projectId
      //If zero found then If manualAsset returns -3 else returns -1
      //If one found then returns its id
      //If > 1 found then returns -2
      var uniqueCode = 0;
      string projectUid = String.Empty;
      ;

      if (!potentialProjects.Any())
      {
        if (!string.IsNullOrEmpty(projectUid)) // Manual Import
        {
          // case -2:
          uniqueCode = 29; /* todo */
          // projectId = -3; 
        }
        //else if (isJohnDoeAsset) // John Doe   todo
        //{
        //  // case -2:
        //  uniqueCode = 29; /* todo */
        //                   // projectId = -3; 
        //}
        else // Auto
        {
          uniqueCode = 29; /* todo */
          // projectId = -1;
        }
      }
      else if (potentialProjects.Distinct().Count() > 1)
      {
        uniqueCode = 32; /* todo */
        //projectId = -2;
      }
      else
        projectUid = potentialProjects.ToList()[0].ProjectUID;

      log.LogDebug(
        $"ProjectAndAssetUidsExecutor: returning uniqueCode: {uniqueCode} projectUid: {projectUid}  assetUid: {assetUid}.");

      return GetProjectAndAssetUidsResult.CreateGetProjectAndAssetUidsResult(projectUid, assetUid, uniqueCode);
    }

    //Special case: Allow manual import of tag file if user has manual 3D subscription.
    //ProjectUID is present for Manual Import, and is -1 for auto processing of tag files and non-zero for manual processing.
    //Radio serial may not be present in the tag file. The logic below replaces the 'john doe' handling in Raptor for these tag files.
    private bool IsJohnDoe(string projectUid, List<Subscriptions> projectCustomerSubs)
    {
      var isJohnDoeAsset = false;
      if (!string.IsNullOrEmpty(projectUid))
      {
        log.LogDebug(
          $"ProjectAndAssetUidsExecutor: manual import for project - about to check for manual 3D subscription. projectUid {projectUid}");

        if (projectCustomerSubs.Any())
        {
          isJohnDoeAsset = true;
        }

        log.LogDebug(
          $"ProjectAndAssetUidsExecutor: Manual3d/NoAsset. isJohnDoeAsset {isJohnDoeAsset} ");
      }

      return isJohnDoeAsset;
    }


    private int GetMostSignificantServiceType(string assetUid, string projectCustomerUid,
      List<Subscriptions> projectCustomerSubs, List<Subscriptions> assetCustomerSubs, List<Subscriptions> assetSubs)
    {
      var serviceType = serviceTypeMappings.serviceTypes.Find(st => st.name == "Unknown").NGEnum;
      var subs = new List<Subscriptions>();
      if (projectCustomerSubs.Any()) subs = subs.Concat(projectCustomerSubs.Select(s => s)).ToList();
      if (assetCustomerSubs.Any()) subs = subs.Concat(assetCustomerSubs.Select(s => s)).ToList();
      if (assetSubs.Any()) subs = subs.Concat(assetSubs.Select(s => s)).ToList();

      log.LogDebug(
        $"ProjectAndAssetUidsExecutor: GetMostSignificantServiceType() for assetUid: {assetUid} projectCustomer: {projectCustomerUid}, subs: {JsonConvert.SerializeObject(subs)})");

      if (subs.Any())
      {
        //Look for highest level machine subscription which is current
        foreach (var sub in subs)
        {
          // Manual3d is least significant
          if (sub.serviceTypeId == serviceTypeMappings.serviceTypes
                .Find(st => st.name == "Manual 3D Project Monitoring").NGEnum)
          {
            if (serviceType != serviceTypeMappings.serviceTypes.Find(st => st.name == "3D Project Monitoring").NGEnum)
            {
              log.LogDebug(
                $"ProjectAndAssetUidsExecutor: GetMostSignificantServiceType() found Manual3DProjectMonitoring for assetUid {assetUid}");
              serviceType = serviceTypeMappings.serviceTypes.Find(st => st.name == "Manual 3D Project Monitoring")
                .NGEnum;
            }
          }

          // 3D PM is most significant
          // if 3D asset-based, the assets customer must be the same as the Projects customer 
          if (sub.serviceTypeId ==
              serviceTypeMappings.serviceTypes.Find(st => st.name == "3D Project Monitoring").NGEnum)
          {
            if (serviceType != serviceTypeMappings.serviceTypes.Find(st => st.name == "3D Project Monitoring").NGEnum)
            {
              //Allow manual tag file import for customer who has the 3D subscription for the asset
              //and allow automatic tag file processing in all cases (can't tell customer for automatic)
              log.LogDebug(
                $"ProjectAndAssetUidsExecutor: GetMostSignificantServiceType() found e3DProjectMonitoring for assetUid {assetUid} sub.customerUid {sub.customerUid}");
              if (string.IsNullOrEmpty(projectCustomerUid) || sub.customerUid == projectCustomerUid)
              {
                serviceType = serviceTypeMappings.serviceTypes.Find(st => st.name == "3D Project Monitoring").NGEnum;
                break;
              }
            }
          }
        }
      }

      log.LogDebug(
        $"ProjectAndAssetUidsExecutor: GetMostSignificantServiceType() for assetUid {assetUid}, returning serviceTypeNG {serviceType}.");
      return serviceType;
    }

    private async Task<List<Project>> GetPotentialProjects
    (string assetOwningCustomerUid, List<Subscriptions> assetSubs, string tccCustomerUid,
      GetProjectAndAssetUidsRequest request)
    {
      var potentialProjects = new List<Project>();

      //Look for projects with (request.latitude, request.longitude) inside their boundary
      //and belonging to customers who have a Project Monitoring subscription
      //for asset with id request.assetId at time request.timeOfPosition 
      //and the customer owns the asset. (In VL multiple customers can have subscriptions
      //for an asset but only the owner gets the tag file data).

      //  standard 2d / 3d project aka construction project
      //    IGNORE any tccOrgID
      //    must have valid assetID, which must have a 3d sub.
      if (!string.IsNullOrEmpty(assetOwningCustomerUid) && assetSubs.Any())
      {
        var standardProjects = (await dataRepository.GetStandardProject(assetOwningCustomerUid, request.Latitude,
          request.Longitude,
          request.TimeOfPosition)).ToList();
        if (standardProjects.Any())
        {
          potentialProjects.AddRange(standardProjects);
          log.LogDebug(
            $"ProjectAndAssetUidsExecutor: Loaded standardProjects which lat/long is within {JsonConvert.SerializeObject(standardProjects)}");
        }
        else
        {
          log.LogDebug("ProjectAndAssetUidsExecutor: No standardProjects loaded");
        }
      }

      // ProjectMonitoring project
      //  MUST have a TCCOrgID
      //  TccCustomerUid must have a PM sub (sql join in GetProjectMonitoringProject)
      //  allow johnDoe assets and valid assetIDs.
      //  Don't allow manually imported tagfiles i.e. ProjectUid provided.
      if (!string.IsNullOrEmpty(tccCustomerUid) && string.IsNullOrEmpty(request.ProjectUid))
      {
        var pmProjects = (await dataRepository.GetProjectMonitoringProject(tccCustomerUid,
          request.Latitude, request.Longitude, request.TimeOfPosition,
          (int) ProjectType.ProjectMonitoring,
          (serviceTypeMappings.serviceTypes.Find(st => st.name == "Project Monitoring").NGEnum))).ToList();
        if (pmProjects.Any())
        {
          potentialProjects.AddRange(pmProjects);
          log.LogDebug(
            "ProjectAndAssetUidsExecutor: Loaded pmProjects which lat/long is within {JsonConvert.SerializeObject(pmProjects)}");
        }
        else
        {
          log.LogDebug("ProjectAndAssetUidsExecutor: No pmProjects loaded");
        }
      }

      // Landfill project
      //  MUST have a TCCOrgID
      //  TccCustomerUid must have a Landfill sub (sql join in GetProjectMonitoringProject)
      //  allow johnDoe assets, valid assetIDs and manual import i.e. ProjectUid provided.
      if (!string.IsNullOrEmpty(tccCustomerUid))
      {
        var landfillProjects = (await dataRepository.GetProjectMonitoringProject(tccCustomerUid,
            request.Latitude, request.Longitude, request.TimeOfPosition,
            (int) ProjectType.LandFill, (serviceTypeMappings.serviceTypes.Find(st => st.name == "Landfill").NGEnum)))
          .ToList();
        if (landfillProjects.Any())
        {
          potentialProjects.AddRange(landfillProjects);
          log.LogDebug(
            $"ProjectAndAssetUidsExecutor: Loaded landfillProjects which lat/long is within {JsonConvert.SerializeObject(landfillProjects)}");
        }
        else
        {
          log.LogDebug("ProjectAndAssetUidsExecutor: No landfillProjects loaded");
        }
      }

      return potentialProjects;
    }
  }
}
