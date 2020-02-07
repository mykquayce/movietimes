using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;
using Xunit;

namespace MovieTimes.Service.Services.Tests
{
	public class XPathTests
	{
		[Theory]
		[InlineData("listings.xml", "XSLTFile1.xslt")]
		public async Task XPathTests_BehavesPredictably(string xmlFileName, string transformFileName)
		{
			var a = new XslTransform();

			a.Load(@"Data\\XSLTFile1.xslt");

			a.Transform(@"Data\\listings.xml", @"Data\results.xml");

			/*var xslt = new XslCompiledTransform();

			xslt.Load(@"Data\\XSLTFile1.xslt", settings: default, stylesheetResolver: default);

			xslt.Transform(@"Data\\listings.xml", @"Data\results.xml");*/

			//xslt.Transform(@"Data\\listings.xml", @"Data\results.xml");

			/*static async Task<XmlDocument> ReadAsync(string fileName)
			{
				var path = Path.Combine("Data", fileName);
				var contents = await File.ReadAllTextAsync(path);
				var doc = new XmlDocument();
				doc.LoadXml(contents);
				return doc;
			}

			var doc = await ReadAsync(xmlFileName);
			var xslt = new XslCompiledTransform();

			var xslt2 = new XslTransform();

			xslt2.Load(@"Data\" + transformFileName);

			xslt2.Transform(@"Data\" + xmlFileName, @"Data\results.xml");

			var xsltSettings = new XsltSettings(enableDocumentFunction: true, enableScript: true);
			xslt.Load(await ReadAsync(transformFileName), xsltSettings, stylesheetResolver: default);

			using var writer = new StringWriter();

			using var reader = new StringReader(doc.DocumentElement.OuterXml);
			using var reader2 = new XmlTextReader(reader);

			xslt.Transform(reader2, arguments: default, writer);

			var result = writer.ToString();*/
		}
	}
}
