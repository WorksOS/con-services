//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using VSS.Hosted.VLCommon;
//using VSS.UnitTest.Common;
//using System.Linq;

//namespace UnitTests
//{
//  [TestClass]
//  public class SpatialTzTest : UnitTestBase
//  {
//    int year;
    
//    public SpatialTzTest()
//    {
//      year = DateTime.UtcNow.Year -1;      // some of the tests go forward from today. The ETLs don't support future dates. 
//    }

//    [TestMethod]
//    public void TestNormalUSTimeZoneDaylightSavingTime()
//    {
//      // Trimble Westminster Office, Mountain Time Zone
//      double latitude = 39.8979334114877;
//      double longitude = -105.113026865304;
//      // year/09/01 4:15:00 PM UTC
//      DateTime dateDuringDaylightSavingTimeUTC = new DateTime(year, 09, 01, 16, 15, 00);
//      // year/09/01 10:15:00 AM (Mountain Time is 6 hours behind UTC during Daylight Saving Time)
//      DateTime dateDuringDaylightSavingTimeLocal = new DateTime(year, 09, 01, 10, 15, 00);
//      DateTime? result = API.SpatialTimeZone.ToLocal(dateDuringDaylightSavingTimeUTC, latitude, longitude);
//      Assert.IsNotNull(result, "Expect a good local time");
//      Assert.AreEqual(dateDuringDaylightSavingTimeLocal, result.Value);
//    }

//    [TestMethod]
//    public void TestNormalUSTimeZoneStandardTime()
//    {
//      // Trimble Westminster Office, Mountain Time Zone
//      double latitude = 39.8979334114877;
//      double longitude = -105.113026865304;
//      // year/12/01 4:15:00 PM UTC
//      DateTime dateDuringStandardTimeUTC = new DateTime(year, 12, 01, 16, 15, 00);
//      // year/12/01 09:15:00 AM (Mountain Time is 7 hours behind UTC during Standard Time)
//      DateTime dateDuringStandardTimeLocal = new DateTime(year, 12, 01, 09, 15, 00);
//      DateTime? result = API.SpatialTimeZone.ToLocal(dateDuringStandardTimeUTC, latitude, longitude);
//      Assert.IsNotNull(result, "Expect a good local time");
//      Assert.AreEqual(dateDuringStandardTimeLocal, result.Value);
//    }

//    [TestMethod]
//    [DatabaseTest]
//    [Ignore]
//    //This test case can be run individually as the timezone list and chinaTimeZoneConfig list are static
//    public void TestChinaTimeZone()
//    {

//      //China Lat long
//      double latitude = 36.0272674560547;
//      double longitude = 120.21747827148;
//      DateTime dateUTC = new DateTime(DateTime.UtcNow.Year, 12, 01, 10, 15, 00);
//      // year/12/01 10:15:00 AM (China time is 8 hours ahead UTC during Standard Time)

//      CreateTimeZone_China();

//      CreateConfigSettings_China();

//      DateTime dateLocal = new DateTime(2013, 12, 01, 18, 15, 00);
//      DateTime? localTimeResult = API.SpatialTimeZone.ToLocal(dateUTC, latitude, longitude);
//      Assert.IsNotNull(localTimeResult, "Expect a good local time");
//      Assert.AreEqual(dateLocal, localTimeResult.Value);

//      int? tzID = API.SpatialTimeZone.GetTimeZoneID(latitude, longitude, dateUTC);
//      Assert.IsNotNull(tzID, "Time Zone Id is expected");

//      string result = API.SpatialTimeZone.GetTimeZoneAbbreviation(latitude, longitude, dateUTC);
//      Assert.AreEqual<string>("CST", result, "Should be China Time timezone");

//      int stdBias = API.SpatialTimeZone.GetStandardBias(latitude, longitude, dateUTC);
//      Assert.AreEqual(480, stdBias, "Standard bias for china should be 480 ");

//      int dstBias = API.SpatialTimeZone.GetDaylightSavingBias(latitude, longitude, dateUTC);
//      Assert.AreEqual(0, dstBias, "No Daylight savings for China");
//    }

