
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.ResultHandling;

namespace VSS.Raptor.Service.WebApiTests.Common.Models
{
  [TestClass()]
  public class ColorPaletteTests
  {
    [TestMethod()]
    public void CanCreateColorPaletteTest()
    {
      var validator = new DataAnnotationsValidator();
      ColorPalette palette = ColorPalette.CreateColorPalette(0xA5BC4E, 0.2);
      ICollection<ValidationResult> results;
      Assert.IsTrue(validator.TryValidate(palette, out results));
    }

 
 
  }
}
