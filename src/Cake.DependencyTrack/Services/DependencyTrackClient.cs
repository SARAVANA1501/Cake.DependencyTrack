using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Web;
using Cake.DependencyTrack.Models;

namespace Cake.DependencyTrack.Services
{
    internal class DependencyTrackClient : IDependencyTrackClient
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
            var request = HttpRequestMessage(HttpMethod.Put, body, "api/v1/bom");

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStreamAsync();
            var ss = await JsonSerializer.DeserializeAsync<JsonObject>(responseBody);
            return ss["token"]?.ToString();
        }

        public async Task<string> UploadBomAsync(string projectName, string projectVersion, bool autoCreate, string bomFile)
        {
            var byteContent = System.Text.Encoding.UTF8.GetBytes(bomFile);
            var base64Content = Convert.ToBase64String(byteContent);
            var body = new
            {
                projectName,
                projectVersion,
                autoCreate,
                bom = base64Content
            };
            var request = HttpRequestMessage(HttpMethod.Put, body, "api/v1/bom");

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStreamAsync();
            var ss = await JsonSerializer.DeserializeAsync<JsonObject>(responseBody);
            return ss["token"]?.ToString();
        }

        public HttpRequestMessage HttpRequestMessage(HttpMethod method, object body, string path)
        {
            HttpRequestMessage request = new HttpRequestMessage(method,
                new Uri(_baseUri, path).AbsoluteUri);

            request.Headers.Add("accept", "application/json");
            request.Headers.Add("X-Api-Key", _apiKey);

            request.Content = new StringContent(JsonSerializer.Serialize(body));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return request;
        }

        public async Task<Project> GetProjectDetails(string projectName, string version)
        {
            var requestUri = new Uri(_baseUri, "api/v1/project/lookup")
                .AddQuery("name", projectName)
                .AddQuery("version", version).AbsoluteUri;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
                requestUri);
            request.Headers.Add("accept", "application/json");
            request.Headers.Add("X-Api-Key", _apiKey);
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStreamAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
            return await JsonSerializer.DeserializeAsync<Project>(responseBody, options);
        }

        public async Task<AppVersion> GetServerVersion()
        {
            var requestUri = new Uri(_baseUri, "api/version").AbsoluteUri;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Add("accept", "application/json");
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStreamAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
            return await JsonSerializer.DeserializeAsync<AppVersion>(responseBody, options);
        }
    }

    internal static class HttpExtensions
    {
        public static Uri AddQuery(this Uri uri, string name, string value)
        {
            var httpValueCollection = HttpUtility.ParseQueryString(uri.Query);

            httpValueCollection.Remove(name);
            httpValueCollection.Add(name, value);

            var ub = new UriBuilder(uri);
            ub.Query = httpValueCollection.ToString();

            return ub.Uri;
        }
    }
}