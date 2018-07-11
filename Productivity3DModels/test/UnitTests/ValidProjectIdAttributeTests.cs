using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.Models.Validation;

namespace VSS.Productivity3D.Models.UnitTests
{
  [TestClass]
  public class ValidProjectIdAttributeTests
  {
    [TestMethod]
    [DataRow(true)]
    [DataRow(1.2)]
    [DataRow(45)]
    [DataRow("c35a613c-05a8-4151-af2a-08007b26799b")]
    public void Should_fail_validation_When_input_is_an_incompatible_type(object projectId)
    {
      Assert.IsFalse(new ValidProjectIdAttribute().IsValid(projectId));
    }

    [TestMethod]
    [DataRow(-1)]
    [DataRow(0)]
    public void Should_fail_validation_When_input_is_zero_or_less(long projectId)
    {
      Assert.IsFalse(new ValidProjectIdAttribute().IsValid(projectId));
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(45)]
    [DataRow(42562)]
    public void Should_pass_validation_When_input_is_a_valid_Long(long projectId)
    {
      Assert.IsTrue(new ValidProjectIdAttribute().IsValid(projectId));
    }

    [TestMethod]
    [DataRow(null)]
    public void Should_pass_validation_When_input_is_null_Long(long? projectId)
    {
      Assert.IsTrue(new ValidProjectIdAttribute().IsValid(projectId));
    }
  }
}
