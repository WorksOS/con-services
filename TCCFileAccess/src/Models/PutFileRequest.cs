namespace VSS.TCCFileAccess.Models
{
    public class PutFileRequest
    {
        public string filespaceid;
        public string path;
        public bool replace;
        public bool commitUpload;
        public string filename;
  }
}