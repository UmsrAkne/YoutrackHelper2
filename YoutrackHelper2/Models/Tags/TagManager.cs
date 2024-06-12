using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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

        public async Task AddTag(Tag tag)
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri(uri);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // タグを作成するためのJSONデータを準備
            var jsonContent = JsonConvert.SerializeObject(new { name = tag.Name, });
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // POSTリクエストを送信
            // GETリクエストか、POSTリクエストかで処理の内容が変化する。
            var response = await client.PostAsync("api/tags?fields=id,name", content);
            response.EnsureSuccessStatusCode();
        }
    }
}