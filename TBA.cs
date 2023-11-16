using System.Reflection;
using System.Text.Json;
using Newtonsoft.Json;
using QuickType;

public class TBAClient
{
    string apiKey;

    public TBAClient(string apiKey)
    {
        this.apiKey = apiKey;
    }

    public string TBAGet(string url)
    {
        using (var httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Add("X-TBA-Auth-Key", apiKey);

            var response = httpClient.GetAsync("https://www.thebluealliance.com/api/v3/" + url).GetAwaiter().GetResult();

            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }

            throw new HttpRequestException($"Data. Status code: {response.StatusCode}");
        }
    }

    public TBAResponse<FirstEvent>? GetEvents(string year, long start_idx, long event_return_count)
    {
        var settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        var convert = JsonConvert.DeserializeObject<FirstEvent[]>(TBAGet("events/" + year), settings);

        if (convert is null)
        {
            return new();
        }

        var orderedEvents = convert.OrderBy(e => e.StartDate).ToArray();

        var count = orderedEvents.Length - (start_idx + event_return_count); // Adjusted the formula to make it correct

        return new TBAResponse<FirstEvent>()
        {
            value = orderedEvents.Skip((int)start_idx).Take((int)event_return_count).ToArray(),
            remaining = (int)count
        };
    }

    public TBAResponse<MatchInfo> GetMatchInfo(string eventKey, long start_idx, long match_return_count)
    {
        var settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        var convert = JsonConvert.DeserializeObject<MatchInfo[]>(TBAGet($"event/{eventKey}/matches/simple"), settings);

        if (convert is null)
        {
            return new();
        }

        var orderedEvents = convert.OrderBy(e => e.PredictedTime).ToArray();

        var count = orderedEvents.Length - (start_idx + match_return_count); // Adjusted the formula to make it correct

        return new TBAResponse<MatchInfo>()
        {
            value = orderedEvents.Skip((int)start_idx).Take((int)match_return_count).ToArray(),
            remaining = (int)count
        };
    }

    public MatchData GetDetailedMatchInfo(string matchKey)
    {
        var settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        var convert = JsonConvert.DeserializeObject<MatchData>(TBAGet($"match/{matchKey}"), settings);

        if (convert is null)
        {
            return new();
        }

        return convert;
    }

    public class TBAResponse<T>
    {
        public T[] value;
        public int remaining;
    }
}

