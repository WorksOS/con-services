using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Validation;

namespace VSS.Productivity3D.Models.UnitTests
{
  [TestClass]
  public class ColorPaletteTests
  {
    [TestMethod]
    public void CanCreateColorPaletteTest()
    {
      var validator = new DataAnnotationsValidator();
      ColorPalette palette = ColorPalette.CreateColorPalette(0xA5BC4E, 0.2);
      ICollection<ValidationResult> results;
      Assert.IsTrue(validator.TryValidate(palette, out results));
    }
  }
}