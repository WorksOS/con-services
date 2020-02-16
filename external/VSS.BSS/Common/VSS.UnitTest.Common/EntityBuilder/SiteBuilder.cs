using System;
using System.Collections.Generic;
using System.Linq;
using VSS.UnitTest.Common.Contexts;
using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common.EntityBuilder 
{
  public class SiteBuilder
  {
    private SiteTypeEnum _siteType = SiteTypeEnum.Generic;
    private long _id = IdGen.GetId();
    private string _name = "SITE_NAME_" + IdGen.GetId();
    private string _description = "SITE_DECRIPTION_" + IdGen.GetId();
    private int _color = 1;
    private bool _transparent = true;
    private bool _visible = true;
    private DateTime _updateUtc = DateTime.UtcNow;
    private Customer _customer;
    private User _createdByUser;
    private User _favoriteUser;
    private List<Point> _sitePoints = new List<Point>();
    private double? _areaSt;

    private static readonly IUUIDSequentialGuid UidBuilder = new UUIDSequentialGuid();

    public SiteBuilder(SiteTypeEnum siteType)
    {
      _siteType = siteType;
    }
    public SiteBuilder Id(long id)
    {
      _id = id;
      return this;
    }
    public SiteBuilder AreaST(double? areaST)
    {
      _areaSt = areaST;
      return this;
    }
    public SiteBuilder Name(string name)
    {
      _name = name;
      return this;
    }
    public SiteBuilder Description(string description)
    {
      _description = description;
      return this;
    }
    public SiteBuilder Colour(int color)
    {
      _color = color;
      return this;
    }
    public SiteBuilder Opaque()
    {
      _transparent = false;
      return this;
    }
    public SiteBuilder Invisible()
    {
      _visible = false;
      return this;
    }
    public SiteBuilder UpdateUtc(DateTime updateUtc)
    {
      _updateUtc = updateUtc;
      return this;
    }
    public SiteBuilder WithSitePoints(IList<Point> sitePoints)
    {
      _sitePoints.AddRange(sitePoints);
      return this;
    }
    public SiteBuilder WithPoint(Point sitePoint)
    {
      _sitePoints.Add(sitePoint);
      return this;
    }
    public SiteBuilder WithPoint(double latitude, double longitude)
    {
      _sitePoints.Add(new Point(latitude, longitude));
      return this;
    }
    public SiteBuilder ForCustomer(Customer customer)
    {
      _customer = customer;
      return this;
    }
    public SiteBuilder CreatedByUser(User createdByUser)
    {
      _createdByUser = createdByUser;
      return this;
    }

    public SiteBuilder FavoriteUser(User favoriteUser)
    {
      _favoriteUser = favoriteUser;
      return this;
    }
    public SiteBuilder Type(SiteTypeEnum siteType)
    {
      _siteType = siteType;
      return this;
    }

    public Site Build()
    {
      var site = new Site();

      CheckValidSite(_customer.ID, _name);

      if(_sitePoints.Count > 0)
      {
        GetSitePolygon(site, _sitePoints);
      }
      else
      {
        GetSitePoints(site, 29.9520111083984, -90.0894470214844, 100);
      }
      
      site.ID = _id;
      site.fk_SiteTypeID = (int)_siteType;
      site.Name = _name;
      site.Description = _description;
      site.Colour = _color;
      site.Transparent = _transparent;
      site.Visible = _visible;
      site.UpdateUTC = _updateUtc;

      site.fk_CustomerID = _customer.ID;
      site.fk_UserID = _createdByUser.ID;
      site.AreaST = _areaSt;

      site.SiteUID = UidBuilder.CreateGuid();

      return site;
    }
    public Site Save()
    {
      var site = Build();
 
      ContextContainer.Current.OpContext.Site.AddObject(site);
      ContextContainer.Current.OpContext.SaveChanges();

      SiteAccess.PopulateGeometry(site.ID); // Hmmm, this makes this builder only suitable for a DatabaseTest, ie not for Mock.

      return site;
    }

    private void CheckValidSite(long customerID, string name)
    {
      bool siteExists = ContextContainer.Current.OpContext.SiteReadOnly.Any(site => site.fk_CustomerID == customerID && site.Name == name && site.Visible);
      if (siteExists)
      {
        throw new InvalidOperationException("Can not have multiple sites with the same name");
      }
    }
    private void GetSitePolygon(Site site, IList<Point> points)
    {
      double backBearing = 0;
      double forwardBearing = 0;
      double distance = 0;
      double endLat = 0;
      double endLon = 0;

      foreach (Point p in points)
      {
        site.MaxLon = Math.Max(p.Longitude, site.MaxLon.HasValue ? site.MaxLon.Value : Int32.MinValue);
        site.MaxLat = Math.Max(p.Latitude, site.MaxLat.HasValue ? site.MaxLat.Value : Int32.MinValue);
        site.MinLon = Math.Min(p.Longitude, site.MinLon.HasValue ? site.MinLon.Value : Int32.MaxValue);
        site.MinLat = Math.Min(p.Latitude, site.MinLat.HasValue ? site.MinLat.Value : Int32.MaxValue);
        site.AddPoint(p.Latitude, p.Longitude);
      }

      RobbinsFormulae.EllipsoidRobbinsReverse(site.MinLat.Value, site.MinLon.Value, site.MaxLat.Value, site.MaxLon.Value, out forwardBearing, out backBearing, out distance);

      RobbinsFormulae.EllipsoidRobbinsForward(site.MinLat.Value, site.MinLon.Value, forwardBearing, (distance / 2.0), out endLat, out endLon);

      foreach (Point p in points)
      {
        RobbinsFormulae.EllipsoidRobbinsReverse(endLon, endLat, p.Latitude, p.Longitude,
        out forwardBearing, out backBearing, out distance);
      }
    }

    public void GetSitePoints(Site site, double startLat, double startLon, double size)
    {
      site.AddPoint(startLat, startLon);
      
      double endLat = startLat;
      double endLon = startLon;

      for (int i = 0; i < 3; i++)
      {
        RobbinsFormulae.EllipsoidRobbinsForward(endLat, endLon, i * 90, size, out endLat, out endLon);
        site.AddPoint(endLat, endLon);
      }
    }

  }
}
