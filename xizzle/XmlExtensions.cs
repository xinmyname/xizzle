using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace xizzle
{
    public static class XmlExtensions
    {
        public static IEnumerable<XmlElement> AllElements(this XmlElement root)
        {
            yield return root;
            foreach (var e in root.ChildNodes.OfType<XmlElement>().SelectMany(child => child.AllElements()))
                yield return e;
        }

        public static XmlElement ParentElement(this XmlElement el)
        {
            for (XmlNode it = el.ParentNode; it != null; it = it.ParentNode)
                if (it.NodeType == XmlNodeType.Element)
                    return (XmlElement)it;
            return null;
        }

        public static XmlElement PreviousSiblingElement(this XmlElement el)
        {
            for (XmlNode it = el.PreviousSibling; it != null; it = it.PreviousSibling)
                if (it.NodeType == XmlNodeType.Element)
                    return (XmlElement)it;
            return null;
        }

        public static IEnumerable<XmlElement> Select(this XmlDocument doc, string selector)
        {
            var context = XmlDocumentContext.Open(doc);
            return context.Select(selector);
        }
    }
}
