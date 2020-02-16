using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VSS.UnitTest.Common.Contexts;
using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common.EntityBuilder
{
  public class LanguageBuilder
  {
    #region Language Fields

    private string _isoName = "English";
    #endregion

    public LanguageBuilder ISOName(string isoName)
    {
      _isoName = isoName;
      return this;
    }


    public Language Build()
    {
      Language Language = new Language();

      Language.ISOName = _isoName;
      return Language;
    }
    public Language Save()
    {
      Language Language = Build();

      ContextContainer.Current.OpContext.Language.AddObject(Language);
      ContextContainer.Current.OpContext.SaveChanges();

      return Language;
    }
  }
}

