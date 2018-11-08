using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Filter.Common.Models;

namespace VSS.Productivity3D.Filter.Common.Validators
{
  public class ValidationUtil
  {
    /// <summary>
    /// Gets the service's implementation of <see cref="IProjectListProxy"/>.
    /// </summary>
    protected readonly IProjectListProxy ProjectListProxy;

    /// <summary>
    /// Gets or sets the service's log controller.
    /// </summary>
    protected ILogger Log;

    /// <summary>
    /// Gets or sets the service's exception hander implementation of <see cref="IServiceExceptionHandler"/>.
    /// </summary>
    protected readonly IServiceExceptionHandler ServiceExceptionHandler;

    public ValidationUtil(IProjectListProxy projectListProxy, ILogger log, IServiceExceptionHandler serviceExceptionHandler)
    {
      this.Log = log;
      this.ProjectListProxy = projectListProxy;
      this.ServiceExceptionHandler = serviceExceptionHandler;
    }

    /// <summary>
    /// Hydrates the filterJson string with the boundary data and uses the MasterData Models filter model to do so - to isolate logic there
    /// </summary>
    public static async Task<string> HydrateJsonWithBoundary(IGeofenceRepository geofenceRepository,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, FilterRequestFull filterRequestFull)
    {
      const string functionName = "HydrateJsonWithBoundary";

      MasterData.Models.Models.Filter filterTempForHydration = filterRequestFull.FilterModel(serviceExceptionHandler);

      //If no polygon boundary return original filter JSON
      if (string.IsNullOrEmpty(filterTempForHydration?.PolygonUid))
        return filterRequestFull.FilterJson;

      //Get polygon boundary to add to filter
      Geofence filterBoundary = null;
      try
      {
        filterBoundary = await geofenceRepository.GetGeofence(filterTempForHydration.PolygonUid).ConfigureAwait(false);
      }
      catch (Exception e)
      {
        log.LogError(
          $"{functionName}: geofenceRepository.GetGeofence failed with exception. projectUid:{filterRequestFull.ProjectUid} boundaryUid:{filterTempForHydration.PolygonUid} . Exception Thrown: {e.Message}.");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 41, e.Message);
      }

      if (filterBoundary == null)
      {
        log.LogError(
          $"{functionName}: boundary not found, or not valid: projectUid:{filterRequestFull.ProjectUid} boundaryUid:{filterTempForHydration.PolygonUid}. returned no boundary match");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 40);
      }

      //Add polygon boundary to filter and convert back to JSON
      List<WGSPoint> polygonPoints = GetPointsFromWkt(filterBoundary.GeometryWKT);
      if (polygonPoints.Count == 0)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 45);
      }
      filterTempForHydration.AddBoundary(filterBoundary.GeofenceUID, filterBoundary.Name, polygonPoints);

      string newFilterJson = null;
      try
      {
        newFilterJson = JsonConvert.SerializeObject(filterTempForHydration);
      }
      catch (Exception e)
      {
        log.LogError(
          $"{functionName}: {functionName} failed with exception. projectUid:{filterRequestFull.ProjectUid}. boundaryUid:{filterTempForHydration.PolygonUid} Exception Thrown: {e.Message}.");
        // todo normally we incude e.Message. is e.GetBaseException.message specific to Json exception?
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 43, e.GetBaseException().Message);
      }


      if (string.IsNullOrEmpty(newFilterJson))
      {
        log.LogError(
          $"{functionName}: {functionName} failed. projectUid:{filterRequestFull.ProjectUid}. boundaryUid:{filterTempForHydration.PolygonUid}.");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 44);
      }

      log.LogInformation(
        $"{functionName}: succeeded: projectUid:{filterRequestFull.ProjectUid}. boundaryUid:{filterTempForHydration.PolygonUid}.");
      return newFilterJson;
    }

    /// <summary>
    /// Returns polygons from wkt. Only returns unique ones (i.e 1st and last are not the same)
    /// </summary>
    private static List<WGSPoint> GetPointsFromWkt(string wkt)
    {
      List<WGSPoint> ret = new List<WGSPoint>();
      try
      {
        wkt = wkt.ToUpper();
        string prefix = "POLYGON";

        if (wkt.StartsWith(prefix) && (prefix.Length < wkt.Length))
        {
          //extract x from POLYGON (x) or POLYGON(lon1 lat1, lon2 lat2)
          string relevant = wkt.Substring(prefix.Length, wkt.Length - prefix.Length).Trim(' ').Trim('(', ')');
          //relevant will be lon1 lat1,lon2 lat2 ...
          var pairs = relevant.Split(',').Select(x => x.Trim()).ToList();
          var first = pairs.First();
          var last = pairs.Last();
          if (!first.Equals(last))
            return new List<WGSPoint>();

          foreach (var pair in pairs.Take(pairs.Count - 1))
          {
            string[] vals = pair.Split(' ');
            if (vals.Length == 2)
            {
              string longitude = vals[0].Substring(vals[0].StartsWith("(") ? 1 : 0); //remove prefix ( if present
              string latitude = vals[1].Substring(0, vals[1].Length - (vals[1].EndsWith(")") ? 1 : 0)); // remove suffix ) if present
              var pt = WGSPoint.CreatePoint(Convert.ToDouble(latitude), Convert.ToDouble(longitude));
              ret.Add(pt);
            }
          }

        }

      }
      catch (Exception)
      {
        return ret;
      }
      return ret;
    }
  }
}
