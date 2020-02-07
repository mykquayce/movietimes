<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" indent="yes"/>

  <xsl:template match="cinemas">
    <Cinemas>
      <xsl:for-each select="cinema">
        <Cinema>
          <Id>
            <xsl:value-of select="@id"/>
          </Id>
          <Name>
            <xsl:value-of select="@name"/>
          </Name>
          <Films>
            <xsl:for-each select="listing/film">
              <Film>
                <Edi>
                  <xsl:value-of select="@edi"/>
                </Edi>
                <Title>
                  <xsl:value-of select="@title"/>
                </Title>
                <DateTimes>
                  <xsl:for-each select="shows/show">
                    <DateTime><xsl:value-of select="@time"/></DateTime>
                  </xsl:for-each>
                </DateTimes>
              </Film>
            </xsl:for-each>
          </Films>
        </Cinema>
      </xsl:for-each>
    </Cinemas>
  </xsl:template>
</xsl:stylesheet>
