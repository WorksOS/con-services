using System;
using VSS.TRex.Geometry;

namespace VSS.TRex.SiteModels.Interfaces
{
  public interface ISiteModelMetadataManager
  {
    ISiteModelMetadata Get(Guid siteModel);

    ISiteModelMetadata[] GetAll();

    void Add(Guid siteModelID, ISiteModelMetadata metaData);

    void Update(Guid siteModelID);

    void Update(Guid siteModelID, ISiteModelMetadata metaData);

    void Update(Guid siteModelID,
      BoundingWorldExtent3D siteModelExtent = null,
      string name = null, string description = null, DateTime? lastModifiedDate = null,
      int? machineCount = null, int? designCount = null, int? surveyedSurfaceCount = null);
  }
}
