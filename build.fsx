#I "packages/build/Fake/tools"
#r "FakeLib.dll"

#load "common.fsx"

open Common
open Fake
open System

let dir_build_root = "./build.output/"
let dir_bin = dir_build_root @@ "bin"
let dir_tests  = dir_build_root @@ "tests"

let dir_deploy = dir_build_root @@ "deploy"

let version_info = Git.VersionInfo.FromTag "*"

MSBuildDefaults < 
  { MSBuildDefaults with 
      Verbosity = Some (Quiet)
     
  }

Target "Clean" (fun () ->
  traceHeader "Clean"
  CleanDirs [dir_build_root]
  CreateDir dir_deploy
)

Target "Compile" (fun () ->
  traceHeader "Compile"
  
  !! "iot.Radio.sln"
    |> MSBuildReleaseExt dir_bin ["Platform","ARM"] "Build"
    |> Log "Build-Output: "

)


Target "All" DoNothing

"Clean"
  ==> "Compile"
  ==> "All"

RunTargetOrDefault "All"
