using CsvHelper.Configuration.Attributes;

namespace Bible.Reader.Models
{
    internal class WebZefaniaDownloadsModel
    {
        public string DownloadUrl { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }

        [Name("MD5-Hash")]
        public string MD5_Hash { get; set; }
    }
}
