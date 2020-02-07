<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
								xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								xmlns:msxsl="urn:schemas-microsoft-com:xslt"
								xmlns:App="http://www.tempuri.org/App">
	<msxsl:script implements-prefix="App" language="CSharp">
	<![CDATA[
						public string ToShortDateString(string date)
						{
								//convert date to mm/dd/yyyy format
								DateTime result;
								if (DateTime.TryParse(date, out result))
										return result.ToShortDateString();
								else
										throw new FormatException("Not a date!");
						}
				]]>
	</msxsl:script>
	<xsl:template match="ArrayOfTest">
		<TABLE>
			<xsl:for-each select="Test">
				<TR>
					<TD>
						<xsl:value-of select="App:ToShortDateString(TestDate)" />
					</TD>
				</TR>
			</xsl:for-each>
		</TABLE>
	</xsl:template>
</xsl:stylesheet>
