using System.Data;
using System.Globalization;
using System.Text;
using Bible.Backend.Services;

namespace Bible.Backend.Models
{
    public interface IUnihanReadings
    {
        void AddEntry(string codepoint, string field, string value);
    }

    // Outer key: Unicode codepoint string (e.g., "U+3400")
    // Inner dictionary key: field name (e.g., "kCantonese")
    // Inner dictionary value: field value string
    public class UnihanReadings : Dictionary<string, Dictionary<string, IList<string>>>, IUnihanReadings
    {
        public void AddEntry(string codepoint, string field, string value)
        {
            if (!this.ContainsKey(codepoint))
            {
                this[codepoint] = new Dictionary<string, IList<string>>();
            }

            if (!this[codepoint].ContainsKey(field))
            {
                this[codepoint][field] = new List<string> { value };
            }
            else
            {
                this[codepoint][field].Add(value);
            }
        }
    }

    public class UnihanRelay : IUnihanReadings
    {
        public Action<int, UnihanField, string>? Action { get; set; }

        public void AddEntry(string codepoint, string field, string value)
        {
            //var unicodeCodepoint = ConvertToUnicode(codepoint);
            var unicodeCodepoint = ConvertToCodepoint(codepoint);
            // Parse field string to UnihanField enum (Unknown by default)
            _ = Enum.TryParse<UnihanField>(field, out var unihanField);
            // Invoke optional action with these values
            Action?.Invoke(unicodeCodepoint, unihanField, value);
        }

