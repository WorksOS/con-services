using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;

namespace VSS.Productivity3D.WebApiModels.Compaction.Models.Palettes
{
  /// <summary>
  /// Representation of a palette for details data, both integral (e.g. pass count) and continuous (e.g elevation)
  /// </summary>
  public class DetailPalette : IValidatable
  {
    /// <summary>
    /// The color/value pairs for the palette. There must be at least one item in the list.
    /// The values must be in ascending order. For integral values (e.g. pass count) the color is
    /// used for the exact value. For continuous values (e.g. elevation) the color is used for all
    /// values that fall with the range from the value upto but excluding the next value in the list.
    /// </summary>
    [JsonProperty(PropertyName = "colorValues", Required = Required.Always)]
    [Required]
    public List<ColorValue> ColorValues { get; private set; }

    /// <summary>
    /// The color for values above the last value. 
    /// </summary>
    [JsonProperty(PropertyName = "aboveLastColor", Required = Required.Default)]
    public uint? AboveLastColor { get; private set; }

    /// <summary>
    /// The color for values below the first value. 
    /// </summary>
    [JsonProperty(PropertyName = "belowFirstColor", Required = Required.Default)]
    public uint? BelowFirstColor { get; private set; }


    /// <summary>
    /// Private constructor
    /// </summary>
    private DetailPalette()
    { }

    /// <summary>
    /// Create instance of DetailPalette
    /// </summary>
    public static DetailPalette CreateDetailPalette(
        List<ColorValue> colorValues,
        uint? aboveLastColor,
        uint? belowFirstColor
      )
    {
      return new DetailPalette
      {
        ColorValues = colorValues,
        AboveLastColor = aboveLastColor,
        BelowFirstColor = belowFirstColor
      };
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (ColorValues.Count == 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
                new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                    "There must be at least one color/value pair in the palette"));
      }
      for (int i = 1; i < ColorValues.Count; i++)
      {
        if (ColorValues[i].Value <= ColorValues[i - 1].Value)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
                   new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                       "Palette values must be distinct and in ascending order"));
        }
      }
    }
  }
}
