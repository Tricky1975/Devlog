// Lic:
// Devlog
// Graphic User Interface
// 
// 
// 
// (c) Jeroen P. Broks, 2016, 2017, 2018, 2020, 2021
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 
// Please note that some references to data like pictures or audio, do not automatically
// fall under this licenses. Mostly this is noted in the respective files.
// 
// Version: 21.08.28
// EndLic

//#define KEYDEBUG // crap!



using System;
using System.Reflection;
using System.Collections.Generic;
using TrickyUnits;
using TrickyUnits.GTK;
using Gtk;
namespace Devlog
{
	delegate void GenCallBack(object sender, EventArgs a);

	public static class GUI
	{
		static readonly List<Widget> RequireProject = new List<Widget>();
		static readonly List<Widget> RequireTag = new List<Widget>();
		static MainWindow win;
		static Notebook Tabber;
		static TextView Console;
		static Entry Prompt;
		static ListBox ProjectList;
		static ListBox TagList;
		static HBox TagEditBox;
		static Entry TagEditHead;
		static Entry TagEditEntry;
		static Entry TagEditIcon;
		static bool AllowEdit = true;
		static public string CurrentProjectName = "";
		static dvProject CurrentProject { get { if (CurrentProjectName == "") return null; return dvProject.Get(CurrentProjectName); } }// Acutal return comes later!
		static Gdk.Color EntryLabel = new Gdk.Color(0, 180, 255);
		static TreeView Entries;
		static ListBox CommandHistory;
		static TreeView PrefixTable;

		static public void SetPrompt(string s) { Prompt.Text = s; }

		static Dictionary<string, Entry> GenEntries = new Dictionary<string, Entry>();

		public static void ClearHistory() => CommandHistory.Clear();

		static void AndACTION(object sender, EventArgs a) {
			var cmd = Prompt.Text;
			Prompt.Text = "";
			CommandHistory.AddItem(cmd);
			for (int i = 0; i < cmd.Length; ++i)
				if (cmd[i] < 32 || cmd[i] > 126) {
					QuickGTK.Error($"Non ASCII character found on position: {i} -> {cmd[i]}/{(int)cmd[i]}/{((int)cmd[i]).ToString("X")}");
					return;
				}
			CommandClass.DoCommand(cmd);
		}

