using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon;
using VSS.Nighthawk.ThreeDCommon.ThreeDAPIs;
using VSS.UnitTest.Common;

namespace UnitTests
{
    [TestClass()]
    public class MaterialTypeDBTest : UnitTestBase
    {

    #region ETLtests

    [DatabaseTest]
    [TestMethod()]
    public void MaterialConfigChange_2Switches4Options() 
    {
        // First toggle is sensor 3Off - should do first 2 loads "gravel"
        // Next toggle is sensor 4On - should do 3rd load "mud"
        // then sensor4On is set to material = 0 i.e. disabled
        // 4th load should use prior material i.e. for 3Off "gravel"
        this.customer = Entity.Customer.EndCustomer.Name("FactAssetCyclePeriodTest_Customer").BssId("BSS123").SyncWithRpt().Save();
        this.user = Entity.User.ForCustomer(this.customer).LastName("FactAssetCyclePeriodTest_User").Save();
        ActiveUser activeUser = Entity.ActiveUser.ForUser(this.user).Save();
        this.session = Helpers.Sessions.GetContextFor(activeUser, true, true);

        var asset = Entity.Asset.Name("AAA")
            .WithDevice(Entity.Device.SNM940.OwnerBssId(this.customer.BSSID).Save())
            .WithCoreService()
            .WithService(ServiceTypeEnum.e2DProjectMonitoring)
            .SyncWithRpt().Save();
        this.assetID1 = asset.AssetID;
        SetupAssetMonitoring(this.assetID1, 2 /* truck */, 0 /* radiusMeters */);
        this.cycleID = SetupCycle(this.startSwitch, this.startOn, this.stopSwitch, this.stopOn, new List<long>() { this.assetID1 }, 1);

        Sensor sensor3 = Entity.Sensor.SensorNumber(3).ParentDeviceID(asset.Device.ID).OnDescription("Sensor3 On").OffDescription("Sensor3 Off")
                        .Name(string.Format("Sensor3")).Save();
        Sensor sensor4 = Entity.Sensor.SensorNumber(4).ParentDeviceID(asset.Device.ID).OnDescription("Sensor4 On").OffDescription("Sensor4 Off")
                    .Name(string.Format("Sensor4")).Save();

        var materialList = CreateMaterials().ToList();
        Helpers.NHRpt.DimAsset_Populate();
        var rptMaterials = (from dmt in Ctx.RptContext.DimMaterialTypeReadOnly
                            where dmt.ifk_DimCustomerID == this.customer.ID
                            orderby dmt.ID
                            select dmt).ToList();
        Assert.AreEqual(5, rptMaterials.Count, "Wrong number Materials in NH_RPT");

        SetupEventUTCs();
        var assetsensorMaterialTypeList = CreateAssetSensorMaterialTypes(asset.AssetID, sensor3.ID, sensor4.ID, materialList.ToList(), eventUTCs[0]).ToList();
        Helpers.NHRpt.WorkingAssetSensorMaterialTypeHistory_Populate(eventUTCs[0]);

        DateTime eventUTC = eventUTCs[0];            
        Helpers.NHData.DataHoursLocation_Add(this.assetID1, eventUtc: eventUTC, latitude: latitudes[0], longitude: longitudes[0], odometerMiles: 1000);
        eventUTC = eventUTC.AddMinutes(2);
        DateTime materialToggleUTC1 = eventUTC.AddSeconds(-1);
        Helpers.NHData.DiscreteInputIO_Add(this.assetID1, materialToggleUTC1, 3, false); // switch 3 Off i.e. materials [1]
        Helpers.NHData.DiscreteInputIO_Add(this.assetID1, eventUTC, this.startSwitch, this.startOn);
        DateTime load1EventUTC = eventUTC;            
        Helpers.NHRpt.FactAsset_Cycles_Populate();
        var facts = (from f in Ctx.RptContext.FactAssetLoadCountPeriodReadOnly
                        where f.ifk_DimCustomerID == this.customer.ID
                        orderby f.ifk_DimAssetID, f.StartEventUTC
                        select f).ToList();
        Assert.AreEqual(1, facts.Count, "Wrong number FALoadCounts pass1");
        Assert.AreEqual(76, facts[0].ifk_DimEventTypeID_Start, "Should be L-L fact pass1");
        Assert.AreEqual(load1EventUTC, facts[0].StartEventUTC, "Should start with load1 eventUTC (0) pass1", load1EventUTC);
        Assert.AreEqual(76, facts[0].ifk_DimEventTypeID_Stop, "Should be load stop pass1");
        Assert.AreEqual(load1EventUTC, facts[0].StopEventUTC, "Should end with copy of start Load (0) pass1", load1EventUTC);
        Assert.AreEqual((int)DimLoadQualityTypeEnum.SwitchtoSwitch, facts[0].ifk_DimLoadQualityTypeID, "Wrong Load quality  pass1");
        Assert.AreEqual((int)DimLoadIncompleteTypeEnum.MissingSwitchEvent, facts[0].ifk_DimLoadIncompleteTypeID, "Wrong Load incompleteType  pass1");
        Assert.AreEqual(materialList[1].ID, facts[0].ifk_DimMaterialTypeID, "Wrong materialType pass1");

        eventUTC = eventUTC.AddMinutes(2);
        Helpers.NHData.DiscreteInputIO_Add(this.assetID1, eventUTC, this.stopSwitch, this.stopOn);
        Helpers.NHRpt.FactAsset_Cycles_Populate();
        facts = (from f in Ctx.RptContext.FactAssetLoadCountPeriodReadOnly
                    where f.ifk_DimCustomerID == this.customer.ID
                    orderby f.ifk_DimAssetID, f.StartEventUTC
                    select f).ToList();
        Assert.AreEqual(1, facts.Count, "Should be 1 fac pass2t");
        Assert.AreEqual(76, facts[0].ifk_DimEventTypeID_Start, "Should be L*-D fact pass2");
        Assert.AreEqual(load1EventUTC, facts[0].StartEventUTC, "Should start with load1 eventUTC (0) pass2", load1EventUTC);
        Assert.AreEqual(77, facts[0].ifk_DimEventTypeID_Stop, "Should be L-D* fact pass2");
        Assert.AreEqual(eventUTC, facts[0].StopEventUTC, "Should end with Dump eventUTC pass2");
        Assert.AreEqual((int)DimLoadQualityTypeEnum.SwitchtoSwitch, facts[0].ifk_DimLoadQualityTypeID, "Wrong Load quality pass2");
        Assert.AreEqual((int)DimLoadIncompleteTypeEnum.None, facts[0].ifk_DimLoadIncompleteTypeID, "Wrong Load incompleteType pass2");
        Assert.AreEqual(materialList[1].ID, facts[0].ifk_DimMaterialTypeID, "Wrong materialType pass2");


        eventUTC = eventUTC.AddMinutes(2);
        Helpers.NHData.DiscreteInputIO_Add(this.assetID1, eventUTC, this.startSwitch, this.startOn);
        DateTime load2EventUTC = eventUTC;
        eventUTC = eventUTC.AddMinutes(2);
        DateTime materialToggleUTC2 = eventUTC.AddSeconds(-1);
        Helpers.NHData.DiscreteInputIO_Add(this.assetID1, materialToggleUTC2, 4, true); // switch 4 On i.e. materials [2]
        Helpers.NHData.DiscreteInputIO_Add(this.assetID1, eventUTC, this.stopSwitch, this.stopOn);
        DateTime eventUTCOfLastCompletePair = eventUTC;
        eventUTC = eventUTC.AddMinutes(2);
        Helpers.NHData.DiscreteInputIO_Add(this.assetID1, eventUTC, this.startSwitch, this.startOn);
        DateTime load3EventUTC = eventUTC;
        Helpers.NHRpt.FactAsset_Cycles_Populate();
        facts = (from f in Ctx.RptContext.FactAssetLoadCountPeriodReadOnly
                    where f.ifk_DimCustomerID == this.customer.ID
                    orderby f.ifk_DimAssetID, f.StartEventUTC
                    select f).ToList();
        Assert.AreEqual(3, facts.Count, "Wrong number FALoadCounts pass3");
        Assert.AreEqual(76, facts[0].ifk_DimEventTypeID_Start, "Should be L-L fact pass3");
        Assert.AreEqual(77, facts[0].ifk_DimEventTypeID_Stop, "Should be L*-D fact pass3");
        Assert.AreEqual(load1EventUTC, facts[0].StartEventUTC, "Should start with load1 eventUTC (0) pass3", load1EventUTC);
        Assert.AreEqual((int)DimLoadQualityTypeEnum.SwitchtoSwitch, facts[0].ifk_DimLoadQualityTypeID, "Wrong Load quality 3-0 pass3");
        Assert.AreEqual((int)DimLoadIncompleteTypeEnum.None, facts[0].ifk_DimLoadIncompleteTypeID, "Wrong Load incompleteType 3-0 pass3");
        Assert.AreEqual(materialList[1].ID, facts[0].ifk_DimMaterialTypeID, "Wrong materialType pass3");

        Assert.AreEqual(76, facts[1].ifk_DimEventTypeID_Start, "Should be L*-D fact pass3");
        Assert.AreEqual(77, facts[1].ifk_DimEventTypeID_Stop, "Should be L-D* fact pass3");
        Assert.AreEqual(eventUTCOfLastCompletePair, facts[1].StopEventUTC, "last Dump eventUTC pass3");
        Assert.AreEqual((int)DimLoadQualityTypeEnum.SwitchtoSwitch, facts[1].ifk_DimLoadQualityTypeID, "Wrong Load quality 3-1 pass3");
        Assert.AreEqual((int)DimLoadIncompleteTypeEnum.None, facts[1].ifk_DimLoadIncompleteTypeID, "Wrong Load incompleteType 3-1 pass3");
        Assert.AreEqual(materialList[1].ID, facts[1].ifk_DimMaterialTypeID, "Wrong materialType pass3");

        Assert.AreEqual(76, facts[2].ifk_DimEventTypeID_Start, "Should be L-L fact pass3");
        Assert.AreEqual(eventUTC, facts[2].StartEventUTC, "Last L/L with matching eventUTC pass3");
        Assert.AreEqual(76, facts[2].ifk_DimEventTypeID_Stop, "Should be L-L* fact pass3");
        Assert.AreEqual(eventUTC, facts[2].StopEventUTC, "Last L/L with matching eventUTC pass3");
        Assert.AreEqual((int)DimLoadQualityTypeEnum.SwitchtoSwitch, facts[2].ifk_DimLoadQualityTypeID, "Wrong Load quality 3-2 pass3");
        Assert.AreEqual((int)DimLoadIncompleteTypeEnum.MissingSwitchEvent, facts[2].ifk_DimLoadIncompleteTypeID, "Wrong Load incompleteType 3-2 pass3");
        Assert.AreEqual(materialList[2].ID, facts[2].ifk_DimMaterialTypeID, "Wrong materialType pass3");

        // change Sensor definitions so that last sensor togged (4On) is now no longer active i.e. loads should default to prior sensor 3Off 
        eventUTC = eventUTC.AddMinutes(2);
        long materialID = assetsensorMaterialTypeList[2].ID;
        AssetSensorMaterialType changingSensor = (from asmt in Ctx.OpContext.AssetSensorMaterialType
                                                    where asmt.ID == materialID
                                                    select asmt).FirstOrDefault();
        changingSensor.fk_MaterialTypeID = 0;
        changingSensor.UpdateUTC = eventUTC;        
        Ctx.OpContext.SaveChanges();
        Helpers.NHRpt.WorkingAssetSensorMaterialTypeHistory_Populate(eventUTC);

        eventUTC = eventUTC.AddMinutes(2);
        Helpers.NHData.DiscreteInputIO_Add(this.assetID1, eventUTC, this.startSwitch, this.startOn);
        DateTime load4EventUTC = eventUTC;
        eventUTC = eventUTC.AddMinutes(2);
        Helpers.NHData.DiscreteInputIO_Add(this.assetID1, eventUTC, this.stopSwitch, this.stopOn);            
        Helpers.NHRpt.FactAsset_Cycles_Populate();
        facts = (from f in Ctx.RptContext.FactAssetLoadCountPeriodReadOnly
                    where f.ifk_DimCustomerID == this.customer.ID
                    orderby f.ifk_DimAssetID, f.StartEventUTC
                    select f).ToList();
        Assert.AreEqual(4, facts.Count, "Wrong number FALoadCounts pass4");
        Assert.AreEqual(76, facts[0].ifk_DimEventTypeID_Start, "Should be L-L fact pass4");
        Assert.AreEqual(77, facts[0].ifk_DimEventTypeID_Stop, "Should be L*-D fact pass4");
        Assert.AreEqual(load1EventUTC, facts[0].StartEventUTC, "Should start with load1 eventUTC (0) pass4", load1EventUTC);
        Assert.AreEqual((int)DimLoadQualityTypeEnum.SwitchtoSwitch, facts[0].ifk_DimLoadQualityTypeID, "Wrong Load quality 3-0 pass4");
        Assert.AreEqual((int)DimLoadIncompleteTypeEnum.None, facts[0].ifk_DimLoadIncompleteTypeID, "Wrong Load incompleteType 3-0 pass4");
        Assert.AreEqual(materialList[1].ID, facts[0].ifk_DimMaterialTypeID, "Wrong materialType pass4");

        Assert.AreEqual(76, facts[1].ifk_DimEventTypeID_Start, "Should be L*-D fact pass4");
        Assert.AreEqual(77, facts[1].ifk_DimEventTypeID_Stop, "Should be L-D* fact pass4");
        Assert.AreEqual(eventUTCOfLastCompletePair, facts[1].StopEventUTC, "last Dump eventUTC pass4");
        Assert.AreEqual((int)DimLoadQualityTypeEnum.SwitchtoSwitch, facts[1].ifk_DimLoadQualityTypeID, "Wrong Load quality 3-1 pass4");
        Assert.AreEqual((int)DimLoadIncompleteTypeEnum.None, facts[1].ifk_DimLoadIncompleteTypeID, "Wrong Load incompleteType 3-1 pass4");
        Assert.AreEqual(materialList[1].ID, facts[1].ifk_DimMaterialTypeID, "Wrong materialType pass4");

        Assert.AreEqual(76, facts[2].ifk_DimEventTypeID_Start, "Should be L-L fact pass4");
        Assert.AreEqual(load3EventUTC, facts[2].StartEventUTC, "Last L/L with matching eventUTC pass4");
        Assert.AreEqual(76, facts[2].ifk_DimEventTypeID_Stop, "Should be L-L* fact pass4");
        Assert.AreEqual((int)DimLoadQualityTypeEnum.SwitchtoSwitch, facts[2].ifk_DimLoadQualityTypeID, "Wrong Load quality 3-2 pass4");
        Assert.AreEqual((int)DimLoadIncompleteTypeEnum.MissingSwitchEvent, facts[2].ifk_DimLoadIncompleteTypeID, "Wrong Load incompleteType 3-2 pass4");

        // this should STILL be what sensor4On WAS set to at this time
        Assert.AreEqual(materialList[2].ID, facts[2].ifk_DimMaterialTypeID, "Wrong materialType pass4");

        Assert.AreEqual(76, facts[3].ifk_DimEventTypeID_Start, "Should be L-D fact pass4");
        Assert.AreEqual(load4EventUTC, facts[3].StartEventUTC, "Last L/L with matching eventUTC pass4");
        Assert.AreEqual(77, facts[3].ifk_DimEventTypeID_Stop, "Should be L-D fact pass4");
        Assert.AreEqual(eventUTC, facts[3].StopEventUTC, "Last L/L with matching eventUTC pass4");
        Assert.AreEqual((int)DimLoadQualityTypeEnum.SwitchtoSwitch, facts[3].ifk_DimLoadQualityTypeID, "Wrong Load quality 3-2 pass4");
        Assert.AreEqual((int)DimLoadIncompleteTypeEnum.None, facts[3].ifk_DimLoadIncompleteTypeID, "Wrong Load incompleteType 3-2 pass4");

        // this should not be sensor4On as it was disabled since.
        Assert.AreEqual(materialList[1].ID, facts[3].ifk_DimMaterialTypeID, "Wrong materialType - should have reverted to prior switch pass4");

        // check that assetHistory report shows correct material Types for sensor events.
        List<AssetEventHistoryAccess.AssetEvent> events = AssetEventHistoryAccess.Read(this.assetID1, this.customer.ID, load1EventUTC.AddDays(-1).KeyDate(), load4EventUTC.AddDays(1).KeyDate(), "en-US", false).ToList();
        Assert.AreEqual(9, events.Count, "Wrong number of assetHistory events");
        Assert.AreEqual("Sensor3 Off: gravel", events[0].rawEventDescription, "Wrong materialType - first toggle");
        Assert.AreEqual("Sensor4 On: mud", events[4].rawEventDescription, "Wrong materialType - 2nd toggle");

    }
        
       
    #endregion


