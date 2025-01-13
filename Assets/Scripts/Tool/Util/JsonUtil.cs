using Newtonsoft.Json;
using Newtonsoft.Json.UnityConverters.Math;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class JsonUtil
{
    private static JsonSerializerSettings _jsonSerializerSettings;

    public static void SerializeToFile(object obj, string path)
    {
        //这一串都是为了格式化输出json
        StringWriter textWriter = new StringWriter();
        JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
        {
            Formatting = Formatting.Indented,
            Indentation = 4,
            IndentChar = ' ',
        };

        JsonSerializer serializer = JsonSerializer.Create(GetJsonSerializerSettings());

        //排序

        serializer.Serialize(jsonWriter, obj);
        var file = new FileInfo(path);

        System.IO.File.WriteAllText(file.FullName, textWriter.ToString());
        jsonWriter.Close();
        textWriter.Close();

        return;
    }

    public static string Serialize(object obj)
    {
        //这一串都是为了格式化输出json
        StringWriter textWriter = new StringWriter();

        JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
        {
            Formatting = Formatting.Indented,
            Indentation = 4,
            IndentChar = ' ',
        };

        JsonSerializer serializer = JsonSerializer.Create(GetJsonSerializerSettings());

        //排序

        serializer.Serialize(jsonWriter, obj);

        return textWriter.ToString();
    }

    public static T Deserialize<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json, GetJsonSerializerSettings());
    }

    public static JsonSerializerSettings GetJsonSerializerSettings()
    {
        if (_jsonSerializerSettings != null)
        {
            return _jsonSerializerSettings;
        }

        //自动带入类型
        var setting = new JsonSerializerSettings()
        {
            Converters = new List<JsonConverter> {
                new Vector2Converter(),
                new Vector3Converter(),
            },
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
        };

        _jsonSerializerSettings = setting;
        return setting;
    }
}