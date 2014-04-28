# Adapted from Microsoft's ExportSchema.ps1 and ExportPolicy.ps1 scripts 
# from http://technet.microsoft.com/en-us/library/ff400275%28v=ws.10%29.aspx
#
# Rex Wheeler, Certified Security Solutions http://www.css-security.com
# Last updated 2014-04-27
# No warranty of any kind granted, use at own risk...

# Load the FIM snapins

if(@(get-pssnapin | where-object {$_.Name -eq "FIMAutomation"} ).count -eq 0) {add-pssnapin FIMAutomation}

# Calculate our directories

$runDate = Get-Date -Format yyyyMMdd-HHmm
$exportDir = $PSScriptRoot + "\" + "FIMService-" + $runDate
write-output "Creating Directory $exportDir"
$eat_message = New-Item -ItemType directory -Path $exportDir -Force

# Extract FIM Schema

Write-Host "Exporting schema configuration"

$schema_filename = $exportDir + "\" + "schema.xml"
$schema_log = $exportDir + "\" + "schema_warning.log"

# Note the redirect warnings "3>" only works on Powershell 3.0 and later
$schema = Export-FIMConfig -schemaConfig 3> $schema_log
if ((Get-item $schema_log).Length -gt 0)
{
    Write-Warning "*** Warnings were found in the Portal Policy extract. Examine $schema_log"
}
else
{
    Remove-Item $schema_log
}

if ($schema -eq $null)
{
    Write-Host "Export did not successfully retrieve configuration from FIM."
}
else
{
    Write-Host "Exported " $schema.Count " schema objects"
    if ($schema.count -eq 0)
    {
      
        Write-Host "***WARNING*** no schema objects exported"
    }
    $schema | ConvertFrom-FIMResource -file $schema_filename
    Write-Host "Schema config is saved as " $schema_filename
}

# Extract FIM Policy and Portal config

Write-Host "Exporting policy and portal configuration"

$policy_portal_filename = $exportDir + "\" + "policy_portal.xml"
$policy_portal_log = $exportDir + "\" + "policy_portal_warning.log"

# Note the redirect warnings "3>" only works on Powershell 3.0 and later
$policy_portal = Export-FIMConfig -policyConfig -portalConfig -MessageSize 9999999 3> $policy_portal_log
if ((Get-item $policy_portal_log).Length -gt 0)
{
    Write-Warning "*** Warnings were found in the Portal Policy extract. Examine $policy_portal_log"
}
else
{
    Remove-Item $policy_portal_log
}


if ($policy_portal -eq $null)
{
    Write-Host "Export did not successfully retrieve configuration from FIM."
}
else
{
    Write-Host "Exported " $policy_portal.Count " Policy and Portal objects"
    if ($policy_portal.count -eq 0)
    {
      
        Write-Host "***WARNING*** no Policy or Portal objects exported"
    }
    $policy_portal | ConvertFrom-FIMResource -file $policy_portal_filename
    Write-Host "Policy and portal config is saved as " $policy_portal_filename
}


