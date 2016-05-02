using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSP.MasterData.Project.AcceptanceTests.Models.Project
{
    public class UpdateProjectEvent
    {
        public DateTime ActionUTC { get; set; }
        public DateTime ProjectEndDate { get; set; }
        public string ProjectName { get; set; }
        public string ProjectTimezone { get; set; }
        public ProjectType ProjectType { get; set; }
        public Guid ProjectUID { get; set; }
        public DateTime ReceivedUTC { get; set; }
    }
}
