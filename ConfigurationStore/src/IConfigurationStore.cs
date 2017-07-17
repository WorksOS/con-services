namespace VSS.ConfigurationStore
{
    public interface IConfigurationStore
    {
        string GetValueString(string v);
        bool? GetValueBool(string v);
        int GetValueInt(string v);
        string GetConnectionString(string connectionType);
    }
}