    #region privates

    private void SetupAssetMonitoring(long assetId, int machineType, double radiusMeters, int debounceSeconds = 30, double? maxInSiteHours = null)
    {
        AssetMonitoring am = new AssetMonitoring { fk_AssetID = assetId, fk_MonitoringMachineTypeID = machineType, RadiusMeters = radiusMeters, DebounceSeconds = debounceSeconds, MaxInSiteHours = maxInSiteHours, UpdateUTC = DateTime.UtcNow };
        Ctx.OpContext.AssetMonitoring.AddObject(am);
        Ctx.OpContext.SaveChanges();

        Helpers.NHRpt.DimTables_Populate();
        var DimAssetMonitoring = (from d in Ctx.RptContext.DimAssetMonitoringReadOnly
                                    where d.ifk_DimAssetID == assetId
                                    select d).ToList();
        Assert.AreEqual(1, DimAssetMonitoring.Count(), "Should be in DimAssetMonitoring");
    }


    private long SetupCycle(byte startSwitch, bool startOn, byte stopSwitch, bool stopOn, List<long> assetIds, int expectedAssetCycles)
    {
        //Create cycle and assign to assets
        Cycle cycle = API.Cycle.Create(this.session.NHOpContext, this.customer.ID, DeviceTypeEnum.MANUALDEVICE, "Test Cycle", startSwitch, startOn, stopSwitch, stopOn);
        bool assigned = API.Cycle.Assign(this.session.NHOpContext, cycle.ID, assetIds);

        Helpers.NHRpt.DimTables_Populate();

        //Check DimAssetCycle got populated
        var assetCycles = (from dac in Ctx.RptContext.DimAssetCycleReadOnly
                            where assetIds.Contains(dac.ifk_DimAssetID) && dac.ifk_CycleID == cycle.ID
                            select dac).ToList();
        Assert.AreEqual(expectedAssetCycles, assetCycles.Count, "Wrong number of asset cycles");
        return cycle.ID;
    }