//    [TestMethod]
//    [DatabaseTest]
//    public void TestLatestEventPopulate_China()
//    {
//      var target = new EquipmentAPI();
//      Customer customer = Entity.Customer.Dealer.Save();
//      const string makeCode = "CAT";
//      var familyDescription = "familyDesc";
//      const int year = 2010;
//      var modelDescription = "ModelDescription";
//      var testDate = GetValidTestDate();

//      var device = TestData.TestMTS522;
//      Asset testAsset = target.Create(Ctx.OpContext, "AssetA", makeCode, "AssetA", device.ID,
//                                   (DeviceTypeEnum)device.fk_DeviceTypeID,
//                                   familyDescription, modelDescription, year, Guid.NewGuid());
//      Entity.Service.Essentials.ForDevice(device).WithView(
//        view => view.ForCustomer(customer).ForAsset(testAsset).EndsOn(testDate.AddDays(+3))).Save();

//      DateTime eventUTC = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, DateTime.UtcNow.Second);

//      CreateTimeZone_China();

//      CreateConfigSettings_China();

//      DataHoursLocation dhl = new DataHoursLocation
//      {
//        fk_DimSourceID = (int)DimSourceEnum.TelematicsSync,
//        EventUTC = eventUTC,
//        UpdateUTC = DateTime.UtcNow,
//        InsertUTC = DateTime.UtcNow,
//        AssetID = testAsset.AssetID,

//        RuntimeHours = 100,
//        Latitude = 36.0272674560547,//China Lat Long
//        Longitude = 120.21747827148,
//        Altitude = null,
//        SpeedMPH = null,
//        LocIsValid = true,

//        DebugRefID = null,
//        SourceMsgID = null,

//        OdometerMiles = null,
//        ifk_DimTimeZoneID = null
//      };
//      Ctx.DataContext.DataHoursLocation.AddObject(dhl);
//      Ctx.DataContext.SaveChanges();

//      Helpers.ExecuteStoredProcedure(Database.NH_RPT, "uspPub_DimAsset_Populate");
//      Helpers.ExecuteStoredProcedure(Database.NH_RPT, "uspPub_LatestEvents_PopulateA");

//      using (INH_RPT ctx = ObjectContextFactory.NewNHContext<INH_RPT>())
//      {
//        var rptHourLocation = (from hour in ctx.HoursLocationReadOnly
//                               where hour.ifk_DimAssetID == testAsset.AssetID
//                               select hour).FirstOrDefault();

//        var tzChina = (from tz in ctx.DimTimeZoneReadOnly
//                       where tz.StdAbbrev == "CST" && tz.StdBias == 480
//                       && tz.YearUTC == DateTime.UtcNow.Year && tz.DstBias == null
//                       select tz.ID).FirstOrDefault();

//        Assert.AreEqual(tzChina, rptHourLocation.ifk_DimTimeZoneID);
//      }
//    }

//    [TestMethod]
//    public void TestOnBorderOfTwoTimeZones()
//    {
//      // Point on Mountain Time side of border between Mountain and Central Time Zones
//      double latitude = 37.888175;
//      double longitude = -101.541995;
//      // year/09/01 4:15:00 PM UTC
//      DateTime dateUTC = new DateTime(year, 09, 01, 16, 15, 00);
//      // year/09/01 10:15:00 AM (Mountain Time is 6 hours behind UTC during Daylight Saving Time)
//      DateTime dateMountainTimeLocal = new DateTime(year, 09, 01, 10, 15, 00);
//      DateTime? result = API.SpatialTimeZone.ToLocal(dateUTC, latitude, longitude);
//      Assert.IsNotNull(result, "Expect a good local time");
//      Assert.AreEqual(dateMountainTimeLocal, result.Value);

