using System.Collections.Concurrent;
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
        public static async Task<UsjChapters?> DeserializeAsync(Stream stream, UnihanDictionary? unihanDictionary = null, UsxVisitorOptions? options = null, CancellationToken cancellationToken = default)
        {
            var deserializer = new XDocDeserializer();
            var usxBook = await deserializer.DeserializeAsync<UsxBook>(stream, cancellationToken);
            return GetBook(usxBook, unihanDictionary, options);
        }

        public static UsjChapters GetBook(UsxBook? usxScriptureBook, UnihanDictionary? unihanDictionary = null, UsxVisitorOptions? options = null)
        {
            var visitor = new UsxToStringArraysVisitor(options, unihanDictionary);
            var book = visitor.GetBook(usxScriptureBook);
            return book;
        }

        public UsxToStringArraysVisitor(IOptions<UsxVisitorOptions>? options = null, UnihanDictionary? unihanDictionary = null)
        {
            _options = options?.Value ?? new UsxVisitorOptions();
            if (unihanDictionary != null)
            {
                UnihanDictionary = unihanDictionary;
                _usj.Runes = new();
            }
        }

        private readonly UsxVisitorOptions _options;

        public UnihanDictionary? UnihanDictionary { get; set; }

        private readonly List<UsxFootnote> _footnotes = new();

        private readonly UsjChapters _usj = new();

        private IList<IList<string?>> _items = new List<IList<string?>>();
        private string?[] _values = new string[2];
        private void AddEntry(string? key, string? value)
        {
            _values[0] = key;
            _values[1] = value;
            _items.Add(_values);
            _values = new string[2];
        }

        private void AddEntry(string? value)
        {
            if (_values[0] == null)
                _values[0] = string.Empty;
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
                if (para.Style.StartsWith("p"))
                {
                    AddEntry("p", para.Style != "p" ? para.Style : "");
                }
                else
                {
                    _values[0] = para.Style;
                }
                this.Accept(para.Content);
            }
        }

        public void Visit(UsxChapterMarker chapterMarker)
        {
            if (!string.IsNullOrEmpty(chapterMarker.Style) && !string.IsNullOrEmpty(chapterMarker.Number))
            {
                _usj.Chapters.Add(_items);
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
                _values[0] = usxChar.Style;
            this.Accept(usxChar.Content);
        }

        public void Visit(string text)
        {
            if (text == "\n    ") return;
            if (UnihanDictionary != null)
            {
                foreach (var rune in text.EnumerateRunes())
                {
                    var codepoint = rune.Value;
                    if (UnihanDictionary.ContainsKey(codepoint))
                    {
                        _usj.Runes!.AddOrUpdate(codepoint, 1, (key, count) => count + 1);
                    }
                }
            }
            AddEntry(text);
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

        public UsjChapters GetBook(UsxBook? usxScriptureBook)
        {
            AddEntry("usx", usxScriptureBook?.UsxVersion);
            this.Accept(usxScriptureBook);
            _usj.Chapters.Add(_items);
            _items = Array.Empty<IList<string?>>();
            return _usj;
        }
    }

    public sealed class UsjChapters
    {
        public ConcurrentDictionary<int, int>? Runes { get; set; }

        public IList<IList<IList<string?>>> Chapters { get; set; } =
            new List<IList<IList<string?>>>();
    }
}