using Bible.Backend.Abstractions;
using Bible.Backend.Models;
using Bible.Backend.Services;
using Bible.Core.Models;
using Bible.Core.Models.Scripture;
using Bible.Usx.Models;
using Microsoft.Extensions.Options;
using Unihan.Services;

namespace Bible.Backend.Visitors
{
    public sealed class UsxToBibleBookVisitor : IUsxVisitor
    {
        public static async Task<BibleBook?> DeserializeAsync(Stream stream, BibleBookMetadata? metadata, UnihanLanguage? unihan = null, UsxVisitorOptions? options = null, CancellationToken cancellationToken = default)
        {
            var deserializer = new XDocDeserializer();
            var usxBook = await deserializer.DeserializeAsync<UsxBook>(stream, cancellationToken);
            return GetBook(usxBook, metadata, unihan, options);
        }

        public static BibleBook GetBook(UsxBook? usxBibleBook, BibleBookMetadata? metadata, UnihanLanguage? unihan = null, UsxVisitorOptions? options = null)
        {
            var builder = new BibleBookBuilder();
            if (metadata != null)
            {
                builder.SetBookCode(metadata.BookCode)
                       .SetVersionName(metadata.BibleVersion)
                       .SetLanguage(metadata.IsoLanguage);
            }
            builder.Unihan = unihan;
            var visitor = new UsxToBibleBookVisitor(options, builder);
            var book = visitor.GetBook(usxBibleBook);
            return book;
        }

        public UsxToBibleBookVisitor(IOptions<UsxVisitorOptions>? options = null, BibleBookBuilder? builder = null)
        {
            _options = options?.Value ?? new UsxVisitorOptions
            {
                EnableChapterLinks = true,
                EnableCrossReferences = true,
                EnableRedLetters = true,
                EnableStrongs = true,
                EnableRubyText = true,
                EnableFootnotes = true,
                EnableRunes = builder?.Unihan?.Field
            };
            _builder = builder ?? new BibleBookBuilder();
        }

        private readonly List<UsxFootnote> _footnotes = new();

        private readonly UsxVisitorOptions _options;

        private readonly BibleBookBuilder _builder;

        public void Visit(UsxIdentification identification)
        {
            string versionName;
            int firstSpaceIndex = identification.VersionName.IndexOf(' ') + 1;
            if (identification.VersionName[..firstSpaceIndex].Contains('.'))
                versionName = identification.VersionName[firstSpaceIndex..];
            else
                versionName = identification.VersionName;
            _builder.SetVersionName(versionName);
            _builder.SetBookCode(identification.BookCode);
        }

        public void Visit(UsxPara para)
        {
            _builder.AddScriptureSegment(para.Style, MetadataCategory.Style);
            if (!string.IsNullOrEmpty(para.Style) &&
                para.Style.StartsWith("h", StringComparison.OrdinalIgnoreCase) &&
                para.Text is string heading)
            {
                _builder.SetBookName(heading);
            }
            else
            {
                this.Accept(para?.Content);
            }
        }

        public void Visit(UsxChapterMarker chapterMarker)
        {
            _builder.HandleChapterChange(chapterMarker.Number, chapterMarker.StartId);
        }

        public void Visit(UsxVerseMarker verseMarker)
        {
            _builder.HandleVerseChange(verseMarker.Number, verseMarker.StartId);
        }

        public void Visit(UsxChar usxChar)
        {
            _builder.AddScriptureSegment(usxChar.Style, MetadataCategory.Style);
            this.Accept(usxChar.Content);
        }

        public void Visit(string text)
        {
            _builder.AddScriptureSegment(text);
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
                _builder.AddScriptureSegment(_footnotes.Count.ToString(), MetadataCategory.Footnote);
            }
        }

        private void AppendAnyFootnotes()
        {
            if (_footnotes.Any())
            {
                _builder.HandleVerseChange("0");
                for (var i = 0; i < _footnotes.Count; i++)
                {
                    this.Accept(_footnotes[i].Content);
                }
            }
        }

        public BibleBook GetBook(UsxBook? usxBibleBook)
        {
            this.Accept(usxBibleBook);
            //AppendAnyFootnotes();
            return _builder.Build();
        }
    }
}