using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon;
using ED=VSS.Nighthawk.ExternalDataTypes.Enumerations;
namespace VSS.UnitTest.Common
{
  public class SiteHelper
  {
    public static List<Point> GetSitePoints(double startLat, double startLon, double size, double angle)
    {
      List<Point> points = new List<Point>();
      double endLat = startLat;
      double endLon = startLon;
      points.Add(new Point(startLat, startLon));
      for (int i = 0; i < 3; i++)
      {
        RobbinsFormulae.EllipsoidRobbinsForward(endLat, endLon, i * angle, size, out endLat, out endLon);
        points.Add(new Point(endLat, endLon));
      }
      return points;
    }

    public static List<Point> GetRectSitePoints(double startLat, double startLon, double size)
    {
      return GetSitePoints(startLat, startLon, size, 90.0);
    }

#region Point Locations in Sites
    private static Point _naPoint;
    public static Point PointInNorthAmerica
    {
      get
      {
        if (_naPoint == null)
          _naPoint = new Point(38.67, -100.27);
        return _naPoint;
      }
    }

    private static Point _pointInSouthAmerica;
    public static Point PointInSouthAmerica
    {
      get
      {
        if (_pointInSouthAmerica == null)
          _pointInSouthAmerica = new Point(-12.75, -58.61);
        return _pointInSouthAmerica;
      }
    }

    private static Point _pointInAfrica;
    public static Point PointInAfrica
    {
      get
      {
        if (_pointInAfrica == null)
          _pointInAfrica = new Point(-3.31, 25.01);
        return _pointInAfrica;
      }
    }

    private static Point _pointInEurasia;
    public static Point PointInEurasia
    {
      get
      {
        if (_pointInEurasia == null)
          _pointInEurasia = new Point(54.99, 94.68);
        return _pointInEurasia;
      }
    }

    private static Point _pointInAustralia;
    public static Point PointInAustralia
    {
      get
      {
        if (_pointInAustralia == null)
          _pointInAustralia = new Point(-26.81, 143.73);
        return _pointInAustralia;
      }
    }

        private static Point _pointInAntarctica;
    public static Point PointInAntarctica
    {
      get
      {
        if (_pointInAntarctica == null)
          _pointInAntarctica = new Point(-87.61, 40.11);
        return _pointInAntarctica;
      }
    }

        private static Point _pointInNowhere;
    public static Point PointInNowhere
    {
      get
      {
        if (_pointInNowhere == null)
          _pointInNowhere = new Point(73.54, -43.51); // Greenland
        return _pointInNowhere;
      }
    }

#endregion

#region Test sites of continental extent
      //// South America site for customer 1
      //points = new List<Point>();
      //point = new Point(17.3086878867702, -80.859375);
      //points.Add(point);
      //point = new Point(-7.01366792756646, -28.125);
      //points.Add(point);
      //point = new Point(-60.5869673422586, -76.640625);
      //points.Add(point);
      //southAmericaSiteID = API.Site.Create(Ctx.OpContext, customer1ID.Value, session.UserID.Value, "South America", "South America", 0, false, points);

      //// South America 2 site for customer 1
      //points = new List<Point>();
      //point = new Point(22.8955678072006, -84.375);
      //points.Add(point);
      //point = new Point(-5.64013782182219, -22.1484375);
      //points.Add(point);
      //point = new Point(-63.715471486133, -75.234375);
      //points.Add(point);
      //southAmerica2SiteID = API.Site.Create(Ctx.OpContext, customer1ID.Value, session.UserID.Value, "South America 2", "South America 2", 0, false, points);

      //// Eurasia site for customer 1
      //points = new List<Point>();
      //point = new Point(38.2726885359811, -10.546875);
      //points.Add(point);
      //point = new Point(-10.4878118820565, 150.46875);
      //points.Add(point);
      //point = new Point(77.767582382728, 161.71875);
      //points.Add(point);
      //point = new Point(73.4284236410682, 8.4375);
      //points.Add(point);
      //eurasiaSiteID = API.Site.Create(Ctx.OpContext, customer1ID.Value, session.UserID.Value, "Eurasia", "Eurasia", 0, false, points);

      //// Africa site for customer 2
      //points = new List<Point>();
      //point = new Point(40.4469470596006, -18.984375);
      //points.Add(point);
      //point = new Point(13.2399454992865, 54.84375);
      //points.Add(point);
      //point = new Point(-46.0732306254082, 27.421875);
      //points.Add(point);
      //point = new Point(10.4878118820568, -21.796875);
      //points.Add(point);
      //africaSiteID = API.Site.Create(Ctx.OpContext, customer2ID.Value, session.UserID.Value, "Africa", "Africa", 0, false, points);

      //// Australia site for customer 2
      //points = new List<Point>();
      //point = new Point(-9.79567758282957, 108.28125);
      //points.Add(point);
      //point = new Point(-9.79567758282957, 150.46875);
      //points.Add(point);
      //point = new Point(-42.0329743324413, 154.6875);
      //points.Add(point);
      //point = new Point(-42.0329743324413, 108.28125);
      //points.Add(point);
      //australiaSiteID = API.Site.Create(Ctx.OpContext, customer2ID.Value, session.UserID.Value, "Australia", "Australia", 0, false, points);

      //// Antarctica site for customer 2
      //points = new List<Point>();
      //point = new Point(-67.8755413467294, -175.78125);
      //points.Add(point);
      //point = new Point(-60.9304322029232, 163.125);
      //points.Add(point);
      //point = new Point(-82.1183836069127, 163.125);
      //points.Add(point);
      //point = new Point(-81.9231863260219, -172.265625);
      //points.Add(point);
      //antarcticaSiteID = API.Site.Create(Ctx.OpContext, customer2ID.Value, session.UserID.Value, "Antarctica", "Antarctica", 0, false, points);
#endregion
  }
}
