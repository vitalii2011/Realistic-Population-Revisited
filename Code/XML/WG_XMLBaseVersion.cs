using System.Xml;

namespace RealPop2
{
    public abstract class WG_XMLBaseVersion
    {
        public WG_XMLBaseVersion()
        {
        }

        public abstract void ReadXML(XmlDocument doc);
        public abstract bool WriteXML(string fullPathFileName);
    }
}