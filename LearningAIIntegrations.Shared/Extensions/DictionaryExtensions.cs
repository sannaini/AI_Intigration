using System.Globalization;

namespace LearningAIIntegrations.Shared.Extensions;

public static class DictionaryExtensions
{
    public static string GetString(this IDictionary<string, object> dict, string key)
    {
        var value = GetRequired(dict, key);

        return value switch
        {
            string s => s,
            _ => Convert.ToString(value, CultureInfo.InvariantCulture)
                 ?? throw new InvalidOperationException($"Cannot convert '{key}' to string.")
        };
    }

    public static int GetInt(this IDictionary<string, object> dict, string key)
    {
        var value = GetRequired(dict, key);

        return value switch
        {
            int i => i,
            long l => checked((int)l),
            double d => Convert.ToInt32(d),
            decimal m => Convert.ToInt32(m),
            float f => Convert.ToInt32(f),
            string s => int.Parse(s, CultureInfo.InvariantCulture),
            _ => Convert.ToInt32(value, CultureInfo.InvariantCulture)
        };
    }

    public static long GetLong(this IDictionary<string, object> dict, string key)
    {
        var value = GetRequired(dict, key);

        return value switch
        {
            long l => l,
            int i => i,
            double d => Convert.ToInt64(d),
            decimal m => Convert.ToInt64(m),
            string s => long.Parse(s, CultureInfo.InvariantCulture),
            _ => Convert.ToInt64(value, CultureInfo.InvariantCulture)
        };
    }

    public static decimal GetDecimal(this IDictionary<string, object> dict, string key)
    {
        var value = GetRequired(dict, key);

        return value switch
        {
            decimal m => m,
            double d => Convert.ToDecimal(d),
            float f => Convert.ToDecimal(f),
            int i => i,
            long l => l,
            string s => decimal.Parse(s, CultureInfo.InvariantCulture),
            _ => Convert.ToDecimal(value, CultureInfo.InvariantCulture)
        };
    }

    public static double GetDouble(this IDictionary<string, object> dict, string key)
    {
        var value = GetRequired(dict, key);

        return value switch
        {
            double d => d,
            decimal m => (double)m,
            float f => f,
            int i => i,
            long l => l,
            string s => double.Parse(s, CultureInfo.InvariantCulture),
            _ => Convert.ToDouble(value, CultureInfo.InvariantCulture)
        };
    }

    public static bool GetBool(this IDictionary<string, object> dict, string key)
    {
        var value = GetRequired(dict, key);

        return value switch
        {
            bool b => b,
            string s => bool.Parse(s),
            _ => Convert.ToBoolean(value, CultureInfo.InvariantCulture)
        };
    }

    private static object GetRequired(IDictionary<string, object> dict, string key)
    {
        if (!dict.TryGetValue(key, out var value))
            throw new KeyNotFoundException($"Missing required key '{key}'.");

        return value ?? throw new InvalidOperationException($"Key '{key}' is null.");
    }
}