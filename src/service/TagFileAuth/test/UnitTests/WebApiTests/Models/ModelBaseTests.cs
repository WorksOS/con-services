using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;

namespace WebApiTests.Models
{
  [TestClass]
  public class ModelBaseTests
  {
    protected string RaptorExceptionTemplate = @"""Result"":false,""Code"":{0},""Message"":""{1}""}}";
    protected string TrexExceptionTemplate = @"""Code"":{0},""Message"":""{1}""}}";
    protected static ContractExecutionStatesEnum contractExecutionStatesEnum = new ContractExecutionStatesEnum();

    [TestInitialize]
    public virtual void InitTest()
    {
    }
  }
}