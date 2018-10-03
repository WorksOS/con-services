namespace VSS.TRex.TAGFiles.GridFabric.Responses
{
    public class ProcessTAGFileResponseItem
    {
        public string FileName { get; set; }

        public bool Success { get; set; }

        public string Exception { get; set; }

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public ProcessTAGFileResponseItem()
        {
        }
    }
}
