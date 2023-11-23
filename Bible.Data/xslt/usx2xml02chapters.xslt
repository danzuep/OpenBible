<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <!-- root usx element -->
  <!--<xsl:template match="usx">
    <xsl:copy>
      --><!-- only copy attributes (@*) and nodes descended from or preceding the first chapter element --><!--
      <xsl:apply-templates select="@*|node()[ancestor::chapter or following-sibling::chapter[1]]"/>
    </xsl:copy>
  </xsl:template>-->

  <!-- paragraph elements -->
  <xsl:template match="para">
    <!--only copy attributes and nodes that come before the first chapter element-->
    <xsl:if test="count(preceding-sibling::chapter) = 0">
      <xsl:copy>
        <xsl:apply-templates select="@*|node()"/>
      </xsl:copy>
    </xsl:if>
  </xsl:template>
  
  <!-- chapter elements -->
  <xsl:template match="chapter[@sid]">
    <xsl:copy>
      <!--copy attributes (@*) and dependents of the current element-->
      <xsl:apply-templates select="@*|node()"/>
      <!--generate a unique identifier for the current node-->
      <xsl:variable name="currentChapterNodeId" select="generate-id(current())"/>
      <!--copy all the following nodes and add them as dependents-->
      <xsl:copy-of select="following-sibling::node()[preceding-sibling::chapter[1] = current()][not(self::chapter[@eid])][generate-id(preceding-sibling::chapter[1]) = $currentChapterNodeId]"/>
    </xsl:copy>
  </xsl:template>

  <!-- skip transforming these -->
  <xsl:template match="chapter[@eid]|verse[@eid]|para[@style='ide']|para[@style='rem']"></xsl:template>

  <!-- everything else -->
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

</xsl:stylesheet>