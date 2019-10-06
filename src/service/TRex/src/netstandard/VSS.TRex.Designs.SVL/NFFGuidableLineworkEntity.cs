using System.Diagnostics;

namespace VSS.TRex.Designs.SVL
{
  public class NFFGuidableLineworkEntity : NFFLineworkEntity
  {
    protected int _guidanceID;
    
    public NFFGuidableLineworkEntity()
    {
      _guidanceID = -1;
    }

    protected override void SetHeaderFlags(byte Value)
    {
      // NFFGuidableLineworkEntity class MAY have GuidanceID but cannot be Stationed
      Debug.Assert((Value & NFFConsts.kNFFElementHeaderHasStationing) == 0x0);

      _headerFlags = Value;
    }

    protected virtual void SetGuidanceID(int Value) => _guidanceID = Value;

    public override void Assign(NFFLineworkEntity Entity)
    {
      base.Assign(Entity);

      _guidanceID = (Entity as NFFGuidableLineworkEntity).GuidanceID;
    }

    public int GuidanceID
    {
      get => _guidanceID;
      set => SetGuidanceID(value);
    }

    public override byte ElementTypeInFile(NFFFileVersion FileVersion)
    {
      // The element type in the file has two parts: The actual ordinal element type
      // value is the low order nibble, and flags in the high order nibble

      var Result = base.ElementTypeInFile(FileVersion);

      if (FileVersion < NFFFileVersion.Version1_5)
      {
        if (GuidanceID != -1)
          Result |= NFFConsts.kNFFHasGuidanceID;

        if ((HeaderFlags & NFFConsts.kNFFElementHeaderHasStationing) != 0)
          Result |= NFFConsts.kNFFHasStationing;

        if ((HeaderFlags & NFFConsts.kNFFElementHeaderHasElevation) != 0)
          Result |= NFFConsts.kNFFHasHeight;
      }

      return Result;
    }

    public override byte ElementFlagsInFile(NFFFileVersion FileVersion)
    {
      byte Result = 0;

      Debug.Assert(FileVersion >= NFFFileVersion.Version1_5,
        "Separate element flags byte not valid for pre v1.5 NFF files");

      if (FileVersion >= NFFFileVersion.Version1_5)
      {
        if (_guidanceID != -1)
          Result |= NFFConsts.kNFFElementHeaderHasGuidanceID;

        if ((HeaderFlags & NFFConsts.kNFFElementHeaderHasStationing) != 0)
          Result |= NFFConsts.kNFFElementHeaderHasStationing;

        if ((HeaderFlags & NFFConsts.kNFFElementHeaderHasElevation) != 0)
          Result |= NFFConsts.kNFFElementHeaderHasElevation;

        if ((HeaderFlags & NFFConsts.kNFFElementHeaderHasCrossSlope) != 0)
          Result |= NFFConsts.kNFFElementHeaderHasCrossSlope;
      }

      return Result;
    }

    //Procedure SaveToStream(Stream : TStream);Override;

    /*
    public override void LoadFromStream(BinaryReader reader)
    {
      base.LoadFromStream(reader);

      if (IsGuidable)
        _guidanceID = reader.ReadInt32();
    }
    */
  }
}
