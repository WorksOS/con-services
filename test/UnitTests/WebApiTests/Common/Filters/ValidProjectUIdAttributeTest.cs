using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VSS.Productivity3D.Common.Filters.Validation;

namespace VSS.Productivity3D.WebApiTests.Common.Filters
{
  [TestClass]
  public class ValidProjectUIdAttributeTest
  {
    [TestMethod]
    [DataRow(-1)]
    [DataRow(0)]
    [DataRow(true)]
    [DataRow("00000000-0000-0000-0000-000000000000")]
    [DataRow("c35a613c-05a8-4151-af2a-08007b26799b")]
    public void Should_fail_validation_When_input_is_an_incompatible_type(object projectUId)
    {
      Assert.IsFalse(new ValidProjectUIDAttribute().IsValid(projectUId));
    }

    [TestMethod]
    public void Should_fail_validation_When_input_is_an_empty_Guid()
    {
      Assert.IsFalse(new ValidProjectUIDAttribute().IsValid(Guid.Empty));
    }

    [TestMethod]
    public void Should_pass_validation_When_input_is_null()
    {
      Assert.IsTrue(new ValidProjectUIDAttribute().IsValid(null));
    }

    [TestMethod]
    public void Should_pass_validation_When_input_is_a_Guid()
    {
      Assert.IsTrue(new ValidProjectUIDAttribute().IsValid(Guid.NewGuid()));
    }
  }
}