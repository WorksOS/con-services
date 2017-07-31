using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.Filter.Common.Utilities;
using VSS.Productivity3D.Filter.Common.ResultHandling;

namespace VSS.Productivity3D.Filter.Tests
{
  [TestClass]
  public class AutoMapperTests
  {
    [ClassInitialize]
    public static void ClassInitialize(TestContext testContext)
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
    }


    [TestMethod]
    public void MapProjectToResult()
    {
      var filter = new MasterData.Repositories.DBModels.Filter
      {
        CustomerUid = Guid.NewGuid().ToString(),
        UserUid = Guid.NewGuid().ToString(),
        ProjectUid = Guid.NewGuid().ToString(),
        FilterUid = Guid.NewGuid().ToString(),

        Name = "the Name",
        FilterJson = "the Json",

        IsDeleted = false,
        LastActionedUtc = new DateTime(2017, 01, 21)
      };

      var result = AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter);
      Assert.AreEqual(filter.FilterUid, result.FilterUid, "FilterUid has not been mapped correctly");
      Assert.AreEqual(filter.Name, result.Name, "Name has not been mapped correctly");
      Assert.AreEqual(filter.FilterJson, result.FilterJson, "ProjectType has not been mapped correctly");
    }

  }
}