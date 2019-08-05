using System;
using System.IO;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Designs.Interfaces;

namespace VSS.TRex.Designs.Factories
{
  public class DesignClassFactory : IDesignClassFactory
  {
    public IDesignBase NewInstance(string fileName, double cellSize, Guid siteModelUid)
    {
      IDesignBase result;

      if (string.Compare(Path.GetExtension(fileName), ".ttm", StringComparison.InvariantCultureIgnoreCase) == 0)
        result = new TTMDesign(cellSize);
      else if (string.Compare(Path.GetExtension(fileName), ".svl", StringComparison.InvariantCultureIgnoreCase) == 0)
        result = new SVLAlignmentDesign(cellSize);
      else
        throw new TRexException($"Unknown design file type in design class factory for design {fileName}");

      result.FileName = fileName;
      result.ProjectUid = siteModelUid;

      return result;
    }
  }
}