#pragma warning disable CS8618
namespace QuickType
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class MatchData
    {
        public long actual_time { get; set; }
        public Alliances alliances { get; set; }
        public string comp_level { get; set; }
        public string event_key { get; set; }
        public string key { get; set; }
        public int match_number { get; set; }
        public long post_result_time { get; set; }
        public long predicted_time { get; set; }
        public ScoreBreakdown score_breakdown { get; set; }
        public int set_number { get; set; }
        public long time { get; set; }
        public List<Video> videos { get; set; }
        public string winning_alliance { get; set; }

        public class Alliances
        {
            public Alliance blue { get; set; }
            public Alliance red { get; set; }
        }

        public class Alliance
        {
            public List<string> dq_team_keys { get; set; }
            public int score { get; set; }
            public List<string> surrogate_team_keys { get; set; }
            public List<string> team_keys { get; set; }
        }

        public class ScoreBreakdown
        {
            public TeamScore blue { get; set; }
            public TeamScore red { get; set; }
        }

        public class TeamScore
        {
            public bool activationBonusAchieved { get; set; }
            public int adjustPoints { get; set; }
            public string autoBridgeState { get; set; }
            public int autoChargeStationPoints { get; set; }
            public string autoChargeStationRobot1 { get; set; }
            public string autoChargeStationRobot2 { get; set; }
            public string autoChargeStationRobot3 { get; set; }
            public Community autoCommunity { get; set; }
            public bool autoDocked { get; set; }
            public int autoGamePieceCount { get; set; }
            public int autoGamePiecePoints { get; set; }
            public int autoMobilityPoints { get; set; }
            public int autoPoints { get; set; }
            public int coopGamePieceCount { get; set; }
            public bool coopertitionCriteriaMet { get; set; }
            public string endGameBridgeState { get; set; }
            public int endGameChargeStationPoints { get; set; }
            public string endGameChargeStationRobot1 { get; set; }
            public string endGameChargeStationRobot2 { get; set; }
            public string endGameChargeStationRobot3 { get; set; }
            public int endGameParkPoints { get; set; }
            public int foulCount { get; set; }
            public int foulPoints { get; set; }
            public int linkPoints { get; set; }
            public List<Link> links { get; set; }
            public string mobilityRobot1 { get; set; }
            public string mobilityRobot2 { get; set; }
            public string mobilityRobot3 { get; set; }
            public int rp { get; set; }
            public bool sustainabilityBonusAchieved { get; set; }
            public int techFoulCount { get; set; }
            public Community teleopCommunity { get; set; }
            public int teleopGamePieceCount { get; set; }
            public int teleopGamePiecePoints { get; set; }
            public int teleopPoints { get; set; }
            public int totalChargeStationPoints { get; set; }
            public int totalPoints { get; set; }
        }

        public class Community
        {
            public List<string> B { get; set; }
            public List<string> M { get; set; }
            public List<string> T { get; set; }
        }

        public class Link
        {
            public List<int> nodes { get; set; }
            public string row { get; set; }
        }

        public class Video
        {
            public string key { get; set; }
            public string type { get; set; }
        }
    }


    public class MatchInfo
    {
        public string Key { get; set; }
        public string CompLevel { get; set; }
        public int SetNumber { get; set; }
        public int MatchNumber { get; set; }
        public Alliances Alliances { get; set; }
        public string WinningAlliance { get; set; }
        public string EventKey { get; set; }
        public long Time { get; set; }
        public long PredictedTime { get; set; }
        public long ActualTime { get; set; }
    }

    public class Alliances
    {
        public Alliance Red { get; set; }
        public Alliance Blue { get; set; }
    }

    public class Alliance
    {
        public int Score { get; set; }
        public List<string> TeamKeys { get; set; } = new List<string>();
        public List<string> SurrogateTeamKeys { get; set; } = new List<string>();
        public List<string> DqTeamKeys { get; set; } = new List<string>();
    }


    public partial class FirstEvent
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("district")]
        public object District { get; set; }

        [JsonProperty("division_keys")]
        public object[] DivisionKeys { get; set; }

        [JsonProperty("end_date")]
        public DateTimeOffset EndDate { get; set; }

        [JsonProperty("event_code")]
        public string EventCode { get; set; }

        [JsonProperty("event_type")]
        public long EventType { get; set; }

        [JsonProperty("event_type_string")]
        public string EventTypeString { get; set; }

        [JsonProperty("first_event_code")]
        public string FirstEventCode { get; set; }

        [JsonProperty("first_event_id")]
        public object FirstEventId { get; set; }

        [JsonProperty("gmaps_place_id")]
        public string GmapsPlaceId { get; set; }

        [JsonProperty("gmaps_url")]
        public Uri GmapsUrl { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("lat")]
        public double Lat { get; set; }

        [JsonProperty("lng")]
        public double Lng { get; set; }

        [JsonProperty("location_name")]
        public string LocationName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("parent_event_key")]
        public object ParentEventKey { get; set; }

        [JsonProperty("playoff_type")]
        public long PlayoffType { get; set; }

        [JsonProperty("playoff_type_string")]
        public string PlayoffTypeString { get; set; }

        [JsonProperty("postal_code")]
        public string PostalCode { get; set; }

        [JsonProperty("short_name")]
        public string ShortName { get; set; }

        [JsonProperty("start_date")]
        public DateTimeOffset StartDate { get; set; }

        [JsonProperty("state_prov")]
        public string StateProv { get; set; }

        [JsonProperty("timezone")]
        public string Timezone { get; set; }

        [JsonProperty("webcasts")]
        public Webcast[] Webcasts { get; set; }

        [JsonProperty("website")]
        public Uri Website { get; set; }

        [JsonProperty("week")]
        public long Week { get; set; }

        [JsonProperty("year")]
        public long Year { get; set; }
    }

    public partial class Webcast
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}