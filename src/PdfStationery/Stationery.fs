namespace PdfStationery

open System
open Elmish
open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.FuncUI.Components.Hosts
open Avalonia.FuncUI
open Avalonia.FuncUI.Elmish
open MessageBox.Avalonia.DTO
open MessageBox.Avalonia.Enums
open MessageBox.Avalonia.Models

module Stationery =
    type Model =
        { ReplaceOriginal: bool
          StationeryPdfPath: string
          OriginalPdfPath: string }

    type Msg =
        | ToggleReplaceOriginal
        | OnStationaryPdfPathChanged of path: string
        | OnOriginalPdfPathChanged of path: string
        | OnExceptionThrown of ex: Exception
        | OnOriginalPdfReplaced
        | CmdPrint
        | CmdOpenNewPdfDocument of path: string
        | CmdOpenStationeryFileDialog
        | CmdOpenOriginalFileDialog
        | NoOp

    type Program(parent: HostWindow) =

        let init() =
            { ReplaceOriginal = false
              StationeryPdfPath = ""
              OriginalPdfPath = "" }, Cmd.none

        let canPrint model =
            not (String.IsNullOrWhiteSpace(model.StationeryPdfPath))
            && not (String.IsNullOrWhiteSpace(model.OriginalPdfPath))

        let openFileDialog currentPath =
            async {
                let filter = FileDialogFilter()
                filter.Extensions.Add("pdf")

                let dialog = OpenFileDialog()
                dialog.AllowMultiple <- false
                dialog.InitialFileName <- currentPath
                dialog.Filters.Add(filter)

                let! result = dialog.ShowAsync(parent) |> Async.AwaitTask
                return match result with
                       | [||] -> currentPath
                       | [| newPath |] -> newPath
                       | _ -> failwith "Please select only a single PDF document."
            }


        let showMessage title header message icon =
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
            showMessage "Error" "Oops, something went wrong:" ex.Message Icon.Error

        let showSuccess message =
            showMessage "Success" "Great!" message Icon.Success

        let print model =
            if model.ReplaceOriginal then
                OnOriginalPdfReplaced
            else
                CmdOpenNewPdfDocument model.StationeryPdfPath

        let openFile (path: string) =
            System.Diagnostics.Process.Start(path) |> ignore
            NoOp
        
        let update msg model =
            match msg with
            | ToggleReplaceOriginal -> { model with ReplaceOriginal = not model.ReplaceOriginal }, Cmd.none
            | OnStationaryPdfPathChanged path -> { model with StationeryPdfPath = path }, Cmd.none
            | OnOriginalPdfPathChanged path -> { model with OriginalPdfPath = path }, Cmd.none
            | CmdPrint -> model, Cmd.ofMsg (print model)
            | OnOriginalPdfReplaced ->
                model,
                Cmd.OfAsync.either showSuccess
                    "Your original PDF file has been decorated with your stationery!" (fun _ -> NoOp)
                    (fun _ -> NoOp)
            | CmdOpenStationeryFileDialog ->
                model,
                Cmd.OfAsync.either openFileDialog model.StationeryPdfPath OnStationaryPdfPathChanged OnExceptionThrown
            | CmdOpenOriginalFileDialog ->
                model,
                Cmd.OfAsync.either openFileDialog model.OriginalPdfPath OnOriginalPdfPathChanged OnExceptionThrown
            | CmdOpenNewPdfDocument path -> model, Cmd.ofMsg (openFile path)
            | OnExceptionThrown ex -> model, Cmd.OfAsync.either showError ex (fun _ -> NoOp) (fun _ -> NoOp)
            | NoOp -> model, Cmd.none

        let view model dispatch =
            DockPanel.create
                [ DockPanel.children
                    [ StackPanel.create
                        [ StackPanel.dock Dock.Bottom
                          StackPanel.margin 15.0
                          StackPanel.spacing 5.0
                          StackPanel.children
                              [ TextBlock.create [ TextBlock.text "Stationery PDF" ]
                                StackPanel.create
                                    [ StackPanel.orientation Orientation.Horizontal
                                      StackPanel.width 390.0
                                      StackPanel.spacing 5.0
                                      StackPanel.children
                                          [ TextBox.create
                                              [ TextBox.width 285.0
                                                TextBox.text model.StationeryPdfPath
                                                TextBox.isEnabled false ]
                                            Button.create
                                                [ Button.content "Select"
                                                  Button.width 100.0
                                                  Button.onClick (fun _ -> dispatch CmdOpenStationeryFileDialog) ] ] ]
                                Separator.create [ Separator.height 5.0 ]

                                TextBlock.create [ TextBlock.text "Original PDF" ]
                                StackPanel.create
                                    [ StackPanel.orientation Orientation.Horizontal
                                      StackPanel.width 390.0
                                      StackPanel.spacing 5.0
                                      StackPanel.children
                                          [ TextBox.create
                                              [ TextBox.width 285.0
                                                TextBox.text model.OriginalPdfPath
                                                TextBox.isEnabled false ]
                                            Button.create
                                                [ Button.content "Select"
                                                  Button.width 100.0
                                                  Button.onClick (fun _ -> dispatch CmdOpenOriginalFileDialog) ] ] ]

                                Separator.create [ Separator.height 5.0 ]

                                StackPanel.create
                                    [ StackPanel.orientation Orientation.Horizontal
                                      StackPanel.width 390.0
                                      StackPanel.spacing 10.0
                                      StackPanel.children
                                          [ CheckBox.create
                                              [ CheckBox.isChecked model.ReplaceOriginal
                                                CheckBox.onTapped (fun _ -> dispatch ToggleReplaceOriginal) ]
                                            TextBlock.create
                                                [ TextBlock.text "Replace Original PDF"
                                                  TextBlock.onTapped (fun _ -> dispatch ToggleReplaceOriginal)
                                                  TextBlock.verticalAlignment VerticalAlignment.Center ] ] ]

                                Separator.create [ Separator.height 5.0 ]

                                Button.create
                                    [ Button.content "Print"
                                      Button.onClick (fun _ -> dispatch CmdPrint)
                                      Button.isEnabled (model |> canPrint) ] ] ] ] ]

        do
            Elmish.Program.mkProgram init update view
            |> Program.withHost parent
            |> Program.withConsoleTrace
            |> Program.run
