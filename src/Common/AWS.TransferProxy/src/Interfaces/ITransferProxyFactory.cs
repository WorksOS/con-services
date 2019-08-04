namespace VSS.AWS.TransferProxy.Interfaces
{
  public interface ITransferProxyFactory
  {
    ITransferProxy NewProxy(string storageKey);
  }
}
