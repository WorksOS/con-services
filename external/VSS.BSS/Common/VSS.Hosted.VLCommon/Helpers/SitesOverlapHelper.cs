using System;
using System.Collections.Generic;
using System.Linq;
using ClipperLib;

namespace VSS.Hosted.VLCommon.Helpers
{
  public static class SitesOverlapHelper
  {
    private const float SCALE = 100000;

    public static List<List<IntPoint>> ClipperIntersects(List<IntPoint> oldPolygon, List<IntPoint> newPolygon)
    {
      //Note: the clipper library uses 2D geometry while we have spherical coordinates. But it should be near enough for our purposes.

      Clipper c = new Clipper();
      c.AddPath(oldPolygon, PolyType.ptSubject, true);
      c.AddPath(newPolygon, PolyType.ptClip, true);
      List<List<IntPoint>> solution = new List<List<IntPoint>>();
      bool succeeded = c.Execute(ClipType.ctIntersection, solution, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);
      return succeeded ? solution : null;
    }

    public static List<IntPoint> ClipperPolygon(Site site)
    {
      List<Point> points = site.PolygonPoints as List<Point>;
      List<IntPoint> polygon = new List<IntPoint>(points.ConvertAll(delegate(Point p) { return new IntPoint { X = (Int64)(p.x * SCALE), Y = (Int64)(p.y * SCALE) }; }));
      return polygon;
    }

    public static bool SiteBoundingBoxesOverlap(Site oldSite, Site newSite)
    {
      bool noOverlap = (oldSite.MaxLat < newSite.MinLat || newSite.MinLat > oldSite.MaxLat) &&
                       (oldSite.MaxLon < newSite.MinLon || newSite.MinLon > oldSite.MaxLon);
      return !noOverlap;
    }

    public static Guid? ProjectAssociatedWithLandfillSite(INH_OP ctx, long customerID, Site landfillSite)
    {
      if (landfillSite.fk_SiteTypeID != (int)SiteTypeEnum.Landfill)
        return null;

      //Get all active landfill projects for customer
      var projectSites = (from p in ctx.ProjectReadOnly
                          join s in ctx.SiteReadOnly on p.fk_SiteID equals s.ID
                          where
                              p.fk_CustomerID == customerID && p.Active &&
                              p.fk_ProjectTypeID == (int)ProjectTypeEnum.Landfill
                          select new { p.ProjectUID, Site = s }).ToList();
      //See if new landfill site overlaps any Landfill project
      List<IntPoint> landfillPolygon = ClipperPolygon(landfillSite);

      foreach (var ps in projectSites)
      {
        //Bounding box test first
        if (SitesOverlapHelper.SiteBoundingBoxesOverlap(landfillSite, ps.Site))
        {
          //Check polygon overlap
          List<List<IntPoint>> intersectingPolygons = ClipperIntersects(ClipperPolygon(ps.Site), landfillPolygon);
          if (intersectingPolygons != null && intersectingPolygons.Count > 0)
          {
            return ps.ProjectUID;
          }
        }
      }
      return null;
    }

    public static List<Guid> LandfillSitesAssociatedWithProject(INH_OP ctx, long customerID, Site projectSite)
    {
      if (projectSite.fk_SiteTypeID != (int) SiteTypeEnum.Project)
        return null;

      //Get all active landfill sites for customer
      var landfillSites = (from s in ctx.SiteReadOnly
                          where
                              s.fk_CustomerID == customerID && s.Visible &&
                              s.fk_SiteTypeID == (int)SiteTypeEnum.Landfill
                          select s).ToList();

      if (landfillSites.Count == 0)
        return null;

      List<Guid> siteUids = new List<Guid>();
      //See if any landfill sites overlaps Landfill project site
      List<IntPoint> projectPolygon = ClipperPolygon(projectSite);

      foreach (var ls in landfillSites)
      {
        //Bounding box test first
        if (SitesOverlapHelper.SiteBoundingBoxesOverlap(projectSite, ls))
        {
          //Check polygon overlap
          List<List<IntPoint>> intersectingPolygons = ClipperIntersects(ClipperPolygon(ls), projectPolygon);
          if (intersectingPolygons != null && intersectingPolygons.Count > 0)
          {
            if (ls.SiteUID.HasValue)
              siteUids.Add(ls.SiteUID.Value);
          }
        }
      }
      return siteUids;
    }

  }
}
