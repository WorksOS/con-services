using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TagFileHarvester.Models
{
  public class GetFileSpacesParams
  {
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public string filter;
  }
}
