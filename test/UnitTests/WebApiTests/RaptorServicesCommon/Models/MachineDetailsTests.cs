using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.ResultHandling;

namespace VSS.Raptor.Service.WebApiTests.Common.Models
{
  [TestClass()]
  public class MachineDetailsTests
  {
    [TestMethod()]
    public void CanCreateMachineDetailsTest()
    {
      var validator = new DataAnnotationsValidator();
      MachineDetails machine = MachineDetails.CreateMachineDetails(1034, "Acme Dozer", false);
      ICollection<ValidationResult> results;
      Assert.IsTrue(validator.TryValidate(machine, out results));

      //missing name
      machine = MachineDetails.CreateMachineDetails(1034, string.Empty, false);
      Assert.IsFalse(validator.TryValidate(machine, out results), "missing name failed");

      //too long name
      machine = MachineDetails.CreateMachineDetails(1034, new string('A', 10000), false);
      Assert.IsFalse(validator.TryValidate(machine, out results), "too long name failed");
    }
  }
}
