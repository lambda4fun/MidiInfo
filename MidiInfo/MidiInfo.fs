module MidiInfo

open System
open System.Collections.ObjectModel
open Eto
open Eto.Forms
open NAudio.Midi

let (^) f x = f x

let getDescription (e : MidiEvent) =
    match e with
    | :? MetaEvent as e->
        string e.MetaEventType
    | :? NoteOnEvent as e ->
        sprintf "note = %s(%d); velocity = %d; duration = %d"
            e.NoteName e.NoteNumber e.Velocity e.NoteLength
    | :? NoteEvent as e ->
        sprintf "note = %s(%d); velocity = %d"
            e.NoteName e.NoteNumber e.Velocity
    | _ ->
        ""

type MainForm () as this =
    inherit Form (Title = "MidiInfo", Width = 640, Height = 480)

    do
        let gridView = new GridView ()

        let timeCell =
            new TextBoxCell (Binding =
                Binding.Delegate<MidiEvent, _> (fun e -> string e.AbsoluteTime))
        let deltaTimeCell =
            new TextBoxCell (Binding =
                Binding.Delegate<MidiEvent, _> (fun e -> string e.DeltaTime))
        let channelCell =
            new TextBoxCell (Binding =
                Binding.Delegate<MidiEvent, _> (fun e -> string e.Channel))
        let commandCell =
            new TextBoxCell (Binding =
                Binding.Delegate<MidiEvent, _> (fun e -> string e.CommandCode))
        let descriptionCell =
            new TextBoxCell (Binding =
                Binding.Delegate<MidiEvent, _> (fun e -> getDescription e))

        gridView.Columns.Add ^ new GridColumn (HeaderText = "t [ticks]", DataCell = timeCell)
        gridView.Columns.Add ^ new GridColumn (HeaderText = "Δt [ticks]", DataCell = deltaTimeCell)
        gridView.Columns.Add ^ new GridColumn (HeaderText = "Channel", DataCell = channelCell)
        gridView.Columns.Add ^ new GridColumn (HeaderText = "Event", DataCell = commandCell)
        gridView.Columns.Add ^ new GridColumn (HeaderText = "Description", DataCell = descriptionCell)

        let infoLabel = new Label ()

        let table = new TableLayout ()
        table.Rows.Add ^ TableRow (TableCell gridView, ScaleHeight = true)
        table.Rows.Add ^ TableRow (TableCell infoLabel)

        this.Content <- table

        let toolBar = new ToolBar ()
        let item = new ButtonToolItem (fun _ _ ->
            let dialog = new OpenFileDialog (CheckFileExists = true)
            dialog.Filters.Add ^ FileDialogFilter ("MIDI Files", ".mid", ".midi")
            dialog.Filters.Add ^ FileDialogFilter ("All Files", ".*")
            match dialog.ShowDialog this with
            | DialogResult.Ok ->
                let midiFile = MidiFile dialog.FileName
                let trackIndex = 0
                let rows = ObservableCollection<MidiEvent> ()
                midiFile.Events.[trackIndex]
                |> Seq.iter rows.Add
                gridView.DataStore <- rows :> seq<_> :?> seq<obj>

                let infoText = [
                    System.IO.Path.GetFullPath dialog.FileName
                    sprintf "Division = %d ticks/(1/4 note)" midiFile.DeltaTicksPerQuarterNote] |> String.concat "\r\n"
                infoLabel.Text <- infoText
            | _ -> ())
        item.Text <- "Open"
        toolBar.Items.Add ^ item
        this.ToolBar <- toolBar

[<EntryPoint; STAThread>]
let main _ =
    // Gtk3 causes grid-related error.
    let app = if Environment.OSVersion.Platform = PlatformID.Unix
              then new Application (Platforms.Gtk2)
              else new Application (Platform.Detect)
    let form = new MainForm ()
    app.Run form
    0
