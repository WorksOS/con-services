using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Text;
using System.Data.Entity.Core.Objects;
using System.Xml.Linq;
using Microsoft.SqlServer.Types;


namespace VSS.Hosted.VLCommon
{
  // This partial class was created to allow access to the spatial geometry data, which the EF does not support at the time of writing.
  // Once we are using a version of the EF and .NET that supports these spatial types natively, then this partial class and its access DAO class
  // are no longer needed because the EF can be used to read the data.
  partial class DimTimeZone
  {
    public SqlGeometry PolygonWKT { get; set; }
    public SqlGeometry BoundingBoxWKT { get; set; }


    public string GetAbbreviation(DateTime utc)
    {
      return !string.IsNullOrEmpty(StdAbbrev) ? StdAbbrev.Trim() : GmtAbbrev(IsDaylightSaving(utc) ? (DstBias ?? 0) : StdBias);
    }

    public DateTime GetLocalTime(DateTime utc)
    {
      if (utc == DateTime.MinValue || utc == DateTime.MaxValue || utc.Kind == DateTimeKind.Local)
        return utc;

      if (utc.Ticks == 0)
        return new DateTime(0, DateTimeKind.Local);

      DateTime local = new DateTime(utc.Ticks, DateTimeKind.Local);

      if (IsDaylightSaving(local))
      {
        local = local.Add(TimeSpan.FromMinutes(DstBias ?? 0));
      }
      else
      {
        local = local.Add(TimeSpan.FromMinutes(StdBias));
      }

      return local;
    }


    private bool IsDaylightSaving(DateTime standardTime)
    {
      bool isDaylightSaving = false;

      if (DstStart.HasValue && DstEnd.HasValue)
      {
        if (DstStart.Value.Ticks < DstEnd.Value.Ticks) // Northern Hemisphere
        {
          isDaylightSaving = (standardTime.Ticks >= DstStart.Value.Ticks) && (standardTime.Ticks < DstEnd.Value.Ticks);
        }
        else
        {
          isDaylightSaving = !((standardTime.Ticks >= DstEnd.Value.Ticks) && (standardTime.Ticks < DstStart.Value.Ticks));
        }
      }

      return isDaylightSaving;
    }


    public static string GmtAbbrev(int biasMinutes)
    {
      int hours = biasMinutes / 60;
      int mins = Math.Abs(biasMinutes) - (Math.Abs(hours) * 60);
      return string.Format("GMT{0:00}:{1:00}", hours, mins);
    }

  }

  partial class DimFault : IEquatable<DimFault>
  {
    /// <summary>
    /// Gets the business signature used as the id in the data topic feed. This is used to determine uniqueness/identity 
    /// when performing the lookup for the fmi description set id in the description merge.
    /// </summary>
    /// <value>The data topic feed identity signature.</value>
    //public string Signature
    //{
    //  get
    //  {
    //    var cid = this.DimFaultParameter.Where(e => e.DimFaultParameterType.Description == "CID").FirstOrDefault();
    //    var fmi = this.DimFaultParameter.Where(e => e.DimFaultParameterType.Description == "FMI").FirstOrDefault();
    //    string cidValue = cid != null ? cid.Value.ToString() : string.Empty;
    //    string fmiValue = fmi != null ? fmi.Value.ToString() : string.Empty;
    //    string dataLinkValue = (this.DimDatalink != null && !string.IsNullOrEmpty(this.DimDatalink.Name))
    //                             ? this.DimDatalink.Name
    //                             : string.Empty;

    //    return string.Format("{0}_{1}_{2}", cidValue, fmiValue, dataLinkValue);
        
    //  }
    //}

    public override bool Equals(object obj)
    {
      return Equals(obj as DimFault);
    }

    public bool Equals(DimFault other)
    {
      return (other != null && this.CodedDescription.Equals(other.CodedDescription));
    }

    public override int GetHashCode()
    {
      return CodedDescription.GetHashCode();
    }
  }

  partial class FactAssetUtilizationDaily : IFact { }
  partial class FactFault : IFact { }
  partial class FuelLossCandidate : IFact { }
  partial class FluidAnalysis : IFact { }
  partial class FactCatInspection : IFact { }
  partial class HoursLocation : IFact { }
  partial class vw_LocationHistory : IFact { }
  partial class FactAssetOperationPeriod : IFact { }
  partial class vw_AssetUtilizationDaily : IFact { }
  partial class vw_AssetUsage : IFact { }
  partial class vw_FuelUsage : IFact { }

  partial class DimAssetMonitoringSettings
  {
    private static readonly char BREAK_TIME_SEPARATOR = ':';

    public List<BreakTime> breakTimesList;

    public DimAssetMonitoringSettings WithParsedBreakTimesXML
    {
      get
      {
        ParseBreakTimesXML();

        return this;
      }
    }

    public void ParseBreakTimesXML()
    {
      if (string.IsNullOrEmpty(this.BreakTimesXML))
        return;

      XElement element = XElement.Parse(this.BreakTimesXML);
      Parse(element);
    }

    private void Parse(XElement element)
    {
      if (element != null)
      {
        this.breakTimesList = null;
        if (element.Name == "BreakTimes")
        {
          List<XElement> listElement = element.Descendants("BreakTime").ToList();
          if (listElement != null && listElement.Count > 0)
          {
            this.breakTimesList = new List<BreakTime>();
            for (int i = 0; i < listElement.Count; i++)
            {
              this.breakTimesList.Add(new BreakTime(listElement[i]));
            }
          }
        }
      }
    }

    public XElement ToXElement()
    {
      XElement listElement = new XElement("BreakTimes");
      if (this.breakTimesList != null)
      {
        for (int i = 0; i < this.breakTimesList.Count; i++)
        {
          listElement.Add(this.breakTimesList[i].ToXElement());
        }
      }
      return listElement;
    }
    
    public class BreakTime
    {
      public int fromHours;
      public int fromMinutes;
      public int toHours;
      public int toMinutes;

      public BreakTime()
      {
      }

      public BreakTime(XElement element)
      {
        Parse(element);
      }

      public XElement ToXElement()
      {
        XElement element = new XElement("BreakTime");
        element.Add(ToXElementPart("From", fromHours, fromMinutes));
        element.Add(ToXElementPart("To", toHours, toMinutes));
        return element;
      }

      private XElement ToXElementPart(string elementName, int hours, int minutes)
      {
        return new XElement(elementName, string.Format("{0}{1}{2}", hours, BREAK_TIME_SEPARATOR, minutes));
      }

      private void Parse(XElement element)
      {
        ParseParts(element, "From", out fromHours, out fromMinutes);
        ParseParts(element, "To", out toHours, out toMinutes);       
      }

      private void ParseParts(XElement element, string elementName, out int hours, out int minutes)
      {
        hours = 0;
        minutes = 0;
        XElement child = element.Elements(elementName).FirstOrDefault();
        if (child != null)
        {
          string[] parts = child.Value.Split(new char[] {BREAK_TIME_SEPARATOR});
          int.TryParse(parts[0], out hours);
          int.TryParse(parts[1], out minutes);
        }
      }
 
 
    }
  }

  public partial class NH_RPT
  {
    [EdmFunction("VSS.Hosted.VLCommon.Store", "fn_GetDistanceBetweenPoints")]
    public static double fn_GetDistanceBetweenPoints(double startingLatitude, double startingLongitude, double endingLatitude, double endingLongitude)
    {
      throw new NotSupportedException("Direct calls are not supported.");
    }
  }
}
