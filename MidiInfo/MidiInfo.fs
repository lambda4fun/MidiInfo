module MidiInfo

open System
open Eto.Forms
open NAudio.Midi

type MainForm () =
    inherit Form (Title = "MidiInfo", Width = 640, Height = 480)

[<EntryPoint; STAThread>]
let main _ =
    let app = new Application ()
    let form = new MainForm ()
    app.Run form
    0
