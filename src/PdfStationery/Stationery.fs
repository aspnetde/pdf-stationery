namespace PdfStationery

open System
open System.IO
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
    type State =
        { SourcePath: string
          TemplatePath: string
          ReplaceSource: bool }

    type SaveResult =
        { OriginalPath: string
          TmpPath: string }

    type Foo =
        { TargetPath: string
          TmpPath: string }

    type Msg =
        | ToggleReplaceSource
        | OnStationaryPdfPathChanged of path: string
        | OnSourcePathChanged of path: string
        | OnExceptionThrown of ex: Exception
        | CmdUpdateConfig
        | Success of message: string
        | CmdPrint
        | CmdSaveNewPdfDocument of path: string
        | CmdOpenStationeryFileDialog
        | CmdOpenOriginalFileDialog
        | NoOp

    type Program(parent: HostWindow) =

        let init() =
            let config = Config.get
            { ReplaceSource = config.ReplaceSource
              TemplatePath = config.TemplatePath
              SourcePath = config.SourcePath }, Cmd.none

        let canPrint state =
            not (String.IsNullOrWhiteSpace(state.TemplatePath))
            && not (String.IsNullOrWhiteSpace(state.SourcePath))

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


        let openSaveDialog arguments =
            async {
                let dialog = SaveFileDialog()
                dialog.InitialFileName <- Path.GetFileName arguments.OriginalPath
                dialog.Directory <- Path.GetDirectoryName arguments.OriginalPath
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
                                  TmpPath = arguments.TmpPath })
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


        let updateConfig state =
            Config.set
                { SourcePath = state.SourcePath
                  TemplatePath = state.TemplatePath
                  ReplaceSource = state.ReplaceSource }
            NoOp

        let showError (ex: Exception) =
            showMessage "Error" "Oops, something went wrong:" ex.Message Icon.Error

        let showSuccess message =
            showMessage "Success" "Great!" message Icon.Success

        let print state =
            if state.ReplaceSource then
                PdfMerge.replace state.SourcePath state.TemplatePath
                Success "Your original PDF file has been decorated with your stationery."
            else
                let tmpPath = PdfMerge.createNew state.SourcePath state.TemplatePath
                CmdSaveNewPdfDocument tmpPath

        let savePdf (saveResult: Foo option) =
            match saveResult with
            | None -> NoOp
            | Some result ->
                printfn "Saving: %s" result.TargetPath
                File.Move(result.TmpPath, result.TargetPath, true)
                Success
                    (sprintf "Your PDF has been decorated with your stationery and saved as `%s`."
                         (Path.GetFileName(result.TargetPath)))

        let update msg state =
            match msg with
            | ToggleReplaceSource -> { state with ReplaceSource = not state.ReplaceSource }, Cmd.ofMsg CmdUpdateConfig
            | OnStationaryPdfPathChanged path -> { state with TemplatePath = path }, Cmd.ofMsg CmdUpdateConfig
            | OnSourcePathChanged path -> { state with SourcePath = path }, Cmd.ofMsg CmdUpdateConfig
            | CmdPrint -> state, Cmd.ofMsg (print state)
            | CmdUpdateConfig -> state, Cmd.ofMsg (updateConfig state)
            | Success message ->
                state, Cmd.OfAsync.either showSuccess message (fun _ -> NoOp) (fun _ -> NoOp)
            | CmdOpenStationeryFileDialog ->
                state,
                Cmd.OfAsync.either openFileDialog state.TemplatePath OnStationaryPdfPathChanged OnExceptionThrown
            | CmdOpenOriginalFileDialog ->
                state,
                Cmd.OfAsync.either openFileDialog state.SourcePath OnSourcePathChanged OnExceptionThrown
            | CmdSaveNewPdfDocument path ->
                state,
                Cmd.OfAsync.either openSaveDialog
                    { OriginalPath = state.SourcePath
                      TmpPath = path } savePdf OnExceptionThrown
            | OnExceptionThrown ex -> state, Cmd.OfAsync.either showError ex (fun _ -> NoOp) (fun _ -> NoOp)
            | NoOp -> state, Cmd.none

        let view state dispatch =
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
                                                TextBox.text state.TemplatePath
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
                                                TextBox.text state.SourcePath
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
                                              [ CheckBox.isChecked state.ReplaceSource
                                                CheckBox.onTapped (fun _ -> dispatch ToggleReplaceSource) ]
                                            TextBlock.create
                                                [ TextBlock.text "Replace Original PDF"
                                                  TextBlock.onTapped (fun _ -> dispatch ToggleReplaceSource)
                                                  TextBlock.verticalAlignment VerticalAlignment.Center ] ] ]

                                Separator.create [ Separator.height 5.0 ]

                                Button.create
                                    [ Button.content "Print"
                                      Button.onClick (fun _ -> dispatch CmdPrint)
                                      Button.isEnabled (state |> canPrint) ] ] ] ] ]

        do
            Elmish.Program.mkProgram init update view
            |> Program.withHost parent
            |> Program.withConsoleTrace
            |> Program.run
