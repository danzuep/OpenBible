namespace Bible.Reader.Models
{
    internal sealed class WebZefaniaIndexModel : WebZefaniaDownloadsModel
    {
        public string IsValidXml { get; set; }
        public string ZefaniaXmlSchemaVersion { get; set; }
        public string ZefaniaBibleName { get; set; }
        public string ZefaniaBibleStatus { get; set; }
        public string ZefaniaBibleVersion { get; set; }
        public int? ZefaniaBibleRevision { get; set; }
        public string ZefaniaBibleType { get; set; }
        public string ZefaniaBibleInfoTitle { get; set; }
        public string ZefaniaBibleInfoIdentifier { get; set; }
        public string Warnings { get; set; }
    }
}
