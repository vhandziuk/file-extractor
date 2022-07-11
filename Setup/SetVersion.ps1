function Set-Version {
    param (
        [string]$ReleaseTag
    )
    
    $prevPwd = $PWD
    Set-Location -ErrorAction Stop -LiteralPath $PSScriptRoot

    try {
        $isMatchSuccess = $ReleaseTag -Match '[vV](?<A>\d+)\.(?<B>\d+)(\.)?(?<C>\d+)?(\.)?(?<D>\d+)?'
        $versionString = Join-String -InputObject @($Matches.A, $Matches.B, $Matches.C, $Matches.D) -Separator '.'

        $major = $Matches.A
        $minor = $Matches.B
        $revision = If ($Matches.C) { $Matches.C } Else { '0' }
        $build = If ($Matches.D) { $Matches.D } Else { '0' }

        $fullVersionString = Join-String -InputObject @($major, $minor, $revision, $build) -Separator '.'

        (Get-Content .\Setup_x86.iss) -Replace '1.0.0.0', $versionString | Set-Content .\Setup_x86.iss
        (Get-Content .\Setup_x64.iss) -Replace '1.0.0.0', $versionString | Set-Content .\Setup_x64.iss
        
        (Get-Content .\..\GlobalAssemblyInfo.cs) -Replace '1.0.0.0', $fullVersionString | Set-Content .\..\GlobalAssemblyInfo.cs
    }
    finally {
      $prevPwd | Set-Location
    }   
}