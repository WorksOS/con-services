using System;
using System.Xml.Serialization;

namespace VSS.TRex.TAGFiles.Classes
{
  public class TAGFileMetaData
  {
    [XmlAttribute]
    public Guid? projectId;
    public Guid? assetId;
    public string tagFileName;
    public string tccOrgId;
    public bool IsJohnDoe;
  }
}
