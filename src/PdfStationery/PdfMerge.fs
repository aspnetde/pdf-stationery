module PdfStationery.PdfMerge

open System.IO
open iTextSharp.text
open iTextSharp.text.pdf

let private merge (originalPath: string) (templatePath: string) outputPath =
    use outputStream = new FileStream(outputPath, FileMode.Create)
    let document = Document()
    let writer = PdfWriter.GetInstance(document, outputStream)

    let originalReader = PdfReader(originalPath)
    let templateReader = PdfReader(templatePath)

    document.Open()
    document.SetPageSize(originalReader.GetPageSizeWithRotation(1)) |> ignore
    document.NewPage() |> ignore

    writer.DirectContent.AddTemplate(writer.GetImportedPage(originalReader, 1), 0.0f, 0.0f)
    writer.DirectContent.AddTemplate(writer.GetImportedPage(templateReader, 1), 0.0f, 0.0f)

    document.Close()
    writer.Close()
    outputStream.Close()

let createNew (originalPath: string) (templatePath: string) =
    let tmpPath = Path.GetTempFileName()
    merge originalPath templatePath tmpPath
    tmpPath

let replace (originalPath: string) (templatePath: string) =
    let tmpPath = createNew originalPath templatePath
    File.Move(tmpPath, originalPath, true)
