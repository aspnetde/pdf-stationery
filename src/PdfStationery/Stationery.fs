namespace PdfStationery

open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.Layout

module Stationery =
    type State =
        { ReplaceOriginal: bool }

    let init = { ReplaceOriginal = false }

    type Msg = ToggleReplaceOriginal

    let update (msg: Msg) (state: State): State =
        match msg with
        | ToggleReplaceOriginal -> { state with ReplaceOriginal = not state.ReplaceOriginal }

    let view (state: State) (dispatch) =
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
                                      [ TextBox.create [ TextBox.width 285.0 ]
                                        Button.create
                                            [ Button.content "Select"
                                              Button.width 100.0 ] ] ]
                            Separator.create [ Separator.height 5.0 ]

                            TextBlock.create [ TextBlock.text "Original PDF" ]
                            StackPanel.create
                                [ StackPanel.orientation Orientation.Horizontal
                                  StackPanel.width 390.0
                                  StackPanel.spacing 5.0
                                  StackPanel.children
                                      [ TextBox.create [ TextBox.width 285.0 ]
                                        Button.create
                                            [ Button.content "Select"
                                              Button.width 100.0 ] ] ]

                            Separator.create [ Separator.height 5.0 ]

                            StackPanel.create
                                [ StackPanel.orientation Orientation.Horizontal
                                  StackPanel.width 390.0
                                  StackPanel.spacing 10.0
                                  StackPanel.children
                                      [ CheckBox.create
                                          [ CheckBox.isChecked state.ReplaceOriginal
                                            CheckBox.onTapped (fun _ -> dispatch ToggleReplaceOriginal) ]
                                        TextBlock.create
                                            [ TextBlock.text "Replace Original PDF"
                                              TextBlock.onTapped (fun _ -> dispatch ToggleReplaceOriginal)
                                              TextBlock.verticalAlignment VerticalAlignment.Center ] ] ]

                            Separator.create [ Separator.height 5.0 ]

                            Button.create [ Button.content "Print" ] ] ] ] ]