//      // Shift the longitude over a meter to Central Time Zone
//      longitude = -101.520000;
//      // year/09/01 11:15:00 AM (Central Time is 5 hours behind UTC during Daylight Saving Time)
//      DateTime dateCentralTimeLocal = new DateTime(year, 09, 01, 11, 15, 00);
//      result = API.SpatialTimeZone.ToLocal(dateUTC, latitude, longitude);
//      Assert.IsNotNull(result, "Expect a good local time");
//      Assert.AreEqual(dateCentralTimeLocal, result.Value);
//    }

//    [TestMethod]
//    public void TestInNonDaylightSavingArea()
//    {
//      // Point in Phoenix, AZ (Phoenix Does Not Observe Daylight Saving Time)
//      // During Daylight Saving Time (DST) most of Arizona is at the same time as California (Pacific Daylight Time or PDT).
//      double latitude = 33.506549;
//      double longitude = -112.068578;
//      // year/09/01 4:15:00 PM UTC
//      DateTime dateUTC = new DateTime(year, 09, 01, 16, 15, 00);
//      // year/09/01 09:15:00 AM (Pacific Time is 7 hours behind UTC during Daylight Saving Time)
//      DateTime datePhoenixLocal = new DateTime(year, 09, 01, 09, 15, 00);
//      DateTime? result = API.SpatialTimeZone.ToLocal(dateUTC, latitude, longitude);
//      Assert.IsNotNull(result, "Expect a good local time");
//      Assert.AreEqual(datePhoenixLocal, result.Value);

//      // During standard time, the Arizona time zone is the Mountain Standard Time (MST) zone.
//      // (Mountain Time is 7 hours behind UTC during Standard Time)
//      // year/12/01 4:15:00 PM UTC
//      dateUTC = new DateTime(year, 12, 01, 16, 15, 00);
//      // year/12/01 09:15:00 AM (Mountain Time is 7 hours behind UTC during Standard Time)
//      datePhoenixLocal = new DateTime(year, 12, 01, 09, 15, 00);
//      result = API.SpatialTimeZone.ToLocal(dateUTC, latitude, longitude);
//      Assert.IsNotNull(result, "Expect a good local time");
//      Assert.AreEqual(datePhoenixLocal, result.Value);
//    }

//    [TestMethod]
//    public void TestStandardNewZealandTimeZone()
//    {
//      // Point in New Zealand
//      double latitude = -43.577424;
//      double longitude = 172.475224;
//      // year/09/01 4:15:00 PM UTC
//      DateTime dateUTC = new DateTime(year, 09, 01, 16, 15, 00);
//      // year/09/02 4:15:00 AM (Time is 12 hours ahead of UTC)
//      DateTime dateNewZealandStandardTime = new DateTime(year, 09, 02, 04, 15, 00);
//      DateTime? result = API.SpatialTimeZone.ToLocal(dateUTC, latitude, longitude);
//      Assert.IsNotNull(result, "Expect a good local time");
//      Assert.AreEqual(dateNewZealandStandardTime, result.Value);
//    }

//    [TestMethod]
//    public void TestDaylightSavingNewZealandTimeZone()
//    {
//      // Point in New Zealand
//      double latitude = -43.577424;
//      double longitude = 172.475224;
//      // year+1/03/01 4:15:00 PM UTC
//      DateTime dateUTC = new DateTime(year + 1, 03, 01, 16, 15, 00);
//      // year+1/03/02 5:15:00 AM (Time is 13 hours ahead of UTC)
//      DateTime dateNewZealandDaylightSavingTime = new DateTime(year + 1, 03, 02, 05, 15, 00);
//      DateTime? result = API.SpatialTimeZone.ToLocal(dateUTC, latitude, longitude);
//      Assert.IsNotNull(result, "Expect a good local time");
//      Assert.AreEqual(dateNewZealandDaylightSavingTime, result.Value);
//    }

