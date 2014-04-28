# Adapted from Microsoft's CommitChanges.ps1 and ResumeUndoneImports.ps1 scripts 
# from http://technet.microsoft.com/en-us/library/ff400275%28v=ws.10%29.aspx
#
# Rex Wheeler, Certified Security Solutions http://www.css-security.com
# Last updated 2014-04-27
# No warranty of any kind granted, use at own risk...

param(
[string]$changes = $(throw "-changes is required")
)

if(@(get-pssnapin | where-object {$_.Name -eq "FIMAutomation"} ).count -eq 0) {add-pssnapin FIMAutomation}

if ((Test-Path $changes -PathType Leaf) -eq 0) {throw "Changes file does not exist"}

$runDate = Get-Date -Format yyyyMMdd-HHmmss

$undone_prefix = "_UNDONE_"

$changes_filename = Resolve-Path -LiteralPath $changes

$changes_leaf = Split-Path $changes_filename -Leaf
$changes_container = Split-Path $changes_filename -Parent

$undone_index = $changes_leaf.IndexOf($undone_prefix)

if ($undone_index -eq -1)
{
    $undone_filename = $changes_container + "\" + $changes_leaf.Insert($changes_leaf.LastIndexOf("."), $undone_prefix + $runDate)
}
else
{
    $undone_filename = $changes_container + "\" + $changes_leaf.Replace($changes_leaf.Substring($undone_index, ($changes_leaf.LastIndexOf(".") - $undone_index)), $undone_prefix + $runDate)
}


$imports = ConvertTo-FIMResource -file $changes_filename

if ($imports -eq $null)
  {
    throw (new-object NullReferenceException -ArgumentList "Changes is null.  Check that the changes file has data.")
  }

Write-Host "Importing changes"

$undoneImports = $imports | Import-FIMConfig

$changes_applied = $imports.count - $undoneImports.count

if ($undoneImports -eq $null)
  {
    Write-Host "Import complete. Applied $changes_applied changes."
  }
else
  {
    Write-Host "There were " $undoneImports.Count " uncompleted imports."
    Write-Host "Successfully applied $changes_applied changes."
    Write-Host "Saving undone changes to: $undone_filename"
    $undoneImports | ConvertFrom-FIMResource -file $undone_filename
  }