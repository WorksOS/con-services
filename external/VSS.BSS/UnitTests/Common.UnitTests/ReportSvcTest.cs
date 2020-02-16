using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Transactions;
using VSS.Nighthawk.ServicesAPI;
using EM = VSS.Nighthawk.EntityModels;
using VSS.Nighthawk.NHWebServices;
using System.Collections.Generic;
using CS = VSS.Nighthawk.NHCommonServices;
using System;
using System.Linq;
using VSS.Nighthawk.EntityModels;
using VSS.Nighthawk.NHCommonServices;

namespace UnitTests
{
  [TestClass()]
  public class ReportSvcTest : ServerAPITestBase
  {
    #region Additional test attributes
    // 
    //You can use the following additional attributes as you write your tests:
    //
    //Use ClassInitialize to run code before running the first test in the class
    //[ClassInitialize()]
    //public static void MyClassInitialize(TestContext testContext)
    //{
    //}
    //
    //Use ClassCleanup to run code after all tests in a class have run
    //[ClassCleanup()]
    //public static void MyClassCleanup()
    //{
    //}
    //
    //Use TestInitialize to run code before running each test
    //[TestInitialize()]
    //public void MyTestInitialize()
    //{
    //}
    //
    //Use TestCleanup to run code after each test has run
    //[TestCleanup()]
    //public void MyTestCleanup()
    //{
    //}
    //
    #endregion

    [TestMethod()]
    public void GetUtilizationReportUrl()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        EM.ActiveUser me = Login();
        SessionContext session = API.Session.Validate(me.SessionID);

        EM.Asset asset1 = CreateAssetWithDevice(session, session.CustomerID.Value, "TimeFence1", EM.DeviceTypeEnum.PL321, null);
        EM.Asset asset2 = CreateAssetWithDevice(session, session.CustomerID.Value, "TimeFence2", EM.DeviceTypeEnum.PL321, null);
        CreateAssetSubscription(session, asset1.ID);
        CreateAssetSubscription(session, asset2.ID);
        //Sync NH_OP.Asset with NH_RPT.DimAsset
        PopulateDimTables();

        DateTime now = DateTime.Now;

        ReportSvc rptSvc = new ReportSvc();
        ReportParameters parameters = new ReportParameters()
        {
          sessionID = session.SessionID,
          reportStart = new CS.CalendarDay(now.AddDays(-7).Date),
          reportEnd = new CS.CalendarDay(now.Date),
          locale = "en_US",
          unitsTypeID = (int)EM.UnitsTypeEnum.US,
          showAssetID = true,
          dateFormat = "DD-MM-YYYY",
          decimalSeparator = ".",
          thousandsSeparator = ",",
          assetIDs = new List<long>() { asset1.ID, asset2.ID }
        };
        string rptSessionID = rptSvc.SaveUtilizationReportParameters(me.SessionID, (int)EM.ReportTypeEnum.AssetUtilization, parameters);
        ValidateParams(rptSessionID, parameters);

        string url = rptSvc.GetUtilizationReportUrl(me.SessionID, (int)EM.ReportTypeEnum.AssetUtilization, rptSessionID);
        Assert.IsTrue(!string.IsNullOrEmpty(url), "Should return a report url");

