using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor
{
    [Serializable]
    public struct DesignDescriptor : IEquatable<DesignDescriptor>
    {
        public long DesignID;
        public string FileSpace;
        public string FileSpaceID;
        public string Folder;
        public string FileName;
        public double Offset;

        public DesignDescriptor(long designID,
                                string fileSpace,
                                string fileSpaceID,
                                string folder,
                                string fileName,
                                double offset)
        {
            DesignID = designID;
            FileSpace = fileSpace;
            FileSpaceID = fileSpaceID;
            Folder = folder;
            FileName = fileName;
            Offset = offset;
        }

        public void Init(long designID,
                         string fileSpace,
                         string fileSpaceID,
                         string folder,
                         string fileName,
                         double offset)
        {
            DesignID = designID;
            FileSpace = fileSpace;
            FileSpaceID = fileSpaceID;
            Folder = folder;
            FileName = fileName;
            Offset = offset;
        }

        public string FullPath => Path.Combine(Folder, FileName);

        public bool IsNull => String.IsNullOrEmpty(FileName);
        
        /// <summary>
        /// Overloaded ToString detailing the state of the Design Descriptor
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("[{0}:'{1}', '{2}', '{3}', '{4}', '{5}']", DesignID, FileSpace, FileSpaceID, Folder, FileName, Offset);
        }

        public bool Equals(DesignDescriptor other)
        {
            return (DesignID == other.DesignID) &&
                   (FileSpaceID == other.FileSpaceID) &&
                   (Folder == other.Folder) &&
                   (FileName == other.FileName) &&
                   (Offset == other.Offset);
        }

        public void Clear() => Init(0, "", "", "", "", 0.0);

        public static DesignDescriptor Null()
        {
            DesignDescriptor result = new DesignDescriptor();
            result.Clear();
            return result;
        }
    }
}
