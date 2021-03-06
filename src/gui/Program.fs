﻿open Stmt.Parser
open CoreParser
open Interpreter
open System.Drawing
open System.Windows.Forms

let programInput =
  let textBox = new TextBox()
  textBox.Location   <- System.Drawing.Point(0, 0)
  textBox.Multiline  <- true
  textBox.ScrollBars <- ScrollBars.Vertical
  textBox.Height <- 300
  textBox.Width  <- 300
  textBox

let programLabel =
  let lbl = new Label()
  lbl.Location <- System.Drawing.Point(programInput.Width, 0)
  lbl.AutoSize <- true
  lbl

let mutable env     : string -> Option<int> = fun (s : string) -> None
let mutable program : Option<Stmt.t> = None 
let mutable pp: Stmt.t list = []
let mutable penv: (string -> Option<int>) list = []

let prevStepAction (but : Button) args =
  match pp with
  |[] -> but.Enabled <- false
  |a :: b -> program <- Some a 
             pp <- b 
             if pp = [] then 
               but.Enabled <- false   
             match penv with
             |[] -> ()
             |x :: y -> env <- x
                        penv <- y
  programLabel.Text <- sprintf "%A" program     

let prevStepButton =
  let but = new Button()
  but.Text     <- "Prev Step"
  but.Location <- System.Drawing.Point(programInput.Width - but.Width * 2, programInput.Height)
  but.Enabled  <- false
  but.Click.Add (prevStepAction but)
  but  

let nextStepAction (but : Button) args =
  match program with 
  | None   -> but.Enabled <- false
  | Some p ->
    let (nenv, np) = ss env p
    pp <- program.Value :: pp
    penv <- env :: penv
    env     <- nenv
    program <- np
    programLabel.Text <- sprintf "%A" program
    if program = None then but.Enabled <- false

let nextStepButton =
  let but = new Button()
  but.Text     <- "Next Step"
  but.Location <- System.Drawing.Point(programInput.Width - but.Width, programInput.Height)
  but.Enabled  <- false
  but.Click.Add (nextStepAction but)
  but.Click.Add (fun e -> prevStepButton.Enabled <- true)
  prevStepButton.Click.Add (fun e -> but.Enabled <- true)
  but

let interpretAction args =
  let parseResult = &programInput.Text |> parse ()
  try
    program <- parseResult |> List.head |> fst |> Some
    env <- (fun s -> None)
    nextStepButton.Enabled <- true
    programLabel.Text <- sprintf "%A" program
  with
  | _ -> failwith "test"

let interpretButton =
  let but = new Button()
  but.Text <- "Interpret"
  but.Location <- System.Drawing.Point(0, programInput.Height)
  but.MouseClick.Add interpretAction 
  but

let mainForm =
  let form = new Form(Visible = false, TopMost = true)
  form.Controls.Add(interpretButton)
  form.Controls.Add(nextStepButton)
  form.Controls.Add(prevStepButton)
  form.Controls.Add(programInput)
  form.Controls.Add(programLabel)
  form

[<EntryPoint>]
let main argv = 
  mainForm.Visible <- true
  Application.Run()
  0