    private void SetupEventUTCs()
    {
        // don't allow this to inadvertantly span days, set it to 3pm
        DateTime beginDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day).AddDays(-2).AddHours(15);
        this.eventUTCs = new DateTime[eventMinutes.Length];
        for (int i = 0; i < eventMinutes.Length; i++) this.eventUTCs[i] = beginDate.AddMinutes(eventMinutes[i]);
    }

      private Customer customer = null;
      private User user = null;
      private SessionContext session = null;

      private long assetID1 = -1;

      private byte startSwitch = 1;
      private bool startOn = true;
      private byte stopSwitch = 2;
      private bool stopOn = true;

      private long cycleID = -1;



      private DateTime[] eventUTCs = null;
      private const int NUM_DATA = 20; //length of arrays below

      int[] eventMinutes = new int[]
      {
        30,
        33,
        36,
        43,
        50, 
        60,
        70, 
        85, 
        100, 
        120,
        140, 
        170,
        174, 
        185,
        188,
        191,
        193,
        199,
        202,
        210
      };

      double[] latitudes = new double[]
      {
        39.89768982,
        39.89768982,
        39.89768982,
        39.89768982,
        39.89768982,
        39.89768982,
        39.89768982, 
        39.89768982, 
        39.89768982, 
        39.89768982,
        39.89768982,
        39.89768982, 
        39.89768982,
        39.89768982,
        39.89768982, 
        39.89768982,
        39.89768982,
        39.89768982, 
        39.89768982,
        39.89768982,
      };
      double[] longitudes = new double[]
      {
        -105.1130829,
        -105.1125793,
        -105.1130829, 
        -105.1121597, 
        -105.1130829,
        -105.1117401, 
        -105.1130829, 
        -105.110775, 
        -105.1130829,
        -105.1095161, 
        -105.1130829, 
        -105.1065788, 
        -105.1065788, 
        -105.110775,
        -105.1130829,
        -105.1095161, 
        -105.1130829, 
        -105.1065788, 
        -105.1065788, 
        -105.110775
      };
     
