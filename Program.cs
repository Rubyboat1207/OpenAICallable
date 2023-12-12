
using System.Reflection;
using Newtonsoft.Json;

public static class Program
{

    public static string CreateCallback(string identifier, string luaCodeString) {
        File.WriteAllText("./addon/callbacks/"+identifier, luaCodeString);

        return "callback created successfully. Do not call again, you may be a risk of overriding the file.";
    }

    public static string CreateInit(string luaCodeString) {
        File.WriteAllText("./addon/init.lua", luaCodeString);

        return "init.lua created successfully. Do not call again, you may be a risk of overriding the file.";
    }

    public static string CreateRender(string luaCodeString) {
        File.WriteAllText("./addon/render.lua", luaCodeString);

        return "render.lua created successfully. Do not call again, you may be a risk of overriding the file.";
    }

    public static string CreateManifestTextFile(string jsonString) {
        File.WriteAllText("./addon/manifest.json", jsonString);

        return "manifest created successfully. Do not call again, you may be a risk of overriding the file.";
    }

    public static void Main(String[] args)
    {
        var ai = new OpenAI();
        var req = new OpenAI.Request();

        req.model = "rubyboat-gpt4-regular";
        ai.Url = "https://jeremyai.openai.azure.com";


        req.functions.Add(OpenAI.Function.FromFunction(CreateCallback, new OpenAI.ChatGPTFunctionAttributes(
            "creates a callback file. Call once to create the file. Calling a second time will completly override the contents.", _params: new Dictionary<string, string>() {
                {"identifier", "The identifier of the callback."},
                {"luaCodeString", "The content of the file"},
            }
        )));
        req.functions.Add(OpenAI.Function.FromFunction(CreateInit, new OpenAI.ChatGPTFunctionAttributes(
            "Call once to create the file. Calling a second time will completly override the contents.", _params: new Dictionary<string, string>() {
                {"luaCodeString", "The content of the file"},
            }
        )));
        req.functions.Add(OpenAI.Function.FromFunction(CreateRender, new OpenAI.ChatGPTFunctionAttributes(
            "Call once to create the file. Calling a second time will completly override the contents.", _params: new Dictionary<string, string>() {
                {"luaCodeString", "The content of the file"},
            }
        )));
        req.functions.Add(OpenAI.Function.FromFunction(CreateManifestTextFile, new OpenAI.ChatGPTFunctionAttributes(
            "Call once to create the file. Calling a second time will completly override the contents.", _params: new Dictionary<string, string>() {
                {"jsonString", "The JSON string to be written to disk. Do not pass the raw json into this function. pass a json string into this function."},
            }
        )));
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



        req.messages.Add(new("system", @"Your goal is to create an addon. Addons are a series of files, starting with the manifest.json which there is only one per project which contains several properties.
        version: string, identifier: string, name: string, categories: string[], callbacks: : string[]. callbacks must be registered in the callbacks list in manifest.json, otherwise they do not work.
        other than the manifest.json, everything else is written in lua, in the render.lua and init.lua files. The render.lua function runs every render. init.lua only runs once.
        The Lua documentation for all functions are as follows: ```" + File.ReadAllText("./commonlib.lua") + @"``` Persistent variables persist through reloads. An addon must always run the setHudWindowName function in init.
        to get the handle required for almost all functions, you will run the getCurrentHandle() function, which you can save to a local variable as it will not change throught the duration of the file.
        A widget is required to have a position, optionally a scale. A button or checkbox requires a label. A button requires an onclick. If you are going to do seperate screens, place the screen rendering code into render, not init and use if statements to split screens and
        to disable the rendering of widgets on screen swap. All widgets must be created in init, and never in render. you can reference them using their identifiers. functions do not work outside the current file. functions do not work outside the current file.
        functions do not work outside the current file. functions do not work outside the current file. functions do not work outside the current file. never create your own functions ever. never create your own functions ever.
        you must create a widget before you can modify its properties. defer most logic to render. init is only for window and widget setup. An addon must always run the setHudWindowName function otherwise it will fail.
        Global variables do not persist beyond files, so use persistent data instead. Any updating data that needs to transfer between a callback, init, or render must be in persistent data."));
        req.messages.Add(new("user", Console.ReadLine()));

        ResponseObject? response = ai.Send(req).GetAwaiter().GetResult();

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

            response = ai.Send(req, true).GetAwaiter().GetResult();
        }

    }
}