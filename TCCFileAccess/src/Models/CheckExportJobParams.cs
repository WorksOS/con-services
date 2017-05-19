using System;
using System.Collections.Generic;
using System.Text;

namespace TCCFileAccess.Models
{
  public class CheckExportJobParams 
  {
    [JsonProperty(PropertyName = "jobid", Required = Required.Always)]
    public string jobid;
  }
}
