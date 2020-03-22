namespace PdfStationery

type ConfigValues =
    { SourcePath: string
      TemplatePath: string
      ReplaceSource: bool }
    static member Default =
        { SourcePath = ""
          TemplatePath = ""
          ReplaceSource = false }
