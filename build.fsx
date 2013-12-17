#I "tools/FAKE/tools"
#r "FakeLib.dll"

open Fake
open System
open System.Diagnostics
open System.Globalization
open System.IO
open System.Xml

// params from build server / cmd line

let buildNumber           = getBuildParamOrDefault "buildNumber" "0"
let version               = "0.1."
let releaseBuild          = (getBuildParamOrDefault "release" "build") = "release"
let assemblyVersion       = if releaseBuild
                            then version + "0"
                            else version + buildNumber
let assemblyInfoVersion   = if releaseBuild
                            then assemblyVersion
                            else assemblyVersion + "-alpha"
let rootDir               = "./" |> FullName
let sourceDir             = "./src" |> FullName
let packagesDir           = "./packages" |> FullName
let toolsDir              = "./tools" |> FullName
let buildDir              = "./build" |> FullName
let testDir               = buildDir + "/tests" |> FullName
let nugetDir              = buildDir + "/nuget" |> FullName
let nugetExe              = toolsDir + "/nuget/NuGet.exe" |> FullName
let nugetAccessKey        = getBuildParamOrDefault "nugetAccessKey" "NotSet"

Target "Clean" (fun _ ->
  CleanDirs [buildDir; testDir; nugetDir])

Target "Set version for Teamcity" (fun _ ->
  trace (sprintf "==> buildNumber %s" assemblyInfoVersion)
  SetBuildNumber (assemblyInfoVersion)
)

Target "Update AssemblyInfo" (fun _ ->
  !! (sourceDir + "/**/*AssemblyInfo.fs")
  |> Seq.iter (fun fileName ->
      ReplaceAssemblyInfoVersions (fun p ->
        { p with
            AssemblyVersion = assemblyVersion
            AssemblyFileVersion = assemblyVersion
            AssemblyInformationalVersion = assemblyInfoVersion
            AssemblyConfiguration = "Release"
            OutputFileName = fileName })
    )
)

Target "nuget.config" (fun _ ->
  let nugetConfig = rootDir + "nuget.config"
  let xml = new XmlDocument()
  xml.Load(nugetConfig)
  let node = xml.SelectSingleNode(@"configuration/config/add[@key = 'repositoryPath']")
  node.Attributes.["value"].InnerText <- packagesDir
  xml.Save(nugetConfig)
)

Target "Restore nuget packages" (fun _ ->
  let settings =
    { RestoreSinglePackageDefaults with
        ToolPath = nugetExe
        OutputPath = packagesDir
        ExcludeVersion = true
    }
  RestorePackageId (fun info -> settings) "xunit.runners"
  RestorePackages()
)

let msbuild target config outFolder proj =
  let buildProperties = [
                          "Configuration", config;
                          "OutputPath", outFolder;
                        ]
  build (fun p -> { p with
                      Targets = [target]
                      Properties = buildProperties
                      ToolsVersion = Some "4.0"
                      MaxCpuCount = Some (Some 4)
                      Verbosity = Some MSBuildVerbosity.Minimal
                  }) (sourceDir + proj |> FullName)

Target "Compile" (fun _ ->
  let proj = "/Fancy/Fancy.fsproj"
  let outDir = buildDir + "/bin"
  msbuild "Rebuild" "Release" outDir proj
)

Target "Compile tests" (fun _ ->
  let proj = "/Test.Fancy/Test.Fancy.fsproj"
  msbuild "Rebuild" "Debug" testDir proj
)

Target "Run tests" (fun _ ->
    xUnit (fun p -> {p with ToolPath = packagesDir + "/Xunit.Runners/tools/xunit.console.clr4.exe"})
          (!! (testDir + "/Test*.dll"))
)

Target "Create nuget package" (fun _ ->
  let dir = nugetDir + "/lib/net45"
  CleanDir dir
  CopyFiles dir (!! (buildDir + "/bin/Fancy*.dll"))
  NuGet (fun p ->
      {p with
          Authors = ["Morgan Persson"]
          Project = "Fancy"
          Description = "F# extension for Nancy"
          Summary = "Fancy - less noise when using Nancy."
          Version = assemblyInfoVersion
          OutputPath = nugetDir
          WorkingDir = nugetDir
          Dependencies = ["Nancy", "[0.21.1]"]
          AccessKey = nugetAccessKey
          Publish = hasBuildParam "nugetAccessKey" })
      "fancy.nuspec"
)

Target "Create nuget sample" (fun _ ->
  let dir = nugetDir + "/sample/Content"
  CleanDir dir
  CopyFiles dir (!! (sourceDir + "/nugetSample/*.*"))
  NuGet (fun p ->
      {p with
          Authors = ["Morgan Persson"]
          Project = "Fancy.Sample"
          Description = "This package contains code demonstrating the use of Fancy."
          Summary = "Fancy - less noise when using Nancy."
          Version = assemblyInfoVersion
          OutputPath = nugetDir
          WorkingDir = sprintf "%s/sample" nugetDir
          Dependencies = ["Fancy", sprintf "[%s]" assemblyInfoVersion; "Nancy.Hosting.Self", "[0.21.1]"]
          AccessKey = nugetAccessKey
          Publish = hasBuildParam "nugetAccessKey" })
      "fancy.sample.nuspec"
)

Target "Default" (fun _ -> () )

// Dependencies
"Clean"
  ==> "Set version for Teamcity"
  ==> "Update AssemblyInfo"
  ==> "nuget.config"
  ==> "Restore nuget packages"
  ==> "Compile"
  ==> "Compile tests"
  ==> "Run tests"
  ==> "Create nuget package"
  ==> "Create nuget sample"
  ==> "Default"

// Start build
Run <| getBuildParamOrDefault "target" "Default"
