using System.Linq;
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

        //[JsonPropertyName("metadata")]
        //public IList<KeyValuePair<string, string>> Metadata { get; set; }

        //[JsonPropertyName("contents")]
        //public IList<KeyValuePair<string, string>> Contents { get; set; }

        //[JsonPropertyName("metadata")]
        //public ILookup<string, string> Meta { get; set; }

        //[JsonPropertyName("contents")]
        //public ILookup<string, string> Cont { get; set; }

        public Usj()
        {
            Metadata = new List<KVP>();
            Contents = new List<KVP>();
            //Metadata = new List<KeyValuePair<string, string>>();
            //Contents = new List<KeyValuePair<string, string>>();
        }
    }

    /* 
    Pseudocode / Plan:
    1. Replace the existing KVP struct serialization (which currently emits {"key": "...", "value": "..."})
       with a converter that serializes as a single-property object where the property name is the Key
       and the property value is the Value (dictionary-style): { "<key>": "<value>" }. 

    2. Annotate the readonly struct KVP with JsonConverterAttribute referencing a custom converter.

    3. Implement a System.Text.Json.JsonConverter<KVP>:
       - Write:
         a. Start an object.
         b. Write a single property using the KVP.Key as the property name and KVP.Value as a JSON string.
         c. End the object.
       - Read:
         a. Ensure the reader is at StartObject.
         b. Read the next token. If EndObject, return an empty KVP or throw (choose consistent behavior).
         c. Expect a PropertyName token; read the property name into 'key'.
         d. Read the property value:
            - If it's a string, get the string.
            - Otherwise parse the value as JSON and take its raw text (so non-string values are preserved as text).
         e. Advance to EndObject and return new KVP(key, value).
       - Use fully qualified System.Text.Json types to avoid adding using directives (so this snippet can be inserted into the existing file safely).

    4. Keep the struct immutable with the same constructor and properties.

    End of plan.
    */

    [JsonConverter(typeof(KVPJsonConverter))]
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

    internal sealed class KVPJsonConverter : JsonConverter<KVP>
    {
        public override KVP Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
        {
            if (reader.TokenType != System.Text.Json.JsonTokenType.StartObject)
                throw new System.Text.Json.JsonException("Expected start of object for KVP.");

            if (!reader.Read())
                throw new System.Text.Json.JsonException("Unexpected end when reading KVP.");

            if (reader.TokenType == System.Text.Json.JsonTokenType.EndObject)
                return new KVP(string.Empty, string.Empty);

            if (reader.TokenType != System.Text.Json.JsonTokenType.PropertyName)
                throw new System.Text.Json.JsonException("Expected property name for KVP.");

            string key = reader.GetString() ?? string.Empty;

            if (!reader.Read())
                throw new System.Text.Json.JsonException("Unexpected end when reading KVP value.");

            string value;
            if (reader.TokenType == System.Text.Json.JsonTokenType.String)
            {
                value = reader.GetString() ?? string.Empty;
            }
            else
            {
                using var doc = System.Text.Json.JsonDocument.ParseValue(ref reader);
                value = doc.RootElement.GetRawText();
            }

            if (!reader.Read())
                throw new System.Text.Json.JsonException("Unexpected end after KVP value.");

            if (reader.TokenType != System.Text.Json.JsonTokenType.EndObject)
                throw new System.Text.Json.JsonException("Expected end of object after KVP.");

            return new KVP(key, value);
        }

        public override void Write(System.Text.Json.Utf8JsonWriter writer, KVP value, System.Text.Json.JsonSerializerOptions options)
        {
            //writer.WriteStartObject();
            // Write the property name as the key and the property value as a JSON string.
            writer.WriteString(value.Key ?? string.Empty, value.Value);
            //writer.WriteEndObject();
        }
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