        string amp = "&";
        string actualReportSessionID = ExtractParam(url, amp, "ReportSessionID=");
        Assert.AreEqual(rptSessionID, actualReportSessionID, "Wrong report sessionID in report url");
      }
    }

    [TestMethod()]
    public void GetFleetReportUrl()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        EM.ActiveUser me = Login();
        SessionContext session = API.Session.Validate(me.SessionID);

        EM.Asset asset1 = CreateAssetWithDevice(session, session.CustomerID.Value, "TimeFence1", EM.DeviceTypeEnum.PL321, null);
        EM.Asset asset2 = CreateAssetWithDevice(session, session.CustomerID.Value, "TimeFence2", EM.DeviceTypeEnum.PL321, null);
        CreateAssetSubscription(session, asset1.ID);
        CreateAssetSubscription(session, asset2.ID);
        //Sync NH_OP.Asset with NH_RPT.DimAsset
        PopulateDimTables();

        DateTime now = DateTime.Now;

        ReportSvc rptSvc = new ReportSvc();
        ReportParameters parameters = new ReportParameters()
        {
          sessionID = session.SessionID,
          reportStartUTC = ServicesUtility.ToFlexUTC(now.AddDays(-7)),
          reportEndUTC = ServicesUtility.ToFlexUTC(now.Date),
          locale = "en_US",
          unitsTypeID = (int)EM.UnitsTypeEnum.US,
          showAssetID = true,
          dateFormat = "DD-MM-YYYY",
          decimalSeparator = ".",
          thousandsSeparator = ",",
          dateSeparator = "/",
          timeSeparator = ":",
          standardName = "Mountain Standard Time",
          includeHoursLocation = true,
          groupBySingleOperator = true,
          operatorID = 123232,
          assetIDs = new List<long>() { asset1.ID, asset2.ID }
        };
        string rptSessionID = rptSvc.SaveFleetReportParameters(me.SessionID, (int)EM.ReportTypeEnum.AssetHistory, parameters);
        ValidateParams(rptSessionID, parameters);
        
        string url = rptSvc.GetFleetReportUrl(me.SessionID, (int)EM.ReportTypeEnum.AssetHistory, rptSessionID);
        Assert.IsTrue(!string.IsNullOrEmpty(url), "Should return a report url");

        string amp = "&";
        string actualReportSessionID = ExtractParam(url, amp, "ReportSessionID=");
        Assert.AreEqual(rptSessionID, actualReportSessionID, "Wrong report sessionID in report url");
      }
    }

    [TestMethod()]
    public void GetHealthReportUrl()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        EM.ActiveUser me = Login();
        SessionContext session = API.Session.Validate(me.SessionID);

        EM.Asset asset1 = CreateAssetWithDevice(session, session.CustomerID.Value, "TimeFence1", EM.DeviceTypeEnum.PL321, null);
        EM.Asset asset2 = CreateAssetWithDevice(session, session.CustomerID.Value, "TimeFence2", EM.DeviceTypeEnum.PL321, null);
        CreateAssetSubscription(session, asset1.ID);
        CreateAssetSubscription(session, asset2.ID);
        //Sync NH_OP.Asset with NH_RPT.DimAsset
        PopulateDimTables();

        DateTime now = DateTime.Now;

        ReportSvc rptSvc = new ReportSvc();
        ReportParameters parameters = new ReportParameters()
        {
          sessionID = session.SessionID,
          reportStartUTC = ServicesUtility.ToFlexUTC(now.AddDays(-7).Date),
          reportEndUTC = ServicesUtility.ToFlexUTC(now.Date),
          locale = "en_US",
          unitsTypeID = (int)EM.UnitsTypeEnum.US,
          showAssetID = true,
          dateFormat = "DD-MM-YYYY",
          decimalSeparator = ".",
          thousandsSeparator = ",",
          dateSeparator = "/",
          timeSeparator = ":",
          clockIndicator = 12,
          standardName = "Mountain Standard Time",
          eventsSelected = true,
          diagnosticsSelected = true,
          severityHighSelected = true,
          severityMediumSelected = true,
          severityLowSelected = true,
          groupBySingleOperator = true,
          operatorID = 1232324,
          assetIDs = new List<long>() { asset1.ID, asset2.ID }
        };
        string rptSessionID = rptSvc.SaveHealthReportParameters(me.SessionID, (int)EM.ReportTypeEnum.AssetHealth, parameters);
        ValidateParams(rptSessionID, parameters);
        
        string url = rptSvc.GetHealthReportUrl(me.SessionID, (int)EM.ReportTypeEnum.AssetHealth, rptSessionID);
        Assert.IsTrue(!string.IsNullOrEmpty(url), "Should return a report url");

        string amp = "&";
        string actualReportSessionID = ExtractParam(url, amp, "ReportSessionID=");
        Assert.AreEqual(rptSessionID, actualReportSessionID, "Wrong report sessionID in report url");
      }
    }

    [TestMethod()]
    public void GetAssetsSupported()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        EM.ActiveUser me = Login();
        SessionContext session = API.Session.Validate(me.SessionID);

        EM.Asset asset1 = CreateAssetWithDevice(session, session.CustomerID.Value, "TimeFence1", EM.DeviceTypeEnum.PL321, null);
        EM.Asset asset2 = CreateAssetWithDevice(session, session.CustomerID.Value, "123454321", EM.DeviceTypeEnum.MTS522523, null);
        EM.Asset asset3 = CreateAssetWithDevice(session, session.CustomerID.Value, "TimeFence3", EM.DeviceTypeEnum.PL321, null);
        EM.Asset asset4 = CreateAssetWithDevice(session, session.CustomerID.Value, "xyz123", EM.DeviceTypeEnum.PL121, null);

        CreateAssetSubscription(session, asset1.ID, EM.ServiceTypeEnum.VLCORE);
        CreateAssetSubscription(session, asset1.ID, EM.ServiceTypeEnum.CATHEALTH);
        CreateAssetSubscription(session, asset2.ID, EM.ServiceTypeEnum.VLCORE);
        // NOTE: we are not assigning asset3 the VL Core subscription, but we are
        // assigning it some other subscription
        CreateAssetSubscription(session, asset3.ID, EM.ServiceTypeEnum.CATMAINT);
        CreateAssetSubscription(session, asset4.ID, EM.ServiceTypeEnum.VLCORE);
        CreateAssetSubscription(session, asset4.ID, EM.ServiceTypeEnum.CATHEALTH);
        CreateAssetSubscription(session, asset4.ID, EM.ServiceTypeEnum.CATUTIL);

        WorkingSetSaver.Populate(session.SessionID);

        ReportSvc rptSvc = new ReportSvc();

        List<SelectedAsset> assets = rptSvc.GetUtilizationAssetsSupported(session.SessionID, (int)EM.ReportTypeEnum.AssetUtilization, 
          new List<long>() { asset1.ID, asset2.ID, asset3.ID, asset4.ID });
        // other than the basic core plans, we are not currently filtering on service plans, so I changed 2 to 3 in the following line:
        Assert.AreEqual(3, assets.Count, "Wrong number of assets supported for asset utilization report");
        bool found1 = false;
        bool found2 = false;
        bool found3 = false;
        bool found4 = false;
        foreach (SelectedAsset asset in assets)
        {
          if (asset.ID == asset1.ID) found1 = true;
          else if (asset.ID == asset2.ID) found2 = true;
          else if (asset.ID == asset3.ID) found3 = true;
          else if (asset.ID == asset4.ID) found4 = true;
        }
        Assert.IsTrue(found1, "Asset1 should be supported for utilization report");
        Assert.IsTrue(found2, "Asset2 should be supported for utilization report");
        Assert.IsFalse(found3, "Asset3 should NOT be supported for utilization report");
        Assert.IsTrue(found4, "Asset4 should be supported for utilization report");
      }
    }

    [TestMethod()]
    public void SaveFleetReportParameters()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        EM.ActiveUser me = Login();
        SessionContext session = API.Session.Validate(me.SessionID);

        EM.Asset asset1 = CreateAssetWithDevice(session, session.CustomerID.Value, "TimeFence1", EM.DeviceTypeEnum.PL321, null);
        EM.Asset asset2 = CreateAssetWithDevice(session, session.CustomerID.Value, "TimeFence2", EM.DeviceTypeEnum.PL321, null);
        EM.Asset asset3 = CreateAssetWithDevice(session, session.CustomerID.Value, "TimeFence3", EM.DeviceTypeEnum.PL321, null);
        CreateAssetSubscription(session, asset1.ID);
        CreateAssetSubscription(session, asset2.ID);
        CreateAssetSubscription(session, asset3.ID);
        //Sync NH_OP.Asset with NH_RPT.DimAsset
        PopulateDimTables();

        ReportSvc rptSvc = new ReportSvc();
        ReportParameters parameters = new ReportParameters()
        {
          sessionID = session.SessionID,
          reportStartUTC = ServicesUtility.ToFlexUTC(DateTime.UtcNow.AddDays(-10)),
          reportEndUTC = ServicesUtility.ToFlexUTC(DateTime.UtcNow),
          locale = "en_US",
          unitsTypeID = (int)EM.UnitsTypeEnum.US,
          showAssetID = true,
          dateFormat = "MM/DD/YYYY",
          decimalSeparator = ".",
          thousandsSeparator = ",",
          dateSeparator = "/",
          timeSeparator = ":",
          standardName = "Mountain Standard Time",
          assetIDs = new List<long>() { asset1.ID, asset2.ID }
        };

        string rptSessionID = rptSvc.SaveFleetReportParameters(me.SessionID, (int)ReportTypeEnum.AssetHistory, parameters);
        ValidateParams(rptSessionID, parameters, 1);
 
        //French
        parameters = new ReportParameters()
        {
          sessionID = session.SessionID,
          reportStartUTC = ServicesUtility.ToFlexUTC(DateTime.UtcNow.AddDays(-30)),
          reportEndUTC = ServicesUtility.ToFlexUTC(DateTime.UtcNow.AddDays(-7)),
          locale = "fr_FR",
          unitsTypeID = (int)EM.UnitsTypeEnum.Metric,
          showAssetID = false,
          dateFormat = "YYYY-MM-DD",
          decimalSeparator = ",",
          thousandsSeparator = ".",
          dateSeparator = "-",
          timeSeparator = ".",
          standardName = "Central European Time",
          assetIDs = new List<long>() { asset1.ID, asset3.ID }
        };

        rptSessionID = rptSvc.SaveFleetReportParameters(me.SessionID, (int)ReportTypeEnum.AssetHistory, parameters);
        ValidateParams(rptSessionID, parameters, 2);
      }
    }

    [TestMethod()]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SaveInvalidReportParameters()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        EM.ActiveUser me = Login();
        SessionContext session = API.Session.Validate(me.SessionID);

        EM.Asset asset1 = CreateAssetWithDevice(session, session.CustomerID.Value, "TimeFence1", EM.DeviceTypeEnum.PL321, null);
        EM.Asset asset2 = CreateAssetWithDevice(session, session.CustomerID.Value, "TimeFence2", EM.DeviceTypeEnum.PL321, null);
        CreateAssetSubscription(session, asset1.ID);
        CreateAssetSubscription(session, asset2.ID);
        //Sync NH_OP.Asset with NH_RPT.DimAsset
        PopulateDimTables();

        ReportSvc rptSvc = new ReportSvc();
        ReportParameters parameters = new ReportParameters()
        {
          sessionID = session.SessionID,
          reportStartUTC = ServicesUtility.ToFlexUTC(DateTime.UtcNow.AddDays(-10)),
          reportEndUTC = ServicesUtility.ToFlexUTC(DateTime.UtcNow),
          locale = null,//invalid value
          unitsTypeID = 0,
          showAssetID = true,
          dateFormat = "MM/DD/YYYY",
          decimalSeparator = ".",
          thousandsSeparator = ",",
          dateSeparator = "/",
          timeSeparator = ":",
          standardName = "Mountain Standard Time",
          assetIDs = null //invalid value
        };

        string rptSessionID = rptSvc.SaveFleetReportParameters(me.SessionID, (int)ReportTypeEnum.AssetHistory, parameters);
      }
    }

    private string ExtractParam(string url, string amp, string paramName)
    {
      string param = amp + paramName;
      int indx1 = url.IndexOf(param) + param.Length;
      int indx2 = url.IndexOf(amp, indx1);
      if (indx2 == -1)
        indx2 = url.Length;
      string actualParam = url.Substring(indx1, indx2 - indx1);
      return actualParam;
    }

    private void ValidateParams(string rptSessionID, ReportParameters expected, int index=0)
    {   
      string context = index > 0 ? index.ToString() : "";
      Assert.IsFalse(string.IsNullOrEmpty(rptSessionID), "Should be able to save report parameters " + context);

      using (EM.NH_RPT ctx = EM.Model.NewNHContext<EM.NH_RPT>())
      {
        var rptParams = (from rp in ctx.ReportParameterReadOnly
                         where rp.ReportSessionID == rptSessionID
                         select rp).FirstOrDefault();
        Assert.IsNotNull(rptParams, "Report parameters should not be null " + context);

        ReportParameters actual = new ReportParameters(rptParams.XMLData);
        Assert.AreEqual<string>(expected.ToString(), actual.ToString(), "Wrong report parameters " + context);
      }
    }
  }
}
