using FluentAssertions;
using VSS.TRex.Profiling.Models;
using Xunit;

namespace VSS.TRex.Tests.Profiling
{
    public class InterceptRecTests
    {
      [Fact]
      public void Test_InterceptRec_Creation()
      {
        InterceptRec intercept = new InterceptRec(1, 2, 3, 4, 5, 6);

        Assert.True(intercept.OriginX == 1 && 
                    intercept.OriginY == 2 && 
                    intercept.MidPointX == 3 &&
                    intercept.MidPointY == 4 && 
                    intercept.ProfileItemIndex == 5 && 
                    intercept.InterceptLength == 6);
      }

      [Fact]
      public void Test_InterceptRec_Equality_Exact1()
      {
        InterceptRec intercept = new InterceptRec(1, 2, 3, 4, 5, 6);
        InterceptRec intercept2 = new InterceptRec(1, 2, 3, 4, 5, 6);

        Assert.Equal(intercept, intercept2);
      }

      [Fact]
      public void Test_InterceptRec_Equality_Exact2()
      {
        InterceptRec intercept = new InterceptRec(1, 2, 3, 4, 5, 6);
        InterceptRec intercept2 = new InterceptRec(1, 2, 3, 4, 5, 6);

        Assert.Equal(intercept, intercept2);
        Assert.True(intercept.Equals(intercept2), "Intercept recs not equal when they should be (1)");
        Assert.True(intercept.Equals(intercept2.OriginX, intercept2.OriginY, intercept2.ProfileItemIndex), "Intercept recs not equal when they should be (2)");
      }

      [Fact]
      public void Test_InterceptRec_Equality_InExact()
      {
        InterceptRec intercept = new InterceptRec(0.99995f, 1.99995f, 3, 4, 4.99995f, 6);
        InterceptRec intercept2 = new InterceptRec(1, 2, 3, 4, 5, 6);

        Assert.True(intercept.Equals(intercept2), "Intercept recs not equal when they should be (1)");
        Assert.True(intercept.Equals(intercept2.OriginX, intercept2.OriginY, intercept2.ProfileItemIndex), "Intercept recs not equal when they should be (2)");
      }

      [Fact]
      public void Test_InterceptRec_Inequality_InExact()
      {
        InterceptRec intercept = new InterceptRec(0.999f, 1.999f, 3, 4, 4.999f, 6);
        InterceptRec intercept2 = new InterceptRec(1, 2, 3, 4, 5, 6);

        Assert.False(intercept.Equals(intercept2), "Intercept recs not equal when they should be (1)");
        Assert.False(intercept.Equals(intercept2.OriginX, intercept2.OriginY, intercept2.ProfileItemIndex), "Intercept recs not equal when they should be (2)");
      }

      [Fact]
      public void Test_InterceptRec_CompareTo()
      {
        var intercept1 = new InterceptRec(1, 2, 3, 4, 5, 6);
        var intercept2 = new InterceptRec(1, 2, 3, 4, 5, 6);
        var intercept3 = new InterceptRec(1, 2, 3, 4, 0, 6);

        intercept1.CompareTo(intercept2).Should().Be(0);
        intercept1.CompareTo(intercept3).Should().Be(1);
        intercept3.CompareTo(intercept1).Should().Be(-1);
    }

      [Fact]
      public void Test_InterceptRec_Compare()
      {
        var intercept1 = new InterceptRec(1, 2, 3, 4, 5, 6);
        var intercept2 = new InterceptRec(1, 2, 3, 4, 5, 6);
        var intercept3 = new InterceptRec(1, 2, 3, 4, 0, 6);

        intercept1.Compare(intercept1, intercept2).Should().Be(0);
        intercept1.Compare(intercept1, intercept3).Should().Be(1);
        intercept3.Compare(intercept3, intercept1).Should().Be(-1);
      }
  }
}
