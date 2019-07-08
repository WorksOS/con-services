using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Hydrology.WebApi.Abstractions.ResultsHandling
{
  public class HydroErrorCodesProvider : ContractExecutionStatesEnum
  {
    public HydroErrorCodesProvider()
    {
      DynamicAddwithOffset("Invalid ProjectUid.", 1);
      DynamicAddwithOffset("Invalid FilterUid.", 2);
      DynamicAddwithOffset("Must have a valid resultant zip file name.", 3);
      DynamicAddwithOffset("Resolution must be between 0.005 and < 1,000,000.", 4);
      DynamicAddwithOffset("Current ground design has too few TIN entities, must have at least 3.", 5);
      DynamicAddwithOffset("TTM conversion failed. triangleCount differs with dxf", 6);
      DynamicAddwithOffset("Failed to zip images.", 7);
      DynamicAddwithOffset("Levels must be between 2 and 240.", 8);
      DynamicAddwithOffset("Unable to import hydro surface from the original ground design.", 9);
      DynamicAddwithOffset("Unable to find any valid hydro surface in the original ground design.", 10);
      DynamicAddwithOffset("Unable to generate a ponding image from the hydro surface.", 11);
      DynamicAddwithOffset("Unable to find any ponding image from the hydro surface.", 12);
      DynamicAddwithOffset("Unable to save binary image to disk.", 13);
      DynamicAddwithOffset("Unable to generate a drainage violation image from the hydro surface.", 14);
      DynamicAddwithOffset("Unable to find any drainage violation  image from the hydro surface.", 15);
      DynamicAddwithOffset("Unable to retrieve latest ground from 3dp.", 16);
      DynamicAddwithOffset("Only DXF original ground file type supported at present. Extension: {0}", 17);
      DynamicAddwithOffset("MinSlope must be between 0.005 and 100.0.", 18);
      DynamicAddwithOffset("MaxSlope must be between 0.005 and 100.0.", 19);
      DynamicAddwithOffset("MaxSlope must be greater than MinSlope.", 20);
      DynamicAddwithOffset("{0} must be a valid color.", 21);
      DynamicAddwithOffset("3dp design surface not returned.", 22);
    }
  }
}

