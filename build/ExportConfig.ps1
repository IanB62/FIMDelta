# Adapted from Microsoft's ExportSchema.ps1 and ExportPolicy.ps1 scripts 
# from http://technet.microsoft.com/en-us/library/ff400275%28v=ws.10%29.aspx
#
# Rex Wheeler, Certified Security Solutions http://www.css-security.com
# Last updated 2014-05-18
# No warranty of any kind granted, use at own risk...

# Get PS Version and fix $PSScriptRoot if needed
$PsVersion3 = (Get-Host | select -ExpandProperty Version | select -ExpandProperty Major) -gt 2
if (-not $PsVersion3) {$PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent}

if (-not $PsVersion3) {Write-Warning "Not running on PowerShell 3.0 or greater. Export warnings will not be saved to a log file."}

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

if ($PsVersion3)
{
    $schema = Export-FIMConfig -schemaConfig 3> $schema_log
    if ((Get-item $schema_log).Length -gt 0)
    {
        Write-Warning "Warnings were found in the Portal Policy extract. Examine $schema_log"
    }
    else
    {
        Remove-Item $schema_log
    }
}
else
{
    $schema = Export-FIMConfig -schemaConfig
}

if ($schema -eq $null)
{
    Write-Warning "Export did not successfully retrieve configuration from FIM."
}
else
{
    Write-Host "Exported " $schema.Count " schema objects"
    if ($schema.count -eq 0)
    {
      
        Write-Warning "No schema objects exported"
    }
    $schema | ConvertFrom-FIMResource -file $schema_filename
    Write-Host "Schema config is saved as " $schema_filename
}

# Extract FIM Policy and Portal config

Write-Host "Exporting policy and portal configuration"

$policy_portal_filename = $exportDir + "\" + "policy_portal.xml"
$policy_portal_log = $exportDir + "\" + "policy_portal_warning.log"

# Note the redirect warnings "3>" only works on Powershell 3.0 and later
if ($PsVersion3)
{
    $policy_portal = Export-FIMConfig -policyConfig -portalConfig -MessageSize 9999999 3> $policy_portal_log
    if ((Get-item $policy_portal_log).Length -gt 0)
    {
        Write-Warning "Warnings were found in the Portal Policy extract. Examine $policy_portal_log"
    }
    else
    {
        Remove-Item $policy_portal_log
    }

}
else
{
    $policy_portal = Export-FIMConfig -policyConfig -portalConfig -MessageSize 9999999
}

if ($policy_portal -eq $null)
{
    Write-Warning "Export did not successfully retrieve configuration from FIM."
}
else
{
    Write-Host "Exported " $policy_portal.Count " Policy and Portal objects"
    if ($policy_portal.count -eq 0)
    {
      
        Write-Warning "No Policy or Portal objects exported"
    }
    $policy_portal | ConvertFrom-FIMResource -file $policy_portal_filename
    Write-Host "Policy and portal config is saved as " $policy_portal_filename
}


