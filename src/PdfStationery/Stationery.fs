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

module Stationery =
    type State =
        { SourcePath: string
          TemplatePath: string
          ReplaceSource: bool }

    type Msg =
        | ToggleReplaceSource
        | ChangeTemplatePath of path: string
        | ChangeSourcePath of path: string
        | ShowErrorMessage of ex: Exception
        | ShowSuccessMessage of message: string
        | CmdUpdateConfig
        | CmdPrint
        | CmdSelectStorageLocationForNewPdf of path: string
        | CmdOpenFileDialogForSource
        | CmdOpenFileDialogForTemplate
        | NoOp

    type Program(parent: HostWindow) =
        let init() =
            let config = Config.get
            { ReplaceSource = config.ReplaceSource
              TemplatePath = config.TemplatePath
              SourcePath = config.SourcePath }, Cmd.none

        let updateConfig (state: State) =
            Config.set
                { SourcePath = state.SourcePath
                  TemplatePath = state.TemplatePath
                  ReplaceSource = state.ReplaceSource }
            NoOp

        let canPrint state =
            not (String.IsNullOrWhiteSpace(state.TemplatePath)) &&
            not (String.IsNullOrWhiteSpace(state.SourcePath))

        let print state =
            if state.ReplaceSource then
                Printer.replacePdf state.SourcePath state.TemplatePath
                ShowSuccessMessage "Your original PDF file has been decorated with your stationery."
            else
                let tmpPath = Printer.newPdf state.SourcePath state.TemplatePath
                CmdSelectStorageLocationForNewPdf tmpPath

        let savePdf saveResult =
            match saveResult with
            | None -> NoOp
            | Some result ->
                File.Move(result.TmpPath, result.TargetPath, true)
                ShowSuccessMessage
                    (sprintf "Your PDF has been decorated with your stationery and saved as `%s`."
                         (Path.GetFileName(result.TargetPath)))

        let noOp = (fun _ -> NoOp)

        let update msg (state: State) =
            match msg with
            | ToggleReplaceSource -> { state with ReplaceSource = not state.ReplaceSource }, Cmd.ofMsg CmdUpdateConfig
            | ChangeTemplatePath path -> { state with TemplatePath = path }, Cmd.ofMsg CmdUpdateConfig
            | ChangeSourcePath path -> { state with SourcePath = path }, Cmd.ofMsg CmdUpdateConfig

            | CmdPrint -> state, Cmd.ofMsg (print state)
            | CmdUpdateConfig -> state, Cmd.ofMsg (updateConfig state)

            | ShowSuccessMessage message -> state, Cmd.OfAsync.either showSuccess message noOp noOp
            | ShowErrorMessage ex -> state, Cmd.OfAsync.either showError ex noOp noOp

            | CmdOpenFileDialogForSource ->
                state, Cmd.OfAsync.either openFileDialog (state.TemplatePath, parent) ChangeTemplatePath ShowErrorMessage
            | CmdOpenFileDialogForTemplate ->
                state, Cmd.OfAsync.either openFileDialog (state.SourcePath, parent) ChangeSourcePath ShowErrorMessage
            | CmdSelectStorageLocationForNewPdf path ->
                let args = { SourcePath = state.SourcePath; TmpPath = path }
                state, Cmd.OfAsync.either openSaveDialog (args, parent) savePdf ShowErrorMessage

            | NoOp -> state, Cmd.none

        let view state dispatch =
            DockPanel.create
                [ DockPanel.children
                    [ StackPanel.create
                        [ StackPanel.dock Dock.Bottom
                          StackPanel.margin 15.0
                          StackPanel.spacing 5.0
                          StackPanel.children
                              [ TextBlock.create [
                                  TextBlock.text "Stationery PDF"
                                  TextBlock.width 390.0
                                ]
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
                                                  Button.onClick (fun _ -> dispatch CmdOpenFileDialogForSource) ] ] ]
                                Separator.create [ Separator.height 5.0 ]

                                TextBlock.create [
                                    TextBlock.text "Original PDF"
                                    TextBlock.width 390.0
                                ]
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
                                                  Button.onClick (fun _ -> dispatch CmdOpenFileDialogForTemplate) ] ] ]
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
                                      Button.width 390.0
                                      Button.onClick (fun _ -> dispatch CmdPrint)
                                      Button.isEnabled (state |> canPrint) ] ] ] ] ]

        do
            Elmish.Program.mkProgram init update view
            |> Program.withHost parent
            |> Program.withConsoleTrace
            |> Program.run
