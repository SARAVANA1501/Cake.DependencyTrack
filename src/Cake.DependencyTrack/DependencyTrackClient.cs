using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Nodes;


namespace Cake.DependencyTrack
{
    internal class DependencyTrackClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly Uri _baseUri;

        public DependencyTrackClient(HttpClient httpClient, string baseUrl, string apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
            _baseUri = new Uri(baseUrl);
        }

        public async Task<string> UploadBomAsync(string projectId, string bomFile)
        {
            var byteContent = System.Text.Encoding.UTF8.GetBytes(bomFile);
            var base64Content = Convert.ToBase64String(byteContent);
            var body = new
            {
                project = projectId,
                bom = base64Content
            };
            var request = HttpRequestMessage(HttpMethod.Put, body,"api/v1/bom");

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStreamAsync();
            var ss = await JsonSerializer.DeserializeAsync<JsonObject>(responseBody);
            return ss["token"]?.ToString();
        }

        private HttpRequestMessage HttpRequestMessage(HttpMethod method, object body, string path)
        {
            HttpRequestMessage request = new HttpRequestMessage(method,
                new Uri(_baseUri, path).AbsoluteUri);

            request.Headers.Add("accept", "application/json");
            request.Headers.Add("X-Api-Key", _apiKey);

            request.Content = new StringContent(JsonSerializer.Serialize(body));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return request;
        }
    }
}