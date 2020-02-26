using System.Text.Json;

namespace IEvangelist.Retweet.Extensions
{
    static  class StringExtensions
    {
        internal static string ToJson<T>(this T obj) => JsonSerializer.Serialize<T>(obj);

        internal static T FromJson<T>(this string json) => JsonSerializer.Deserialize<T>(json);
    }
}
