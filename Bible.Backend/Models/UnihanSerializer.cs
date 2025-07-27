using Bible.Backend.Abstractions;
using Bible.Backend.Services;

namespace Bible.Backend.Models
{
    /// <inheritdoc cref="IUnihanReadings"/>
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