
using System.Collections.Generic;
using System.Net;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.Proxies;
using VSS.Raptor.Service.Common.ResultHandling;

namespace VSS.Raptor.Service.Common.Utilities
{
  public static class RaptorValidator
  {
 
    public static void ValidatePalettes(List<ColorPalette> palettes, DisplayMode mode)
    {
      if (palettes != null)
      {
        int count = 0;
        switch (mode)
        {
          case DisplayMode.Height:
            //VL has 30 hardwired but Raptor can handle anything
            //count = 30;
            break;
          case DisplayMode.CCV:
            count = 11;
            break;
          case DisplayMode.CCVPercentChange:
          case DisplayMode.CCVPercent:
            count = 5;
            break;
          case DisplayMode.CMVChange:
            palettes.Insert(0,ColorPalette.CreateColorPalette(0,0));
            count = 6;
            break;
          case DisplayMode.Latency:
            break;
          case DisplayMode.PassCount:
            count = 9;
            break;
          case DisplayMode.PassCountSummary:
            count = 3;
            break;
          case DisplayMode.RMV:
          case DisplayMode.Frequency:
          case DisplayMode.Amplitude:
            break;
          case DisplayMode.CutFill:
            count = 7;
            break;
          case DisplayMode.Moisture:
            break;
          case DisplayMode.TemperatureSummary:
            count = 3;
            break;
          case DisplayMode.GPSMode:
            break;
          case DisplayMode.CCVSummary:
          case DisplayMode.CCVPercentSummary:
            count = 6;
            break;
          case DisplayMode.CompactionCoverage:
            count = 2;
            break;
          case DisplayMode.TargetThicknessSummary:
          case DisplayMode.VolumeCoverage:          
            count = 3;
            break;
          case DisplayMode.MDP:
          case DisplayMode.MDPPercent:
            count = 5;
            break;
          case DisplayMode.MDPSummary:
          case DisplayMode.MDPPercentSummary:
            count = 6;
            break;
          case DisplayMode.MachineSpeed:
            count = 5;
            break;
          case DisplayMode.TargetSpeedSummary:
            count = 3;
            break;
        }

        if (mode != DisplayMode.Height && count != palettes.Count)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                  string.Format("Palette for overlay type {0} should contain {1} items", mode, count)));
        }

        for (int i = 1; i < palettes.Count; i++)
        {
          //Special case of above color for elevation
          var invalid = (mode == DisplayMode.Height && i == palettes.Count - 1 && palettes[i].value == -1) ? false : 
            mode == DisplayMode.CutFill
              ? palettes[i].value > palettes[i - 1].value
              : palettes[i].value < palettes[i - 1].value;
          
          if (invalid)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
                new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                    string.Format("Palette values must be ordered")));
          }
        }

        if (mode == DisplayMode.CCV || mode == DisplayMode.CCVPercent || mode == DisplayMode.MDP || mode == DisplayMode.MDPPercent)
          if (palettes[0].value != 0)
            throw new ServiceException(HttpStatusCode.BadRequest,
                new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                    string.Format("First value in palette must be 0 for this type of tile.")));

        foreach (var palette in palettes)
        {
          palette.Validate();
        }
      }
    }

    public static void ValidateDesign(DesignDescriptor designDescriptor, DisplayMode mode, RaptorConverters.VolumesType computeVolType)
    {
      bool noDesign = false;

      if (computeVolType == RaptorConverters.VolumesType.BetweenDesignAndFilter ||
          computeVolType == RaptorConverters.VolumesType.BetweenFilterAndDesign)
      {
        noDesign = designDescriptor == null;
      }

      if (noDesign)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
             new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                 string.Format("Design descriptor required for cut/fill and design to filter or filter to design volumes display")));
      }

     /* if (mode == DisplayMode.VolumeCoverage && computeVolType == RaptorConverters.VolumesType.None)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                string.Format(
                    "None volume detection method is not applicable.")));
      }*/


      if (mode == DisplayMode.CutFill && designDescriptor == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                string.Format(
                    "Design descriptor required for cut/fill and design to filter or filter to design volumes display")));
      }

     
      if (designDescriptor != null)
        designDescriptor.Validate();
    }

    public static void ValidateVolumesFilters(RaptorConverters.VolumesType computeVolType, Filter filter1, long filterId1, Filter filter2, long filterId2)
    {
      switch (computeVolType)
      {
        case RaptorConverters.VolumesType.Between2Filters:
          if ((filter1 == null && filterId1 <= 0) || (filter2 == null && filterId2 <= 0))
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
                 new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                     string.Format("Two filters required for filter to filter volumes display")));

          }

          break;
        case RaptorConverters.VolumesType.BetweenDesignAndFilter:
        case RaptorConverters.VolumesType.BetweenFilterAndDesign:
          if (filter1 == null && filterId1 <= 0 && filter2 == null && filterId2 <= 0)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
                 new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                     string.Format("One filter required for design to filter or filter to design volumes display")));

          }
          break;
        default:
          throw new ServiceException(HttpStatusCode.BadRequest,
               new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                   string.Format("This type of volumes calculation is not supported")));

      }
      if (filter1 != null)
        filter1.Validate();

      if (filter2 != null)
        filter2.Validate();
    }

  }



}
