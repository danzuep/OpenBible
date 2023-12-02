using Bible.Core.Abstractions;
using Bible.Core.Models;
using System.Linq;

namespace Bible.Reader.Services
{
    public sealed class TestBibleReader : IDataService<BibleModel>
    {
        public BibleModel Load(string fileName, string suffix = ".xml")
        {
            var numbers = ParallelEnumerable.Range(1, suffix.Length);
            var bible = new BibleModel
            {
                Information = new BibleInformation
                {
                    Name = "Books, Chapters, and Verses"
                },
                Books = numbers.Select(n =>
                    new BibleBook
                    {
                        Reference = new BibleReference
                        {
                            BookName = $"Book #{n}"
                        },
                        Id = n,
                        Chapters = numbers.Select(n =>
                            new BibleChapter
                            {
                                Id = n,
                                Reference = new BibleReference
                                {
                                    BookName = $"Chapter #{n}"
                                },
                                Verses = numbers.Select(n =>
                                    new BibleVerse
                                    {
                                        Id = n,
                                        Text = $"Verse {n}"
                                    }).ToArray()
                            }).ToArray()
                    }).ToArray()
            };
            return bible;
        }
    }
}
