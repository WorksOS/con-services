using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VSS.UnitTest.Common.Contexts;
using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common.EntityBuilder
{
  public class MakeBuilder
  {
    #region Make Fields

    private string _code = "TST";
    private string _name = "TEST MAKE";
    private DateTime _updateUTC = DateTime.UtcNow;
    #endregion

    public MakeBuilder Code(string code)
    {
      _code = code;
      return this;
    }

    public MakeBuilder Name(string name)
    {
      _name = name;
      return this;
    }

    public MakeBuilder UpdateUTC(DateTime updateUTC)
    {
      _updateUTC = updateUTC;
      return this;
    }
 
    public Make Build()
    {
      Make make = new Make();

      make.Code = _code;
      make.Name = _name;
      make.UpdateUTC = _updateUTC;
      
      return make;
    }
    public Make Save()
    {
      Make make = Build();

      ContextContainer.Current.OpContext.Make.AddObject(make);
      ContextContainer.Current.OpContext.SaveChanges();

      return make;
    }
  }
}
