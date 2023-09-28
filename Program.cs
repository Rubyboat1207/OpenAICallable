
using System.Reflection;
using Newtonsoft.Json;

public static class Program {

    public enum Unit {
        celsius,
        fahrenheit
    }

    public static string GetWeatherAt(double lon, double lat) {
        var client = new OpenWeatherMapClient(Environment.GetEnvironmentVariable("OPENWEATHER_API_KEY"));

        var weather = client.GetWeatherByCoordinatesAsync(lon, lat).GetAwaiter().GetResult();

        Console.WriteLine(weather);

        return weather;
    }

    public static string[] GetFilesOnDisk(string path)
    {
        try
        {
            // Get all directories and files at the specified path
            string[] directories = Directory.GetDirectories(path)
                                            .Select(d => d + Path.DirectorySeparatorChar)
                                            .ToArray();
            string[] files = Directory.GetFiles(path);

            // Combine the two arrays
            string[] result = new string[directories.Length + files.Length];
            directories.CopyTo(result, 0);
            files.CopyTo(result, directories.Length);

            return result;
        }
        catch (Exception ex)
        {
            // Handle exceptions (like path not found, access denied, etc.)
            Console.WriteLine($"Error: {ex.Message}");
            return new string[0];
        }
    }



    public static void Main(String[] args) {
        var req = new OpenAI.Request();


        req.functions.Add(OpenAI.Function.FromFunction(GetFilesOnDisk));
        req.functions.Add(OpenAI.Function.FromFunction(GetWeatherAt));

                
        req.messages.Add(new("user", Console.ReadLine()));

        ResponseObject? response = OpenAI.Send(req).GetAwaiter().GetResult();

        // talk cycle
        while(true) {
            if(response is null) {
                return;
            }
            
            var choice = response.Choices[0];

            if(choice.FinishReason == "stop") {
                if(choice.Message is null) {
                    return;
                }
                if(choice.Message.Role is null) {
                    return;
                }
                if(choice.Message.Content is null) {
                    return;
                }
                req.messages.Add(new (choice.Message.Role, choice.Message.Content));
                Console.WriteLine("ChatGPT: " + choice.Message.Content);
                req.messages.Add(new("user", Console.ReadLine()));
            }else if(choice.FinishReason == "function_call") {
                if(choice.Message is null) {
                    return;
                }
                FunctionCall? call = choice.Message.FunctionCall;
                if(call is null) {
                    return;
                }
                object? retval = req.CallDelegate(call);

                Console.WriteLine($"Called {call.Name}. Processing...");

                req.messages.Add(new("function", JsonConvert.SerializeObject(retval), call.Name));
            }

            response = OpenAI.Send(req, true).GetAwaiter().GetResult();
        }
        
    }
}