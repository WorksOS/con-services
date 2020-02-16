using System;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon.Resources;
using VSS.UnitTest.Common;

namespace UnitTests
{
  [TestClass()]
  public class NHSvrTest : UnitTestBase
  {
    [TestMethod()]
    public void TestGetString()
    {
      string content = VLResourceManager.GetString("AlertMessageBody", "en-US");
      Assert.IsTrue(content.Length > 0, "Expected to get an AlertMessageBody for en-US");
      content = VLResourceManager.GetString("AlertMessageBody", "fr-FR");
      Assert.IsTrue(content.Length > 0, "Expected to get an AlertMessageBody for fr-FR");
    }

    [TestMethod()]
    public void AlertMessageBodyTest()
    {
      foreach (string lang in (from languages in Ctx.OpContext.LanguageReadOnly where languages.ID > 0 select languages.ISOName))
      {
        NHSvr.Culture = new CultureInfo(lang);
        string fmt = NHSvr.AlertMessageBody;

        try
        {
          string.Format(fmt, "0", "1", "2", "3", "4", "5","6");
        }
        catch (Exception)
        {
          Assert.IsTrue(false, string.Format("Problem with translation of AlertMessageBody for {0}",lang));
        }
      }
    }

    [TestMethod()]
    public void AlertMessageBodySMSTest()
    {
      foreach (string lang in (from languages in Ctx.OpContext.LanguageReadOnly where languages.ID > 0 select languages.ISOName))
      {
        NHSvr.Culture = new CultureInfo(lang);
        string fmt = NHSvr.AlertMessageBodySMS;

        try
        {
          string.Format(fmt, "0", "1", "2", "3");
        }
        catch (Exception)
        {
          Assert.IsTrue(false, string.Format("Problem with translation of AlertMessageBodySMS for {0}", lang));
        }
      }
    }

    [TestMethod()]
    public void AlertSubjectTest()
    {
      foreach (string lang in (from languages in Ctx.OpContext.LanguageReadOnly where languages.ID > 0 select languages.ISOName))
      {
        NHSvr.Culture = new CultureInfo(lang);
        string fmt = NHSvr.AlertSubject;

        try
        {
          string.Format(fmt, "0", "1", "2");
        }
        catch (Exception)
        {
          Assert.IsTrue(false, string.Format("Problem with translation of AlertSubject for {0}", lang));
        }
      }
    }

    [TestMethod()]      
    public void FaultAlertMessageBodyTest()
    {
      foreach (string lang in (from languages in Ctx.OpContext.LanguageReadOnly where languages.ID > 0 select languages.ISOName))
      {
        NHSvr.Culture = new CultureInfo(lang);
        string fmt = NHSvr.FaultAlertMessageBody;

        try
        {
          string.Format(fmt, "0","1","2","3","4","5","6","7","8");
        }
        catch (Exception)
        {
          Assert.IsTrue(false, string.Format("Problem with translation of FaultAlertMessageBody for {0}", lang));
        }
      }
    }

    [TestMethod()]
    public void FaultAlertMessageBodySMSTest()
    {
      foreach (string lang in (from languages in Ctx.OpContext.LanguageReadOnly where languages.ID > 0 select languages.ISOName))
      {
        NHSvr.Culture = new CultureInfo(lang);
        string fmt = NHSvr.FaultAlertMessageBodySMS;

        try
        {
          string.Format(fmt, "0", "1", "2", "3", "4");
        }
        catch (Exception)
        {
          Assert.IsTrue(false, string.Format("Problem with translation of FaultAlertMessageBodySMS for {0}", lang));
        }
      }
    }

    [TestMethod()]
    public void NewAccountRegEmailBodyTest()
    {
      foreach (string lang in (from languages in Ctx.OpContext.LanguageReadOnly where languages.ID > 0 select languages.ISOName))
      {
        NHSvr.Culture = new CultureInfo(lang);
        string fmt = NHSvr.NewAccountRegEmailBody;

        try
        {
          string.Format(fmt, "0", "1", "2", "3");
        }
        catch (Exception)
        {
          Assert.IsTrue(false, string.Format("Problem with translation of NewAccountRegEmailBody for {0}", lang));
        }
      }
    }

    [TestMethod()]
    public void NewAccountSupportEmailBodyTest()
    {
      foreach (string lang in (from languages in Ctx.OpContext.LanguageReadOnly where languages.ID > 0 select languages.ISOName))
      {
        NHSvr.Culture = new CultureInfo(lang);
        string fmt = NHSvr.NewAccountSupportEmailBody;

        try
        {
          string.Format(fmt, "0", "1", "2", "3","4");
        }
        catch (Exception)
        {
          Assert.IsTrue(false, string.Format("Problem with translation of NewAccountSupportEmailBody for {0}", lang));
        }
      }
    }
  }
}
