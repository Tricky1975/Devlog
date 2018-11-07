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
// Version: 18.11.07
// EndLic

//#define KEYDEBUG // crap!

﻿using System.Collections.Generic;
using TrickyUnits;
using Gtk;
namespace Devlog
{
    public static class GUI
    {
        static readonly List<Widget> RequireProject = new List<Widget>();
        static MainWindow win;
        static Notebook Tabber;
        static TextView Console;
        static Entry Prompt;

        public static void Init()
        {
            MKL.Lic("Development Log - GUI.cs", "GNU General Public License 3");
            MKL.Version("Development Log - GUI.cs", "18.11.07");
            Application.Init();
            win = new MainWindow();
            win.ModifyBg(StateType.Normal, new Gdk.Color(0, 0, 0));
            win.SetSizeRequest(1200, 800);
            win.Resizable = false;
            win.Title = $"Development log - version {MKL.Newest} - Coded by: Tricky";
            Tabber = new Notebook(); Tabber.SetSizeRequest(1000, 770);
            Console = new TextView();
            Prompt = new Entry();
            var overlord = new VBox();
            var superior = new HBox();
            var sidebar = new VBox(); sidebar.SetSizeRequest(200, 770);
            var mainarea = new VBox();
            win.Add(overlord);
            overlord.Add(superior); superior.SetSizeRequest(1200, 600);
            overlord.Add(Console); Console.SetSizeRequest(1200, 170);
            overlord.Add(Prompt); Prompt.SetSizeRequest(1200, 30);
            Console.ModifyBase(StateType.Normal, new Gdk.Color(0, 20, 0));
            Console.ModifyText(StateType.Normal, new Gdk.Color(0, 255, 0));
            Prompt.ModifyBase(StateType.Normal, new Gdk.Color(25, 18, 0));
            Prompt.ModifyText(StateType.Normal, new Gdk.Color(255, 180, 0));
            superior.Add(sidebar);
            superior.Add(mainarea);
            mainarea.Add(Tabber);
            NewTab("General");
            NewTab("Tags");
            NewTab("Entries");
            NewTab("AutoPrefix");
            WriteLn("Welcome to Devlog!");
            WriteLn("Coded by: Tricky");
            WriteLn($"(c) 2016-20{qstr.Left(MKL.Newest)} Jeroen P. Broks");
            WriteLn("Released under the terms of the General Public License v3");
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
