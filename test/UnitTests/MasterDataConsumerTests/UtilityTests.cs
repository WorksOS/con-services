using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectWebApi.Models;
using ProjectWebApiCommon.Utilities;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Visionlink.Project.UnitTests
{
  [TestClass]
  public class UtilityTests
  {
    [TestMethod]
    public void MapCreateProjectRequestToEvent()
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();

      var request = CreateProjectRequest.CreateACreateProjectRequest
      (Guid.NewGuid(), Guid.NewGuid(),
        123, ProjectType.Standard, "projectName", "this is the description",
        new DateTime(2017, 01, 20), new DateTime(2017, 02, 15), "NZ whatsup",
        "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965, 172.595831670724 -43.5427038560109))",
        456, null, null);

      CreateProjectEvent kafkaEvent = AutoMapperUtility.Automapper.Map<CreateProjectEvent>(request);
      Assert.AreEqual(request.ProjectUid, kafkaEvent.ProjectUID, "ProjectUid has not been mapped correctly");
      Assert.AreEqual(request.CustomerId, kafkaEvent.CustomerID, "CustomerId has not been mapped correctly");
      Assert.AreEqual(DateTime.MinValue, kafkaEvent.ActionUTC, "ActionUTC has not been mapped correctly");
      Assert.AreEqual(DateTime.MinValue, kafkaEvent.ReceivedUTC, "ReceivedUTC has not been mapped correctly");

      CreateProjectRequest copyOfRequest = AutoMapperUtility.Automapper.Map<CreateProjectRequest>(request);
      Assert.AreEqual(request.ProjectUid, copyOfRequest.ProjectUid, "ProjectUid has not been mapped correctly");
      Assert.AreEqual(request.CoordinateSystemFileName, copyOfRequest.CoordinateSystemFileName, "CoordinateSystemFileName has not been mapped correctly");
    }
  }
}
