using System;

namespace VSS.TRex.SiteModels.Interfaces
{
  public interface ISiteModelMetadataManager
  {
    ISiteModelMetadata Get(Guid siteModel);

    ISiteModelMetadata[] GetAll();

    void Add(Guid siteModelID, ISiteModelMetadata metaData);

    void Update(Guid siteModelID);

    void Update(Guid siteModelID, ISiteModelMetadata metaData);
  }
}
