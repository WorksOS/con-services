using System.Collections.Generic;

namespace VSS.Hosted.VLCommon
{

  //public class ExpectedWorkingRunningLists
  //{
  //  public List<UtilizationTableData> totals;
  //  public List<UtilizationChartData> monthly;
  //  public List<UtilizationChartData> weekly;
  //  public List<UtilizationChartData> daily;
  //}

  public class UtilizationData
  {
    public long assetID;
    public string assetSerialNumber;
    public string assetName;
    public string equipmentVIN;
    public int assetIconID;
    public int deviceTypeID;

    public double actualRuntime;
    public double actualWorkingtime;
    public double idleHrs;

    public List<int> runtimeHoursCalloutTypeIDs;
    public List<int> idleHoursCalloutTypeIDs;
    public List<int> workingHoursCalloutTypeIDs;
  }

  public class UtilizationTableData : UtilizationData
  {
    public double expectedRuntime;
    public double efficiency;
    public double runningUtilization;
    public double workingUtilization;
  }

  public class FuelUtilizationTableData : UtilizationData
  {    
    public double idleBurnRateGallons;
    public double workingBurnRateGallons;
    public double averageBurnRateGallons;
    public double idleFuelBurnedGallons;
    public double workingFuelBurnedGallons;
    public double totalFuelBurnedGallons;

    public double workingHrs;    
    public double runningHrs;

    public List<int> runtimeFuelBurnedCalloutTypeIDs;
    public List<int> idleAndWorkingFuelBurnedCalloutTypeIDs;
  }
}