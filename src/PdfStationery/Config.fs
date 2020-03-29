[<AutoOpen>]
module PdfStationery.Config

open System
open System.IO
open Newtonsoft.Json

type ConfigValues =
    { SourcePath: string
      TemplatePath: string
      ReplaceSource: bool }
    static member Default =
        { SourcePath = ""
          TemplatePath = ""
          ReplaceSource = false }

let private getConfigPath() =
    let folder = Environment.GetFolderPath Environment.SpecialFolder.LocalApplicationData
    Path.Combine(folder, "config.json")

let set values =
    try
        File.WriteAllText((getConfigPath()), JsonConvert.SerializeObject values)
    with ex ->
        printfn "Could not write config! %O" ex
        
let get =
    try
        let path = getConfigPath()
        if not (File.Exists path) then ConfigValues.Default |> set
        JsonConvert.DeserializeObject<ConfigValues>(File.ReadAllText(getConfigPath()))
    with ex ->
        printfn "Could not load config! %O" ex
        ConfigValues.Default
