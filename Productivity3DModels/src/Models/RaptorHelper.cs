using System.Collections.Generic;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Exceptions;

namespace VSS.Productivity3D.Models.Models
{
  public abstract class RaptorHelper : ProjectID
  {
    protected uint cmvDetailsColorNumber = CMV_DETAILS_NUMBER_OF_COLORS;
    protected uint cmvPercentChangeColorNumber = CMV_PERCENT_CHANGE_NUMBER_OF_COLORS;
    public bool setSummaryDataLayersVisibility = true;

    private const int CMV_DETAILS_NUMBER_OF_COLORS = 5;
    private const int CMV_PERCENT_CHANGE_NUMBER_OF_COLORS = 6;
    private const int CMV_PERCENT_CHANGE_NUMBER_OF_COLORS_V2 = 9;

    public void ValidatePalettes(List<ColorPalette> palettes, DisplayMode mode)
    {
      if (palettes != null)
      {
        uint count = 0;
        switch (mode)
        {
          case DisplayMode.Height:
            //VL has 30 hardwired but Raptor can handle anything
            //count = 30;
            break;
          case DisplayMode.CCV:
            count = cmvDetailsColorNumber;
            break;
          case DisplayMode.CCVPercentChange:
          case DisplayMode.CCVPercent:
            count = 5;
            break;
          case DisplayMode.CMVChange:
            count = palettes.Count == CMV_PERCENT_CHANGE_NUMBER_OF_COLORS - 1 ? cmvPercentChangeColorNumber : CMV_PERCENT_CHANGE_NUMBER_OF_COLORS_V2;

            if (count == CMV_PERCENT_CHANGE_NUMBER_OF_COLORS)
              palettes.Insert(0, new ColorPalette(0, 0));
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
              $"Palette for overlay type {mode} should contain {count} items"));
        }

        for (int i = 1; i < palettes.Count; i++)
        {
          //Special case of below/above colors for elevation
          var invalid = (mode == DisplayMode.Height && ((i == 1 && palettes[0].value == -1) || (i == palettes.Count - 1 && palettes[i].value == -1))) ? false :
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

    public void ValidateDesign(DesignDescriptor designDescriptor, DisplayMode mode, VolumesType computeVolType)
    {
      bool noDesign = false;

      if (computeVolType == VolumesType.BetweenDesignAndFilter ||
          computeVolType == VolumesType.BetweenFilterAndDesign)
      {
        noDesign = designDescriptor == null;
      }

      if (noDesign)
      {
        throw new MissingDesignDescriptorException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            string.Format("Design descriptor required for cut/fill and design to filter or filter to design volumes display")));
      }


      if (computeVolType != VolumesType.Between2Filters)
      {
        if (mode == DisplayMode.CutFill && designDescriptor == null)
        {
          throw new MissingDesignDescriptorException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              string.Format(
                "Design descriptor required for cut/fill and design to filter or filter to design volumes display")));
        }


        if (designDescriptor != null)
          designDescriptor.Validate();
      }
    }

    public void ValidateVolumesFilters(VolumesType computeVolType, FilterResult filter1, long filterId1, FilterResult filter2, long filterId2)
    {
      switch (computeVolType)
      {
        case VolumesType.Between2Filters:
          if ((filter1 == null && filterId1 <= 0) || (filter2 == null && filterId2 <= 0))
          {
            throw new TwoFiltersRequiredException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                string.Format("Two filters required for filter to filter volumes display")));

          }

          break;
        case VolumesType.BetweenDesignAndFilter:
        case VolumesType.BetweenFilterAndDesign:
          if (filter1 == null && filterId1 <= 0 && filter2 == null && filterId2 <= 0)
          {
            throw new SingleFilterRequiredException(HttpStatusCode.BadRequest,
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
