using System.Collections.Generic;
using System.Globalization;
using System.Text;

/// <summary>
/// Minimal JSON reader for tutorial_screens.json. JsonUtility cannot read it:
/// a board marker is a mixed tuple like [5, 0, "@", "r"].
/// Objects become Dictionary&lt;string, object&gt;, arrays List&lt;object&gt;,
/// numbers double, plus string / bool / null.
/// </summary>
public static class TutorialJson
{
    public static object Parse(string json)
    {
        int i = 0;
        return Value(json, ref i);
    }

    static void Ws(string s, ref int i)
    {
        while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
    }

    static object Value(string s, ref int i)
    {
        Ws(s, ref i);
        char c = s[i];
        if (c == '{') return Obj(s, ref i);
        if (c == '[') return Arr(s, ref i);
        if (c == '"') return Str(s, ref i);
        if (s.Substring(i).StartsWith("true")) { i += 4; return true; }
        if (s.Substring(i).StartsWith("false")) { i += 5; return false; }
        if (s.Substring(i).StartsWith("null")) { i += 4; return null; }
        int start = i;
        while (i < s.Length && "+-0123456789.eE".IndexOf(s[i]) >= 0) i++;
        return double.Parse(s.Substring(start, i - start), CultureInfo.InvariantCulture);
    }

    static Dictionary<string, object> Obj(string s, ref int i)
    {
        var d = new Dictionary<string, object>();
        i++; // {
        Ws(s, ref i);
        if (s[i] == '}') { i++; return d; }
        while (true)
        {
            Ws(s, ref i);
            string k = Str(s, ref i);
            Ws(s, ref i);
            i++; // :
            d[k] = Value(s, ref i);
            Ws(s, ref i);
            if (s[i++] == '}') return d; // else ','
        }
    }

    static List<object> Arr(string s, ref int i)
    {
        var l = new List<object>();
        i++; // [
        Ws(s, ref i);
        if (s[i] == ']') { i++; return l; }
        while (true)
        {
            l.Add(Value(s, ref i));
            Ws(s, ref i);
            if (s[i++] == ']') return l; // else ','
        }
    }

    static string Str(string s, ref int i)
    {
        var sb = new StringBuilder();
        i++; // opening quote
        while (s[i] != '"')
        {
            char c = s[i++];
            if (c != '\\') { sb.Append(c); continue; }
            char e = s[i++];
            switch (e)
            {
                case 'n': sb.Append('\n'); break;
                case 't': sb.Append('\t'); break;
                case 'r': sb.Append('\r'); break;
                case 'b': sb.Append('\b'); break;
                case 'f': sb.Append('\f'); break;
                case 'u':
                    sb.Append((char)int.Parse(s.Substring(i, 4), NumberStyles.HexNumber));
                    i += 4;
                    break;
                default: sb.Append(e); break; // \" \\ \/
            }
        }
        i++; // closing quote
        return sb.ToString();
    }

    // ---- typed accessors, so screen code reads like the JSON -------------

    public static object Get(this Dictionary<string, object> d, string key) =>
        d != null && d.TryGetValue(key, out var v) ? v : null;

    public static bool Has(this Dictionary<string, object> d, string key) =>
        d != null && d.ContainsKey(key) && d[key] != null;

    public static string Str(this Dictionary<string, object> d, string key) => d.Get(key) as string;

    public static int Int(this Dictionary<string, object> d, string key, int fallback = 0) =>
        d.Get(key) is double n ? (int)n : fallback;

    public static float Num(this Dictionary<string, object> d, string key, float fallback = 0) =>
        d.Get(key) is double n ? (float)n : fallback;

    public static List<object> List(this Dictionary<string, object> d, string key) => d.Get(key) as List<object>;

    public static Dictionary<string, object> Dict(this Dictionary<string, object> d, string key) =>
        d.Get(key) as Dictionary<string, object>;

    public static List<string> Strings(this Dictionary<string, object> d, string key)
    {
        var l = d.List(key);
        var r = new List<string>();
        if (l != null) foreach (var o in l) r.Add(o as string);
        return r;
    }
}
