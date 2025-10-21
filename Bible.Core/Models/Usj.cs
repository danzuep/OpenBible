using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Bible.Core.Models
{
    public class Usj
    {
        [JsonPropertyName("usx")]
        public string UsxVersion { get; set; }

        [JsonPropertyName("code")]
        public string BookCode { get; set; }

        [JsonPropertyName("book")]
        public string BookVersion { get; set; }

        [JsonPropertyName("metadata")]
        public IList<KVP> Metadata { get; set; }

        [JsonPropertyName("contents")]
        public IList<KVP> Contents { get; set; }
    }

    /// <inheritdoc cref="KeyValuePair{TKey, TValue}"/>
    public readonly struct KVP
    {
        public KVP(string key, string value)
        {
            Key = key;
            Value = value;
        }

        [JsonPropertyName("key")]
        public string Key { get; }

        [JsonPropertyName("value")]
        public string Value { get; }
    }

    //public class Book
    //{
    //    public string usx { get; set; }
    //    public string code { get; set; }
    //    public string book { get; set; }
    //    public string h { get; set; }
    //    public string toc1 { get; set; }
    //    public string toc2 { get; set; }
    //    public string toc3 { get; set; }
    //    public string mt1 { get; set; }
    //}

    //public class Body
    //{
    //    public string chapter { get; set; }
    //    public string section { get; set; }
    //    public string style { get; set; }
    //    public string verse { get; set; }
    //    public string text { get; set; }
    //}
}
