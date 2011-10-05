using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using Should;

namespace xizzle.tests
{
    public class XizzleContext
    {
        public XElement Query(string xml)
        {
            return XElement.Parse(xml);
        }
    }

    [TestFixture]
    public class XizzleTests : XizzleContext
    {
        [Test]
        public void Star()
        {
            Query("<a><b>text</b></a>")
                .Select("*")
                .Count()
                .ShouldEqual(2);
        }

        [Test]
        public void SingleElement()
        {
            Query("<a><b>text</b></a>")
                .Select("b").Single()
                .Name
                .ShouldEqual("b");
        }

        [Test]
        public void MultipleElements()
        {
            Query("<a><b>text1</b><b>text2</b><b>text3</b></a>")
                .Select("b")
                .Count()
                .ShouldEqual(3);
        }

        [Test]
        public void ChildElement()
        {
            Query("<a><b><c/></b><b>text2</b><b>text3</b></a>")
                .Select("b > c").Single()
                .Name
                .ShouldEqual("c");
        }

        [Test]
        public void MultipleChildElement()
        {
            Query("<a><b><c/></b><b>text2</b><b><c/></b></a>")
                .Select("b > c")
                .Count()
                .ShouldEqual(2);
        }

        [Test]
        public void NamedElement()
        {
            Query("<a><b><c/></b><b name='t2'>text2</b><b>text3</b></a>")
                .Select("#t2").Single()
                .Value
                .ShouldEqual("text2");
        }

        [Test]
        public void EqualsSelectorAttribute()
        {
            Query("<a><b index='0'><c/></b><b index='1'>text2</b><b index='2'>text3</b></a>")
                .Select("b[index='2']").Single()
                .Value
                .ShouldEqual("text3");
        }

        [Test]
        public void DashMatchSelectorAttribute()
        {
            Query("<a><b lang='en'>color</b><b lang='en-uk'>colour</b><b lang='ne'>kleur</b></a>")
                .Select("b[lang|='en']")
                .Count()
                .ShouldEqual(2);
        }

        [Test]
        public void IncludesSelectorAttribute()
        {
            Query("<a><b region='us ca mx'>color</b><b region='gb au nz'>colour</b><b region='ne'>kleur</b></a>")
                .Select("b[region~='au']").Single()
                .Value
                .ShouldEqual("colour");
        }

        [Test]
        public void SuffixSelectorAttribute()
        {
            Query("<a><b idx='1st'/><c idx='2nd'/><d idx='3rd'/><e idx='4th'/></a>")
                .Select("*[idx$='rd']").Single()
                .Name
                .ShouldEqual("d");
        }

        [Test]
        public void NotEqualsSelectorAttribute()
        {
            Query("<a><b idx='0'><c/></b><b index='1'>text2</b><b index='2'>text3</b></a>")
                .Select("b[index!='1']").Single()
                .Value
                .ShouldEqual("text3");
        }

        [Test]
        public void SubstringSelectorAttribute()
        {
            Query("<a><b word='bomb'/><b word='comb'/><b word='come'/><b word='dome'/></a>")
                .Select("b[word*='mb']")
                .Count()
                .ShouldEqual(2);
        }

        [Test]
        public void PrefixSelectorAttribute()
        {
            Query("<a><b word='bomb'/><b word='comb'/><b word='come'/><b word='dome'/></a>")
                .Select("b[word^='bo']").Single()
                .Attributes("word").Single()
                .Value
                .ShouldEqual("bomb");
        }

        [Test]
        public void MultipleSelectors()
        {
            Query("<a><b/><b/><c/><c/></a>")
                .Select("b,c")
                .Count()
                .ShouldEqual(4);
        }

        [Test]
        public void AdjacentSelector()
        {
            Query("<a><b/><c>found</c><b/><d/><c/></a>")
                .Select("b + c").Single()
                .Value
                .ShouldEqual("found");
        }

        [Test]
        public void SiblingsSelector()
        {
            Query("<a><c/><b/><c/><c/></a>")
                .Select("b ~ c")
                .Count()
                .ShouldEqual(2);
        }
    }
}