# Adapted from Microsoft's SyncSchema.ps1 and SyncPolicy.ps1 scripts 
# from http://technet.microsoft.com/en-us/library/ff400275%28v=ws.10%29.aspx
#
# Rex Wheeler, Certified Security Solutions http://www.css-security.com
# Last updated 2014-05-18
# No warranty of any kind granted, use at own risk...

# SourceDirectory contains the files from ExportConfig that came from the environment or configuration you are migrating FROM
# TargetDirectory contains the files from ExportConfig that came from the environment of configuration you are migrating TO
# Typically SourceDirectory would be your Development environment and TargetDirectory would be your Production environment

param(
    [string]$SourceDirectory = $(throw "-SourceDirectory is required"),
    [string]$TargetDirectory = $(throw "-TargetDirectory is required")
)


function perform_diff([string]$SourceDirectory_filename, [string]$TargetDirectory_filename, [string]$diff_filename, [string]$environment)
{
    Write-Output "Diff for $environment"

    $joinrules = @{
        # === Schema configuration ===
        # This is based on the system names of attributes and objects
        # Notice that BindingDescription is joined using its reference attributes.
        ObjectTypeDescription = "Name";
        AttributeTypeDescription = "Name";
        BindingDescription = "BoundObjectType BoundAttributeType";

        # === Customer-dependent join rules ===
        # Person and Group objects are not configuration will not be migrated.
        # However, some configuration objects like Sets may refer to these objects.
        # For this reason, we need to know how to join Person objects between
        # systems so that configuration objects have the same semantic meaning.

        # NOTE that the Person object may need something other than DisplayName to properly align
        #      between environments. MailNickname or AccountName can be good candidates
        #      Person = "MailNickname DisplayName";
        #      Person = "AccountName DisplayName";
        Person = "DisplayName";
        Group = "DisplayName";
    
        # === Policy configuration ===
        # Sets, MPRs, Workflow Definitions, and so on. are best identified by DisplayName
        # DisplayName is set as the default join criteria and applied to all object
        # types not listed here.
    
        # === Portal configuration ===
        ConstantSpecifier = "BoundObjectType BoundAttributeType ConstantValueKey";
        SearchScopeConfiguration = "DisplayName SearchScopeResultObjectType Order";
        ObjectVisualizationConfiguration = "DisplayName AppliesToCreate AppliesToEdit AppliesToView"
    }

    Write-Host "Loading source $environment : $SourceDirectory_filename" 

    $SourceDirectory_objects = ConvertTo-FIMResource -file $SourceDirectory_filename
    if($SourceDirectory_objects -eq $null)
    {
        throw (new-object NullReferenceException -ArgumentList "Source $environment is null")
    }

    Write-Host "Loaded " $SourceDirectory_objects.Count " objects."


    Write-Host "Loading target $environment : $TargetDirectory_filename" 

    $TargetDirectory_objects = ConvertTo-FIMResource -file $TargetDirectory_filename
    if($TargetDirectory_objects -eq $null)
    {
        throw (new-object NullReferenceException -ArgumentList "Target $environment is null")
    }

    Write-Host "Loaded " $TargetDirectory_objects.Count " objects."


    Write-Host
    Write-Host "Joining $environment objects"
    Write-Host
     
    $matches = Join-FIMConfig -source $SourceDirectory_objects -target $TargetDirectory_objects -join $joinrules -defaultJoin DisplayName
    if($matches -eq $null)
    {
        throw (new-object NullReferenceException -ArgumentList "No joins found.  Check that the join succeeded and join criteria is correct for your environment.")
    }

    Write-Host "Comparing $environment objects"
    $changes = $matches | Compare-FIMConfig
    if($changes -eq $null)
    {
        Write-Warning "Found no changes in $environment"
    }
    else
    {
        Write-Host "Identified " $changes.Count " changes."
        Write-Host "Saving changes to " $diff_filename

        $changes | ConvertFrom-FIMResource -file $diff_filename
    }

}

# Get PS Version and fix $PSScriptRoot if needed
$PsVersion3 = (Get-Host | select -ExpandProperty Version | select -ExpandProperty Major) -gt 2
if (-not $PsVersion3) {$PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent}

if(@(get-pssnapin | where-object {$_.Name -eq "FIMAutomation"} ).count -eq 0) {add-pssnapin FIMAutomation}

if ((Test-Path $SourceDirectory -PathType Container) -eq 0) {throw "Source directory does not exist"}
if ((Test-Path $TargetDirectory -PathType Container) -eq 0) {throw "Target directory does not exist"}

$SourceDirectory = Resolve-Path -LiteralPath $SourceDirectory
$TargetDirectory = Resolve-Path -LiteralPath $TargetDirectory

$SourceDirectory_leaf = Split-Path $SourceDirectory -Leaf
$TargetDirectory_leaf = Split-Path $TargetDirectory -Leaf

$diffDir = $PSScriptRoot + "\" + "Diff-" + $SourceDirectory_leaf + "-" + $TargetDirectory_leaf
write-output "Creating Directory $diffDir"
$eat_message = New-Item -ItemType directory -Path $diffDir -Force

$SourceDirectory_schema_filename = $SourceDirectory + "\" + "schema.xml"
$TargetDirectory_schema_filename = $TargetDirectory + "\" + "schema.xml"
$schema_changes_filename = $diffDir + "\" + "schema_changes.xml"

perform_diff $SourceDirectory_schema_filename $TargetDirectory_schema_filename $schema_changes_filename "Schema"

Write-Output ""
Write-Output ""

$SourceDirectory_policy_filename = $SourceDirectory + "\" + "policy_portal.xml"
$TargetDirectory_policy_filename = $TargetDirectory + "\" + "policy_portal.xml"
$schema_policy_filename = $diffDir + "\" + "policy_portal_changes.xml"

perform_diff $SourceDirectory_policy_filename $TargetDirectory_policy_filename $schema_policy_filename "Policy and Portal"

