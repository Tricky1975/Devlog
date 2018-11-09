// Lic:
// 	Development Log
// 	
// 	
// 	
// 	
// 	(c) Jeroen P. Broks, 2016, 2017, 2018, All rights reserved
// 	
// 		This program is free software: you can redistribute it and/or modify
// 		it under the terms of the GNU General Public License as published by
// 		the Free Software Foundation, either version 3 of the License, or
// 		(at your option) any later version.
// 		
// 		This program is distributed in the hope that it will be useful,
// 		but WITHOUT ANY WARRANTY; without even the implied warranty of
// 		MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// 		GNU General Public License for more details.
// 		You should have received a copy of the GNU General Public License
// 		along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 		
// 	Exceptions to the standard GNU license are available with Jeroen's written permission given prior 
// 	to the project the exceptions are needed for.
// Version: 18.11.09
// EndLic

//#define KEYDEBUG // crap!

using System;
﻿using System.Collections.Generic;
using TrickyUnits;
using Gtk;
namespace Devlog
{
    delegate void GenCallBack(object sender, EventArgs a);

    public static class GUI
    {
        static readonly List<Widget> RequireProject = new List<Widget>();
        static MainWindow win;
        static Notebook Tabber;
        static TextView Console;
        static Entry Prompt;
        static bool AllowEdit = true;
        static dvProject CurrentProject { get { return null; } }// Acutal return comes later!
        static Gdk.Color EntryLabel = new Gdk.Color(0, 180, 255);


        static Dictionary<string, Entry> GenEntries = new Dictionary<string, Entry>();

        static void AndACTION(object sender,EventArgs a){
            CommandClass.DoCommand(Prompt.Text);
            Prompt.Text = "";
        }

        static Entry GeneralAdd(VBox Panel, string codename, string Caption){ //,GenCallBack MyCallBack){
            var e = new Entry();
            var l = new Label(Caption);
            l.ModifyFg(StateType.Normal, EntryLabel);
            var b = new HBox();
            b.SetSizeRequest(1000, 30);
            e.SetSizeRequest(500, 30);
            l.SetSizeRequest(500, 30);
            b.Add(l);
            b.Add(e);
            Panel.Add(b);
            RequireProject.Add(e);
            GenEntries[codename] = e;
            // e.Changed += MyCallBack; // C# FAILED!
            return e;
        }
        static void GeneralInit(VBox Panel){
            Panel.ModifyBg(StateType.Normal, new Gdk.Color(0, 0, 20));
            GeneralAdd(Panel, "GITHUB", "Github Repository:").Changed += delegate (object s, EventArgs e) { if (AllowEdit) { CurrentProject.GitHub = ((Entry)s).Text; } };
            GeneralAdd(Panel, "TARGET", "Site Target Dir:").Changed += delegate (object s, EventArgs e) { if (AllowEdit) { CurrentProject.Target = ((Entry)s).Text; } };
            GeneralAdd(Panel, "TEMPLATE", "Template file:").Changed += delegate (object s, EventArgs e) { if (AllowEdit) { CurrentProject.Template = ((Entry)s).Text; } };
        }

        static void AutoEnable(){
            var b = CurrentProject != null;
            foreach (Widget w in RequireProject) w.Sensitive = b;
        }

        public static void Init()
        {
            MKL.Lic    ("Development Log - GUI.cs","GNU General Public License 3");
            MKL.Version("Development Log - GUI.cs","18.11.09");
            Application.Init();
            win = new MainWindow();
            win.ModifyBg(StateType.Normal, new Gdk.Color(0, 0, 0));
            win.SetSizeRequest(1200, 800);
            win.Resizable = false;
            win.Title = $"Development log - version {MKL.Newest} - Coded by: Tricky";
            Tabber = new Notebook(); Tabber.SetSizeRequest(1000, 770);
            Tabber.ModifyBg(StateType.Normal, new Gdk.Color(0, 0, 20));
            Console = new TextView();
            Prompt = new Entry();
            var overlord = new VBox();
            var superior = new HBox();
            var sidebar = new VBox(); sidebar.SetSizeRequest(200, 770);
            var mainarea = new VBox();
            var cscroll = new ScrolledWindow();
            var promptbar = new HBox();
            win.Add(overlord);
            overlord.Add(superior); superior.SetSizeRequest(1200, 600);
            overlord.Add(cscroll); cscroll.SetSizeRequest(1200, 170); cscroll.Add(Console);
            overlord.Add(promptbar); promptbar.SetSizeRequest(1200, 30); promptbar.Add(Prompt);
            var pOk = new Button("Ok");
            pOk.SetSizeRequest(50, 30);
            pOk.Clicked += AndACTION;
            Prompt.SetSizeRequest(1150, 30);
            promptbar.Add(pOk);
            Console.ModifyBase(StateType.Normal, new Gdk.Color(0, 20, 0));
            Console.ModifyText(StateType.Normal, new Gdk.Color(0, 255, 0));
            Prompt.ModifyBase(StateType.Normal, new Gdk.Color(25, 18, 0));
            Prompt.ModifyText(StateType.Normal, new Gdk.Color(255, 180, 0));
            superior.Add(sidebar);
            superior.Add(mainarea);
            mainarea.Add(Tabber);
            GeneralInit(NewTab("General"));
            NewTab("Tags");
            NewTab("Entries");
            NewTab("AutoPrefix");
            WriteLn("Welcome to Devlog!");
            WriteLn("Coded by: Tricky");
            WriteLn($"(c) 2016-20{qstr.Left(MKL.Newest,2)} Jeroen P. Broks");
            WriteLn("Released under the terms of the General Public License v3");
            AutoEnable();
//#if KEYDEBUG
            //WriteLn("KEYDEBUG is set!");
            //Prompt.KeyPressEvent += KeyDebug;
            //Prompt.KeyPressEvent += StoreProperty;
//#endif

        }

        /*
//#if KEYDEBUG
        static void StoreProperty(object o, KeyPressEventArgs args)
        {
            Gdk.EventKey key = args.Event;
            System.Console.WriteLine(key.KeyValue);
            if (key.KeyValue == (uint)Gdk.Key.Return)
            {
                System.Console.WriteLine("YO!");
            }
        }


        public static void KeyDebug(object sender,KeyPressEventArgs a){
            System.Console.WriteLine($"Key pressed {a.Event.KeyValue}");
            WriteLn($"Key pressed {a.Event.KeyValue}");
        }
//#endif
        */

        public static void Write(string yeah) { Console.Buffer.Text += yeah; }
        public static void WriteLn(string yeah) => Write($"{yeah}\n");

        public static VBox NewTab(string caption){
            var ret = new VBox();
            var lab = new Label(caption);
            Tabber.AppendPage(ret, lab);
            return ret;
        }

        public static void Start(){
            win.ShowAll();
            Application.Run();
        }

    }
}