//    [TestMethod]
//    [Ignore]
//    //This test is ignored because  the implementation of GetApproximateDimTimeZone() method makes the spatial timezone to return always not null value 
//    public void TestPointInOcean()
//    {
//      double latitude = 25.647010;
//      double longitude = -94.389232;
//      // year/09/01 4:15:00 PM UTC
//      DateTime dateUTC = new DateTime(year, 09, 01, 16, 15, 00);
//      DateTime? result = API.SpatialTimeZone.ToLocal(dateUTC, latitude, longitude);
//      Assert.IsNull(result, "Expect a null local time");
//    }

//    [TestMethod]
//    public void TestHalfHourZone()
//    {
//      // India is 5 1/2 hours ahead of UTC
//      double latitude = 20.593684;
//      double longitude = 78.962879;
//      // year/09/01 4:15:00 PM UTC
//      DateTime dateUTC = new DateTime(year, 09, 01, 16, 15, 00);
//      // year/09/01 9:45:00 PM
//      DateTime dateIndiaLocal = new DateTime(year, 09, 01, 21, 45, 00);

//      DateTime? result = API.SpatialTimeZone.ToLocal(dateUTC, latitude, longitude);
//      Assert.IsNotNull(result, "Expect a good local time");
//      Assert.AreEqual(dateIndiaLocal, result.Value);
//    }

//    [TestMethod]
//    [Ignore]
//    public void TestBackThirteenMonths()
//    {
//      // Trimble Westminster Office, Mountain Time Zone
//      double latitude = 39.8979334114877;
//      double longitude = -105.113026865304;
//      // year/09/01 4:15:00 PM UTC
//      DateTime dateDuringDaylightSavingTimeUTC = new DateTime(year, 09, 01, 16, 15, 00);
//      // Go back 13 months to year-1/08/01 4:15:00 PM UTC, still DST
//      DateTime dateThirteenMonthsPrior = dateDuringDaylightSavingTimeUTC.AddMonths(-13);
//      // year-1/08/01 10:15:00 AM (Mountain Time is 6 hours behind UTC during Daylight Saving Time)
//      DateTime localDateThirteenMonthsPrior = new DateTime(year - 1, 08, 01, 10, 15, 00);
//      DateTime? result = API.SpatialTimeZone.ToLocal(dateThirteenMonthsPrior, latitude, longitude);
//      Assert.IsNotNull(result, "Expect a good local time");
//      Assert.AreEqual(localDateThirteenMonthsPrior, result.Value);
//    }


//    [TestMethod]
//    [Ignore]
//    public void TestStandardNewZealandTimeZoneThirteenMonthsBack()
//    {
//      // Point in New Zealand
//      double latitude = -43.577424;
//      double longitude = 172.475224;
//      // year/09/01 4:15:00 PM UTC
//      DateTime dateUTC = new DateTime(year, 09, 01, 16, 15, 00);
//      // Date 13 months ago year-1/08/01 4:15:00 PM UTC
//      DateTime dateThirteenMonthsPrior = dateUTC.AddMonths(-13);
//      // year-1/08/02 4:15:00 AM (Time is 12 hours ahead of UTC)
//      DateTime dateNewZealandStandardTimeThirteenMonthsPrior = new DateTime(year - 1, 08, 02, 04, 15, 00);
//      DateTime? result = API.SpatialTimeZone.ToLocal(dateThirteenMonthsPrior, latitude, longitude);
//      Assert.IsNotNull(result, "Expect a good local time");
//      Assert.AreEqual(dateNewZealandStandardTimeThirteenMonthsPrior, result.Value);
//    }


//    [TestMethod]
//    [Ignore]
//    public void TestDaylightSavingNewZealandTimeZoneThirteenMonthsBack()
//    {
//      // Point in New Zealand
//      double latitude = -43.577424;
//      double longitude = 172.475224;
//      // year/03/01 4:15:00 PM UTC
//      DateTime dateUTC = new DateTime(year, 03, 01, 16, 15, 00);
//      // Date 13 months previous is year-1/02/01 4:15:00 PM UTC
//      DateTime dateThirteenMonthsBack = dateUTC.AddMonths(-13);
//      // year-1/02/02 5:15:00 AM (Time is 13 hours ahead of UTC)
//      DateTime dateNewZealandDaylightSavingTimeThirteenMonthsBack = new DateTime(year - 1, 02, 02, 05, 15, 00);
//      DateTime? result = API.SpatialTimeZone.ToLocal(dateThirteenMonthsBack, latitude, longitude);
//      Assert.IsNotNull(result, "Expect a good local time");
//      Assert.AreEqual(dateNewZealandDaylightSavingTimeThirteenMonthsBack, result.Value);
//    }


