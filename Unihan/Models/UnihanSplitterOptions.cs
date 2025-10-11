using Microsoft.Extensions.Options;

namespace Unihan.Models
{
    public sealed class UnihanSplitterOptions : IOptions<UnihanSplitterOptions>
    {
        public int PageSize { get; set; } = 1000;
        public string FileName { get; set; } = "Unihan_Readings";
        public string InputPath => $"{FileName}.txt";
        public string Prefix { get; set; } = "unihan";
        public bool Precompress { get; set; } = true;

        public UnihanSplitterOptions Value => this;
    }
}