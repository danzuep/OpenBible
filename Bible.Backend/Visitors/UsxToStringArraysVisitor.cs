using Bible.Backend.Abstractions;
using Bible.Backend.Models;
using Bible.Backend.Services;
using Bible.Usx.Models;
using Microsoft.Extensions.Options;
using Unihan.Models;

namespace Bible.Backend.Visitors
{
    public sealed class UsxToStringArraysVisitor : IUsxVisitor
    {
        public static async Task<IList<IList<IList<string?>>>?> DeserializeAsync(Stream stream, UnihanDictionary? unihanDictionary = null, UsxVisitorOptions? options = null, CancellationToken cancellationToken = default)
        {
            var deserializer = new XDocDeserializer();
            var usxBook = await deserializer.DeserializeAsync<UsxBook>(stream, cancellationToken);
            return GetBook(usxBook, unihanDictionary, options);
        }

        public static IList<IList<IList<string?>>> GetBook(UsxBook? usxScriptureBook, UnihanDictionary? unihanDictionary = null, UsxVisitorOptions? options = null)
        {
            var visitor = new UsxToStringArraysVisitor(options);
            visitor.UnihanDictionary = unihanDictionary;
            var book = visitor.GetBook(usxScriptureBook);
            return book;
        }

        public UsxToStringArraysVisitor(IOptions<UsxVisitorOptions>? options = null)
        {
            _options = options?.Value ?? new UsxVisitorOptions();
        }

        public UnihanDictionary? UnihanDictionary { get; set; }

        private readonly List<UsxFootnote> _footnotes = new();

        private readonly UsxVisitorOptions _options;

        private readonly IList<IList<IList<string?>>> _usj =
            new List<IList<IList<string?>>>();

        private static IList<IList<string?>> _items = new List<IList<string?>>();
        private static string?[] _values = new string[2];
        private void AddEntry(string? key, string? value)
        {
            _values[0] = key;
            _values[1] = value;
            _items.Add(_values);
            _values = new string[2];
        }

        public void Visit(UsxIdentification identification)
        {
            AddEntry("code", identification.BookCode);
            if (string.IsNullOrEmpty(identification.VersionName)) return;
            var versionName = identification.VersionName.Trim([' ','-']);
            int firstSpaceIndex = versionName.IndexOf(' ') + 1;
            if (versionName[..firstSpaceIndex].Contains('.'))
                AddEntry("book", versionName[firstSpaceIndex..]);
            else
                AddEntry("book", versionName);
        }

        public void Visit(UsxPara para)
        {
            if (!string.IsNullOrEmpty(para?.Style))
            {
                if (_usj.Count < 1)
                {
                    AddEntry(para.Style, para.Text);
                }
                else
                {
                    AddEntry(para.Style, "");
                    this.Accept(para.Content);
                }
            }
        }

        public void Visit(UsxChapterMarker chapterMarker)
        {
            if (!string.IsNullOrEmpty(chapterMarker.Style) && !string.IsNullOrEmpty(chapterMarker.Number))
            {
                _usj.Add(_items);
                _items = new List<IList<string?>>();
                AddEntry(chapterMarker.Style, chapterMarker.Number);
            }
        }

        public void Visit(UsxVerseMarker verseMarker)
        {
            if (!string.IsNullOrEmpty(verseMarker.Style) && !string.IsNullOrEmpty(verseMarker.Number))
                AddEntry(verseMarker.Style, verseMarker.Number);
        }

        public void Visit(UsxChar usxChar)
        {
            if (!string.IsNullOrEmpty(usxChar.Style))
                AddEntry(usxChar.Style, "");
            this.Accept(usxChar.Content);
        }

        public void Visit(string text)
        {
            if (UnihanDictionary != null)
            {
                foreach (var rune in text.EnumerateRunes())
                {
                    AddEntry("", rune.ToString());
                    AddUnihan(rune.Value, UnihanDictionary);
                }
            }
            else if (!string.IsNullOrEmpty(text) && !text.Equals("\n    "))
            {
                AddEntry("", text);
            }
        }

        private void AddUnihan(int codepoint, UnihanDictionary unihanDictionary, string category = "unihan")
        {
            if (unihanDictionary == null) return;
            if (unihanDictionary.TryGetValue(codepoint, out var metadata) && metadata != null)
            {
                foreach (var value in metadata)
                {
                    AddEntry(category, value);
                }
            }
        }

        public void Visit(UsxMilestone milestone)
        {
            AddEntry(milestone.Style ?? "ms", "");
        }

        public void Visit(UsxLineBreak lineBreak)
        {
            AddEntry("optbreak", "");
        }

        public void Visit(UsxCrossReference reference)
        {
            if (_options.EnableCrossReferences)
            {
                AddEntry("ref", reference.Location);
                this.Accept(reference.Content);
            }
        }

        public void Visit(UsxFootnote footnote)
        {
            if (_options.EnableFootnotes)
            {
                _footnotes.Add(footnote);
                AddEntry("note", footnote.Caller);
                this.Accept(footnote.Content);
            }
        }

        public IList<IList<IList<string?>>> GetBook(UsxBook? usxScriptureBook)
        {
            AddEntry("usx", usxScriptureBook?.UsxVersion);
            this.Accept(usxScriptureBook);
            _usj.Add(_items);
            _items = Array.Empty<IList<string?>>();
            return _usj;
        }
    }
}