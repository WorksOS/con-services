using System.Collections.Generic;
using System.Threading.Tasks;
using TestUtility;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;

namespace IntegrationTests.WebApiTests
{
  public class ProjectV2ForTBCTests : WebApiTestsBase
  {
    private static List<TBCPoint> _boundaryLL;

    public ProjectV2ForTBCTests()
    {
      _boundaryLL = new List<TBCPoint>
      {
        new TBCPoint(-43.5, 172.6),
        new TBCPoint(-43.5003, 172.6),
        new TBCPoint(-43.5003, 172.603),
        new TBCPoint(-43.5, 172.603)
      };
    }    
      
    
    private static Task<string> CreateProjectV5TBC(TestSupport ts, string projectName, ProjectType projectType)
    {
      return ts.CreateProjectViaWebApiV5TBC(projectName, ts.FirstEventDate, ts.LastEventDate, "New Zealand Standard Time", projectType, _boundaryLL);
    }
  }
}