		static Entry GeneralAdd(VBox Panel, string codename, string Caption)
		{ //,GenCallBack MyCallBack){
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

		static void GeneralInit(VBox Panel)
		{
			Panel.ModifyBg(StateType.Normal, new Gdk.Color(0, 0, 20));
			GeneralAdd(Panel, "GITHUB", "Github Repository:").Changed += delegate (object s, EventArgs e) { if (AllowEdit) { CurrentProject.GitHub = ((Entry)s).Text; } };
			GeneralAdd(Panel, "TARGET", "Site Target Dir:").Changed += delegate (object s, EventArgs e) { if (AllowEdit) { CurrentProject.Target = ((Entry)s).Text; } };
			GeneralAdd(Panel, "TEMPLATE", "Template file:").Changed += delegate (object s, EventArgs e) { if (AllowEdit) { CurrentProject.Template = ((Entry)s).Text; } };
		}

		static void GeneralUpdate(){
			AllowEdit = false;
			GenEntries["GITHUB"].Text = CurrentProject.GitHub;
			GenEntries["TARGET"].Text = CurrentProject.Target;
			GenEntries["TEMPLATE"].Text = CurrentProject.Template;
			AllowEdit = true;
		}

		static void AutoEnable()
		{
			var b = CurrentProject != null;
			foreach (Widget w in RequireProject) w.Sensitive = b;
			foreach (Widget w in RequireTag) w.Sensitive = b && TagList.ItemText != "";
		}

		static public void RenewProjects()
		{
			ProjectList.Clear();
			string[] filelist;
			string workspace = MainClass.GetConfig("WORKSPACE");
			if (workspace == "")
			{
				QuickGTK.Error("I don't know where to look for projects! Please use the command GLOBALCONFIG WORKSPACE=/home/username/MyWorkSpace/ or something like that");
				return;
			}
			try
			{
				filelist = FileList.GetDir(workspace + "/Projects", 0, true, false);
			}
			catch
			{
				QuickGTK.Error("Reading the project folder failed! Has it been properly configured?");
				return;
			}
			foreach (string f in filelist)
			{
				if (qstr.Suffixed(f, ".prj")) ProjectList.AddItem(qstr.Left(f, f.Length - 4));
			}
		}



		static void InitSidebar(VBox sidebar)
		{
			var lb = new ListBox("Projects"); ProjectList = lb;
			var sw = new ScrolledWindow();
			sw.Add(lb.Gadget);
			Assembly asm = Assembly.GetExecutingAssembly();
			System.IO.Stream stream;
			//= asm.GetManifestResourceStream("MyData.Properties.Icon.png");
			//Gdk.Pixbuf PIX = new Gdk.Pixbuf(stream);
			//win.Icon = PIX;
			//stream.Dispose();
			stream = asm.GetManifestResourceStream("Devlog.Mascot.Mascot.png");
			var mascot = new Image(stream);
			mascot.SetAlignment((float)0.5, 2);
			stream.Dispose();
			sidebar.Add(sw);
			sidebar.Add(mascot);
			lb.Gadget.CursorChanged += delegate (object sender, EventArgs a) { Use(lb.ItemText); };
			lb.Gadget.RulesHint = true;
		}

		static public void Use(string prj)
		{
			System.Console.WriteLine($"User picked {prj}");
			CurrentProjectName = prj;
			var p = CurrentProject;
			GeneralUpdate();
			UpdateTags();
			UpdateEntries(p.HighestRecordNumber - 200, p.HighestRecordNumber);
			UpdatePrefix();
			AutoEnable();
		}

		static void TagsInit(VBox panel)
		{
			TagList = new ListBox("Tags");
			TagEditBox = new HBox();
			TagEditBox.SetSizeRequest(1000, 30);
			var sw = new ScrolledWindow(); panel.Add(sw);
			var tv = TagList.Gadget; sw.Add(tv);
			var lb1 = new Label("Head"); lb1.ModifyFg(StateType.Normal, new Gdk.Color(0, 180, 255));
			var lb2 = new Label("Content"); lb2.ModifyFg(StateType.Normal, new Gdk.Color(0, 180, 255));
			var lb3 = new Label("Icon"); lb3.ModifyFg(StateType.Normal, new Gdk.Color(0, 180, 255));
			TagEditHead = new Entry(); RequireTag.Add(TagEditHead);
			TagEditEntry = new Entry(); RequireTag.Add(TagEditEntry);
			TagEditIcon = new Entry(); RequireTag.Add(TagEditIcon);
			TagEditBox.Add(lb1);
			TagEditBox.Add(TagEditHead);
			TagEditBox.Add(lb2);
			TagEditBox.Add(TagEditEntry);
			TagEditBox.Add(lb3);
			TagEditBox.Add(TagEditIcon);
			panel.Add(TagEditBox);
			TagList.Gadget.RulesHint = true;
			RequireProject.Add(TagList.Gadget);
			TagList.Gadget.CursorChanged += OnTagSelect;
			TagEditHead.Changed += OnEditTag;
			TagEditEntry.Changed += OnEditTag;
			TagEditIcon.Changed += OnEditTag;
		}

		static public void UpdateTags()
		{
			TagList.Clear();
			if (CurrentProject == null) return;
			CurrentProject.Data.List("Tags").Sort();
			foreach (string tag in CurrentProject.Data.List("Tags")) if (tag.Trim() != "") TagList.AddItem(tag.Trim());
		}

		static void OnTagSelect(object sender, EventArgs a)
		{
			AllowEdit = false;
			var tag = TagList.ItemText;
			TagEditHead.Text = CurrentProject.GetData($"HEAD.{tag}");
			TagEditEntry.Text = CurrentProject.GetData($"INHD.{tag}");
			TagEditIcon.Text = CurrentProject.GetData($"ICON.{tag}");
			AllowEdit = true;
			AutoEnable();
		}

		static void OnEditTag(object sender, EventArgs a)
		{
			if (!AllowEdit) return;
			var s = (Entry)sender;
			var h = "INHD";
			if (s == TagEditHead) h = "HEAD";
			if (s == TagEditIcon) h = "ICON";
			CurrentProject.DefData($"{h}.{TagList.ItemText}", s.Text);
		}

		static void ConsoleDOWN(object sender, EventArgs e) { Console.ScrollToIter(Console.Buffer.EndIter, 0, false, 0, 0); }


		static void InitPrefix(VBox Panel){
			var sw = new ScrolledWindow();
			PrefixTable = new TreeView();
			var pt = PrefixTable; // I AM LAZY!!!

			// Alert (will contain a "!" if the CD is higher than the Reset value)
			var tvc = new TreeViewColumn();
			var NameCell = new CellRendererText();
			tvc.Title = "";
			tvc.PackStart(NameCell, true);
			tvc.AddAttribute(NameCell, "text", 0);
			pt.AppendColumn(tvc);

			// Name
			tvc = new TreeViewColumn();
			NameCell = new CellRendererText();
			tvc.Title = "Name:";
			tvc.PackStart(NameCell, true);
			tvc.AddAttribute(NameCell, "text", 1);
			pt.AppendColumn(tvc);

			// CD
			tvc = new TreeViewColumn();
			NameCell = new CellRendererText();
			tvc.Title = "Countdown:";
			tvc.PackStart(NameCell, true);
			tvc.AddAttribute(NameCell, "text", 2);
			pt.AppendColumn(tvc);

			// Reset
			tvc = new TreeViewColumn();
			NameCell = new CellRendererText();
			tvc.Title = "Reset:";
			tvc.PackStart(NameCell, true);
			tvc.AddAttribute(NameCell, "text", 3);
			pt.AppendColumn(tvc);

			// Prefix
			tvc = new TreeViewColumn();
			NameCell = new CellRendererText();
			tvc.Title = "Prefix:";
			tvc.PackStart(NameCell, true);
			tvc.AddAttribute(NameCell, "text", 4);
			pt.AppendColumn(tvc);

			// Closure
			RequireProject.Add(pt);
			sw.Add(pt);
			Panel.Add(sw);
		}

		static void InitEntries(VBox Panel){
			var sentries = new ScrolledWindow();
			Entries = new TreeView();
			Entries.RulesHint = true;
			sentries.Add(Entries);
			// record number
			var tvc = new TreeViewColumn();
			var NameCell = new CellRendererText();
			tvc.Title = "Record:";
			tvc.PackStart(NameCell, true);
			tvc.AddAttribute(NameCell, "text", 0);
			Entries.AppendColumn(tvc);

			// Tag
			tvc = new TreeViewColumn();
			NameCell = new CellRendererText();
			tvc.Title = "Tag:";
			tvc.PackStart(NameCell, true);
			tvc.AddAttribute(NameCell, "text", 1);
			Entries.AppendColumn(tvc);

			// Content
			tvc = new TreeViewColumn();
			NameCell = new CellRendererText();
			tvc.Title = "Content:";
			tvc.PackStart(NameCell, true);
			tvc.AddAttribute(NameCell, "text", 2);
			Entries.AppendColumn(tvc);

			// Date
			tvc = new TreeViewColumn();
			NameCell = new CellRendererText();
			tvc.Title = "Date:";
			tvc.PackStart(NameCell, true);
			tvc.AddAttribute(NameCell, "text", 3);
			Entries.AppendColumn(tvc);

			// Time
			tvc = new TreeViewColumn();
			NameCell = new CellRendererText();
			tvc.Title = "Time:";
			tvc.PackStart(NameCell, true);
			tvc.AddAttribute(NameCell, "text", 4);
			Entries.AppendColumn(tvc);

			// Finish
			RequireProject.Add(Entries);
			Panel.Add(sentries);

		}

		public static void UpdateEntries(int start, int einde){
			if (start>einde) {
				WriteLn("HUH? I cannot update the entries when the start value is lower than the end value");
				return;
			}
			var cp = CurrentProject; if (cp == null) return;
			var ls = new ListStore(typeof(string), typeof(string), typeof(string), typeof(string), typeof(string));
			//var bt = QOpen.ReadFile(cp.EntryFile);
			//while (!bt.EOF){
			for (int i = einde; i >= start;i--){
				var e = new dvEntry(cp, i,true); //(bt, start, einde);
				if (e == null) break;
				if (e.Loaded) ls.AppendValues($"{i}", e.Tag, e.Pure, e.Date, e.Time);
			}
			//bt.Close();
			Entries.Model = ls;
		}

		public static void UpdatePrefix(){
			var cp = CurrentProject; if (cp == null) return;
			var ls = new ListStore(/*alert*/typeof(string),/*key*/typeof(string),/*CD*/typeof(string),/*Reset*/typeof(string),/*Prefix*/typeof(string));
			foreach(string key in cp.Prefixes.Keys){
				var PF = cp.Prefixes[key];
				var alert = ""; if (PF.CD > PF.Reset) alert = "!";
				ls.AppendValues(alert, key, $"{PF.CD}", $"{PF.Reset}", $"{PF.Prefix}");
			}
			PrefixTable.Model = ls;
		}

		public static void ClearConsole() { Console.Buffer.Text = ""; }

		static void InitHistory(VBox panel){
			var scroll = new ScrolledWindow(); panel.Add(scroll);
			CommandHistory = new ListBox("Command history");
			scroll.Add(CommandHistory.Gadget);
			CommandHistory.Gadget.CursorChanged += delegate (object sender, EventArgs e) { Prompt.Text = CommandHistory.ItemText; };
		}

		public static void Init()
		{
			MKL.Lic    ("Development Log - GUI.cs","GNU General Public License 3");
			MKL.Version("Development Log - GUI.cs","21.08.28");
			Application.Init();
			win = new MainWindow();
			win.ModifyBg(StateType.Normal, new Gdk.Color(0, 0, 0));
			win.SetSizeRequest(1200, 800);
			win.Resizable = false;
			win.Title = $"Development log - version {MKL.Newest} - Coded by: Tricky";
			Tabber = new Notebook(); Tabber.SetSizeRequest(1000, 770);
			Tabber.ModifyBg(StateType.Normal, new Gdk.Color(0, 0, 20));
			Console = new TextView();
			Console.Editable = false;
			Console.ModifyFont(Pango.FontDescription.FromString("Courier 18"));
			Console.SizeAllocated += new SizeAllocatedHandler(ConsoleDOWN);
			Prompt = new Entry();
			var overlord = new VBox();
			var superior = new HBox();
			var sidebar = new VBox(); sidebar.SetSizeRequest(200, 770);
			var mainarea = new VBox();
			var cscroll = new ScrolledWindow();
			var promptbar = new HBox();
			InitSidebar(sidebar);
			win.Add(overlord);
			overlord.Add(superior); superior.SetSizeRequest(1200, 600);
			overlord.Add(cscroll); cscroll.SetSizeRequest(1200, 170); cscroll.Add(Console);
			overlord.Add(promptbar); promptbar.SetSizeRequest(1200, 30); promptbar.Add(Prompt);
			var pOk = new Button("Ok");
			pOk.SetSizeRequest(50, 30);
			pOk.Clicked += AndACTION;
			Prompt.Activated += AndACTION;
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
			TagsInit(NewTab("Tags"));
			InitEntries(NewTab("Entries"));
			InitPrefix(NewTab("AutoPrefix"));
			InitHistory(NewTab("Command History"));
			WriteLn("Welcome to Devlog!");
			WriteLn("Coded by: Tricky");
			WriteLn($"(c) 2016-20{qstr.Left(MKL.Newest,2)} Jeroen P. Broks");
			WriteLn("Released under the terms of the General Public License v3\n");
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

		public static void Write(string yeah,bool wantvars=false) {
			var result = yeah;
			if (wantvars && CurrentProject!=null) {
				var vs = CurrentProject.Data.Vars();
				foreach (string clown in vs) 
					if (qstr.Prefixed(clown, "VAR."))
					{
						var wvar = clown.Substring(4);
						var oldresult = "";
						do
						{
							oldresult = result;
							result = result.Replace($"${wvar}", CurrentProject.Data.C(clown));
						} while (oldresult != result);
					}
			}
			Console.Buffer.Text += result; 
		}

		public static void WriteLn(string yeah,bool wantvars=false) => Write($"{yeah}\n",wantvars);

		public static VBox NewTab(string caption){
			var ret = new VBox();
			var lab = new Label(caption);
			lab.ModifyFg(StateType.Normal, new Gdk.Color(255, 255, 0));
			Tabber.AppendPage(ret, lab);
			return ret;
		}

		public static void Start(){
			win.ShowAll();
			Application.Run();
		}

	}
}