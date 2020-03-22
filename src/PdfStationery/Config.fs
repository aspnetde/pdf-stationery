[<AutoOpen>]
module PdfStationery.Config

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
    Path.Combine(Directory.GetCurrentDirectory(), "config.json")

let get =
    try
        let path = getConfigPath()
        if not (File.Exists path) then File.WriteAllText(path, JsonConvert.SerializeObject ConfigValues.Default)
        JsonConvert.DeserializeObject<ConfigValues>(File.ReadAllText(getConfigPath()))
    with ex ->
        printfn "Could not load config! %O" ex
        ConfigValues.Default

let set values =
    try
        File.WriteAllText((getConfigPath()), JsonConvert.SerializeObject values)
    with ex ->
        printfn "Could not write config! %O" ex
