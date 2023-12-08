namespace Bible.Reader.Models
{
    public sealed class WebBibleInfoModel
    {
        public string Name { get; set; }
        public string Language { get; set; }
        public string Identifier { get; set; }
        public string DownloadUrl { get; set; }

        public override string ToString() =>
            $"{Identifier} - {Name}";
    }
}
