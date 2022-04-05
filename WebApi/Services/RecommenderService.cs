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

    public async Task<Object> PostRecommendation(IEnumerable<Poi> list)
    {
        var json = JsonConvert.SerializeObject(list);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        var result = await _httpClient.PostAsync("recommend", data);
        return result.Content;
    }  
}
