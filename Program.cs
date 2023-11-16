
using System.Reflection;
using Newtonsoft.Json;

public static class Program
{
    static TBAClient tbaClient = new TBAClient(Environment.GetEnvironmentVariable("tba_key"));
    public enum Unit
    {
        celsius,
        fahrenheit
    }

    public static string GetWeatherAt(double lon, double lat, string city)
    {
        var client = new OpenWeatherMapClient(Environment.GetEnvironmentVariable("OPENWEATHER_API_KEY"));

        var weather = client.GetWeatherByCoordinatesAsync(lat, lon).GetAwaiter().GetResult();

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



    public static void Main(String[] args)
    {
        var req = new OpenAI.Request();


        // req.functions.Add(OpenAI.Function.FromFunction(GetFilesOnDisk));
        // req.functions.Add(OpenAI.Function.FromFunction(GetWeatherAt));
        // req.functions.Add(OpenAI.Function.FromFunction(tbaClient.GetEvents, new("Gets a list of all events for a given year", new() {
        //     {"year", "the current year"},
        //     {"start_idx", "The start index into the list of all events for the last year"},
        //     {"event_return_count", "The length of the sublist that is returned in the response. Only use up to a maximum of 5."}
        // })));
        // req.functions.Add(OpenAI.Function.FromFunction(tbaClient.GetMatchInfo, new("Lists matches of an event", new() {
        //     {"eventKey", "is the event_code of a match returned from GetEvents"},
        //     {"start_idx", "The start index into the list of all events for the last year"},
        //     {"match_return_count", "The length of the sublist that is returned in the response. Only use up to a maximum of 5."}
        // })));
        // req.functions.Add(OpenAI.Function.FromFunction(tbaClient.GetDetailedMatchInfo, new("Lists matches of an event", new() {
        //     {"matchKey", "is the key value of a match returned from GetMatchInfo"}
        // })));
        


        req.messages.Add(new("system", "The year is 2023, events are FRC robotics events. You may have to split function calls into multiple, you can only handle 5 return values at once. You may only run functions provided. You can also run multiple in a row, even if they are the same function."));
        req.messages.Add(new("user", Console.ReadLine()));

        ResponseObject? response = OpenAI.Send(req).GetAwaiter().GetResult();

        // talk cycle
        while (true)
        {
            if (response is null)
            {
                return;
            }

            var choice = response.Choices[0];

            if (choice.FinishReason == "stop")
            {
                if (choice.Message is null)
                {
                    return;
                }
                if (choice.Message.Role is null)
                {
                    return;
                }
                if (choice.Message.Content is null)
                {
                    return;
                }
                req.messages.Add(new(choice.Message.Role, choice.Message.Content));
                Console.WriteLine("ChatGPT: " + choice.Message.Content);
                req.messages.Add(new("user", Console.ReadLine()));
            }
            else if (choice.FinishReason == "function_call")
            {
                if (choice.Message is null)
                {
                    return;
                }
                FunctionCall? call = choice.Message.FunctionCall;
                if (call is null)
                {
                    return;
                }
                Console.WriteLine($"Called {call.Name}({choice.Message.FunctionCall.Arguments}). Processing...");
                object? retval = req.CallDelegate(call);


                req.messages.Add(new("function", JsonConvert.SerializeObject(retval), call.Name));
            }

            response = OpenAI.Send(req, true).GetAwaiter().GetResult();
        }

    }
}