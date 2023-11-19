#r "lib/Cake.DependencyTrack.dll"

using Cake.DependencyTrack;
using Cake.DependencyTrack.Models;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
string apiKey="";

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
   // Executed BEFORE the first task.
   Information("Running tasks...");
});

Teardown(ctx =>
{
   // Executed AFTER the last task.
   Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Prepare")
.Does(async (context) => {
    //Reset password
    var data = new[]
    {
        new KeyValuePair<string, string>("username", "admin"),
        new KeyValuePair<string, string>("password", "admin"),
        new KeyValuePair<string, string>("newPassword", "1234567"),
        new KeyValuePair<string, string>("confirmPassword", "1234567"),
    };
    var response = await new HttpClient().PostAsync("http://localhost:8081/api/v1/user/forceChangePassword", new FormUrlEncodedContent(data));
    response.EnsureSuccessStatusCode();
    
    //Generate token
    var data1 = new[]
    {
        new KeyValuePair<string, string>("username", "admin"),
        new KeyValuePair<string, string>("password", "1234567")
    };
    var response1 =
        await new HttpClient().PostAsync("http://localhost:8081/api/v1/user/login", new FormUrlEncodedContent(data1));
    response1.EnsureSuccessStatusCode();
    var token = await response1.Content.ReadAsStringAsync();
    
    //Get self group
    var requestMessage = new HttpRequestMessage(HttpMethod.Get, "http://localhost:8081/api/v1/user/self");
    requestMessage.Headers.Authorization =
        new AuthenticationHeaderValue("Bearer", token);
    
    var response2 = await new HttpClient().SendAsync(requestMessage);
    response2.EnsureSuccessStatusCode();
    var responseBody = await response2.Content.ReadAsStreamAsync();
    var teamuuid = (await JsonSerializer.DeserializeAsync<JsonObject>(responseBody))?["teams"]?[0]?["uuid"]?.ToString();
    
    //Generate API key
    var requestMessage1 = new HttpRequestMessage(HttpMethod.Put, $"http://localhost:8081/api/v1/team/{teamuuid}/key");
    requestMessage1.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    var response3 = await new HttpClient().SendAsync(requestMessage1);
    response3.EnsureSuccessStatusCode();
    var sss = await response3.Content.ReadAsStreamAsync();
    apiKey = (await JsonSerializer.DeserializeAsync<JsonObject>(sss))?["key"]?.ToString();
});

Task("Default")
.IsDependentOn("Prepare")
.Does(async (context) => {
    FileInfo f = new FileInfo("TestData/bom.xml");
    var settings = new UploadBomSettings{
       ProjectName="test",
       Version="CI",
       AutoCreate=true,
       AbsoluteBomFilePath=f.FullName,
       ServerSettings=new ServerSettings{
            BaseServerUrl="http://localhost:8081",
            ApiKey=apiKey
       },
       ShouldValidateMetrics=true,
       MetricsThresholdSettings=new MetricsThresholdSettings{
            CriticalCount=2,
            HighCount=2,
            MediumCount=2,
            LowCount=2
       }
    };
    await context.UploadBomFile(settings);
});

RunTarget(target);
