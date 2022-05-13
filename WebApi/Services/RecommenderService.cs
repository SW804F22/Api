using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using WebApi.Models;
namespace WebApi.Services;
public class RecommenderService
{
    private readonly HttpClient _httpClient;
    private readonly string address = "http://localhost/";
    public RecommenderService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(address);
    }
    public virtual async Task<IEnumerable<Poi>> PostRecommendation(string user, IEnumerable<Poi> list)
    {
        var json = JsonConvert.SerializeObject(list);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"recommend/{user}", data);
        response = response.EnsureSuccessStatusCode();
        // TODO: Might need refactoring depending on the result from the api
        var result = await response.Content.ReadFromJsonAsync<IEnumerable<Poi>>();
        Debug.Assert(result != null, nameof(result) + " != null");
        return result;
    }
}
