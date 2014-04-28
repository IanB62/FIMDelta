# Adapted from Microsoft's ExportSchema.ps1 and ExportPolicy.ps1 scripts 
# from http://technet.microsoft.com/en-us/library/ff400275%28v=ws.10%29.aspx
#
# Rex Wheeler, Certified Security Solutions http://www.css-security.com
# Last updated 2014-04-27
# No warranty of any kind granted, use at own risk...

param(
[string]$SourceDirectory = $(throw "-SourceDirectory is required"),
[string]$TargetDirectory = $(throw "-TargetDirectory is required"),
[string]$Category = $(throw "-category is required (typically 'schema' or 'policy_portal'")
)

if ((Test-Path $SourceDirectory -PathType Container) -eq 0) {throw "Source directory does not exist"}
if ((Test-Path $TargetDirectory -PathType Container) -eq 0) {throw "Target directory does not exist"}

$SourceDirectory = Resolve-Path -LiteralPath $SourceDirectory
$TargetDirectory = Resolve-Path -LiteralPath $TargetDirectory

$SourceDirectory_leaf = Split-Path $SourceDirectory -Leaf
$TargetDirectory_leaf = Split-Path $TargetDirectory -Leaf

$diffDir = $PSScriptRoot + "\" + "Diff-" + $SourceDirectory_leaf + "-" + $TargetDirectory_leaf

$SourceDirectory_extract_filename = $SourceDirectory + "\" + $Category + ".xml"
$TargetDirectory_extract_filename = $TargetDirectory + "\" + $Category + ".xml"
$extract_changes_filename = $diffDir + "\" + $Category + "_changes.xml"
$extract_changes_filtered_filename = $diffDir + "\" + $Category + "_changes_filtered.xml"

$deltapath = $PSScriptRoot + "\FIMDelta.exe"

$dummy = [System.Diagnostics.Process]::Start($deltapath, "`"$SourceDirectory_extract_filename`" `"$TargetDirectory_extract_filename`" `"$extract_changes_filename`" `"$extract_changes_filtered_filename`"")
