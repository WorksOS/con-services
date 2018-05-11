using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.TRex.TAGFiles.Classes.Validator
{
    /// <summary>
    /// All details relating to a tagfile request including tagfile contents
    /// </summary>
    public class TagfileDetail
    {
        public long projectId;
        public Guid assetId;
        public string tagFileName;
        public byte[] tagFileContent;
        public string tccOrgId;
    }
}
