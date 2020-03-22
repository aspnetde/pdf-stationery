namespace PdfStationery

open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.Layout

module Stationery =
    type State =
        { count: int }

    let init = { count = 0 }

    type Msg =
        | Increment
        | Decrement
        | Reset

    let update (msg: Msg) (state: State): State =
        match msg with
        | Increment -> { state with count = state.count + 1 }
        | Decrement -> { state with count = state.count - 1 }
        | Reset -> init

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
                                      [ CheckBox.create [ CheckBox.isChecked true ]
                                        TextBlock.create
                                            [ TextBlock.text "Replace Original PDF"
                                              TextBlock.verticalAlignment VerticalAlignment.Center ] ] ]

                            Separator.create [ Separator.height 5.0 ]

                            Button.create
                                [ Button.onClick (fun _ -> dispatch Reset)
                                  Button.content "Print" ] ] ] ] ]
