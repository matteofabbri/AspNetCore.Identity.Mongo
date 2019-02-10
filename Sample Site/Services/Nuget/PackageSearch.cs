using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SampleSite.Nuget
{
    public class PackageSearch
    {
        public int TotalHits { get; set; }

        public DateTime LastReopen { get; set; }

        public string Index { get; set; }

        public Package[] Data { get; set; }

        public static async Task<PackageSearch> GetAsync(string q, bool includePreRelease=false)
        {
            var queryUrl = $"https://api-v2v3search-0.nuget.org/query?prerelease={includePreRelease}&q={Uri.EscapeDataString(q)}";
            var json = await (new HttpClient()).GetStringAsync(queryUrl).ConfigureAwait(false);

            return JsonConvert.DeserializeObject<PackageSearch>(json);
        }
    }
}
