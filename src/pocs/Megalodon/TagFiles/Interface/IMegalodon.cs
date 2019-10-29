
namespace TagFiles.Interface
{
  public interface IMegalodon
  {
    void StartProcess(string ip, int port);
    void EndProcess();
  }
}
