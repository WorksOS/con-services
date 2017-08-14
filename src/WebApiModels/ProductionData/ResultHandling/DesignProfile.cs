using System.Collections.Generic;
using VSS.Velociraptor.PDSInterface.DesignProfile;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  public class DesignProfile : BaseProfile
  {
    public List<DesignProfileVertex> vertices;
    public int importedFileTypeID;
  }
}