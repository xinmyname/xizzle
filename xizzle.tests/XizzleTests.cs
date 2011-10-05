using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using Should;

namespace xizzle.tests
{
    [TestFixture]
    public class XizzleTests
    {
        private XizzleConventions _originalConventions;

        [Test]
        public void Star()
        {
            XElement.Parse("<a><b>text</b></a>")
                .Select("*")
                .Count()
                .ShouldEqual(2);
        }

        [Test]
        public void SingleElement()
        {
            XElement.Parse("<a><b>text</b></a>")
                .Select("b").Single()
                .Name
                .ShouldEqual("b");
        }

        [Test]
        public void MultipleElements()
        {
            XElement.Parse("<a><b>text1</b><b>text2</b><b>text3</b></a>")
                .Select("b")
                .Count()
                .ShouldEqual(3);
        }

        [Test]
        public void ChildElement()
        {
            XElement.Parse("<a><b><c/></b><b>text2</b><b>text3</b></a>")
                .Select("b > c").Single()
                .Name
                .ShouldEqual("c");
        }

        [Test]
        public void MultipleChildElement()
        {
            XElement.Parse("<a><b><c/></b><b>text2</b><b><c/></b></a>")
                .Select("b > c")
                .Count()
                .ShouldEqual(2);
        }

        [Test]
        public void DefaultIdNameFindsCorrectElement()
        {
            XElement.Parse("<a><b><c/></b><b id='t2'>text2</b><b>text3</b></a>")
                .Select("#t2").Single()
                .Value
                .ShouldEqual("text2");
        }

        [Test]
        public void EqualsSelectorAttribute()
        {
            XElement.Parse("<a><b index='0'><c/></b><b index='1'>text2</b><b index='2'>text3</b></a>")
                .Select("b[index='2']").Single()
                .Value
                .ShouldEqual("text3");
        }

        [Test]
        public void DashMatchSelectorAttribute()
        {
            XElement.Parse("<a><b lang='en'>color</b><b lang='en-uk'>colour</b><b lang='ne'>kleur</b></a>")
                .Select("b[lang|='en']")
                .Count()
                .ShouldEqual(2);
        }

        [Test]
        public void IncludesSelectorAttribute()
        {
            XElement.Parse("<a><b region='us ca mx'>color</b><b region='gb au nz'>colour</b><b region='ne'>kleur</b></a>")
                .Select("b[region~='au']").Single()
                .Value
                .ShouldEqual("colour");
        }

        [Test]
        public void SuffixSelectorAttribute()
        {
            XElement.Parse("<a><b idx='1st'/><c idx='2nd'/><d idx='3rd'/><e idx='4th'/></a>")
                .Select("*[idx$='rd']").Single()
                .Name
                .ShouldEqual("d");
        }

        [Test]
        public void NotEqualsSelectorAttribute()
        {
            XElement.Parse("<a><b idx='0'><c/></b><b index='1'>text2</b><b index='2'>text3</b></a>")
                .Select("b[index!='1']").Single()
                .Value
                .ShouldEqual("text3");
        }

        [Test]
        public void SubstringSelectorAttribute()
        {
            XElement.Parse("<a><b word='bomb'/><b word='comb'/><b word='come'/><b word='dome'/></a>")
                .Select("b[word*='mb']")
                .Count()
                .ShouldEqual(2);
        }

        [Test]
        public void PrefixSelectorAttribute()
        {
            XElement.Parse("<a><b word='bomb'/><b word='comb'/><b word='come'/><b word='dome'/></a>")
                .Select("b[word^='bo']").Single()
                .Attributes("word").Single()
                .Value
                .ShouldEqual("bomb");
        }

        [Test]
        public void MultipleSelectors()
        {
            XElement.Parse("<a><b/><b/><c/><c/></a>")
                .Select("b,c")
                .Count()
                .ShouldEqual(4);
        }

        [Test]
        public void AdjacentSelector()
        {
            XElement.Parse("<a><b/><c>found</c><b/><d/><c/></a>")
                .Select("b + c").Single()
                .Value
                .ShouldEqual("found");
        }

        [Test]
        public void SiblingsSelector()
        {
            XElement.Parse("<a><c/><b/><c/><c/></a>")
                .Select("b ~ c")
                .Count()
                .ShouldEqual(2);
        }

        [Test]
        public void DocumentIsDisposedWhenLeavingContextScope()
        {
            XElement root;

            using (XElementContext context = XElement.Parse("<xml/>").Open())
                root = context.RootElement;

            XElementContext.Has(root)
                .ShouldBeFalse();
        }

        [Test]
        public void CanOverrideIdAttributeNameConvention()
        {
            const string xml = "<a><b><c/></b><b NAME='t2'>text2</b><b>text3</b></a>";
            var conventions = new XizzleConventions {IdAttributeName = () => "NAME"};

            using (var context = XElementContext.Parse(xml, conventions))
            {
                context.Select("#t2").Single()
                    .Value
                    .ShouldEqual("text2");
            }
        }

        [Test]
        public void CanOverrideDefaultIdAttributeNameConvention()
        {
            XElementContext.DefaultConventions =
                new XizzleConventions
                {
                    IdAttributeName = () => "NAME"
                };

            const string xml = "<a><b><c/></b><b NAME='t2'>text2</b><b>text3</b></a>";

            using (var context = XElementContext.Parse(xml))
            {
                context.Select("#t2").Single()
                    .Value
                    .ShouldEqual("text2");
            }
        }

        [SetUp]
        public void SaveDefaultConventions()
        {
            _originalConventions = XElementContext.DefaultConventions;
        }

        [TearDown]
        public void RestoreDefaultConventions()
        {
            XElementContext.DefaultConventions = _originalConventions;
        }
    }
}