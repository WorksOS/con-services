using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VSS.UnitTest.Common.Contexts;
using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common.EntityBuilder
{
  public class DevicePersonalityBuilder
  {
    private Device _device;
    private PersonalityTypeEnum _personalityType = PersonalityTypeEnum.None;
    private string _value;

    public DevicePersonalityBuilder()
    {
    }

    public DevicePersonalityBuilder PersonalityType(PersonalityTypeEnum pt)
    {
      _personalityType = pt;
      return this;
    }

    public DevicePersonalityBuilder Value(string v)
    {
      _value = v;
      return this;
    }

    public DevicePersonalityBuilder ForDevice(Device d)
    {
      _device = d;
      return this;
    }

    public DevicePersonality Build()
    {
      var dp = new DevicePersonality();
      
      var pt = new PersonalityType();
      pt.ID = (int)_personalityType;
      pt.Name = "woteva";

      dp.fk_DeviceID = _device.ID;
      dp.fk_PersonalityTypeID = pt.ID;
      dp.Value = _value;

      return dp;
    }

    public virtual DevicePersonality Save()
    {
      DevicePersonality item = Build();

      ContextContainer.Current.OpContext.DevicePersonality.AddObject(item);
      ContextContainer.Current.OpContext.SaveChanges();

      return item;
    }
  }
}
