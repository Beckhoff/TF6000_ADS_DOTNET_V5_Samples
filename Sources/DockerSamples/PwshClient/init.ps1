# Print out the Environment-Variables (IConfiguation settings)
#Get-ChildItem env:

# Only for debugging purposes (when the TcXaeMgmt is provided as COPY instruction in dockerfile)
$debugModuleExist = Test-Path -path .\TcXaeMgmt
if ($debugModuleExist)
{
    import-module -name .\TcXaeMgmt\TcXaeMgmt.psm1
    update-FormatData -AppendPath .\TcXaeMgmt\TcXaeMgmt.format.ps1xml
}

get-module TcXaeMgmt

# Wait, the router/broker needs time to start ...
Start-Sleep -Seconds 1

# Show the AmsRouter Endpoint (the Loopback settings)
Write-Host 'LocalEndpoint:'
Get-AmsRouterEndpoint
# Show the Local AmsNetId
Write-Host 'Local AmsNetId:'
Get-AmsNetId

Write-Host '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++'
Write-Host 'Attach to the powershell console within this docker instance with:'
Write-Host 'PS> docker exec -it [containerID] pwsh'
Write-Host '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++'

#Connect to the AdsTestServer on Port 25000
$s = New-TcSession -port 25000
Write-Host 'Connection established to:'
Write-Host $s

# Testing connections for 10000 Seconds
#$s | test-adsRoute -port 25000 -count 10000
while($true)
{
    $state = get-AdsState -session $s -StateOnly
    Write-Host "[PowershellClient] State of Server '$($s.NetId):$($s.Port)' is: $state"
    Start-Sleep -Seconds 1
}
