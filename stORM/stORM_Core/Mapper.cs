using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace stORM.stORM_Core;

public static class Mapper
{
    public static List<U> Map<U>(string result)
    {
        if (result == null || result == string.Empty) return null;

        try
        {
            return JArray
                 .Parse(result)
                 .Select(jsonMap => JsonConvert.DeserializeObject<U>(jsonMap.ToString())).ToList();
        }
        catch (Exception ex)
        {
            throw new Exception("Unable to map the entities!");
        }
    }

    public static int Count(string result)
    {
        if (result == null) return 0;

        try
        {
            return JsonSerializer.Deserialize<List<JsonElement>>(result)[0].GetProperty("COUNT").GetInt32();
        }
        catch (Exception ex) 
        {
            throw new Exception("Unable to map the count of results!");
        }
    }
}
