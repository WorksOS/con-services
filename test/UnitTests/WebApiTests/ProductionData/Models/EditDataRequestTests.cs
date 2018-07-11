using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Validation;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApiTests.ProductionData.Models
{
   [TestClass]
  public class EditDataRequestTests
  {
     [TestMethod]
     public void CanCreateEditDataRequestTest()
     {
       var validator = new DataAnnotationsValidator();
       EditDataRequest request = EditDataRequest.CreateEditDataRequest(
                 projectId, false, dataEdit);
       ICollection<ValidationResult> results;
       Assert.IsTrue(validator.TryValidate(request, out results));

       //missing project id
       request = EditDataRequest.CreateEditDataRequest(-1, false, dataEdit);
       Assert.IsFalse(validator.TryValidate(request, out results));
     }

     [TestMethod]
     public void ValidateSuccessTest()
     {
       EditDataRequest request = EditDataRequest.CreateEditDataRequest(
                 projectId, false, dataEdit);
       request.Validate();
     }

     [TestMethod]
     public void ValidateFailMissingDataEditTest()
     {
       //missing dataEdit
       EditDataRequest request = EditDataRequest.CreateEditDataRequest(projectId, false, null);
       Assert.ThrowsException<ServiceException>(() => request.Validate());
     }

    private long projectId = 1234;
     private ProductionDataEdit dataEdit = ProductionDataEdit.CreateProductionDataEdit(
       10538563, DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(-2), "Acme Dozer", null);


  }
}
