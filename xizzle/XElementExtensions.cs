using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace xizzle
{
    public static class XElementExtensions
    {
        public static XElement PreviousSiblingElement(this XElement el)
        {
            for (XNode it = el.PreviousNode; it != null; it = it.PreviousNode)
                if (it.NodeType == XmlNodeType.Element)
                    return (XElement)it;
            return null;
        }

        public static IEnumerable<XElement> Select(this XElement root, string selector)
        {
            var context = XElementContext.Get(root);
            return context.Select(selector);
        }

        public static XElementContext Open(this XElement root, XizzleConventions conventions = null)
        {
            return XElementContext.Get(root, conventions);
        }
    }
}
