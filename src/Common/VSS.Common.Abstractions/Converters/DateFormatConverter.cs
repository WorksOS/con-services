using Newtonsoft.Json.Converters;

namespace VSS.Common.Abstractions.Converters
{
  public class CustomDateFormatConverter : IsoDateTimeConverter
  {
    public CustomDateFormatConverter(string format)
    {
      DateTimeFormat = format;
    }
  }
}