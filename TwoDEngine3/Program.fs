module TwoDEngine3
open System
open System.IO
open System.Reflection
open ManagerRegistry

type SysColor = System.Drawing.Color

(* ECONOMIZER modification BEGIN *)
open System.Runtime.InteropServices
[<DllImport("winmm.dll", SetLastError=true)>]
extern int timeBeginPeriod(int period)
extern int timeEndPeriod(int period)

let setTimeBeginPeriod (period: int) =
    let result = timeBeginPeriod period
    if result <> 0 then
        failwithf "Failed to set time period. Error code: %d" (Marshal.GetLastWin32Error())
let setTimeEndPeriod (period: int) =
    let result = timeEndPeriod period
    if result <> 0 then
        failwithf "Failed to end time period. Error code: %d" (Marshal.GetLastWin32Error())
(* ECONOMIZER modification END *)

(*   Asteriods test program by JPK *)


let rec RecursiveGetAssemblies (path:string option) =
    let mypath =
                 match path with
                 | Some s -> s
                 | None -> Environment.CurrentDirectory
    let dinfo = DirectoryInfo(mypath)
    let assemblies =
        dinfo.EnumerateFiles("*.dll")
        |> Seq.map (fun fileInfo -> Assembly.LoadFile fileInfo.FullName)
    dinfo.GetDirectories()
    |> Seq.fold (
            fun state dinfo ->
                Seq.concat [RecursiveGetAssemblies (Some dinfo.FullName);assemblies] 
        ) assemblies
    
                 



[<EntryPoint>]
[<STAThread>]
let main argv =
    //tempoary: set up the managers
    // To be replaced with runtime dynmaic loading
    
    RecursiveGetAssemblies None
    |> Seq.iter (fun assembly ->
        assembly.GetTypes()
        |> Array.filter (fun t ->
                CustomAttributeExtensions.IsDefined(t,typeof<Manager>)
            )
        |> Array.iter (fun t -> ManagerRegistry.addManager(t))
     )
    
    timeBeginPeriod 1 |> ignore // ECONOMIZER modification for millisecond accurate suspend for power saving
    Asteroids.Start()
    timeEndPeriod 1 |> ignore // ECONOMIZER modification to return multimedia timer to default granularity
    0 // return an integer exit code
    
   
