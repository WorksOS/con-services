namespace VSS.UnifiedProductivity.Service.Utils
{
  public interface IConfigurationStore
  {
    string GetValueString(string v);
    bool? GetValueBool(string v);
    int GetValueInt(string v);
  }
}