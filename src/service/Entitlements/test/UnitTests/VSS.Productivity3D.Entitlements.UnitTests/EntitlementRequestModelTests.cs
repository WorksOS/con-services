using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Entitlements.Abstractions.Models.Request;
using VSS.Productivity3D.Productivity3D.Models.Validation;
using Xunit;

namespace VSS.Productivity3D.Entitlements.UnitTests
{
  public class EntitlementRequestModelTests
  {
    [Fact]
    public void EntitlementRequestModel_MissingRequiredValues()
    {
      var request = new EntitlementRequestModel();
      ICollection<ValidationResult> results;
      var validator = new DataAnnotationsValidator();
      Assert.False(validator.TryValidate(request, out results));
      Assert.Equal(4, results.Count);
    }

    [Fact]
    public void EntitlementRequestModel_MissingJwtUserUid()
    {
      var request = new EntitlementRequestModel();
      var result = request.Validate(null);
      Assert.Equal(ContractExecutionStatesEnum.ValidationError, result.Code);
      Assert.Equal("JWT uuid is empty.", result.Message);
    }

    [Fact]
    public void EntitlementRequestModel_DifferentJwtUserUid()
    {
      var request = new EntitlementRequestModel{UserUid = Guid.NewGuid().ToString()};
      var result = request.Validate(Guid.NewGuid().ToString());
      Assert.Equal(ContractExecutionStatesEnum.ValidationError, result.Code);
      Assert.Equal("Provided uuid does not match JWT.", result.Message);
    }
  }
}
