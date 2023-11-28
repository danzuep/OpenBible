using System.Net.Http.Json;

namespace Bible.Reader.Services
{
    public sealed class WebZefaniaService
    {
        //https://github.com/kohelet-net-admin/zefania-xml-bibles/blob/master/Bibles/downloads.csv
        private static readonly string _serviceBaseAddress = "https://raw.githubusercontent.com/";
        private static readonly string _serviceIndexPath = "kohelet-net-admin/zefania-xml-bibles/master/Bibles/";

        private readonly HttpClient _httpClient = new HttpClient() { BaseAddress = new Uri(_serviceBaseAddress) };

        public async Task GetBiblesAsync(string endpointsSuffix = "index.csv")
        {
            var endpointsQuery = $"{_serviceIndexPath}{endpointsSuffix}";
            var endpoints = await _httpClient.GetFromJsonAsync<IList<string>>(endpointsQuery);
            var kvp = new Dictionary<string, string>();
            if (endpoints != null)
            {
                var namesPath = $"{_serviceBaseAddress}{_serviceIndexPath}";
                //var names = endpoints.Select(ep => ep[namesPath.Length..]);
                //var languageGroups = from name in names
                //    group names by name.Split('/')[0] into languageGroup
                //    select languageGroup;
                foreach (var endpoint in endpoints)
                {
                    var nameParts = endpoint[namesPath.Length..].Split('/');
                    var key = string.Join("-", nameParts[0].ToLowerInvariant(), nameParts[1]);
                    kvp.Add(key, endpoint);
                }
            }
        }

    }
}