//    [TestMethod]
//    public void TestInvalidDate()
//    {
//      double latitude = 25.647010;
//      double longitude = -94.389232;
//      // 1960/09/01 4:15:00 PM UTC
//      DateTime dateUTC = new DateTime(1960, 09, 01, 16, 15, 00);
//      try
//      {
//        DateTime? result = API.SpatialTimeZone.ToLocal(dateUTC, latitude, longitude);
//        Assert.IsNull(result, "Expect a null local time");
//      }
//      catch (ArgumentException ae)
//      {
//        if (!ae.ToString().Contains("Invalid DateTime"))
//          Assert.Fail("expected 'Invalid DateTime'");
//      }
//    }

//    [TestMethod]
//    public void TestInvalidLatitude()
//    {
//      double latitude = 92.1234;
//      double longitude = -94.389232;
//      DateTime dateUTC = new DateTime(year, 09, 01, 16, 15, 00);
//      try
//      {
//        DateTime? result = API.SpatialTimeZone.ToLocal(dateUTC, latitude, longitude);
//        Assert.IsNull(result, "Expect a null local time");
//      }
//      catch (ArgumentException ae)
//      {
//        if (!ae.ToString().Contains("Invalid Latitude"))
//          Assert.Fail("expected 'Invalid Latitude'");
//      }
//    }

//    [TestMethod]
//    [Ignore]
//    //This test is added in order to verify the timezones in local system
//    public void TestApproximationLogic_ExpectStdAbbrevMatch()
//    {
//      Point[] locations = 
//      {
//        new Point(49.19451904,-122.9427795),  // PST
//        new Point (21.783213,87.975426),  // IST
//        new Point(31.205167,86.731567), // XJT
//        new Point(21.8375204,89.502182), //BDT
//        new Point(7.094334,93.902206), //IST
//        new Point(8.193820,93.240967), //IST
//        new Point(10.305124,-71.688538), //VET
//        new Point(46.464704,6.612396), //CET
//        new Point(49.273713,-1.971016),  //GMT
//        new Point(28.160098,34.560242), //AST
//        new Point(-44.98355,-72.157715), //CLT
//        new Point(18.310654,-64.88364),  // AST
//        new Point(38.71529,40.312767), //EET
//        new Point(38.596256,-90.35336), //CST
//        new Point(20.30828,-97.95996), //CST
//        new Point(-32.819786,151.75555), //AEDT
//        new Point(-16.637108,-71.617035), //PET
//        new Point(53.335091,7.202654), //CET
//        new Point(46.147884,-102.57385), //MST
//        new Point(51.396219,7.793711), //CET
//        new Point(51.11642,4.2006187), //CET
//        new Point(43.226273,-79.218079), //EST
//        new Point(-38.35382,141.61664), //AEDT
//        new Point(46.193569,-60.267792),//AST 
//        new Point(43.694588,7.266384), //CET
//        new Point(52.408752,4.824071), //CET
//        new Point(40.802502,-73.927246), //EST
//        new Point(41.675182,-87.45813), //CST
//        new Point(44.670204,-63.599487), //AST
//        new Point(51.557472,-2.9727478),//GMT
//        new Point(51.557472,-2.9727478),//GMT
//        new Point(51.938057,4.05284), //CET
//        new Point(37.567348,-0.950134), //CET
//        new Point(42.142467,-80.08197), //EST
//        new Point(21.307861,-158.025543), //HST
//      };
      
//      var tz = new SpatialTimeZone();
//      var dtz = new List<DimTimeZone>();

