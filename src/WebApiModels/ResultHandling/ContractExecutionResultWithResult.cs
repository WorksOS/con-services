using System;
using System.Collections.Generic;
using System.Text;
using VSS.Common.ResultsHandling;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling
{
    public class ContractExecutionResultWithResult : ContractExecutionResult
    {
      public bool Result { get; set; } = false;
    }
}
