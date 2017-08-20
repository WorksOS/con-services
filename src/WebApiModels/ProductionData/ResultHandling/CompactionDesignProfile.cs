using System.Collections.Generic;
using VSS.Velociraptor.PDSInterface.DesignProfile;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  public class CompactionDesignProfile : BaseDesignProfile
  {
    public List<DesignProfileVertex> Vertices;
    public int IimportedFileTypeID;
  }
}