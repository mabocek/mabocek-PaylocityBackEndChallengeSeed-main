using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;

namespace ApiTests;

internal static class ShouldExtensions
{
    public const string defaultApplicationJson = "application/json";
    public static Task ShouldReturn(this HttpResponseMessage response, HttpStatusCode expectedStatusCode)
    {
        Assert.Equal(expectedStatusCode, response.StatusCode);
        return Task.CompletedTask;
    }

    public static async Task ShouldReturn<T>(this HttpResponseMessage response, HttpStatusCode expectedStatusCode, T expectedContent)
    {
        await response.ShouldReturn(expectedStatusCode);
        Assert.Equal(ShouldExtensions.defaultApplicationJson, response.Content.Headers.ContentType?.MediaType);
        var result = JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(result);
        Assert.Equal(JsonConvert.SerializeObject(expectedContent), JsonConvert.SerializeObject(result));
    }
}

