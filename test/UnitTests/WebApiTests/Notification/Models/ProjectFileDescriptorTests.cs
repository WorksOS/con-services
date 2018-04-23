using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApiModels.Notification.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.WebApiTests.Notification.Models
{
  [TestClass]
  public class ProjectFileDescriptorTests
  {
    [TestMethod]
    public void CanCreateProjectFileDescriptorTest()
    {
      var validator = new DataAnnotationsValidator();
      ProjectFileDescriptor projectFileDescr = ProjectFileDescriptor.CreateProjectFileDescriptor(projectId, projectUid, fileDescr, coordSystemFileName, userUnits, fileId);
      ICollection<ValidationResult> results;
      Assert.IsTrue(validator.TryValidate(projectFileDescr, out results));

      //Missing fileDescr
      projectFileDescr = ProjectFileDescriptor.CreateProjectFileDescriptor(projectId, projectUid, null, coordSystemFileName, userUnits, fileId);
      Assert.IsFalse(validator.TryValidate(projectFileDescr, out results));
    }

    [TestMethod]
    public void ValidateSuccessTest()
    {
      ProjectFileDescriptor projectFileDescr = ProjectFileDescriptor.CreateProjectFileDescriptor(projectId, projectUid, fileDescr, coordSystemFileName, userUnits, fileId);
      projectFileDescr.Validate();
    }

    [TestMethod]
    public void ValidateFailMissingFileIdTest()
    {
      ProjectFileDescriptor projectFileDescr = ProjectFileDescriptor.CreateProjectFileDescriptor(projectId, projectUid, fileDescr, coordSystemFileName, userUnits, 0);
      Assert.ThrowsException<ServiceException>(() => projectFileDescr.Validate());
    }

    [TestMethod]
    public void ValidateFailMissingProjectIdsTest()
    {
      ProjectFileDescriptor projectFileDescr = ProjectFileDescriptor.CreateProjectFileDescriptor(-1, Guid.Empty, fileDescr, coordSystemFileName, userUnits, fileId);
      Assert.ThrowsException<ServiceException>(() => projectFileDescr.Validate());
    }


    private long projectId = 1234;
    private Guid projectUid = Guid.NewGuid();
    private FileDescriptor fileDescr = FileDescriptor.CreateFileDescriptor("u72003136-d859-4be8-86de-c559c841bf10",
      "BC Data/Sites/Integration10/Designs", "Cycleway.ttm");
    private long fileId = 9914;
    private DxfUnitsType userUnits = DxfUnitsType.Meters;
    private string coordSystemFileName = "test.dc";
  }
}
