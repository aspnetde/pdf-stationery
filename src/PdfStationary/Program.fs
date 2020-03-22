// Learn more about F# at http://fsharp.org

open System
open System.IO
open iTextSharp
open iTextSharp.text
open iTextSharp.text.pdf

[<EntryPoint>]
let main argv =
    let sourcePath = "/home/thomas/Downloads/invoice.pdf"
    let templatePath = "/home/thomas/Downloads/stationary.pdf"
    
    let outputPath = sprintf "/home/thomas/Downloads/tmp-%i.pdf" Environment.TickCount
    use outputStream = new FileStream(outputPath, FileMode.Create)
    let document = Document()
    let writer = PdfWriter.GetInstance(document, outputStream)
    document.Open()
    
    let source = PdfReader(sourcePath)
    let template = PdfReader(templatePath)
    
    document.SetPageSize(source.GetPageSizeWithRotation(1)) |> ignore
    document.NewPage() |> ignore
    
    let page1 = writer.GetImportedPage(source, 1)
    writer.DirectContent.AddTemplate(page1, 0.0f, 0.0f)
    
    let page2 = writer.GetImportedPage(template, 1)
    writer.DirectContent.AddTemplate(page2, 0.0f, 0.0f)
    
    document.Close()
    writer.Close()
    outputStream.Close()
    
    
    printfn "Hello World from F#!"
    0 // return an integer exit code
