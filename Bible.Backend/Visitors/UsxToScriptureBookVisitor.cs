using Bible.Backend.Abstractions;
using Bible.Backend.Models;
using Bible.Backend.Services;
using Bible.Core.Models.Scripture;
using Microsoft.Extensions.Options;

namespace Bible.Backend.Visitors
{
    public sealed class UsxToScriptureBookVisitor : IUsxVisitor
    {
        public static async Task<ScriptureBook?> DeserializeAsync(Stream stream, UnihanLookup? unihan = null, UsxVisitorOptions? options = null, CancellationToken cancellationToken = default)
        {
            var deserializer = new XDocDeserializer();
            var usxBook = await deserializer.DeserializeAsync<UsxBook>(stream, cancellationToken);
            return GetBook(usxBook, unihan, options);
        }

        public static ScriptureBook GetBook(UsxBook? usxScriptureBook, UnihanLookup? unihan = null, UsxVisitorOptions? options = null)
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

        public UnihanLookup? Unihan { get; set; }

        private readonly List<UsxFootnote> _footnotes = new();

        private readonly UsxVisitorOptions _options;

        private readonly ScriptureSegmentBuilder _builder;

        private readonly IReadOnlyList<string> _headingParaStyles =
            UsxToMarkdownVisitor.ParaStylesToHide;

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
            if (Unihan?.Field != null)
            {
                foreach (var rune in text.EnumerateRunes())
                {
                    AddUnihan(rune.Value, Unihan.Field.Value);
                    _builder.AddScriptureSegment(rune.ToString());
                }
            }
            else
            {
                _builder.AddScriptureSegment(text);
            }
        }

        private void AddUnihan(int codepoint, UnihanField unihanField)
        {
            if (Unihan != null && Unihan.TryGetValue(codepoint, out var metadata))
            {
                foreach (var kvp in metadata)
                {
                    Extract(kvp, unihanField, MetadataCategory.Pronunciation);
                    //Extract(kvp, UnihanField.kDefinition, MetadataCategory.Definition);
                }
            }

            void Extract(KeyValuePair<UnihanField, IList<string>> kvp, UnihanField field, MetadataCategory category)
            {
                if (kvp.Key == field)
                {
                    foreach (var value in kvp.Value)
                    {
                        _builder.AddScriptureSegment(value, category);
                    }
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