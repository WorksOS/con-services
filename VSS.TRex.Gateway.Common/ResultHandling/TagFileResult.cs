using System;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.TRex.Rendering.Implementations.Core2;

namespace VSS.TRex.Gateway.Common.ResultHandling
{
  public class TagFileResult : ContractExecutionResult
  {

    /// <summary>
    /// Private constructor
    /// </summary>
    private TagFileResult()
    { }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static TagFileResult Create(int code, String message)
    {
      return new TagFileResult()
             {
                 Code = code,
                 Message = message
             };

    }
  }
}

