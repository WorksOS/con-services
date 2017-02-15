//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;

//namespace WebApiTests.Models
//{
//  [TestClass]
//  public class AssetRequestBaseTests
//    {

//    [TestMethod]
//    public void CanCreateAssetRequestBase()
//    {
//      //Uncomment once DataAnnotationsValidator released for .dotnetcore
      
//      //var validator = new DataAnnotationsValidator();
//      var request = new AssetRequestBase { assetIdentifier = Guid.NewGuid().ToString() };
//      ICollection<ValidationResult> results = new List<ValidationResult>();
//      var context = new ValidationContext(request, null, null);
//      Assert.IsTrue(Validator.TryValidateObject(request, context, results));   
      
//    }

//    [TestMethod]
//    public void ValidateSuccessAssetRequestBase()
//    {
//      var request = new AssetRequestBase { assetIdentifier = Guid.NewGuid().ToString() };
//      request.Validate();
//    }

//    [TestMethod]
//    public void ValidateFailInvalidGuid()
//    {
//      var request = new AssetRequestBase();
//      Assert.ThrowsException<ServiceException>(() => request.Validate());
//    }
//  }
//}