//      for (int i = 0; i < locations.Count(); i++)
//      {
//        dtz.Add(tz.GetApproximateDimTimeZone(DateTime.UtcNow, locations[i].y, locations[i].x));
//      }


//      Assert.AreEqual(dtz[0].StdAbbrev, "PST");
//      Assert.AreEqual(dtz[1].StdAbbrev, "IST");
//      Assert.AreEqual(dtz[2].StdAbbrev, "XJT");
//      Assert.AreEqual(dtz[3].StdAbbrev, "BDT");
//      Assert.AreEqual(dtz[4].StdAbbrev, "IST");
//      Assert.AreEqual(dtz[5].StdAbbrev, "IST");
//      Assert.AreEqual(dtz[6].StdAbbrev, "VET");
//      Assert.AreEqual(dtz[7].StdAbbrev, "CET");
//      Assert.AreEqual(dtz[8].StdAbbrev, "GMT");
//      Assert.AreEqual(dtz[9].StdAbbrev, "AST");
//      Assert.AreEqual(dtz[10].StdAbbrev, "CLT");
//      Assert.AreEqual(dtz[11].StdAbbrev, "AST");
//      Assert.AreEqual(dtz[12].StdAbbrev, "EET");
//      Assert.AreEqual(dtz[13].StdAbbrev, "CST");
//      Assert.AreEqual(dtz[14].StdAbbrev, "CST");
//      Assert.AreEqual(dtz[15].StdAbbrev, "AEST");
//      Assert.AreEqual(dtz[16].StdAbbrev, "PET");
//      Assert.AreEqual(dtz[17].StdAbbrev, "CET");
//      Assert.AreEqual(dtz[18].StdAbbrev, "MST");
//      Assert.AreEqual(dtz[19].StdAbbrev, "CET");
//      Assert.AreEqual(dtz[20].StdAbbrev, "CET");
//      Assert.AreEqual(dtz[21].StdAbbrev, "EST");
//      Assert.AreEqual(dtz[22].StdAbbrev, "AEST");
//      Assert.AreEqual(dtz[23].StdAbbrev, "AST");
//      Assert.AreEqual(dtz[24].StdAbbrev, "CET");
//      Assert.AreEqual(dtz[25].StdAbbrev, "CET");
//      Assert.AreEqual(dtz[26].StdAbbrev, "EST");
//      Assert.AreEqual(dtz[27].StdAbbrev, "CST");
//      Assert.AreEqual(dtz[28].StdAbbrev, "AST");
//      Assert.AreEqual(dtz[29].StdAbbrev, "GMT");
//      Assert.AreEqual(dtz[30].StdAbbrev, "GMT");
//      Assert.AreEqual(dtz[31].StdAbbrev, "CET");
//      Assert.AreEqual(dtz[32].StdAbbrev, "CET");
//      Assert.AreEqual(dtz[33].StdAbbrev, "EST");
//      Assert.AreEqual(dtz[34].StdAbbrev, "HST");
//    }


    


//    [TestMethod]
//    public void TestInvalidLongitude()
//    {
//      double Longitude = 29.1234;
//      double longitude = -194.389232;
//      DateTime dateUTC = new DateTime(year, 09, 01, 16, 15, 00);
//      try
//      {
//        DateTime? result = API.SpatialTimeZone.ToLocal(dateUTC, Longitude, longitude);
//        Assert.IsNull(result, "Expect a null local time");
//      }
//      catch (ArgumentException ae)
//      {
//        if (!ae.ToString().Contains("Invalid Longitude"))
//          Assert.Fail("expected 'Invalid Longitude'");
//      }
//    }

//    [TestMethod]
//    public void TestTimeZoneAbbrev()
//    {
//      //Mountain Standard Time - Denver: Latitude:  39° 43' North Longitude:  104° 59' West  
//      double latitude = 39.716667;
//      double longitude = -104.983333;
//      DateTime dateUTC = new DateTime(year, 03, 01, 16, 15, 00);
//      string result = API.SpatialTimeZone.GetTimeZoneAbbreviation(latitude, longitude, dateUTC);
//      Assert.AreEqual<string>("MST", result, "Should be Mountain Standard Time timezone");

