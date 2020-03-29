[<AutoOpen>]
module PdfStationery.Dialogs

open Avalonia.Controls
open System
open System.IO
open MessageBox.Avalonia.DTO
open MessageBox.Avalonia.Enums
open MessageBox.Avalonia.Models

let private showMessage title header message icon =
    async {
        let closeButton = ButtonDefinition()
        closeButton.Type <- ButtonType.Default
        closeButton.Name <- "OK"

        let windowParameters = MessageBoxCustomParams()
        windowParameters.ShowInCenter <- true
        windowParameters.CanResize <- true
        windowParameters.ContentTitle <- title
        windowParameters.ContentHeader <- header
        windowParameters.ContentMessage <- message
        windowParameters.Icon <- icon
        windowParameters.ButtonDefinitions <- [ closeButton ]

        let window =
            MessageBox.Avalonia.MessageBoxManager.GetMessageBoxCustomWindow(windowParameters)

        window.Show()
        |> Async.AwaitIAsyncResult
        |> Async.Ignore
        |> ignore
    }

let showError (ex: Exception) =
    showMessage (Error |> translate) (ErrorMessage |> translate) ex.Message Icon.Error

let showSuccess message =
    showMessage (Success |> translate) (SuccessMessage |> translate) message Icon.Success

let openFileDialog args =
    let currentPath, parent = args
    async {
        let filter = FileDialogFilter()
        filter.Name <- PdfDocumentFilter |> translate
        filter.Extensions.Add("pdf")

        let dialog = OpenFileDialog()
        dialog.AllowMultiple <- false
        dialog.InitialFileName <- currentPath
        dialog.Filters.Add(filter)

        let! result = dialog.ShowAsync(parent) |> Async.AwaitTask
        return match result with
               | [||] -> currentPath
               | [| newPath |] -> newPath
               | _ -> failwith "This should never have happened. Please select only a single PDF document."
    }

type SaveDialogArguments =
    { SourcePath: string
      TmpPath: string }

type SaveDialogResult =
    { TargetPath: string
      TmpPath: string }

let openSaveDialog args =
    let args, parent = args
    async {
        let initialFileName = Path.GetFileName args.SourcePath
        let initialFileName =
            if initialFileName.ToLowerInvariant().EndsWith ".pdf" then
                initialFileName
            else
                initialFileName + ".pdf"
                
        let dialog = SaveFileDialog()
        dialog.InitialFileName <- initialFileName
        dialog.Directory <- Path.GetDirectoryName args.SourcePath
        dialog.DefaultExtension <- "pdf"

        let! result = dialog.ShowAsync(parent) |> Async.AwaitTask
        return match result with
               | null -> None
               | path ->
                   let path =
                       if path.ToLowerInvariant().EndsWith ".pdf" then path
                       else path + ".pdf"
                   Some
                       ({ TargetPath = path
                          TmpPath = args.TmpPath })
    }
