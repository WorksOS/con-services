using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.ResultHandling;

namespace VSS.Productivity3D.WebApiTests.RaptorServicesCommon.Models
{
  [TestClass]
  public class MachineDetailsTests
  {
    [TestMethod]
    public void CanCreateTest()
    {
      var validator = new DataAnnotationsValidator();
      MachineDetails machine = MachineDetails.Create(1034, "Acme Dozer", false);
      Assert.IsTrue(validator.TryValidate(machine, out ICollection<ValidationResult> results));

      // not provided name
      machine = MachineDetails.Create(1034, null, false);
      Assert.IsFalse(validator.TryValidate(machine, out results), "not provided name failed");

      // missing name
      machine = MachineDetails.Create(1034, string.Empty, false);
      Assert.IsFalse(validator.TryValidate(machine, out results), "empty name failed");

      // too long name
      machine = MachineDetails.Create(1034, new string('A', 10000), false);
      Assert.IsFalse(validator.TryValidate(machine, out results), "too long name failed");
    }
  }
}
