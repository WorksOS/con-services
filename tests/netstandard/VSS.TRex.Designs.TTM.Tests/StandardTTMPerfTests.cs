using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace VSS.TRex.Designs.TTM.Tests
{
    public class StandardTTMPerfTests
    {
      [Fact]
      public void Test_TINLoad()
      {
        VSS.TRex.Designs.TTM.TrimbleTINModel tin = new VSS.TRex.Designs.TTM.TrimbleTINModel();

        // 165Mb TIN 
        DateTime startTime = DateTime.Now;
        tin.LoadFromFile(@"C:\Users\rwilson\Downloads\5644616_oba9c0bd14_FRL.ttm");
        DateTime endTime = DateTime.Now;
        Assert.True(false, $"Duration to load file containing {tin.Triangles.Count} triangles and {tin.Vertices.Count} vertices: {endTime - startTime}");
    }
  }
}
