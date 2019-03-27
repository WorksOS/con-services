namespace TCCToDataOcean.Utils
{
  public interface IEnvironmentHelper
  {
    string GetVariable(string key, int errorNumber);
  }
}
