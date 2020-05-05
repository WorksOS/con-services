using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCSS.IntegrationTests.Utils.Types;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace CCSS.Tile.Service.IntegrationTests
{
  public class RequestResult : IEquatable<RequestResult>
  {
    public RequestResultCode Code { get; set; }
    public string Message { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContractExecutionResult" /> class.
    /// </summary>
    /// <param name="code">The resulting code. Default value is <see cref="ContractExecutionStates.ExecutedSuccessfully" /></param>
    /// <param name="message">The verbose user-friendly message. Default value is empty string.</param>
    protected RequestResult(RequestResultCode code, string message = "")
    {
      Code = code;
      Message = message;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContractExecutionResult" /> class with default
    ///     <see cref="ContractExecutionStates.ExecutedSuccessfully" /> result
    /// </summary>
    /// <param name="message">The verbose user-friendly message.</param>
    protected RequestResult(string message)
        : this(RequestResultCode.ExecutedSuccessfully, message)
    {
    }

    /// <summary>
    /// Default constructor for serialization
    /// </summary>
    public RequestResult()
    {

    }

    #region Equality test
    public bool Equals(RequestResult other)
    {
      if (other == null)
        return false;

      return this.Code == other.Code &&
             this.Message == other.Message;
    }

    public static bool operator ==(RequestResult a, RequestResult b)
    {
      if ((object)a == null || (object)b == null)
        return Object.Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(RequestResult a, RequestResult b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is RequestResult && this == (RequestResult)obj;
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }
    #endregion

    #region ToString override
    /// <summary>
    /// ToString override
    /// </summary>
    /// <returns>A string representation.</returns>
    public override string ToString()
    {
      return JsonConvert.SerializeObject(this, Formatting.Indented);
    }
    #endregion




  }
}
