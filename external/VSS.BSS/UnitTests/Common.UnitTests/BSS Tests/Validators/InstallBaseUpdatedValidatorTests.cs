//using System;
//using System.Text;
//using System.Collections.Generic;
//using System.Linq;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using VSS.Hosted.VLCommon.Bss;

//using VSS.UnitTest.Common;namespace UnitTests.BSS_Tests
//{
//  [TestClass]
//  public class InstallBaseUpdatedValidatorTests : BssUnitTestBase
//  {
//    InstallBaseUpdatedValidator validator;

//    [TestInitialize]
//    public void TestInitialize()
//    {
//      validator = new InstallBaseUpdatedValidator();
//    }

//    [TestMethod]
//    [ExpectedException(typeof(InvalidOperationException))]
//    public void Validate_NullMessage_ThrowInvalidOperationException()
//    {
//      validator.Validate(null);
//    }

//    [TestMethod]
//    public void Validate_NoErrorsNoWarnings_Success()
//    {
//      var message = BSS.IBUpdated.Build();
//      validator.Validate(message);
//      Assert.AreEqual(0, validator.Warnings.Count, "No warnings expected.");
//      Assert.AreEqual(0, validator.Errors.Count, "No Errors expected.");
//    }
//  }
//}
