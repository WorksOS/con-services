using System;
using VSS.MasterData.Models.ResultHandling.Abstractions;

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