//      //Pacific Std Time - San Francisco: Latitude:  37° 46' North Longitude:  122° 26' West  
//      latitude = 37.766667;
//      longitude = -122.433333;
//      result = API.SpatialTimeZone.GetTimeZoneAbbreviation(latitude, longitude, dateUTC);
//      Assert.AreEqual<string>("PST", result, "Should be Pacific Standard Time timezone");

//      //Eastern Std Time - New York: Latitude:  40° 44' North  Longitude:  73° 55' West  
//      latitude = 40.733333;
//      longitude = -73.916667;
//      result = API.SpatialTimeZone.GetTimeZoneAbbreviation(latitude, longitude, dateUTC);
//      Assert.AreEqual<string>("EST", result, "Should be Eastern Standard Time timezone");

//      //Central Std Time - Chicago: Latitude:  41° 51' North  Longitude:  87° 41' West  
//      latitude = 41.85;
//      longitude = -87.683333;
//      result = API.SpatialTimeZone.GetTimeZoneAbbreviation(latitude, longitude, dateUTC);
//      Assert.AreEqual<string>("CST", result, "Should be Central Standard Time timezone");

//      //Middle of the ocean - No time zone - GMT
//      latitude = 0;
//      longitude = 0;
//      result = API.SpatialTimeZone.GetTimeZoneAbbreviation(latitude, longitude, dateUTC);
//      Assert.AreEqual<string>("GMT", result, "Should be GMT for no timezone");

//      //Multiple timezones
//      //Commenting out as there is no possibility of multiple timezones for this lat long.
//      //latitude = 21.5;
//      //longitude = -78;
//      //result = API.SpatialTimeZone.GetTimeZoneAbbreviation(latitude, longitude, dateUTC);
//      //Assert.AreEqual<string>("COT, ECT, PET, EST", result, "Should be multiple TZ abbreviations");
//    }

//    private void CreateConfigSettings_China()
//    {
//      //Populate the table NH_RPT..Configuration with China Timezone details
//      VSS.Hosted.VLCommon.Configuration minX = new VSS.Hosted.VLCommon.Configuration
//      {
//        Name = "ChinaMinX",
//        Value = "67.2",
//        Description = "Min X coordinate of China Bounding Box (should be added only in China Stack)"
//      };
//      Ctx.RptContext.Configuration.AddObject(minX);
//      Ctx.RptContext.SaveChanges();

//      VSS.Hosted.VLCommon.Configuration maxX = new VSS.Hosted.VLCommon.Configuration
//      {
//        Name = "ChinaMaxX",
//        Value = "135.5",
//        Description = "Max X coordinate of China Bounding Box (should be added only in China Stack)"
//      };
//      Ctx.RptContext.Configuration.AddObject(maxX);
//      Ctx.RptContext.SaveChanges();

//      VSS.Hosted.VLCommon.Configuration minY = new VSS.Hosted.VLCommon.Configuration
//      {
//        Name = "ChinaMinY",
//        Value = "17.8",
//        Description = "Min Y coordinate of China Bounding Box (should be added only in China Stack)"
//      };
//      Ctx.RptContext.Configuration.AddObject(minY);
//      Ctx.RptContext.SaveChanges();

//      VSS.Hosted.VLCommon.Configuration maxY = new VSS.Hosted.VLCommon.Configuration
//      {
//        Name = "ChinaMaxY",
//        Value = "55",
//        Description = "Max Y coordinate of China Bounding Box (should be added only in China Stack)"
//      };
//      Ctx.RptContext.Configuration.AddObject(maxY);
//      Ctx.RptContext.SaveChanges();

//      VSS.Hosted.VLCommon.Configuration chinaStdBias = new VSS.Hosted.VLCommon.Configuration
//      {
//        Name = "ChinaStdBias",
//        Value = "480",
//        Description = "Standard Bias of China (should be added only in China Stack)"
//      };
//      Ctx.RptContext.Configuration.AddObject(chinaStdBias);
//      Ctx.RptContext.SaveChanges();

