using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Raptor.Service.WebApiModels.ProductionData.Models;
using VSS.Raptor.Service.Common.ResultHandling;

namespace VSS.Raptor.Service.WebApiTests.ProductionData.Models
{
   [TestClass()]
  public class EditDataRequestTest
  {
     [TestMethod()]
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

     [TestMethod()]
     public void ValidateSuccessTest()
     {
       EditDataRequest request = EditDataRequest.CreateEditDataRequest(
                 projectId, false, dataEdit);
       request.Validate();
     }

     [TestMethod()]
     [ExpectedException(typeof(ServiceException))]
     public void ValidateFailMissingDataEditTest()
     {
       //missing dataEdit
       EditDataRequest request = EditDataRequest.CreateEditDataRequest(projectId, false, null);
       request.Validate();

     }

     private long projectId = 1234;
     private ProductionDataEdit dataEdit = ProductionDataEdit.CreateProductionDataEdit(
       10538563, DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(-2), "Acme Dozer", null);


  }
}
