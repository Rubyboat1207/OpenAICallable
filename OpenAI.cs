using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

public class OpenAI
{
    public class Request
    {
        public string model = "gpt-3.5-turbo-0613";
        public List<Message> messages;
        public List<Function> functions;

        public Request()
        {
            messages = new();
            functions = new();
        }

        public object? CallDelegate(FunctionCall call)
        {
            // Fetch the corresponding function based on the function name
            Function? functionToCall = functions.FirstOrDefault(f => f.name == call.Name);

            if (functionToCall is null || functionToCall._delegate is null)
            {
                throw new Exception($"No function registered with the name {call.Name}");
            }

            // Assuming that the arguments are serialized in a JSON string
            Dictionary<string, dynamic>? argsDict = JsonConvert.DeserializeObject<
                Dictionary<string, dynamic>
            >(call.Arguments);

            if (argsDict is null)
            {
                return functionToCall._delegate.DynamicInvoke();
            }

            // Prepare a list to hold the arguments
            var argsList = new List<object?>();
            var parameters = functionToCall._delegate.Method.GetParameters();

            foreach (var param in parameters)
            {
                if(param.Name is null) continue;
                if (argsDict.TryGetValue(param.Name, out var value))
                {
                    argsList.Add(value);
                }
                else if (param.HasDefaultValue) // Check if the parameter has a default value
                {
                    argsList.Add(param.DefaultValue);
                }
                else
                {
                    throw new Exception(
                        $"Value for parameter {param.Name} not provided and it doesn't have a default value."
                    );
                }
            }

            // Invoke the delegate
            return functionToCall._delegate.DynamicInvoke(argsList.ToArray());
        }
    }

    public class Message
    {
        public string role;
        public string content;
        public string? name;

        public Message(string role, string content, string? name=null)
        {
            this.role = role;
            this.content = content;
            this.name = name;
        }
    }

    public class Function
    {
        public string name { get; set; }
        public string? description { get; set; }
        public FunctionParameters parameters;

        [Newtonsoft.Json.JsonIgnore]
        public Delegate? _delegate;

        public Function(string name)
        {
            this.name = name;
            parameters = new FunctionParameters();
        }

        public static Function FromFunction(Delegate _delegate)
        {
            MethodInfo methodInfo = _delegate.Method;
            Function func = new(methodInfo.Name);
            func._delegate = _delegate;

            var paramList = methodInfo.GetParameters();

            foreach (var parameter in paramList)
            {
                if (parameter is null)
                {
                    continue;
                }
                if (parameter.Name is null)
                {
                    continue;
                }

                ParameterProperties props = new(FunctionParameters.GetType(parameter));

                func.parameters.properties.Add(parameter.Name, props);

                if (!parameter.IsOptional)
                {
                    func.parameters.required.Add(parameter.Name);
                }

                if (parameter.ParameterType.IsEnum)
                {
                    props._enum = new();
                    foreach (var value in Enum.GetValues(parameter.ParameterType))
                    {
                        if (value is null)
                            continue;

                        var str = value.ToString();

                        if (str is null)
                            continue;
                        props._enum.Add(str);
                    }
                }
            }

            return func;
        }
    }

    public class FunctionParameters
    {
        public string type;
        public Dictionary<string, ParameterProperties> properties;
        public List<string> required;

        public FunctionParameters()
        {
            type = "object";

            properties = new();
            required = new();
        }

        public static string GetType(ParameterInfo info)
        {
            var type = info.ParameterType;

            if (type == typeof(string))
            {
                return "string";
            }
            else if (type == typeof(int))
            {
                return "number";
            }
            else if (type == typeof(float))
            {
                return "number";
            }else if (type == typeof(double))
            {
                return "number";
            }
            else if (type.IsEnum)
            {
                return "string";
            }

            throw new Exception();
        }
    }

    public class ParameterProperties
    {
        public string type { get; set; }
        public string? description { get; set; }

        [JsonProperty("enum")]
        public List<string>? _enum;

        public ParameterProperties(string type)
        {
            this.type = type;
        }
    }

    public static async Task<ResponseObject?> Send(Request request, bool silent=true)
    {
        var Url = "https://api.openai.com/v1/chat/completions";
        var key = Environment.GetEnvironmentVariable("openai_key");

        using (var httpClient = new HttpClient())
        {
            // Set up headers
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {key}");
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            };
            // Construct JSON data
            var jsonData = JsonConvert.SerializeObject(request, settings);
            if(!silent) {
                Console.WriteLine(jsonData);
            }

            var response = await httpClient.PostAsync(
                Url,
                new StringContent(jsonData, Encoding.UTF8, "application/json")
            );

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                if(!silent) {
                    Console.WriteLine(responseContent);
                }
                return JsonConvert.DeserializeObject<ResponseObject>(responseContent);
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
                Console.WriteLine(await response.Content.ReadAsStringAsync());
                return null;
            }
        }
    }
}

public class ResponseObject
{
    [JsonProperty("id")]
    public string? Id { get; set; }

    [JsonProperty("object")]
    public string? Object { get; set; }

    [JsonProperty("created")]
    public long? Created { get; set; }

    [JsonProperty("model")]
    public string? Model { get; set; }

    [JsonProperty("choices")]
    public List<Choice>? Choices { get; set; }

    [JsonProperty("usage")]
    public Usage? Usage { get; set; }
}

public class Choice
{
    [JsonProperty("index")]
    public int? Index { get; set; }

    [JsonProperty("message")]
    public Message? Message { get; set; }

    [JsonProperty("finish_reason")]
    public string? FinishReason { get; set; }
}

public class Message
{
    [JsonProperty("role")]
    public string? Role { get; set; }

    [JsonProperty("content")]
    public string? Content { get; set; }

    [JsonProperty("function_call")]
    public FunctionCall? FunctionCall { get; set; }

    [JsonProperty("name")]
    public string? Name {get; set;}
}

public class FunctionCall
{
    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("arguments")]
    public string? Arguments { get; set; }
}

public class Usage
{
    [JsonProperty("prompt_tokens")]
    public int? PromptTokens { get; set; }

    [JsonProperty("completion_tokens")]
    public int? CompletionTokens { get; set; }

    [JsonProperty("total_tokens")]
    public int? TotalTokens { get; set; }
}
