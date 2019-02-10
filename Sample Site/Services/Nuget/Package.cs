using System;

namespace SampleSite.Nuget
{
    public class Package
    {
        public string Id{ get; set; }
        public Version Version{ get; set; }
        public string Description { get; set; }
        public string Summary{ get; set; }
        public string Title{ get; set; }
        public string ProjectUrl{ get; set; }
        public string[] Tags{ get; set; }
        public string[] Authors{ get; set; }
        public long TotalDownloads{ get; set; }
        public bool Verified{ get; set; }
        public NuGetVersion[] Versions{ get; set; }
    }
}
