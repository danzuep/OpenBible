namespace Unihan.Models
{
    public sealed class UnihanSplitterOptions
    {
        public int PageSize { get; set; } = 256;
        public string FileName { get; set; } = "Unihan_Readings";
        public string InputPath => $"{FileName}.txt";
        public string OutputPath => $"{FileName}.json";
        public string OutputDirectory { get; set; } = "unihan";
        public bool WriteIndented { get; set; } = false;
        public bool Precompress { get; set; } = true;
    }
}