

public class CSFloat : Restful
{
    public CSFloat(string env, string baseUrl) : base(env, baseUrl)
    {
    }

    public override void AddAuthKey(HttpClient httpClient, string oldURL, out string url)
    {
        url = oldURL + "&token=" + apiKey;
    }

    // public void GetSkin(string name, ) {
    //     APIGet()
    // }
}