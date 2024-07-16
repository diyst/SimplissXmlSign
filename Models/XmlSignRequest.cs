namespace XmlSign.Models
{
    public class XmlSignRequest
    {
        public string XmlString { get; set; }
        public string[] NodesToSign { get; set; }
    }
}
