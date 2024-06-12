using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace YoutrackHelper2.Models.Tags
{
    public class TagManager : ITagManager
    {
        private string uri;
        private string token;

        public void SetConnection(string url, string tokenStr)
        {
            uri = $"{url}/api";
            token = tokenStr;
        }

        public async Task<List<Tag>> GetTags()
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri(uri);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("api/tags?fields=id,name");
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Tag>>(responseBody);
        }
    }
}