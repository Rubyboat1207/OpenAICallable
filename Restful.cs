using Newtonsoft.Json;

public abstract class Restful {
    protected string apiKey;
    protected string baseUrl;

    public Restful(string env, string baseUrl) {
        var key = Environment.GetEnvironmentVariable(env);

        if(key is null) {
            throw new Exception("Set your environment variables stewpid.");
        }

        this.baseUrl = baseUrl;
    }

    public abstract void AddAuthKey(HttpClient httpClient, string oldURL, out string url);

    public dynamic? APIGet(string url)
    {
        using (var httpClient = new HttpClient())
        {
            string finalUrl;
            AddAuthKey(httpClient, baseUrl + url, out finalUrl);

            var response = httpClient.GetAsync(baseUrl + url).GetAwaiter().GetResult();

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<dynamic?>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            }

            throw new HttpRequestException($"Data. Status code: {response.StatusCode}");
        }
    }
}