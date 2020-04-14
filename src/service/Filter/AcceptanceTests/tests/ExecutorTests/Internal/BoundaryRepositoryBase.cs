using System;

namespace ExecutorTests.Internal
{
  public class BoundaryRepositoryBase : TestControllerBase
  {
    public void Setup()
    {
      SetupDI();      
    }

    protected static string GenerateWKTPolygon()
    {
      var x = new Random().Next(-200, -100);
      var y = new Random().Next(100);

      return $"POLYGON(({x}.347189366818 {y}.8361907402694,{x}.349260032177 {y}.8361656688414,{x}.349217116833 {y}.8387897637231,{x}.347275197506 {y}.8387145521594,{x}.347189366818 {y}.8361907402694,{x}.347189366818 {y}.8361907402694))";
    }
  }
}
