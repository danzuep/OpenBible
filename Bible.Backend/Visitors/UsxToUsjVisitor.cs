using Bible.Backend.Abstractions;
using Bible.Backend.Models;
using Bible.Backend.Services;
using Bible.Core.Models;
using Bible.Usx.Models;
using Microsoft.Extensions.Options;
using Unihan.Models;

namespace Bible.Backend.Visitors
{
    public sealed class UsxToUsjVisitor : IUsxVisitor
    {
        public static async Task<Usj?> DeserializeAsync(Stream stream, UnihanDictionary? unihanDictionary = null, UsxVisitorOptions? options = null, CancellationToken cancellationToken = default)
        {
            var deserializer = new XDocDeserializer();
            var usxBook = await deserializer.DeserializeAsync<UsxBook>(stream, cancellationToken);
            return GetBook(usxBook, unihanDictionary, options);
        }

        public static Usj GetBook(UsxBook? usxScriptureBook, UnihanDictionary? unihanDictionary = null, UsxVisitorOptions? options = null)
        {
            var visitor = new UsxToUsjVisitor(options);
            visitor.UnihanDictionary = unihanDictionary;
            var book = visitor.GetBook(usxScriptureBook);
            return book;
        }

        public UsxToUsjVisitor(IOptions<UsxVisitorOptions>? options = null)
        {
            _options = options?.Value ?? new UsxVisitorOptions();
        }

        public UnihanDictionary? UnihanDictionary { get; set; }

        private readonly List<UsxFootnote> _footnotes = new();

        private readonly UsxVisitorOptions _options;

        private readonly Usj _usj = new();

        public void Visit(UsxIdentification identification)
        {
            _usj.BookCode = identification.BookCode;
            if (string.IsNullOrEmpty(identification.VersionName)) return;
            var versionName = identification.VersionName.Trim([' ','-']);
            int firstSpaceIndex = versionName.IndexOf(' ') + 1;
            if (versionName[..firstSpaceIndex].Contains('.'))
                _usj.BookVersion = versionName[firstSpaceIndex..];
            else
                _usj.BookVersion = versionName;
        }

        public void Visit(UsxPara para)
        {
            if (!string.IsNullOrEmpty(para?.Style))
            {
                if (_usj.Contents == null || !_usj.Contents.Any())
                {
                    _usj.Metadata.Add(para.Style, para.Text);
                }
                else
                {
                    if (para.Style.Equals("p"))
                    {
                        _usj.Contents.Add(new("style", para.Style));
                    }
                    else
                    {
                        _usj.Contents.Add(new(para.Style, para.Text));
                    }
                    this.Accept(para.Content);
                }
            }
        }

        public void Visit(UsxChapterMarker chapterMarker)
        {
            if (!string.IsNullOrEmpty(chapterMarker.Style) && !string.IsNullOrEmpty(chapterMarker.Number))
                _usj.Contents.Add(new(chapterMarker.Style, chapterMarker.Number));
        }

        public void Visit(UsxVerseMarker verseMarker)
        {
            if (!string.IsNullOrEmpty(verseMarker.Style) && !string.IsNullOrEmpty(verseMarker.Number))
                _usj.Contents.Add(new(verseMarker.Style, verseMarker.Number));
        }

        public void Visit(UsxChar usxChar)
        {
            if (!string.IsNullOrEmpty(usxChar.Style) && !string.IsNullOrEmpty(usxChar.Text))
                _usj.Contents.Add(new(usxChar.Style, usxChar.Text));
            this.Accept(usxChar.Content);
        }

        public void Visit(string text)
        {
            if (UnihanDictionary != null)
            {
                foreach (var rune in text.EnumerateRunes())
                {
                    _usj.Contents.Add(new(null, rune.ToString()));
                    AddUnihan(rune.Value, UnihanDictionary);
                }
            }
            else
            {
                _usj.Contents.Add(new(null, text));
            }
        }

        private void AddUnihan(int codepoint, UnihanDictionary unihanDictionary, string category = "unihan")
        {
            if (unihanDictionary == null) return;
            if (unihanDictionary.TryGetValue(codepoint, out var metadata) && metadata != null)
            {
                foreach (var value in metadata)
                {
                    _usj.Contents.Add(new(category, value));
                }
            }
        }

        public void Visit(UsxMilestone milestone)
        {
            _usj.Contents.Add(new("style", milestone.Style ?? "ms"));
        }

        public void Visit(UsxLineBreak lineBreak)
        {
            _usj.Contents.Add(new("style", "optbreak"));
        }

        public void Visit(UsxCrossReference reference)
        {
            if (_options.EnableCrossReferences)
            {
                _usj.Contents.Add(new("ref", reference.Location));
                this.Accept(reference.Content);
            }
        }

        public void Visit(UsxFootnote footnote)
        {
            if (_options.EnableFootnotes)
            {
                _footnotes.Add(footnote);
                _usj.Contents.Add(new("note", footnote.Caller));
                this.Accept(footnote.Content);
            }
        }

        public Usj GetBook(UsxBook? usxScriptureBook)
        {
            _usj.UsxVersion = usxScriptureBook?.UsxVersion;
            this.Accept(usxScriptureBook);
            //_usj.Meta = _usj.Metadata.ToLookup(kv => kv.Key, kv => kv.Value);
            //_usj.Cont = _usj.Contents.ToLookup(kv => kv.Key, kv => kv.Value);
            return _usj;
        }
    }
}