namespace VSS.TCCFileAccess.Models
{
    public class PutFileResponse : ApiResult
    {
        public string entryId;
        public string path;
        public string md5hash;
    }
}