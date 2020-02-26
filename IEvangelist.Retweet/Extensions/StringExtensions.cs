using System.Text.Json;

namespace IEvangelist.Retweet.Extensions
{
    static  class StringExtensions
    {
        static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            IgnoreNullValues = true,
            WriteIndented = true,
        };

        internal static string ToJson<T>(this T obj) => JsonSerializer.Serialize<T>(obj, Options);

        internal static T FromJson<T>(this string json) => JsonSerializer.Deserialize<T>(json);
    }
}
