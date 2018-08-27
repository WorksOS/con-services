using System;

namespace VSS.TRex.TAGFiles.Classes.Validator
{
    /// <summary>
    /// All details relating to a tagfile request including tagfile contents
    /// </summary>
    public class TagFileDetail
    {
        public Guid? projectId;
        public Guid? assetId;
        public string tagFileName;
        public byte[] tagFileContent;
        public string tccOrgId;
        public bool IsJohnDoe;
    }
}
