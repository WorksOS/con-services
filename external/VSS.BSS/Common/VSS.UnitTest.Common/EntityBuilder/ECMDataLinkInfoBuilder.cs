using VSS.UnitTest.Common.Contexts;
using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common.EntityBuilder
{
  public class ECMDataLinkInfoBuilder
  {
    private long _id = IdGen.GetId();
    private ECMInfo _ecmInfo = null;
    private Datalink _datalink= null;
    private MID _mid = null;
    private short? _svcToolSupportChangeLevel;
    private short? _applicationLevel;

    public virtual ECMDataLinkInfoBuilder ECMInfo(ECMInfo ecmInfo)
    {
      _ecmInfo = ecmInfo;
      return this;
    }

    public virtual ECMDataLinkInfoBuilder Datalink(Datalink datalink)
    {
      _datalink = datalink;
      return this;
    }

    public virtual ECMDataLinkInfoBuilder MID(MID mid)
    {
      _mid = mid;
      return this;
    }

    public virtual ECMDataLinkInfoBuilder SvcToolSupportChangeLevel(short svcToolSupportChangeLevel)
    {
      _svcToolSupportChangeLevel = svcToolSupportChangeLevel;
      return this;
    }

    public virtual ECMDataLinkInfoBuilder ApplicationLevel(short applicationLevel)
    {
      _applicationLevel = applicationLevel;
      return this;
    }

    public ECMDatalinkInfo Build()
    {
      ECMDatalinkInfo ecmDatalinkInfo = new ECMDatalinkInfo();

      ecmDatalinkInfo.ID = _id;
      ecmDatalinkInfo.fk_ECMInfoID = _ecmInfo.ID;
      ecmDatalinkInfo.fk_DatalinkID = _datalink.ID;
      ecmDatalinkInfo.MID = _mid;

      if (_svcToolSupportChangeLevel.HasValue) ecmDatalinkInfo.SvcToolSupportChangeLevel = _svcToolSupportChangeLevel.Value;
      if (_applicationLevel.HasValue) ecmDatalinkInfo.ApplicationLevel = _applicationLevel.Value;

      return ecmDatalinkInfo;
    }

    public virtual ECMDatalinkInfo Save()
    {
      ECMDatalinkInfo ecmDataLinkInfo = Build();

      ContextContainer.Current.OpContext.ECMDatalinkInfo.AddObject(ecmDataLinkInfo);
      ContextContainer.Current.OpContext.SaveChanges();

      return ecmDataLinkInfo;
    }


  }
}
