using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Converters;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Validation;

namespace VSS.Productivity3D.Models.UnitTests
{
  [TestClass]
  public class BoundingBoxTests
  {
    [TestMethod]
    public void CanCreateBoundingBox2DGridTest()
    {
      var validator = new DataAnnotationsValidator();
      BoundingBox2DGrid bbox = new BoundingBox2DGrid(380646.982394, 812634.205106, 380712.19834, 812788.92875);
      ICollection<ValidationResult> results;
      Assert.IsTrue(validator.TryValidate(bbox, out results));
    }

    [TestMethod]
    public void ValidateBoundingBox2DGridSuccessTest()
    {
      BoundingBox2DGrid bbox = new BoundingBox2DGrid(380646.982394, 812634.205106, 380712.19834, 812788.92875);
      bbox.Validate();
    }

    [TestMethod]
    public void ValidateBoundingBox2DGridFailTest()
    {
      //not bl and tr
      BoundingBox2DGrid bbox = new BoundingBox2DGrid(380712.19834, 812634.205106, 380646.982394, 812788.92875);
      Assert.ThrowsException<ServiceException>(() => bbox.Validate());
    }

    [TestMethod]
    public void CanCreateBoundingBox2DLatLonTest()
    {
      var validator = new DataAnnotationsValidator();
      BoundingBox2DLatLon bbox = new BoundingBox2DLatLon(
        -106.604076 * Coordinates.DEGREES_TO_RADIANS, 35.109149 * Coordinates.DEGREES_TO_RADIANS, -105.234 * Coordinates.DEGREES_TO_RADIANS, 35.39012 * Coordinates.DEGREES_TO_RADIANS);
      ICollection<ValidationResult> results;
      Assert.IsTrue(validator.TryValidate(bbox, out results));

      //too big value
      bbox = new BoundingBox2DLatLon(
        -106.604076 * Coordinates.DEGREES_TO_RADIANS, 35.109149 * Coordinates.DEGREES_TO_RADIANS, -105.234 * Coordinates.DEGREES_TO_RADIANS, 525.5 * Coordinates.DEGREES_TO_RADIANS);
      Assert.IsFalse(validator.TryValidate(bbox, out results));
    }

    [TestMethod]
    public void ValidateBoundingBox2DLatLonSuccessTest()
    {
      BoundingBox2DLatLon bbox = new BoundingBox2DLatLon(-0.758809, 3.010479, -0.741023, 3.0567);
      bbox.Validate();
    }

    [TestMethod]
    public void ValidateBoundingBox2DLatLonFailTest()
    {
      //not bl and tr
      BoundingBox2DLatLon bbox = new BoundingBox2DLatLon(-0.741023, 3.010479, -0.758809, 3.0567);
      Assert.ThrowsException<ServiceException>(() => bbox.Validate());
    }
  }
}
