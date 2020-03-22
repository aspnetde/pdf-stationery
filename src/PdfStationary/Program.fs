// Learn more about F# at http://fsharp.org

open System
open System.IO
open iTextSharp
open iTextSharp.text
open iTextSharp.text.pdf

[<EntryPoint>]
let main argv =
    let sourcePath = "/Users/thomas/Downloads/source.pdf"
    let templatePath = "/Users/thomas/Downloads/template.pdf"
    let outputPath = sprintf "/Users/thomas/Downloads/tmp-%i.pdf" Environment.TickCount
    
    use outputStream = new FileStream(outputPath, FileMode.Create)
    let document = Document()
    let writer = PdfWriter.GetInstance(document, outputStream)
    
    let sourceReader = PdfReader(sourcePath)
    let templateReader = PdfReader(templatePath)
    
    document.Open()
    document.SetPageSize(sourceReader.GetPageSizeWithRotation(1)) |> ignore
    document.NewPage() |> ignore
    
    writer.DirectContent.AddTemplate(writer.GetImportedPage(sourceReader, 1), 0.0f, 0.0f)
    writer.DirectContent.AddTemplate(writer.GetImportedPage(templateReader, 1), 0.0f, 0.0f)
    
    document.Close()
    writer.Close()
    outputStream.Close()
    
    printfn "PDF successfully created!"
    0
