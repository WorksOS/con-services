using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using VSS.TRex.Designs.SVL.Comparers;
using VSS.TRex.Designs.SVL.Utilities;

namespace VSS.TRex.Designs.SVL
{
  public class NFFStreamInfoList : List<NFFStreamInfo>
  {
    //   Procedure SaveToStream(NFFFileVersion : NFFFileVersion; Stream : TStream);

    public void LoadFromStream(NFFFileVersion NFFFileVersion, BinaryReader reader)
    {
      var DirectoryHeader = new NFFDirectoryStreamHeader();

      var b = reader.ReadBytes(Marshal.SizeOf(DirectoryHeader));

      var handle = GCHandle.Alloc(b, GCHandleType.Pinned);
      DirectoryHeader = (NFFDirectoryStreamHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(NFFDirectoryStreamHeader));

      if (NFFUtils.MagicNumberToANSIString(DirectoryHeader.MajicNumber) != NFFConsts.kNFFIndexFileMajicNumber)
        return;

      // No stream lists are read from pre version 1.2 format files.
      if (NFFFileVersion < NFFFileVersion.Version1_2)
        return;

      int Offset= 0;
      int Length= 0;
      int Number = reader.ReadInt32();
      for (int I = 0; I < Number; I++)
      {
        string Name = NFFUtils.ReadWideStringFromStream(reader);
        if (NFFFileVersion >= NFFFileVersion.Version1_2)
        {
          Offset = reader.ReadInt32();
          Length = reader.ReadInt32();
        }

        Add(new NFFStreamInfo(Name, Offset, Length));
      }

      // Be paranoid and resort the stream list
      Sort();
    }

    protected void Sort()
    {
      // Sort all the elements in the list on the basis of the stream name
      Sort(new NFFStreamListComparer());

      // Make sure the header stream is at the start of the list

      var master = Locate(NFFConsts.kNFFIndexStorageName);
      if (master == null)
        throw new Exception("No master alignment in file");

      Remove(master);
      Insert(0, master);
    }

    public NFFStreamInfo Locate(string Filename)
    {
      return this.FirstOrDefault(x => string.Compare(x.Name, Filename, StringComparison.OrdinalIgnoreCase) == 0);
    }
  }
}
