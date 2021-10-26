// Lic:
// DevLog
// Commands
// 
// 
// 
// (c) Jeroen P. Broks, 2016, 2017, 2018, 2020
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
// Version: 20.07.24
// EndLic

using System;
using System.Collections.Generic;
using TrickyUnits;
using TrickyUnits.GTK;

namespace Devlog
{

	delegate void MyCommand(string arg);

	static class CommandClass
	{
		static Random myrand = new Random();
		static int Rand(int min, int max) => myrand.Next(min, max);
		static dvProject CurrentProject { get { if (GUI.CurrentProjectName == "") return null; return dvProject.Get(GUI.CurrentProjectName); } }// Acutal return comes later!
		static public readonly Dictionary<string, MyCommand> Commands = new Dictionary<string, MyCommand>();

		static void Annoy(string arg) => QuickGTK.Info(arg); // This is just a test ;)

		static public void Bye(string unneeded){
			// TODO: Some saving stuff will take place here later!
		}


		static void GlobalConfig(string cfga){
			var c = cfga.Trim();
			var p = c.IndexOf('=');
			var cfield = c.Substring(0, p).ToUpper();
			var cvalue = c.Substring(p + 1);
			MainClass.DefConfig(cfield, cvalue);
			GUI.WriteLn($"Global Config Variable: {cfield} = {cvalue}");
		}

		static void LetVar(string koe){
			var p = koe.IndexOf('=');
			if (p == -1) { GUI.WriteLn("Incorrect definition!"); return; }
			var k = koe.Substring(0, p);
			var v = koe.Substring(p+1);
			if (CurrentProject == null) { GUI.WriteLn("No porject"); return; }
			CurrentProject.Data.D($"VAR.{k.Trim()}", v.Trim());
			CurrentProject.SaveMe();
		}

		static void NewTag(string atag){
			CurrentProject.Data.CL("TAGS");
			var tag = atag.Trim().ToUpper();
			if (tag == "") { GUI.WriteLn("No Tag!"); }
			else if (CurrentProject == null) { GUI.WriteLn("No project!"); }
			else if (tag.IndexOf(' ') >= 0) { GUI.WriteLn("Invalid tag!"); }
			else if (CurrentProject.Data.List("TAGS").Contains(tag)) { GUI.WriteLn($"Ttag {tag} already exists!"); }
			else {
				var FR = Rand(CurrentProject.GetDataDefaultInt("FCOLMIN.R", 127), CurrentProject.GetDataDefaultInt("FCOLMAX.R", 255));
				var FG = Rand(CurrentProject.GetDataDefaultInt("FCOLMIN.G", 127), CurrentProject.GetDataDefaultInt("FCOLMAX.G", 255));
				var FB = Rand(CurrentProject.GetDataDefaultInt("FCOLMIN.B", 127), CurrentProject.GetDataDefaultInt("FCOLMAX.B", 255));
				var BR = (int)(FR / 20);
				var BG = (int)(FG / 20);
				var BB = (int)(FB / 20);
				if (CurrentProject.GetDataDefaultInt("BCOLMAX.R", 0) > 0) BR = Rand(CurrentProject.GetDataDefaultInt("BCOLMIN.R", 6), CurrentProject.GetDataDefaultInt("BCOLMAX.R", 12));
				if (CurrentProject.GetDataDefaultInt("BCOLMAX.G", 0) > 0) BG = Rand(CurrentProject.GetDataDefaultInt("BCOLMIN.G", 6), CurrentProject.GetDataDefaultInt("BCOLMAX.G", 12));
				if (CurrentProject.GetDataDefaultInt("BCOLMAX.B", 0) > 0) BB = Rand(CurrentProject.GetDataDefaultInt("BCOLMIN.B", 6), CurrentProject.GetDataDefaultInt("BCOLMAX.B", 12));
				CurrentProject.Data.Add("TAGS", tag);
				CurrentProject.DefData($"HEAD.{tag}", $"background-color:rgb(0,0,0); color:rgb({FR},{FG},{FB});");
				CurrentProject.DefData($"INHD.{tag}", $"background-color:rgb({BR},{BG},{BB}); color:rgb({FR},{FG},{FB});");
				GUI.WriteLn($"Tag {tag} added");
				AddEntry($"SITE Added tag {tag}");
				GUI.UpdateTags();
			}
		}

