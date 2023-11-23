<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <!-- verse elements -->
  <xsl:template match="verse[@sid]">
    <xsl:copy>
      <!--copy attributes (@*) and dependents of the current element-->
      <xsl:apply-templates select="@*|node()"/>
      <!--generate a unique identifier for the current node-->
      <xsl:variable name="currentNodeId" select="generate-id(current())"/>
      <!--[preceding-sibling::verse[1] = current()] ensures that only nodes after the current verse element are considered.-->
      <!--Compare the ID of the first preceding sibling verse element with the value of the currentNodeId variable to ensure that the selected nodes have the same first preceding <verse> element as the current <verse> element.-->
      <xsl:copy-of select="following-sibling::node()[preceding-sibling::verse[1] = current()][not(self::verse[@eid])][generate-id(preceding-sibling::verse[1]) = $currentNodeId]"/>
    </xsl:copy>
  </xsl:template>

  <!-- paragraph elements -->
  <xsl:template match="para[@style='p']">
    <xsl:copy>
      <!-- only copy valid child XML verse elements (self::*) -->
      <xsl:apply-templates select="@*|verse[self::*]"/>
      <!-- only copy valid child XML verse elements (self::*) that are inside or preceding a chapter element -->
      <!--<xsl:apply-templates select="@*|verse[self::* and (ancestor::chapter or following-sibling::chapter)]"/>-->
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