    private List<Point> GetSitePoints()
    {
        List<Point> points = new List<Point>();
        points.Add(new Point(43.153, 171));
        points.Add(new Point(44.6, 171));
        points.Add(new Point(44.6, 173));
        points.Add(new Point(43.153, 173));
        return points;
    }

    private IEnumerable<MaterialType> CreateMaterials()
    {
        var materials = (new List<MaterialType>()
        {
            Entity.MaterialType.CreateMaterialType("dirt",this.customer),
            Entity.MaterialType.CreateMaterialType("gravel", this.customer),
            Entity.MaterialType.CreateMaterialType("mud",this.customer),         
            Entity.MaterialType.CreateMaterialType("pebbles",this.customer),
            Entity.MaterialType.CreateMaterialType("sand", this.customer), 
        });
        foreach (var material in materials)
            Ctx.OpContext.MaterialType.AddObject(material);
        Ctx.OpContext.SaveChanges();
        return materials;
    }

    private IEnumerable<AssetSensorMaterialType> CreateAssetSensorMaterialTypes(long assetID, long sensor1, long sensor2, List<MaterialType> materials, DateTime createdUTC)
    {
        var assetsensorMaterialTypeList = (new List<AssetSensorMaterialType>()
        {
            CreateAssetSensorMaterialType(assetID, sensor1, true, materials[0].ID, createdUTC ), //"dirt"
            CreateAssetSensorMaterialType(assetID, sensor1, false, materials[1].ID, createdUTC ), //"gravel"
            CreateAssetSensorMaterialType(assetID, sensor2, true, materials[2].ID, createdUTC ), //"mud"
            CreateAssetSensorMaterialType(assetID, sensor2, false, materials[3].ID, createdUTC ), //"pebbles"
        });
        foreach (var assetSensorMaterialType in assetsensorMaterialTypeList)
            Ctx.OpContext.AssetSensorMaterialType.AddObject(assetSensorMaterialType);
        Ctx.OpContext.SaveChanges();
        return assetsensorMaterialTypeList;
    }

    public AssetSensorMaterialType CreateAssetSensorMaterialType(long assetID, long sensorID, bool isOn, long materialTypeID, DateTime createdUTC)
    {
        return ( new AssetSensorMaterialType()
        {
            fk_AssetID = assetID,
            fk_SensorID = sensorID,
            IsOn = isOn,
            fk_MaterialTypeID = materialTypeID,
            UpdateUTC = createdUTC
        });
    }

    #endregion

    }
}
