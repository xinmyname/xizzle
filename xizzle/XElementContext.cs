using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace xizzle
{
    public class XElementContext : IDisposable
    {
        private static readonly FlexDict _patterns;
        private static readonly Regex _re;
        private static readonly Dictionary<XElement, XElementContext> _contexts;

        private readonly Dictionary<string, XElement> _idDict;
        private readonly Dictionary<string, HashSet<XElement>> _typeDict;
        private readonly Dictionary<string, HashSet<XElement>> _attrDict;
        private readonly XElement _root;

        static XElementContext()
        {
            _patterns = new FlexDict
                {
                    {"GOS"/*Group of Selectors*/, @"^\s*{Selector}(\s*,\s*{Selector})*\s*$"},
                    {"Selector", @"{SOSS}{CombinatorSOSS}*{PseudoElement}?"},
                    {"CombinatorSOSS", @"\s*{Combinator}\s*{SOSS}"},
                    {"SOSS"/*Sequence of Simple Selectors*/, @"({TypeSelector}|{UniversalSelector}){SimpleSelector}*|{SimpleSelector}+"},
                    {"SimpleSelector", @"{AttributeSelector}|{IDSelector}|{PseudoSelector}"},
                    
                    {"TypeSelector", @"{Identifier}"},
                    {"UniversalSelector", @"\*"},
                    {"AttributeSelector", @"\[\s*{Identifier}(\s*{ComparisonOperator}\s*{AttributeValue})?\s*\]"},
                    {"IDSelector", @"#{Identifier}"},
                    {"PseudoSelector", @":{Identifier}{PseudoArgs}?"},
                    {"PseudoElement", @"::{Identifier}"},

                    {"PseudoArgs", @"\(({String}|[^)])*\)"},

                    {"ComparisonOperator", @"[~^$*|!]?="},
                    {"Combinator", @"[ >+~]"},
                    
                    {"Identifier", @"-?[a-zA-Z\u00A0-\uFFFF_][a-zA-Z\u00A0-\uFFFF_0-9-]*"},

                    {"AttributeValue", @"{Identifier}|{String}"},
                    {"String", @"""((?>[^\\""\r\n]|\\\r\n|\\.)*)""|'((?>[^\\'\r\n]|\\\r\n|\\.)*)'"},
                };

            _re = new Regex(_patterns["GOS"], RegexOptions.ExplicitCapture | RegexOptions.Singleline);

            _contexts = new Dictionary<XElement, XElementContext>();
        }

        public static XElementContext Get(XElement root)
        {
            XElementContext context;

            lock (root)
            {
                if (_contexts.ContainsKey(root))
                    context = _contexts[root];
                else
                {
                    context = new XElementContext(root);
                    _contexts.Add(root, context);
                }
            }

            return context;
        }

        public XElementContext(XElement root)
        {
            _root = root;

            _idDict = new Dictionary<string, XElement>();
            _typeDict = new Dictionary<string, HashSet<XElement>>();
            _attrDict = new Dictionary<string, HashSet<XElement>>();

            foreach (var el in _root.DescendantsAndSelf())
            {
                IEnumerable<XAttribute> nameAttributes = el.Attributes()
                    .Where(a => String.Compare(a.Name.LocalName, "name", true) == 0);
                
                foreach (XAttribute attr in nameAttributes)
                    _idDict[attr.Value] = el;

                if (!_typeDict.ContainsKey(el.Name.LocalName))
                    _typeDict[el.Name.LocalName] = new HashSet<XElement>();
                _typeDict[el.Name.LocalName].Add(el);

                foreach (XAttribute a in el.Attributes())
                {
                    if (!_attrDict.ContainsKey(a.Name.LocalName))
                        _attrDict[a.Name.LocalName] = new HashSet<XElement>();
                    _attrDict[a.Name.LocalName].Add(el);
                }
            }
        }

        private void Filter_ID(ref HashSet<XElement> set, string id)
        {
            if (_idDict.ContainsKey(id))
            {
                XElement el = _idDict[id];
                if (set == null)
                    set = new HashSet<XElement> { _idDict[id] };
                else
                    set.Filter(e => e == el);
            }
            else set = new HashSet<XElement>();
        }

        private void Filter_Type(ref HashSet<XElement> set, string type)
        {
            if (_typeDict.ContainsKey(type))
            {
                if (set == null)
                    set = new HashSet<XElement>(_typeDict[type]);
                else
                    set.IntersectWith(_typeDict[type]);
            }
            else set = new HashSet<XElement>();
        }

        private static string Slice(string str, int? start = null, int? end = null, int step = 1)
        {
            if (step == 0) throw new ArgumentException("Step size cannot be zero", "step");

            if (start == null) start = step > 0 ? 0 : str.Length - 1;
            else if (start < 0) start = start < -str.Length ? 0 : str.Length + start;
            else if (start > str.Length) start = str.Length;

            if (end == null) end = step > 0 ? str.Length : -1;
            else if (end < 0) end = end < -str.Length ? 0 : str.Length + end;
            else if (end > str.Length) end = str.Length;

            if (start == end || start < end && step < 0 || start > end && step > 0) return "";
            if (step == 1) return str.Substring(start.Value, end.Value - start.Value);

            var sb = new StringBuilder((int)Math.Ceiling((end - start).Value / (float)step));
            for (int i = start.Value; step > 0 && i < end || step < 0 && i > end; i += step)
                sb.Append(str[i]);
            return sb.ToString();
        }

        private void Filter_Attribute(ref HashSet<XElement> set, string name, string @operator, string value)
        {
            if (_attrDict.ContainsKey(name))
            {
                if (set == null)
                    set = new HashSet<XElement>(_attrDict[name]);
                else
                    set.IntersectWith(_attrDict[name]);

                if (@operator != null)
                {
                    if (value == null) throw new ArgumentNullException("value", "Value cannot be null if operator is not null");

                    if (value[0] == value.Last() && (value[0] == '"' || value[0] == '\''))
                        value = Regex.Unescape(Slice(value, 1, -1));

                    switch (@operator)
                    {
                        case "=":
                            set.Filter(e => e.Attributes(name).Single().Value == value);
                            break;
                        case "~=":
                            set.Filter(e => Regex.IsMatch(e.Attributes(name).Single().Value, @"(^|\s)" + Regex.Escape(value) + @"(\s|$)"));
                            break;
                        case "^=":
                            set.Filter(e => e.Attributes(name).Single().Value.StartsWith(value));
                            break;
                        case "$=":
                            set.Filter(e => e.Attributes(name).Single().Value.EndsWith(value));
                            break;
                        case "*=":
                            set.Filter(e => e.Attributes(name).Single().Value.Contains(value));
                            break;
                        case "!=":
                            set.Filter(e => !e.Attributes(name).Single().Value.Contains(value));
                            break;
                        case "|=":
                            set.Filter(e => Regex.IsMatch(e.Attributes(name).Single().Value, "^" + Regex.Escape(value) + "(-|$)"));
                            break;
                        default:
                            throw new Exception(string.Format("Bad comparison operator: {0}", @operator));
                    }
                }
            }
            else set = new HashSet<XElement>();
        }

        private HashSet<XElement> Find_SOSS(Match match, Capture capture)
        {
            HashSet<XElement> set = null;

            foreach (Capture idCapture in match.Groups["IDSelector"].SubcapturesOf(capture))
                Filter_ID(ref set, idCapture.Value.Substring(1));

            foreach (Capture typeCapture in match.Groups["TypeSelector"].SubcapturesOf(capture))
                Filter_Type(ref set, typeCapture.Value);

            foreach (Capture attributeCapture in match.Groups["AttributeSelector"].SubcapturesOf(capture))
            {
                string op, value;
                string name = match.Groups["Identifier"].SubcapturesOf(attributeCapture).First().Value;
                Capture opCap = match.Groups["ComparisonOperator"].SubcapturesOf(attributeCapture).SingleOrDefault();
                if (opCap == null)
                {
                    op = null;
                    value = null;
                }
                else
                {
                    op = opCap.Value;
                    value = match.Groups["AttributeValue"].SubcapturesOf(attributeCapture).Single().Value;
                }
                Filter_Attribute(ref set, name, op, value);
            }

            return set ?? new HashSet<XElement>(_root.DescendantsAndSelf());
        }

        public IEnumerable<XElement> Select(string selector)
        {
            var match = _re.Match(selector);

            foreach (Capture selectorCapture in match.Groups["Selector"].Captures)
            {
                var selectorCaptures = match.Groups["SOSS"].SubcapturesOf(selectorCapture).Reverse().ToList();
                var combinatorCaptures = match.Groups["Combinator"].SubcapturesOf(selectorCapture).Reverse().ToList();
                var rightElements = Find_SOSS(match, selectorCaptures[0]);

                for (int i = 0; i < combinatorCaptures.Count; ++i)
                {
                    var leftElements = Find_SOSS(match, selectorCaptures[i + 1]);
                    switch (combinatorCaptures[i].Value)
                    {
                        case ">": // Child Combinator
                            rightElements.Filter(e => leftElements.Contains(e.Parent));
                            break;
                        case " ": // Descendant Combinator
                            rightElements.Filter(e =>
                            {
                                for (var p = e.Parent; p != null; p = p.Parent)
                                    if (leftElements.Contains(p))
                                        return true;
                                return false;
                            });
                            break;
                        case "+": // Adjacent Sibling Combinator
                            rightElements.Filter(e => leftElements.Contains(e.PreviousSiblingElement()));
                            break;
                        case "~": // General Sibling Combinator
                            rightElements.Filter(e =>
                            {
                                for (var p = e.PreviousSiblingElement(); p != null; p = p.PreviousSiblingElement())
                                    if (leftElements.Contains(p))
                                        return true;
                                return false;
                            });
                            break;
                    }
                }

                foreach (var e in rightElements)
                    yield return e;
            }
        }

        public void Dispose()
        {
            lock (_root)
            {
                _contexts.Remove(_root);
            }
        }
    }
}