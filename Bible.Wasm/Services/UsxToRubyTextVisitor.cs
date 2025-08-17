using Bible.Backend.Abstractions;
using Bible.Backend.Models;
using Bible.Backend.Services;
using Bible.Backend.Visitors;
using Bible.Core.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace Bible.Wasm.Services
{
    public sealed class UsxToRubyTextVisitor : IUsxVisitor
    {
        public static async Task<BibleParagraphList> DeserializeAsync(Stream stream, IOptions<UsxVisitorOptions>? options = null, CancellationToken cancellationToken = default)
        {
            var deserializer = new XDocDeserializer();
            var usxBook = await deserializer.DeserializeAsync<UsxBook>(stream, cancellationToken);
            var visitor = new UsxToRubyTextVisitor(options);
            return visitor.Build(usxBook);
        }

        private readonly RubyTextBuilder _builder;
        private readonly UsxVisitorOptions _options;

        private readonly List<UsxFootnote> _footnotes = new();

        public UsxToRubyTextVisitor(IOptions<UsxVisitorOptions>? options = null, RubyTextBuilder? builder = null)
        {
            _options = options?.Value ?? new UsxVisitorOptions
            {
                EnableChapterLinks = true,
                EnableCrossReferences = true,
                EnableRedLetters = true,
                EnableStrongs = true,
                EnableRubyText = true,
                EnableFootnotes = true
            };
            _builder = builder ?? new RubyTextBuilder();
        }

        /// <summary>
        /// Gets the footnotes collected during building
        /// </summary>
        public IReadOnlyList<BibleFootnote> Footnotes => _builder.Footnotes;

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

        public void Visit(UsxChapterMarker chapterMarker)
        {
            _builder.HandleChapterChange(chapterMarker.Number, chapterMarker.StartId);
        }

        public void Visit(UsxVerseMarker verseMarker)
        {
            _builder.HandleVerseChange(verseMarker.Number, verseMarker.StartId);
        }

        public void Visit(UsxPara para)
        {
            if (!string.IsNullOrEmpty(para.Style) && para.Style.StartsWith("p", StringComparison.OrdinalIgnoreCase))
            {
                _builder.HandleParagraphChange();
            }

            this.Accept(para.Content);
        }

        public void Visit(UsxChar usxChar)
        {
            _builder.AddWord(MakeRenderFragment(usxChar.Content));

            if (!_options.EnableRubyText)
            {
                return;
            }

            this.Accept(usxChar.Content);
        }

        public void Visit(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                _builder.AddWord(MakeRenderFragment(text));
            }
        }

        public void Visit(UsxMilestone milestone)
        {
            // Could interpret milestones as sentence or paragraph boundaries
        }

        public void Visit(UsxLineBreak lineBreak)
        {
            _builder.AddWord(MakeRenderFragment("\n"));
        }

        public void Visit(UsxCrossReference crossReference)
        {
            if (_options.EnableCrossReferences)
            {
                this.Accept(crossReference.Content);
            }
        }

        public void Visit(UsxFootnote footnote)
        {
            if (!_options.EnableFootnotes)
                return;

            _footnotes.Add(footnote);

            // Using builder's AddWord with footnote content to auto-assign footnote ID
            _builder.AddWord(MakeRenderFragment(footnote.Caller), null, _footnotes.Count);

            _builder.AddFootnote(_footnotes.Count, MakeRenderFragment(footnote.Content));
        }

        public BibleParagraphList Build(UsxBook? usxBibleBook)
        {
            this.Accept(usxBibleBook);
            AppendFootnotes();
            return _builder.Build();
        }

        private void AppendFootnotes()
        {
            if (_footnotes.Any())
            {
                for (var i = 0; i < _footnotes.Count; i++)
                {
                    _builder.AddFootnote(i + 1, MakeRenderFragment(_footnotes[i].Content));
                }
            }
        }

        // Helper to create a RenderFragment from string or object content
        private static RenderFragment MakeRenderFragment(object? content) => builder =>
        {
            if (content == null) return;

            switch (content)
            {
                case string s:
                    builder.AddContent(0, s);
                    break;
                case RenderFragment rf:
                    builder.AddContent(0, rf);
                    break;
                default:
                    builder.AddContent(0, content.ToString());
                    break;
            }
        };
    }
}