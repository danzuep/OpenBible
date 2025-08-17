using System.Data;
using System.Text;
using Unihan.Services;

namespace Unihan.Models
{
    /// <inheritdoc cref="IUnihanReadings"/>
    public class UnihanLookup : Dictionary<int, UnihanFieldLookup>, IUnihanReadings
    {
        private string? _isoLanguage;
        public string? IsoLanguage
        {
            get => _isoLanguage;
            set
            {
                _isoLanguage = value;
                if (!string.IsNullOrEmpty(value) &&
                    ISO6393UnihanLookup.TryGetValue(value, out var unihanField))
                {
                    Field = unihanField;
                }
            }
        }

        public UnihanField? Field { get; set; }

        public void AddEntry(string codepoint, string field, string value)
        {
            var unicodeCodepoint = UnihanParserService.ConvertToCodepoint(codepoint);
            // Parse field string to UnihanField enum (Unknown by default)
            _ = Enum.TryParse<UnihanField>(field, out var unihanField);
            // Invoke action with these values
            AddEntry(unicodeCodepoint, unihanField, value);
        }

        public void AddEntry(int codepoint, UnihanField field, string value)
        {
            if (!TryGetValue(codepoint, out var record))
            {
                record = new UnihanFieldLookup();
                this[codepoint] = record;
            }
            record.Add(field, value);
        }

        public bool TryGetEntryText(int codepoint, IList<UnihanField>? fields, out string entryText)
        {
            if (!ContainsKey(codepoint))
            {
                entryText = string.Empty;
                return false;
            }

            var stringBuilder = new StringBuilder();

            var dictionary = this[codepoint];
            if (fields == null)
            {
                foreach (var kvp in dictionary)
                {
                    stringBuilder.Append(kvp.Key);
                    stringBuilder.Append(": { ");
                    stringBuilder.Append(string.Join("; ", kvp.Value));
                    stringBuilder.AppendLine(" }");
                }
            }
            else
            {
                foreach (var kvp in dictionary.Where(kvp => fields.Contains(kvp.Key)))
                {
                    //if (fields != null && !fields.Contains(kvp.Key)) continue;
                    if (kvp.Key == UnihanField.kDefinition)
                    {
                        stringBuilder.Append(": ");
                    }
                    stringBuilder.Append(string.Join("; ", kvp.Value));
                }
            }

            entryText = stringBuilder.ToString();
            return true;
        }

        public static readonly Dictionary<string, UnihanField> ISO6393UnihanLookup = new Dictionary<string, UnihanField>
        {
            { "cmn", UnihanField.kMandarin },
            { "zho", UnihanField.kMandarin },
            { "zho-Hans", UnihanField.kMandarin },
            { "zho-Hant", UnihanField.kCantonese },
            { "yue", UnihanField.kCantonese },
            { "nan", UnihanField.kBopomofo },
            { "jpn", UnihanField.kJapanese },
            { "kor", UnihanField.kHangul },
            { "ltc", UnihanField.kTang },
            { "vie", UnihanField.kVietnamese },
            { "zha", UnihanField.kZhuang },
        };

        public static readonly Dictionary<string, string> ISO_639_3_Unihan_Languages = new Dictionary<string, string>
        {
            { "cmn", "Mandarin Chinese" },
            { "yue", "Cantonese Chinese" },
            { "nan", "Taiwanese Hokkien" },
            { "zho", "Han language group" },
            { "zha", "Zhuang Chinese" },
            { "ltc", "Middle Chinese" },
            { "jpn", "Japanese" },
            { "kor", "Korean" },
            { "vie", "Vietnamese" },
        };

        public static readonly Dictionary<string, string> ISO_15924_Unihan_Scripts = new Dictionary<string, string>
        {
            { "Hans", "Simplified Chinese script" },    // Used primarily in Mainland China, Singapore, and Malaysia
            { "Hant", "Traditional Chinese script" },   // Used primarily in Taiwan, Hong Kong, and Macau
            { "Hang", "Hangul script" },                // Korean alphabet used in both South Korea and North Korea
            { "Hani", "Han characters" },               // Logographic characters used in Chinese (Hanzi), Japanese (Kanji), and Korean (Hanja)
            { "Kana", "Japanese Kana script" },         // Includes Hiragana and Katakana syllabaries used in Japanese writing
            { "Latn", "Latin alphabet" },               // Used for romanization of Chinese, Vietnamese, and other languages
        };

