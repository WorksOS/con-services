using System;

namespace VSS.TRex.Common.Interfaces.Interfaces
{
  // Defines a base Interface for ISiteModel to prevent circular references between VSS.TRex.SiteModels.Interfaces and VSS.TRex.Designs.interfaces
  public interface ISiteModelBase
  {
    Guid ID { get; set; }

    double CellSize { get; }
  }
}
