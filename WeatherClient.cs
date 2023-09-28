using System;
using System.Net.Http;
using System.Threading.Tasks;

public class OpenWeatherMapClient
{
    private const string BaseUrl = "https://api.openweathermap.org/data/2.5/weather";
    private readonly string _apiKey;

    public OpenWeatherMapClient(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ArgumentException("API key cannot be null or empty.", nameof(apiKey));
        }

        _apiKey = apiKey;
    }

    public async Task<string> GetWeatherByCoordinatesAsync(double lat, double lon)
    {
        using (var client = new HttpClient())
        {
            var url = $"{BaseUrl}?lat={lat}&lon={lon}&appid={_apiKey}";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            throw new HttpRequestException($"Failed to fetch weather data. Status code: {response.StatusCode}");
        }
    }
}