        public static Dictionary<string, List<UnihanField>> ScriptUnihanLookup = new Dictionary<string, List<UnihanField>>
        {
            { "Hans", new List<UnihanField> { UnihanField.kMandarin, UnihanField.kFanqie, UnihanField.kHanyuPinlu, UnihanField.kXHC1983, UnihanField.kZhuang } },
            { "Hant", new List<UnihanField> { UnihanField.kCantonese, UnihanField.kSMSZD2003Readings, UnihanField.kTang, UnihanField.kTGHZ2013 } },
            { "Hang", new List<UnihanField> { UnihanField.kHangul, UnihanField.kKorean } },
            { "Kana", new List<UnihanField> { UnihanField.kJapanese, UnihanField.kJapaneseKun, UnihanField.kJapaneseOn } },
            { "Latn", new List<UnihanField> { UnihanField.kVietnamese } },
        };

        /// <see href="https://www.unicode.org/reports/tr38/"/>
        public static Dictionary<UnihanField, string> UnihanNameLookup = new Dictionary<UnihanField, string>
        {
            { UnihanField.kCantonese, "yue-Latn" },         // Cantonese language, Latin script, Jyutping pronunciation(s)
            { UnihanField.kDefinition, "eng-Latn" },        // English language, Latin script, dictionary definition(s)
            { UnihanField.kFanqie, "cmn-Hans" },            // Mandarin language, Simplified Chinese script, Fanqie pronunciation(s)
            { UnihanField.kHangul, "kor-Hang" },            // Korean language, Hangul script, Hangul pronunciation(s) and source
            { UnihanField.kHanyuPinlu, "cmn-Latn" },        // Mandarin language, Latin script, Hanyu Pīnyīn pronunciation(s) and frequencies from 現代漢語頻率詞典
            { UnihanField.kHanyuPinyin, "cmn-Latn" },       // Mandarin language, Latin script, location and Hànyǔ Pīnyīn pronunciation(s) from 漢語大字典
            { UnihanField.kJapanese, "jpn-Kana" },          // Japanese language, Kana script, katakana Han Chinese 音読み and hiragana Japanese 訓読み
            { UnihanField.kJapaneseKun, "jpn-Kana" },       // Japanese language, Latin script, romanised kun'yomi Japanese reading(s)
            { UnihanField.kJapaneseOn, "jpn-Kana" },        // Japanese language, Latin script, romanised on'yomi Han Chinese reading(s)
            { UnihanField.kKorean, "kor-Latn" },            // Korean language, Latin script, romanised
            { UnihanField.kMandarin, "cmn-Latn" },          // Mandarin language, Latin script, Pīnyīn pronunciation(s)
            { UnihanField.kSMSZD2003Readings, "zho-Latn" }, // Chinese language, Latin script, Pīnyīn and Jyutping from the 2003 商務新字典
            { UnihanField.kTang, "ltc-Latn" },              // Middle Chinese language, Latin script, Tang pronunciation(s)
            { UnihanField.kTGHZ2013, "cmn-Latn" },          // Mandarin language, Latin script, location and Pīnyīn pronunciation(s) from the 2013 通用规范汉字字典
            { UnihanField.kVietnamese, "vie-Latn" },        // Vietnamese language, Latin script, Vietnamese pronunciation(s)
            { UnihanField.kXHC1983, "cmn-Latn" },           // Mandarin language, Latin script, location and Hànyǔ Pīnyīn pronunciation(s) from the 1983 现代汉语词典
            { UnihanField.kZhuang, "zha-Latn" },            // Zhuang language, Latin script, Zhuang pronunciation(s) from 古壮字字典
        };
    }
}