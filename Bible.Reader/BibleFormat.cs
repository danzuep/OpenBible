namespace Bible.Reader
{
    /// <summary>
    /// Formats gleaned from [SimpleBibleReader](https://trumpet-call.org/simplebiblereader/) (Public Domain).
    /// https://github.com/schierlm/BibleMultiConverter/tree/master/biblemulticonverter-schemas/src/main/resources
    /// Open access: https://app.thedigitalbiblelibrary.org/entries/open_access_entries?type=text
    /// RelaxNG to XML converter: https://relaxng.org/jclark/trang.html
    /// </summary>
    public enum BibleFormat
    {
        Unknown,
        Usfm,       // [Unified Standard Format Markers for Paratext](https://ubsicap.github.io/usfm/)
        Usx3,       // [Unified Scripture XML v3](https://ubsicap.github.io/usx/) [rng](https://github.com/ubsicap/usx/blob/master/schema/usx.rng)
        Usfx,       // [Unified Standard Format XML](https://ebible.org/usfx/) [xsd](https://github.com/Freely-Given-org/BibleOrgSys/blob/main/ExternalSchemas/usfx.xsd)
        Osis,       // [Open Scriptural Information Standard](https://crosswire.org/osis/) [xsd](https://github.com/schierlm/BibleMultiConverter/blob/master/biblemulticonverter-schemas/src/main/resources/osisCore.2.1.1.xsd)
        Zef05,      // [Zefania XML 2005](https://bgfdb.de/zefaniaxml/bml/) [xsd](https://raw.githubusercontent.com/kohelet-net-admin/zefania-xml-bibles/master/Schema/zef2005.xsd)
        Zef14,      // Zefania XML 2014 [xsd](https://raw.githubusercontent.com/kohelet-net-admin/zefania-xml-bibles/master/Schema/zef2014.xsd)
    }
}