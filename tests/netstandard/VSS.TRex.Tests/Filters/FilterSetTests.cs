using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using Xunit;

namespace VSS.TRex.Tests.Filters
{
  public class FilterSetTests
  {
    [Fact]
    public void Test_FilterSet_Creation_SingleNull1()
    {
      var f = new FilterSet();

      Assert.True(f.Filters != null, "Filters in filter set null after creation");
      Assert.True(f.Filters.Length == 0, "Filterset count not zero after creation");
    }

    [Fact]
    public void Test_FilterSet_Creation_SingleNull2()
    {
      var f = new FilterSet((ICombinedFilter)null);

      Assert.True(f.Filters != null, "Filters in filter set null after creation");
      Assert.True(f.Filters.Length == 0, "Filterset count not zero after creation");
    }

    [Fact]
    public void Test_FilterSet_Creation_SingleNull3a()
    {
      var f = new FilterSet((ICombinedFilter)null, new CombinedFilter());

      Assert.True(f.Filters != null, "Filters in filter set null after creation");
      Assert.True(f.Filters.Length == 1, "Filterset count not one after creation");
    }

    [Fact]
    public void Test_FilterSet_Creation_SingleNull3b()
    {
      var f = new FilterSet(new CombinedFilter(), (ICombinedFilter)null);

      Assert.True(f.Filters != null, "Filters in filter set null after creation");
      Assert.True(f.Filters.Length == 1, "Filterset count not one after creation");
    }

    [Fact]
    public void Test_FilterSet_Creation_SingleNull4()
    {
      var f = new FilterSet(new [] {(ICombinedFilter)null, new CombinedFilter(), new CombinedFilter()});

      Assert.True(f.Filters != null, "Filters in filter set null after creation");
      Assert.True(f.Filters.Length == 2, "Filterset count not two after creation");
    }

    [Fact]
    public void Test_FilterSet_Creation_DoubleNull1()
    {
      var f = new FilterSet(new[] { (ICombinedFilter)null, (ICombinedFilter)null });

      Assert.True(f.Filters != null, "Filters in filter set null after creation");
      Assert.True(f.Filters.Length == 0, "Filterset count not zero after creation");
    }

    [Fact]
    public void Test_FilterSet_Creation_DoubleNull2()
    {
      var f = new FilterSet(new[] { (ICombinedFilter)null, (ICombinedFilter)null, new CombinedFilter() });

      Assert.True(f.Filters != null, "Filters in filter set null after creation");
      Assert.True(f.Filters.Length == 1, "Filterset count not one after creation");
    }

    [Fact]
    public void Test_FilterSet_Creation_AllNull1()
    {
      var f = new FilterSet((ICombinedFilter)null, (ICombinedFilter)null);

      Assert.True(f.Filters != null, "Filters in filter set null after creation");
      Assert.True(f.Filters.Length == 0, "Filterset count not zero after creation");
    }

    [Fact]
    public void Test_FilterSet_Creation_AllNull2()
    {
      var f = new FilterSet(new[] { (ICombinedFilter)null, (ICombinedFilter)null, (ICombinedFilter)null });

      Assert.True(f.Filters != null, "Filters in filter set null after creation");
      Assert.True(f.Filters.Length == 0, "Filterset count not zero after creation");
    }

    [Fact]
    public void Test_FilterSet_Creation_EmptyArray()
    {
      var f = new FilterSet(new ICombinedFilter[0]);

      Assert.True(f.Filters != null, "Filters in filter set null after creation");
      Assert.True(f.Filters.Length == 0, "Filterset count not zero after creation");
    }
  }
}
