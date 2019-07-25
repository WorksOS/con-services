using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace VSS.TRex.Designs.SVL
{
  public class TNFFStreamInfoList : List<TNFFStreamInfo>
  {
    //   Procedure SaveToStream(NFFFileVersion : TNFFFileVersion; Stream : TStream);

    public void LoadFromStream(TNFFFileVersion NFFFileVersion, BinaryReader reader)
    {
      var DirectoryHeader = new TNFFDirectoryStreamHeader();

      var b = reader.ReadBytes(Marshal.SizeOf(DirectoryHeader));

      var handle = GCHandle.Alloc(b, GCHandleType.Pinned);
      DirectoryHeader = (TNFFDirectoryStreamHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(TNFFDirectoryStreamHeader));

      if (NFFUtils.MagicNumberToANSIString(DirectoryHeader.MajicNumber) != NFFConsts.kNFFIndexFileMajicNumber)
        return;

      // No stream lists are read from pre version 1.2 format files.
      if (NFFFileVersion < TNFFFileVersion.nffVersion1_2)
        return;

      int Offset= 0;
      int Length= 0;
      int Number = reader.ReadInt32();
      for (int I = 1; I < Number; I++)
      {
        string Name = NFFUtils.ReadWideStringFromStream(reader);
        if (NFFFileVersion >= TNFFFileVersion.nffVersion1_2)
        {
          Offset = reader.ReadInt32();
          Length = reader.ReadInt32();
        }

        Add(new TNFFStreamInfo(Name, Offset, Length));
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

    public TNFFStreamInfo Locate(string Filename)
    {
      return this.FirstOrDefault(x => string.Compare(x.Name, Filename, StringComparison.OrdinalIgnoreCase) == 0);
    }
  }
}
