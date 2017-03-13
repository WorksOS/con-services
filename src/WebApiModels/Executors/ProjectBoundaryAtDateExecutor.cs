using Repositories.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;

namespace VSS.TagFileAuth.Service.WebApiModels.Executors
{
  /// <summary>
  /// The executor which gets the project boundary of the project for the requested project id.
  /// </summary>
  public class ProjectBoundaryAtDateExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the get project boundary request and finds active projects of the asset owner at the given date time.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a GetProjectBoundaryAtDateResult if successful</returns>      
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      GetProjectBoundaryAtDateRequest request = item as GetProjectBoundaryAtDateRequest;

      bool result = false;
      TWGS84FenceContainer projectBoundary = new TWGS84FenceContainer();
      Project project = null;

      project = LoadProject(request.projectId);
      if (project != null)
      {
        if (project.StartDate <= request.tagFileUTC.Date && request.tagFileUTC.Date <= project.EndDate &&
            !string.IsNullOrEmpty(project.GeometryWKT)
            )
        {
          result = true;
          projectBoundary.FencePoints = ParseBoundaryData(project.GeometryWKT);
        }
      }

      try
      {
        return GetProjectBoundaryAtDateResult.CreateGetProjectBoundaryAtDateResult(result, projectBoundary);
      }
      catch
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Failed to get project boundary"));
      }

    }

   
  }
  //public class ProjectBoundaryValidator
  //{
  //  public static List<TWGS84Point> ParseBoundaryData(string s)
  //  {
  //    var points = new List<TWGS84Point>();

  //    string[] pointsArray = s.Remove(s.Length - 1).Split(';');

  //    for (int i = 0; i < pointsArray.Length; i++)
  //    {
  //      double[] coordinates = new double[2];

  //      //gets x and y coordinates split by comma, trims whitespace at pos 0, converts to double array
  //      coordinates = pointsArray[i].Trim().Split(',').Select(c => double.Parse(c)).ToArray();

  //      points.Add(new TWGS84Point(coordinates[1], coordinates[0]));
  //    }
  //    return points;
  //  }

  //  // validation is done before putting project on kafka que. Shouldn't be needed again.
  //  //public static void Validate(string boundary)
  //  //{
  //  //  try
  //  //  {
  //  //    var points = ParseBoundaryData(boundary);

  //  //    if (points.Count < 3)
  //  //    {
  //  //      throw new ServiceException(HttpStatusCode.BadRequest,
  //  //          "Invalid project's boundary as it should contain at least 3 points");
  //  //    }
  //  //  }
  //  //  catch
  //  //  {
  //  //    throw new ServiceException(HttpStatusCode.BadRequest,
  //  //        "Invalid project's boundary");
  //  //  }
  //  //}

  //}
}