using System.Xml.Linq;

namespace Bible.Reader.Extensions
{
    internal static class XmlExtensions
    {
        public static bool TestAttribute(this XElement xmlElement, string attributeName, string expectedValue) =>
            xmlElement?.Attribute(attributeName)?.Value == expectedValue;
    }
}