        /// <summary>
        /// Extract codepoint from key string hex code like "U+XXXX" or "U+XXXXX"
        /// <seealso cref="System.Text.Rune"/>
        /// </summary>
        internal static int ConvertToCodepoint(string codepointStr)
        {
            if (!codepointStr.AsSpan(0, 2).Equals("U+", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Invalid format", nameof(codepointStr));

            if (!int.TryParse(codepointStr.AsSpan(2), NumberStyles.HexNumber, null, out int codepoint))
                throw new FormatException("Invalid hex number.");

            if (codepoint < 0 || codepoint > 0x10FFFF)
                throw new ArgumentOutOfRangeException(nameof(codepoint), "Invalid Unicode codepoint.");

            return codepoint;
        }

        internal static string ConvertToUnicode(string codepointStr)
        {
            var codepoint = ConvertToCodepoint(codepointStr);
            var unicode = char.ConvertFromUtf32(codepoint);
            return unicode;
        }
    }

    public class UnihanSerializer : IUnihanReadings, IDisposable
    {
        private JsonBufferWriter<UnihanJsonEntry>? _jsonBufferWriter;
        private readonly UnihanRelay _unihanRelay;

        public UnihanSerializer()
        {
            _unihanRelay = new UnihanRelay { Action = AddEntry };
        }

        /// <inheritdoc cref="UnihanParserService.ParseAsync{T}(StreamReader)"/>
        public async Task ParseAsync(string inputPath, string outputPath)
        {
            OutputPath = outputPath;
            await UnihanParserService.ParseAsync(inputPath, _unihanRelay);
            _jsonBufferWriter?.Dispose();
            _jsonBufferWriter = null;
        }

        public string? OutputPath { get; set; }

        public void AddEntry(string codepoint, string field, string value)
        {
            _unihanRelay.AddEntry(codepoint, field, value);
        }

        // Adds entry to an ansynchronous write buffer
        public void AddEntry(int codepoint, UnihanField field, string value)
        {
            if (_jsonBufferWriter == null)
            {
                var options = new JsonBufferWriterOptions { OutputPath = OutputPath };
                _jsonBufferWriter = new JsonBufferWriter<UnihanJsonEntry>(options);
            }
            var jsonEntry = new UnihanJsonEntry(codepoint, field, value);
            _jsonBufferWriter.AddEntry(jsonEntry);
        }

        public void Dispose()
        {
            _jsonBufferWriter?.Dispose();
        }

        private class UnihanJsonEntry
        {
            public UnihanJsonEntry(int codepoint, UnihanField field, string value)
            {
                Key = codepoint;
                Field = field;
                Value = value;
            }

            public int Key { get; }
            public UnihanField Field { get; }
            public string Value { get; }
        }
    }

    public class UnihanLookup : Dictionary<int, Dictionary<UnihanField, IList<string>>>, IUnihanReadings
    {
        private readonly UnihanRelay _unihanRelay;

        public UnihanLookup()
        {
            _unihanRelay = new UnihanRelay { Action = AddEntry };
        }

        public UnihanField? Field { get; set; }

        public void AddEntry(string codepoint, string field, string value)
        {
            _unihanRelay.AddEntry(codepoint, field, value);
        }

        public void AddEntry(int codepoint, UnihanField field, string value)
        {
            if (!this.ContainsKey(codepoint))
            {
                this[codepoint] = new Dictionary<UnihanField, IList<string>>();
            }

            if (!this[codepoint].ContainsKey(field))
            {
                this[codepoint][field] = new List<string> { value };
            }
            else
            {
                this[codepoint][field].Add(value);
            }
        }

        public bool TryGetEntryText(int codepoint, IList<UnihanField>? fields, out string entryText)
        {
            if (!this.ContainsKey(codepoint))
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

        public static readonly Dictionary<string, string> ISO_639_3_Unihan_Languages = new Dictionary<string, string>
        {
            { "cmn", "Mandarin Chinese" },
            { "jpn", "Japanese" },
            { "kor", "Korean" },
            { "ltc", "Middle Chinese" },
            { "vie", "Vietnamese" },
            { "yue", "Cantonese Chinese" },
            { "zha", "Zhuang Chinese" },
            { "zho", "Chinese (generic)" },
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

        public static Dictionary<string, List<UnihanField>> NameUnihanLookup = new Dictionary<string, List<UnihanField>>
        {
            { "jpn", new List<UnihanField> { UnihanField.kJapanese, UnihanField.kJapaneseKun, UnihanField.kJapaneseOn } },
            { "kor", new List<UnihanField> { UnihanField.kHangul, UnihanField.kKorean } },
            { "vie", new List<UnihanField> { UnihanField.kVietnamese } },
            { "zho-Hans", new List<UnihanField> { UnihanField.kMandarin, UnihanField.kFanqie, UnihanField.kHanyuPinlu, UnihanField.kXHC1983, UnihanField.kZhuang } },
            { "zho-Hant", new List<UnihanField> { UnihanField.kCantonese, UnihanField.kSMSZD2003Readings, UnihanField.kTang, UnihanField.kTGHZ2013 } },
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

    //U+7684	kCantonese	dik1
    //U+7684	kDefinition	possessive, adjectival suffix
    //U+7684	kFanqie	都歷
    //U+7684	kHangul	적:0E
    //U+7684	kHanyuPinlu	de(75596) dì(157) dí(84)
    //U+7684	kHanyuPinyin	42644.160:dì,dí,de
    //U+7684	kJapanese	テキ チャク キョウ ギョウ まと あきらか
    //U+7684	kJapaneseKun	MATO AKIRAKA
    //U+7684	kJapaneseOn	TEKI
    //U+7684	kKorean	CEK
    //U+7684	kMandarin	de
    //U+7684	kSMSZD2003Readings	dì粵dik1 dí粵dik1 de粵dik1
    //U+7684	kTGHZ2013	069.080:de 070.170:dī 071.080:dí 072.100:dì
    //U+7684	kTang	dek
    //U+7684	kVietnamese	đích
    //U+7684	kXHC1983	0224.040:de 0231.060:dí 0239.060:dì

    //[Flags]
    public enum UnihanField
    {
        Unknown = 0,
        kCantonese,
        kFanqie,
        kHangul,
        kHanyuPinlu,
        kHanyuPinyin,
        kJapanese,
        kJapaneseKun,
        kJapaneseOn,
        kKorean,
        kMandarin,
        kSMSZD2003Readings,
        kTang,
        kTGHZ2013,
        kVietnamese,
        kXHC1983,
        kZhuang,
        kDefinition
    }
}