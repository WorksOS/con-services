using System;

namespace BookMark
{
    public class XmlBookMark
    {
        public string Customer { get; set; }
        public DateTime BookmarkUtc { get; set; }
        public DateTime LastUpdateDateTime { get; set; }
        public string LastFilesProcessed { get; set; }
        public string LastFilesErrorneous { get; set; }
        public string TotalFilesProcessed { get; set; }
    }
}
