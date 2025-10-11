using Bible.Backend.Abstractions;
using Bible.Backend.Models;
using Bible.Backend.Services;
using Bible.Core.Models.Scripture;
using Bible.Usx.Models;
using Microsoft.Extensions.Options;
using Unihan.Models;
using Unihan.Services;

namespace Bible.Backend.Visitors
{
    public sealed class UsxToScriptureBookVisitor : IUsxVisitor
    {
        public static async Task<ScriptureBook?> DeserializeAsync(Stream stream, UnihanLanguage? unihan = null, UsxVisitorOptions? options = null, CancellationToken cancellationToken = default)
        {
            var deserializer = new XDocDeserializer();
            var usxBook = await deserializer.DeserializeAsync<UsxBook>(stream, cancellationToken);
            return GetBook(usxBook, unihan, options);
        }

        public static ScriptureBook GetBook(UsxBook? usxScriptureBook, UnihanLanguage? unihan = null, UsxVisitorOptions? options = null)
        {
            var visitor = new UsxToScriptureBookVisitor(options);
            visitor.Unihan = unihan;
            var book = visitor.GetBook(usxScriptureBook);
            return book;
        }

        public UsxToScriptureBookVisitor(IOptions<UsxVisitorOptions>? options = null, ScriptureSegmentBuilder? builder = null)
        {
            _options = options?.Value ?? new UsxVisitorOptions();
            _builder = builder ?? new ScriptureSegmentBuilder();
        }

        public UnihanLanguage? Unihan { get; set; }

        private readonly List<UsxFootnote> _footnotes = new();

        private readonly UsxVisitorOptions _options;

        private readonly ScriptureSegmentBuilder _builder;

        private readonly IReadOnlyList<string> _headingParaStyles =
            UsjConstants.ParaStylesToHide;

        public void Visit(UsxIdentification identification)
        {
            _builder.BookMetadata.Segments.Add(new(identification.Style, MetadataCategory.Style));
            _builder.BookMetadata.Id = identification.BookCode;
            int firstSpaceIndex = identification.VersionName.IndexOf(' ') + 1;
            if (identification.VersionName[..firstSpaceIndex].Contains('.'))
                _builder.BookMetadata.Version = identification.VersionName[firstSpaceIndex..];
            else
                _builder.BookMetadata.Version = identification.VersionName;
        }

        public void Visit(UsxPara para)
        {
            _builder.AddScriptureSegment(para.Style, MetadataCategory.Style);
            if (!string.IsNullOrEmpty(para.Style) &&
                para.Style.StartsWith("h", StringComparison.OrdinalIgnoreCase) &&
                para.Text is string heading)
            {
                //Visit(heading);
                if (string.IsNullOrEmpty(_builder.BookMetadata.Name))
                    _builder.BookMetadata.Name = heading;
                else
                    _builder.BookMetadata.Segments.Add(new(heading, MetadataCategory.Text));
            }
            else if (_headingParaStyles.Any(h => !string.IsNullOrEmpty(para.Style) &&
                para.Style.StartsWith(h, StringComparison.OrdinalIgnoreCase)) &&
                para.Text is string metadata)
            {
                _builder.BookMetadata.Segments.Add(new(metadata, MetadataCategory.Meta));
            }
            else
            {
                _builder.AddScriptureSegment("\n", MetadataCategory.Markup);
                this.Accept(para.Content);
            }
        }

        public void Visit(UsxChapterMarker chapterMarker)
        {
            if (byte.TryParse(chapterMarker.Number, out var chapterNumber))
            {
                _builder.HandleChapterChange(chapterNumber);
                _builder.AddScriptureSegment(chapterMarker.Number, MetadataCategory.Chapter);
                _builder.AddScriptureSegment(chapterMarker.StartId, MetadataCategory.Meta);
            }
        }

        public void Visit(UsxVerseMarker verseMarker)
        {
            if (byte.TryParse(verseMarker.Number, out var verseNumber))
            {
                _builder.HandleVerseChange(verseNumber);
                _builder.AddScriptureSegment(verseMarker.Number, MetadataCategory.Verse);
                _builder.AddScriptureSegment(verseMarker.StartId, MetadataCategory.Meta);
            }
        }

        public void Visit(UsxChar usxChar)
        {
            _builder.AddScriptureSegment(usxChar.Style, MetadataCategory.Style);
            this.Accept(usxChar.Content);
        }

        public void Visit(string text)
        {
            if (Unihan?.Dictionary != null)
            {
                foreach (var rune in text.EnumerateRunes())
                {
                    AddUnihan(rune.Value, Unihan.Dictionary);
                    _builder.AddScriptureSegment(rune.ToString());
                }
            }
            else
            {
                _builder.AddScriptureSegment(text);
            }
        }

        private void AddUnihan(int codepoint, UnihanDictionary unihanDictionary)
        {
            if (unihanDictionary == null) return;
            if (unihanDictionary.TryGetValue(codepoint, out var metadata))
            {
                AddScriptureSegments(metadata, MetadataCategory.Pronunciation);
            }

            void AddScriptureSegments(IList<string> values, MetadataCategory category)
            {
                if (values == null) return;
                foreach (var value in values)
                {
                    _builder.AddScriptureSegment(value, category);
                }
            }
        }

        public void Visit(UsxMilestone milestone)
        {
            _builder.AddScriptureSegment(milestone.Style, MetadataCategory.Markup);
        }

        public void Visit(UsxLineBreak lineBreak)
        {
            _builder.AddScriptureSegment("\n", MetadataCategory.Markup);
        }

        public void Visit(UsxCrossReference reference)
        {
            if (_options.EnableCrossReferences)
            {
                _builder.AddScriptureSegment(reference.Location, MetadataCategory.Reference);
                this.Accept(reference.Content);
            }
        }

        public void Visit(UsxFootnote footnote)
        {
            _builder.AddScriptureSegment(footnote.Style, MetadataCategory.Style);
            if (_options.EnableFootnotes)
            {
                _footnotes.Add(footnote);
                _builder.AddScriptureSegment(footnote.Caller, MetadataCategory.Footnote);
                this.Accept(footnote.Content);
            }
        }

        public ScriptureBook GetBook(UsxBook? usxScriptureBook)
        {
            this.Accept(usxScriptureBook);
            return _builder.Build();
        }
    }
}