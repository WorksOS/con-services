namespace VSS.TagFileAuth.Service.Repositories.Interfaces
{
    public interface IRepositoryFactory
    {
      IAssetRepository GetAssetRepository();
    }
}