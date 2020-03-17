namespace VSS.TRex.SubGrids.Interfaces
{
  public interface ISubGridProgressiveResponseRequest
  {
    /// <summary>
    /// Executes the request to send the payload to the originating node
    /// </summary>
    /// <param name="payload"></param>
    bool Execute(ISubGridProgressiveResponseRequestComputeFuncArgument payload);
  }
}
