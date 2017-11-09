﻿
using System.Collections.Generic;

namespace VSS.Productivity3D.WebApi.Models.MapHandling
{
  public class Geometry
  {
    public string type { get; set; }
    public List<List<List<double>>> coordinates { get; set; }
  }

  public class Properties
  {
    public string name { get; set; }
  }

  public class Feature
  {
    public string type { get; set; }
    public Geometry geometry { get; set; }
    public Properties properties { get; set; }
  }

  public class RootObject
  {
    public string type { get; set; }
    public List<Feature> features { get; set; }
  }

}
