using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSP.MasterData.Project.AcceptanceTests.Utils;
using VSP.MasterData.Project.AcceptanceTests.Models.Project;

namespace VSP.MasterData.Project.AcceptanceTests.Scenarios.ScenarioSupports
{
    public class WebApiSupport
    {
        public CreateProjectEvent MakeCreateProjectRequest(Guid projectUid)
        {
            return new CreateProjectEvent
            {
                ActionUTC = DateTime.UtcNow,
                ProjectBoundary = " ",
                ProjectEndDate = DateTime.Today.AddMonths(10),
                ProjectStartDate = DateTime.Today.AddMonths(-3),
                ProjectName = "AT_PRO-" + DateTime.Now.ToString("yyyyMMddhhmmss"),
                ProjectTimezone = "New Zealand Standard Time",
                ProjectType = ProjectType.LandFill,
                ProjectID = CommonUtils.Random.Next(3000, 4000),
                ProjectUID = projectUid,
                ReceivedUTC = DateTime.UtcNow
            };
        }
    }
}
