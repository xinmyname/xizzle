using System;
using System.Linq;
using System.Xml.Linq;
using Sgml;
using xizzle;

namespace HtmlParsingSample
{
    /// <summary>
    /// Use SgmlReader and xizzle to parse HTML, like sharp-query does
    /// Big caveat - there's no way to select a class. I'm thinking of adding a convention to handle that,
    /// but I just need XML parsing right now.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var testPageStream = typeof(Program).Assembly.GetManifestResourceStream("HtmlParsingSample.TestPage.htm");

            using (var reader = SgmlReader.Create(testPageStream))
            using (var context = XElementContext.Load(reader))
            {
                Console.WriteLine(context.Select("#first").Single().Value);

                var itemOptions = context.Select("#items option");

                Console.WriteLine("There are {0} items.", itemOptions.Count());

                foreach (XElement optionElement in itemOptions)
                    Console.WriteLine("    {0}) {1}", optionElement.Attribute("value").Value, optionElement.Value);
            }
        }
    }
}