		static void AddEntry(string junk){
			var p = junk.IndexOf(' '); if (p < 0) { GUI.WriteLn("ADD: Syntax error!"); return; }
			var tag = junk.Substring(0, p).ToUpper().Trim();
			var content = junk.Substring(p+1).Trim();
			var cp = CurrentProject;
			if (cp == null) { GUI.WriteLn("ADD: No project!"); return; }
			if (!cp.GotTag(tag)) { GUI.WriteLn($"ADD: Tag {tag} does not exist!"); return; }
			// Prefix handling
			var MustSave = false;
			foreach (string id in cp.Prefixes.Keys) {
				var pf = cp.Prefixes[id];
				pf.CD--;
				//GUI.WriteLn($"Prefix {id} CD at: {pf.CD}/{pf.Reset}");
				if (pf.CD <= 0) { content = $"{pf.Prefix} {content}"; pf.CD += Math.Abs(pf.Reset); MustSave = true; }
			}
			if (MustSave) cp.SaveMe();
			var e = new dvEntry(cp, tag, content);
			GUI.WriteLn($"Added entry #{e.RecID}");
			GUI.UpdateEntries(cp.HighestRecordNumber-200,cp.HighestRecordNumber);
			GUI.UpdatePrefix();
			// Autopush
			cp.autopush--;
			if (cp.autopush<0){
				DoCommand("GEN");
				DoCommand("PUSH");
				cp.autopush = 10;
				cp.SaveMe();
			} else if (cp.autopush==0) {
				GUI.WriteLn("Next addition will trigger the autopush");
			} else {
				GUI.WriteLn($"Auto-Push after {cp.autopush+1} more additions");
			}
		}

		static void Delete(string id){
			var cp = CurrentProject;
			if (cp == null) { GUI.WriteLn("DELETE: No project"); return; }
			var i = qstr.ToInt(id);
			if (!cp.Indexes.ContainsKey(i)) { GUI.WriteLn("DELETE: Unidentified entry number"); return;  }
			cp.Indexes.Remove(i);
			GUI.WriteLn($"Entry #{i} has been unlinked!");
			GUI.UpdateEntries(cp.HighestRecordNumber - 200,cp.HighestRecordNumber);
		}

		static void Go(string url){
			var result = url;
			if (CurrentProject != null)
			{
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
			} else { GUI.WriteLn("WARNING! There is no project loaded, so variables will not be parsed!"); }
			GUI.WriteLn($"Opening URL: {result}");
			if (!OURI.OpenUri(result)) GUI.WriteLn($"ERROR! Opening URL {result} failed!");
		}

		static int Takes {
			get {
				if (CurrentProject == null) { Annoy("No project!"); return 0; }
				return Math.Max(0, qstr.ToInt(CurrentProject.Data.C("TAKES")));
			}
			set {
				if (CurrentProject == null) { Annoy("No project!"); return; }
				CurrentProject.Data.D("TAKES", $"{value}");
			}
		}
		static void Take(string n) {
			if (CurrentProject==null) { Annoy("No project!"); return; }
			if (n.Trim() == "")
				Takes++;
			else if (n.Trim() == "RESET")
				Takes = 1;
			else
				Takes = Math.Max(1, qstr.ToInt(n.Trim()));
			if (!CurrentProject.GotTag("TEST")) NewTag("TEST");
			AddEntry($"TEST Take {Roman.ToRoman(Takes)}");
		}

		static void Create(string project) {
			GUI.WriteLn("Creating project");
			var prj = project.Replace(" ", "_").Replace("/", ".").Replace("\\", ".");
			var np = new dvProject(prj); np.Data.CL("TAGS");
			GUI.WriteLn($"Codename: {np.PrjName}, using file {np.PrjName}");
			GUI.WriteLn("Setting up base tags");
			var tags = np.Data.List("TAGS");
			tags.Add("SITE");
			tags.Add("GENERAL");
			tags.Add("BUG");
			tags.Add("FIXED");
			GUI.WriteLn("Configuring Base tags");
			np.DefData($"HEAD.SITE", $"background-color:rgb(0,0,0); color:rgb(255,255,255);");
			np.DefData($"INHD.SITE", $"background-color:rgb(127,127,127); color:rgb(255,255,255);");
			np.DefData($"HEAD.GENERAL", $"background-color:rgb(0,0,0); color:rgb(0,255,255);");
			np.DefData($"INHD.GENERAL", $"background-color:rgb(0,127,127); color:rgb(0,255,255);");
			np.DefData($"HEAD.BUG", $"background-color:rgb(0,0,0); color:rgb(255,0,0);");
			np.DefData($"INHD.BUG", $"background-color:rgb(127,0,0); color:rgb(255,0,0);");
			np.DefData($"HEAD.FIXED", $"background-color:rgb(0,0,0); color:rgb(255,0,0);");
			np.DefData($"INHD.FIXED", $"background-color:rgb(0,127,0); color:rgb(0,255,0);");
			GUI.WriteLn("First entry!");
			new dvEntry(np, "SITE", $"Devlog created on {DateTime.Now.ToLongDateString()}; {DateTime.Now.ToLongTimeString()}.<p>Codenamed: {prj}");
			GUI.WriteLn("Saving for security's sake");
			np.SaveMe();
			GUI.WriteLn("Renew project list");
			GUI.RenewProjects();
			GUI.WriteLn($"Project {prj} has been created");
		}

