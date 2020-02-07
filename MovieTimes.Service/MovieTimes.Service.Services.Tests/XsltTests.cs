using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace MovieTimes.Service.Services.Tests
{
	public class XsltTests
	{
		[Fact]
		public void XsltTests_BehavesPredictably()
		{
			var xslt = @"     <?xml version=""0"" encoding=""UTF-8""?>
	<xsl:stylesheet version=""0"" xmlns:xsl=""http://www.worg/1999/XSL/Transform"" xmlns:msxsl=""urn:schemas-microsoft-com:xslt"" xmlns:App=""http://www.tempuri.org/App"">
	<msxsl:script implements-prefix=""App"" language=""C#"" />
	<![CDATA[
	public string ToShortDateString(string date)
	{
	//convert date to mm/dd/yyyy format
	DateTime result;
	if (DateTime.TryParse(date, out result))
	return result.ToShortDateString();
	else
	throw new FormatException(""Not a date!"");
	}
	]]>
	</msxsl:script>
	<xsl:template match=""ArrayOfTest"">
	<TABLE>
	<xsl:for-each select=""Test"">
	<TR>
	<TD>
	<xsl:value-of select=""App:ToShortDateString(TestDate)"" />
	</TD>
	</TR>
	</xsl:for-each>
	</TABLE>
	</xsl:template>
	</xsl:stylesheet>";
		}
	}
}
