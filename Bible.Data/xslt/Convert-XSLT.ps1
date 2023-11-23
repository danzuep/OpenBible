# Usage: `. [System.IO.Path]::Combine($Env:ScriptsFolderPath, "Convert-XSLT.ps1"); Convert-XSLT "input.xml" "output.xml" "usx2xslt.xslt";`

Function Convert-XSLT([string]$inputFile, [string]$outputFile, [string]$xsltFile)
{
    # $xml = New-Object System.Xml.XmlTextReader($inputstream)
    # $arglist = New-Object System.Xml.Xsl.XsltArgumentList
    # $output = New-Object System.IO.MemoryStream
    # $reader = New-Object System.IO.StreamReader($output)

    $xslt = New-Object System.Xml.Xsl.XslCompiledTransform
    $xslt.Load($xsltFile)
    $xslt.Transform($inputFile, $outputFile)
}