		static public void Init()
		{
			MKL.Lic    ("Development Log - Command.cs","GNU General Public License 3");
			MKL.Version("Development Log - Command.cs","20.07.24");
			Commands["ANNOY"] = Annoy;
			Commands["BYE"] = Bye;
			Commands["SAY"] = delegate (string yeah) { GUI.WriteLn(yeah, true); };
			Commands["FUCK"] = delegate { Annoy("Did your mother never teach you not to say such words?"); };
			Commands["YULERIA"] = delegate { Annoy("Yuleria's rule of revenge:\nAn amateur kills people! A professional makes them suffer!"); };
			Commands["GLOBALCONFIG"] = GlobalConfig;
			Commands["USE"] = delegate (string useme) { GUI.Use(useme); };
			Commands["SYSVARS"] = delegate { if (CurrentProject == null) { GUI.WriteLn("No project!"); return; } foreach (string v in CurrentProject.Data.Vars()) GUI.WriteLn(v); };
			Commands["VARS"] = delegate { if (CurrentProject == null) { GUI.WriteLn("No project!"); return; } foreach (string v in CurrentProject.Data.Vars()) if (qstr.Prefixed(v,"VAR.")) GUI.WriteLn(v); };
			Commands["LET"] = LetVar;
			Commands["NEWTAG"] = NewTag;
			Commands["ADDTAG"] = NewTag;
			Commands["ADD"] = AddEntry;
			Commands["GEN"] = delegate { Export.Gen(); };
			Commands["ABOUT"] = delegate { 
				GUI.WriteLn($"This tool has been created and copyrighted by Jeroen P. Broks\nIs has been released under the terms of the GPL version 3\n\nCompiled on the next source files:\n\n{MKL.All()}"); 
			};
			Commands["CLH"] = delegate { GUI.ClearHistory(); };
			Commands["CLS"] = delegate { GUI.ClearConsole(); };
			Commands["PUSH"] = delegate { GUI.WriteLn("Thanks to Ziggo I cannot push yet, but that comes later!"); };
			Commands["GO"] = Go;
			Commands["UNLINK"] = Delete;
			Commands["DELETE"] = Delete;
			Commands["KILL"] = Delete;
			Commands["RM"] = Delete;
			Commands["REMOVE"] = Delete;
			Commands["DEL"] = Delete;
			Commands["CREATE"] = Create;
			Commands["SAVE"] = delegate { if (CurrentProject != null) { CurrentProject.SaveMe(); GUI.WriteLn("Saved!"); } else Annoy("No Project!"); };
			Commands["TAKE"] = Take;
			Commands["EDIT"] = delegate (string num) {
				if (CurrentProject == null) { Annoy("No Project!"); return; }
				var e = new dvEntry(CurrentProject, qstr.ToInt(num), true);
				if (e==null) { Annoy("Entry couldn't be accessed!"); return; }
				GUI.SetPrompt($"MODIFY {num} {e.Tag} {e.Pure}");
			};
			Commands["MODIFY"] = delegate (string str) {
				var args = str.Split(' ');
				if (CurrentProject == null) { Annoy("No Project!"); return; }
				if (args.Length<3) { Annoy("Modify syntax error!"); return; }
				var num = qstr.ToInt(args[0]);
				var e = new dvEntry(CurrentProject, num, true);
				if (e == null) { Annoy("Entry couldn't be accessed!"); return; }
				var tag = args[1].ToUpper();
				if (!CurrentProject.GotTag(tag)) { Annoy($"There's no tag: {tag}"); return; }
				var sb = new System.Text.StringBuilder();
				e.Tag = tag;
				for (int i = 2; i < args.Length; ++i) sb.Append($"{args[i]} ");
				e.Pure = sb.ToString().Trim();
				GUI.UpdateEntries(CurrentProject.HighestRecordNumber - 200, CurrentProject.HighestRecordNumber);
			};


		}

		static void ThrowError(string error){
			GUI.WriteLn($"ERROR:\t{error}");
			QuickGTK.Error($"!!ERROR!!\n\n{error}");
		}

		static public void DoCommand(string command){
			var c = command.Trim();
			var p = c.IndexOf(' ');
			string cmd;
			string arg;
			if (p < 0) { cmd = c.ToUpper(); arg = ""; } else {
				cmd = c.Substring(0, p).ToUpper();
				arg = c.Substring(p + 1);
			}
			if (!Commands.ContainsKey(cmd)) ThrowError($"I do not understand the command {cmd}!!"); else Commands[cmd](arg);            
		}

		static public void DoCommands(string[] commands) {
			foreach (string cmd in commands) DoCommand(cmd);
		}

	}
}