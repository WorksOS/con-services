using VSS.TRex.Exports.Surfaces;
using Xunit;

namespace VSS.TRex.Tests.Exports.Surfaces
{
    public class CandidateTests
    {
      [Fact]
      public void Candidate_Creation()
      {
        Candidate candidate = new Candidate();

        Assert.True(candidate.X == 0);
        Assert.True(candidate.Y == 0);

        Assert.True(candidate.Z == 0.0);
        Assert.True(candidate.Import == 0.0);
    }

      [Fact]
      public void Candidate_CreationWithImport()
      {
        const double import = 123.456;

        Candidate candidate = new Candidate(import);

        Assert.True(candidate.X == 0);
        Assert.True(candidate.Y == 0);

        Assert.True(candidate.Z == 0.0);
        Assert.True(candidate.Import == import);
      }

      [Fact]
      public void Candidate_ConsiderAccept()
      {
        const double import = 123.456;

        Candidate candidate = new Candidate(import);

        Assert.True(candidate.Consider(123.457));
        Assert.False(candidate.Consider(123.455));
      }

      [Fact]
      public void Candidate_Update()
      {
        const double import = 123.456;

        Candidate candidate = new Candidate(import);

        candidate.Update(23, 34, 45, 56);
        Assert.True(candidate.X == 23);
        Assert.True(candidate.Y == 34);
        Assert.True(candidate.Z == 45);
        Assert.True(candidate.Import == 56);
      }

  }
}