//      VSS.Hosted.VLCommon.Configuration chinaStdAbbrev = new VSS.Hosted.VLCommon.Configuration
//      {
//        Name = "ChinaStdAbbrev",
//        Value = "CST",
//        Description = "Standard Abbreviation of China (should be added only in China Stack)"
//      };
//      Ctx.RptContext.Configuration.AddObject(chinaStdAbbrev);
//      Ctx.RptContext.SaveChanges();

//      VSS.Hosted.VLCommon.Configuration chinaDstBias = new VSS.Hosted.VLCommon.Configuration
//      {
//        Name = "ChinaDstBias",
//        Value = null,
//        Description = "Standard DST Bias of China (should be added only in China Stack)"
//      };
//      Ctx.RptContext.Configuration.AddObject(chinaDstBias);
//      Ctx.RptContext.SaveChanges();
//    }

//    private void CreateTimeZone_China()
//    {
//      //Inserts the row in DimTimeZone for China Stack
//      var chinaTZ = (from tz in Ctx.RptContext.DimTimeZone
//                     where tz.YearUTC == DateTime.UtcNow.Year && tz.StdBias == 480
//                     select tz).FirstOrDefault();
//      chinaTZ.StdAbbrev = "CST";
//      Ctx.RptContext.SaveChanges();
//    }

//    [TestMethod]
//    [Ignore]
//    public void TestGetTimeZoneDaylightSavingTime()
//    {
//      // Trimble Westminster Office, Mountain Time Zone
//      double latitude = 39.8979334114877;
//      double longitude = -105.113026865304;
//      // year/09/01 4:15:00 PM UTC
//      DateTime dateDuringDaylightSavingTimeUTC = new DateTime(year, 09, 01, 16, 15, 00);
//      // year/09/01 10:15:00 AM (Mountain Time is 6 hours behind UTC during Daylight Saving Time)
//      DateTime dateDuringDaylightSavingTimeLocal = new DateTime(year, 09, 01, 10, 15, 00);
//      //DateTime? result1 = API.SpatialTimeZone.ToLocal(dateDuringDaylightSavingTimeUTC, latitude, longitude);
//      DimTimeZone timezone = API.SpatialTimeZone.GetTimeZone(latitude, longitude, dateDuringDaylightSavingTimeUTC);
//      DateTime? result = timezone.GetLocalTime(dateDuringDaylightSavingTimeUTC);
//      Assert.IsNotNull(result, "Expect a good local time");
//      Assert.AreEqual(dateDuringDaylightSavingTimeLocal, result.Value);
//      Assert.AreEqual(timezone.StdAbbrev.Trim(), "MST");
//    }

//    [TestMethod]
//    public void TestGetTimeZoneStandardTime()
//    {
//      // Trimble Westminster Office, Mountain Time Zone
//      double latitude = 39.8979334114877;
//      double longitude = -105.113026865304;
//      // year/12/01 4:15:00 PM UTC
//      DateTime dateDuringStandardTimeUTC = new DateTime(year, 12, 01, 16, 15, 00);
//      // year/12/01 09:15:00 AM (Mountain Time is 7 hours behind UTC during Standard Time)
//      DateTime dateDuringStandardTimeLocal = new DateTime(year, 12, 01, 09, 15, 00);
//      //DateTime? result = API.SpatialTimeZone.ToLocal(dateDuringStandardTimeUTC, latitude, longitude);
//      DimTimeZone timezone = API.SpatialTimeZone.GetTimeZone(latitude, longitude, dateDuringStandardTimeUTC);
//      DateTime? result = timezone.GetLocalTime(dateDuringStandardTimeUTC);
//      Assert.IsNotNull(result, "Expect a good local time");
//      Assert.AreEqual(dateDuringStandardTimeLocal, result.Value);
//      Assert.AreEqual(timezone.StdAbbrev.Trim(), "MST");
//    }
//  }
//}
