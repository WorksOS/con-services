using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VSS.Common.Exceptions;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Validation;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApiTests.ProductionData.Models
{
  [TestClass]
  public class ProductionDataEditTests
  {
    [TestMethod]
    public void CanCreateEditDataRequestTest()
    {
      var validator = new DataAnnotationsValidator();
      ProductionDataEdit dataEdit = ProductionDataEdit.CreateProductionDataEdit(
             10538563, DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(-2), "Acme Dozer", 3);
      ICollection<ValidationResult> results;
      Assert.IsTrue(validator.TryValidate(dataEdit, out results));

      //missing machine details
      dataEdit = ProductionDataEdit.CreateProductionDataEdit(
             -1, DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(-2), "Acme Dozer", null);
      Assert.IsFalse(validator.TryValidate(dataEdit, out results));
    }

    [TestMethod]
    public void ValidateSuccessTest()
    {
      ProductionDataEdit dataEdit = ProductionDataEdit.CreateProductionDataEdit(
         10538563, DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(-2), "Acme Dozer", 3);
      dataEdit.Validate();
    }

    [TestMethod]
    public void ValidateFailMissingDataEditTest()
    {
      //missing edit data
      ProductionDataEdit dataEdit = ProductionDataEdit.CreateProductionDataEdit(
         10538563, DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(-2), null, null);
      Assert.ThrowsException<ServiceException>(() => dataEdit.Validate());
    }

    [TestMethod]
    public void ValidateFailInvalidDateRangeTest()
    {
      //startUTC > endUTC
      ProductionDataEdit dataEdit = ProductionDataEdit.CreateProductionDataEdit(
         10538563, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-5), "Acme Dozer", null);
      
      Assert.ThrowsException<ServiceException>(() => dataEdit.Validate());
    }
  }
}
