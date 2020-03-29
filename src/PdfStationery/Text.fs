namespace PdfStationery

type Text =
    | DialogTitle
    | Success
    | SuccessMessage
    | SuccessMessageWithReplacement
    | SuccessMessageWithIndividualPath
    | Error
    | ErrorMessage
    | PdfDocumentFilter
    | FormTemplatePdfLabel
    | FormOriginalPdfLabel
    | FormReplacementLabel
    | FormSelectButton
    | FormPrintButton

[<AutoOpen>]
module Text =

    open System.Globalization

    let private getTranslations text =
        match text with
        | DialogTitle -> "PDF Stationery", "PDF Briefpapier"
        | Success -> "Success", "Erfolg"
        | SuccessMessage -> "Great!", "Super!"
        | SuccessMessageWithReplacement ->
            "Your Original PDF file has been decorated with your stationery.",
            "Das Original-PDF wurde mit dem Briefpapier versehen."
        | SuccessMessageWithIndividualPath ->
            "Your Original PDF has been decorated with your stationery and saved as",
            "Ihr Original-PDF wurde mit dem Briefpapier versehen und gespeichert als"
        | Error -> "Error", "Fehler"
        | ErrorMessage -> "Oops, something went wrong:", "Oh, da ist etwas schief gelaufen:"
        | PdfDocumentFilter -> "PDF Documents", "PDF-Dokumente"
        | FormTemplatePdfLabel -> "Stationery PDF", "Briefpapier-PDF"
        | FormOriginalPdfLabel -> "Original PDF", "Original-PDF"
        | FormReplacementLabel -> "Replace Original PDF", "Original-PDF ersetzen"
        | FormSelectButton -> "Select", "Auswählen"
        | FormPrintButton -> "Print", "Drucken"

    let translate text =
        try
            let en, de = text |> getTranslations
            if CultureInfo.CurrentUICulture.Name.StartsWith "en" then en
            else de
        with ex ->
            printfn "Text could not be translated: %s" ex.Message
            "n/a"
