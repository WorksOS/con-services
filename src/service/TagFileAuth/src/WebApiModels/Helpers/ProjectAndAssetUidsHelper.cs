using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Helpers
{
  public static class ProjectAndAssetUidsHelper
  {
    /// <summary>
    /// allows mapping between CG (which Raptor requires) and NG
    /// </summary>
    private static readonly ServiceTypeMappings ServiceTypeMappings = new ServiceTypeMappings();
    public const double TOLERANCE_DECIMAL_DEGREE = 1e-10;

    public static int GetMostSignificantServiceType(ILogger log, string assetUid, string projectCustomerUid,
      List<Subscriptions> projectCustomerSubs, List<Subscriptions> assetCustomerSubs, List<Subscriptions> assetSubs)
    {
      var serviceType = ServiceTypeMappings.serviceTypes.Find(st => st.name == "Unknown").NGEnum;

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
          if (sub.serviceTypeId == (int)ServiceTypeEnum.Manual3DProjectMonitoring)
          {
            if (serviceType != (int)ServiceTypeEnum.ThreeDProjectMonitoring)
            {
              log.LogDebug($"{nameof(GetMostSignificantServiceType)}: found Manual3DProjectMonitoring for assetUid {assetUid}");
              serviceType = (int)ServiceTypeEnum.Manual3DProjectMonitoring;
            }
          }

          // 3D PM is most significant
          // if 3D asset-based, the assets customer must be the same as the Projects customer 
          if (sub.serviceTypeId == (int)ServiceTypeEnum.ThreeDProjectMonitoring)
          {
            if (serviceType != (int)ServiceTypeEnum.ThreeDProjectMonitoring)
            {
              //Allow manual tag file import for customer who has the 3D subscription for the asset
              //and allow automatic tag file processing in all cases (can't tell customer for automatic)
              log.LogDebug($"{nameof(GetMostSignificantServiceType)}: found e3DProjectMonitoring for assetUid {assetUid} sub.customerUid {sub.customerUid}");
              if (string.IsNullOrEmpty(projectCustomerUid) || sub.customerUid == projectCustomerUid)
              {
                serviceType = (int)ServiceTypeEnum.ThreeDProjectMonitoring;
                break;
              }
            }
          }
        }
      }

      log.LogDebug($"{nameof(GetMostSignificantServiceType)}:for assetUid {assetUid}, returning serviceTypeNG {serviceType}.");
      return serviceType;
    }

    /// <summary>
    /// Look for projects with the tag file seed location inside their boundary
    ///     and belonging to asset or tccOrgId customers
    ///     where customer or project (depending on project type) has the correct sub.
    ///
    ///  Note: In VL multiple customers can have subscriptions
    ///    for an asset but only the assets owner (or tccOrgId owner) gets the tag file data.
    /// </summary>
    public static async Task<List<Project.Abstractions.Models.DatabaseModels.Project>> GetPotentialProjectsHaveLatLong
    (ILogger log, DataRepository dataRepository, string assetOwningCustomerUid, List<Subscriptions> assetSubs, string tccCustomerUid,
      GetProjectAndAssetUidsRequest request)
    {
      var potentialProjects = new List<Project.Abstractions.Models.DatabaseModels.Project>();

      //  standard 2d / 3d project 
      //    IGNORE any tccOrgID
      //    must have valid assetID, which must have a 3d sub.
      if (!string.IsNullOrEmpty(assetOwningCustomerUid) && assetSubs.Any())
      {
        potentialProjects.AddRange((await dataRepository.GetIntersectingProjects(assetOwningCustomerUid,
          request.Latitude, request.Longitude, new[] { (int)ProjectType.Standard }, request.TimeOfPosition)));
      }

      if (!string.IsNullOrEmpty(tccCustomerUid))
      {
        // ProjectMonitoring and Landfill projects
        //  MUST have a TCCOrgID
        //  project must have a PM/LF sub 
        //  does not require an asset has been identified
        potentialProjects.AddRange(
          (await dataRepository.GetIntersectingProjects(tccCustomerUid, request.Latitude,
            request.Longitude, new[] { (int)ProjectType.ProjectMonitoring, (int)ProjectType.LandFill },
            request.TimeOfPosition))
          .Where(pm => (pm.ServiceTypeID == (int)ServiceTypeEnum.ProjectMonitoring
                        || pm.ServiceTypeID == (int)ServiceTypeEnum.Landfill)));
      }

      return potentialProjects;
    }

    /// <summary>
    /// Look for projects with the tag file seed location inside their boundary
    ///     and belonging to asset or tccOrgId customers
    ///     where customer or project (depending on project type) has the correct sub.
    ///
    ///  Note: In VL multiple customers can have subscriptions
    ///    for an asset but only the assets owner (or tccOrgId owner) gets the tag file data.
    ///
    ///  In this scenario, we only have the NE of the tag file.
    ///     So we need to a) get all the customers projects, 
    ///                   b) for each project:
    ///                       get projects CSIB; convert NE to that CSIB lat/long; and see if lat/long is within project
    ///  This method increases the chances of resolving to multiple projects, hence being unable to apply the tag file.
    ///     This is because CSIBs are grid based, multiple of the customers projects may have the same NE e.g. 0,0
    /// </summary>
    public static async Task<List<Project.Abstractions.Models.DatabaseModels.Project>> GetPotentialProjectsHaveNorthingEasting
    (ILogger log, DataRepository dataRepository, string assetOwningCustomerUid, List<Subscriptions> assetSubs, string tccCustomerUid,
      GetProjectAndAssetUidsRequest request)
    {
      var potentialProjects = new List<Project.Abstractions.Models.DatabaseModels.Project>();
      if (!request.Northing.HasValue || !request.Easting.HasValue) // handled by validation
        return potentialProjects;

      //  standard 2d / 3d project 
      //    IGNORE any tccOrgID
      //    must have valid assetID, which must have a 3d sub.
      if (!string.IsNullOrEmpty(assetOwningCustomerUid) && assetSubs.Any())
      {
        var customersProjects = await dataRepository.LoadProjects(assetOwningCustomerUid, request.TimeOfPosition, new List<int>((int) ProjectType.Standard));
        if (customersProjects.Any())
        {
          foreach (var project in customersProjects)
          {
            var latLongDegrees = await dataRepository.GenerateLatLong(project.ProjectUID, request.Northing.Value, request.Easting.Value);
            if (Math.Abs(latLongDegrees.Lat) < TOLERANCE_DECIMAL_DEGREE && Math.Abs(latLongDegrees.Lon) < TOLERANCE_DECIMAL_DEGREE)
            {
              log.LogDebug($"{nameof(GetPotentialProjectsHaveNorthingEasting)}: No CSIB was found for projectUID: {project.ProjectUID}.");
            }
            else
            {
              request.Latitude = latLongDegrees.Lat;
              request.Longitude = latLongDegrees.Lon;
              potentialProjects.AddRange((await dataRepository.GetIntersectingProjects(assetOwningCustomerUid,
                latLongDegrees.Lat, latLongDegrees.Lon, new[] {(int) ProjectType.Standard},
                request.TimeOfPosition, project.ProjectUID)));
            }
          }
        }
      }

      // ProjectMonitoring and Landfill projects
      //  MUST have a TCCOrgID
      //  project must have a PM/LF sub 
      //  does not require an asset has been identified
      if (!string.IsNullOrEmpty(tccCustomerUid))
      {
        var customersProjects = await dataRepository.LoadProjects(tccCustomerUid, request.TimeOfPosition, new List<int>() {(int) ProjectType.ProjectMonitoring, (int) ProjectType.LandFill});
        if (customersProjects.Any())
        {
          foreach (var project in customersProjects)
          {
            var latLongDegrees = await dataRepository.GenerateLatLong(project.ProjectUID, request.Northing.Value, request.Easting.Value);
            if (Math.Abs(latLongDegrees.Lat) < TOLERANCE_DECIMAL_DEGREE && Math.Abs(latLongDegrees.Lon) < TOLERANCE_DECIMAL_DEGREE)
            {
              log.LogDebug($"{nameof(GetPotentialProjectsHaveNorthingEasting)}: No CSIB was found for projectUID: {project.ProjectUID}.");
            }
            else
            {
              request.Latitude = latLongDegrees.Lat;
              request.Longitude = latLongDegrees.Lon;
              potentialProjects.AddRange(
                (await dataRepository.GetIntersectingProjects(tccCustomerUid,
                  latLongDegrees.Lat, latLongDegrees.Lon, new[] {(int) ProjectType.ProjectMonitoring, (int) ProjectType.LandFill},
                  request.TimeOfPosition, project.ProjectUID))
                .Where(pm => (pm.ServiceTypeID == (int) ServiceTypeEnum.ProjectMonitoring
                              || pm.ServiceTypeID == (int) ServiceTypeEnum.Landfill))
              );
            }
          }
        }
      }

      return potentialProjects;
    }
  }
}
