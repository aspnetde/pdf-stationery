module PdfStationery.Printer

open System.IO
open iTextSharp.text
open iTextSharp.text.pdf

let private print (sourcePath: string) (templatePath: string) outputPath =
    use outputStream = new FileStream(outputPath, FileMode.Create)
    let document = Document()
    let writer = PdfWriter.GetInstance(document, outputStream)

    let sourceReader = PdfReader(sourcePath)
    let templateReader = PdfReader(templatePath)
    
    document.Open()
    document.SetPageSize(sourceReader.GetPageSizeWithRotation(1)) |> ignore
        
    for i = 1 to sourceReader.NumberOfPages do
        document.NewPage() |> ignore
        writer.DirectContent.AddTemplate(writer.GetImportedPage(sourceReader, i), 0.0f, 0.0f)
        writer.DirectContent.AddTemplate(writer.GetImportedPage(templateReader, 1), 0.0f, 0.0f)

    sourceReader.Close()
    templateReader.Close()
    document.Close()
    writer.Close()
    outputStream.Close()

let newPdf (sourcePath: string) (templatePath: string) =
    let tmpPath = Path.GetTempFileName()
    print sourcePath templatePath tmpPath
    tmpPath

let replacePdf (sourcePath: string) (templatePath: string) =
    let tmpPath = newPdf sourcePath templatePath
    File.Move(tmpPath, sourcePath, true)
