// Lic:
// Devlog
// dvproject
// 
// 
// 
// (c) Jeroen P. Broks, 2016, 2017, 2018, 2021
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


using System;
using System.Collections.Generic;
using System.IO;
using TrickyUnits;
using TrickyUnits.GTK;

namespace Devlog
{
	class dvIndex{
		public int id;
		public long size;
		public long offset;

	}

	class dvPrefix{
		readonly Dictionary<string, string> Raw = new Dictionary<string, string>();

		public dvPrefix(string Prefix="", int CD=10, int RESET=20){
			this.Prefix = Prefix;
			this.CD = CD;
			this.Reset = RESET;
		}

		public void UnParse(string key,List<string> CDPREFIX){
			CDPREFIX.Add($"NEW:{key}");
			foreach (string key2 in Raw.Keys) CDPREFIX.Add($"{key2.ToUpper()}:{Raw[key2]}");
		}

		public string Prefix { get { return Raw["PREFIX"]; } set { Raw["PREFIX"] = value; }}
		public int CD { get { return qstr.ToInt(Raw["CD"]); } set { Raw["CD"] = $"{value}"; }}
		public int Reset { get { return qstr.ToInt(Raw["RESET"]); } set { Raw["RESET"] = $"{value}"; } }
		public void RawSet(string key, string value) { Raw[key] = value; }

	}

	class dvEntry{

		bool DoUpdate = true;
		public bool Loaded = true;
		readonly public int RecID;
		readonly dvProject project;

		void ParseText(){
			core["TEXT"] = PureParse.Go(Pure);
		}

		void UpdateMe(){
			if (!DoUpdate) return;
			var newindex = new dvIndex();
			newindex.id = RecID;
			var bt = new QuickStream(System.IO.File.OpenWrite($"{project.PrjFile}.Content")); // Does this append?
			var start = bt.Size;
			bt.Position = bt.Size;
			foreach(string k in core.Keys) {
				bt.WriteByte(0);
				bt.WriteString(k);
				bt.WriteString(core[k]);
			}
			var einde = bt.Position;
			bt.Close();
			newindex.size = einde - start;
			newindex.offset = start;
			project.Indexes[RecID] = newindex;
			project.SaveIndexes();
		}

		public readonly Dictionary<string, string> core = new Dictionary<string, string>();
		public string Tag { get => core["TAG"]; set { core["TAG"] = value; UpdateMe(); }}
		public string Pure { get => core["PURE"]; set { core["PURE"] = value; ParseText(); UpdateMe(); }}
		public string Text { get => core["TEXT"]; } // No set, as this is the result as parsing from "pure".
		public string Date { get => core["DATE"]; } // No midifications needed here (yet)
		public string Time { get => core["TIME"]; }
		public string Modified { get { if (core.ContainsKey("MODIFIED")) return core["MODIFIED"]; return ""; } set { if (!core.ContainsKey("MODIFIED")) core["MODIFIED"] = value; else core["MODIFIED"] += $"; {value}"; } }

		// Creates new entry
		public dvEntry(object aprj, string atag,string apure){
			// TODO: Pure => text parsing!
			var prj = (dvProject)aprj;
			project = prj;
			DoUpdate = false; // must be first
			Tag = atag;
			Pure = apure;
			var now = DateTime.Now;
			string[] months = { "?", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
			core["DATE"] = $"{now.Day} {months[now.Month]} {now.Year}";
			core["TIME"] = $"{now.Hour}:{qstr.Right($"0{now.Minute}", 2)}";
			RecID = prj.HighestRecordNumber + 1;
			DoUpdate = true;  // must be last
			UpdateMe();
		}

		/* NO!
		// Obtains entry from .Entries file
		public dvEntry(int id,string byProject){
			DoUpdate = false; // must be first
			core["TAG"] = "UNKNOWN";
			core["PURE"] = "?";
			core["TEXT"] = "?";
			core["DATE"] = "5000 B.C, maybe?";
			core["TIME"] = "Mystery time!";
			// None of the values above may ever pop up, but ya never know!
			int tid = -10;
			foreach(string fline in QOpen.LoadString($"{MainClass.WorkSpace}/Entries/{byProject}.Entries").Split('\n')){
				var line = fline.Trim();
				if (line != ""){
					if (line == "PUSH"){
						if (tid == id) { RecID=id; return; }
					} else {
						var pos = line.IndexOf(':');
						if (pos < 0) Console.WriteLine("WARNING! Malformed line in entry file!"); else {
							var key = line.Substring(0, pos).ToUpper().Trim();
							var value = line.Substring(pos).Trim();
							if (key == "NEW") tid = qstr.ToInt(value); else if (tid==id) core[key.ToUpper()] = value;
						}
					}
				}
			}
			DoUpdate = true;  // must be last
		}
		*/

		public dvEntry(object aprj, int want, bool no404=false) { //}, int max) {
			var prj = (dvProject)aprj;
			RecID = want;
			project = prj;
			core["TAG"] = "UNKNOWN";
			core["PURE"] = "?";
			core["TEXT"] = "?";
			core["DATE"] = "5000 B.C, maybe?";
			core["TIME"] = "Mystery time!";
			Loaded = false;
			// None of the values above may ever pop up, but ya never know!
			/* old code 
			int id = -100;
			string line;
			byte ch;
			do {
				line = "";
				while (true) {
					ch = bt.ReadByte();
					if (ch == 10) break;
					if (bt.EOF) { Loaded = false; return; }
					line += qstr.Chr(ch);
				}
				line = line.Trim();
				var sl = line.Split(':');
				if (line!="" && sl[0].Trim().ToUpper()=="NEW") {
					string eline;
					if (sl.Length != 2) { Loaded = false; GUI.WriteLn("ERROR IN ENTRY IDENTIFICATION!"); return;  }
					id = qstr.ToInt(sl[1].Trim());
					RecID = id;
					do {
						eline = "";
						while (true) {
							ch = bt.ReadByte();
							if (ch == 10) break;
							if (bt.EOF) { Loaded = false; return; }
							eline += qstr.Chr(ch);
						}
						eline = eline.Trim();
						if (eline!=""){
							if (eline!="PUSH"){
								var pos = eline.IndexOf(':');
								if (pos < 0) Console.WriteLine("WARNING! Malformed line in entry file!"); else {
									var key = eline.Substring(0, pos).ToUpper().Trim();
									var value = eline.Substring(pos+1).Trim();
									core[key] = value;
								}
							}
						}
					} while (eline != "PUSH");
				}
			} while (id < 0 || id < min || id > max);

		} */
			if (want < 0) return;  // Ignore stuff that is too low
								   //var bix = QOpen.ReadFile($"{prj}.Index");
			var bcn = QuickStream.ReadFile($"{prj.PrjFile}.Content");
			byte tag = 0;
			if (!prj.Indexes.ContainsKey(want)) { if (!no404) GUI.WriteLn($"ERROR! Index {want} not found!"); goto closure; }
			var Index = prj.Indexes[want];
			int id = Index.id; //-100;
			long size = Index.size;
			long offset = Index.offset;
			long einde = offset + size;
			bcn.Position = offset;
			while (bcn.Position < einde) {
				if (bcn.Position > einde) { GUI.WriteLn("ERROR! Index read pointer exceeds ending point!"); goto closure; }
				tag = bcn.ReadByte();
				if (tag != 0) { GUI.WriteLn($"ERROR! Unknown content command tag {tag}"); goto closure; }
				var key = bcn.ReadString();
				var value = bcn.ReadString();
#if DEBUG
				Console.WriteLine($"{key} = \"{value}\"");
#endif
				core[key.ToUpper()] = value;
			}
			Loaded = true;
		closure:
			//bix.Close();
			bcn.Close();
		}
	}

	class dvProject
	{
		public Dictionary<int, dvIndex> Indexes = new Dictionary<int, dvIndex>();
		public SortedDictionary<string, dvPrefix> Prefixes = new SortedDictionary<string, dvPrefix>();
		public int autopush = 10;
		string myname;
		string myfile;
		//int countrecords = -1;
		//int highestrecordnumber = -1;

		public string PrjFile { get { return myfile; }}
		public string PrjName { get { return myname; }}


		static Dictionary<string, dvProject> LoadedProjects = new Dictionary<string, dvProject>();

		static public dvProject Get(string prjname) {
			try {
				if (LoadedProjects.ContainsKey(prjname)) return LoadedProjects[prjname];
				GUI.WriteLn($"Loading project: {prjname}");
				var ret = new dvProject();
				ret.myname = prjname;
				ret.myfile = Dirry.AD($"{MainClass.WorkSpace}/Projects/{ret.myname}");
				try {
					ret.Data = GINI.ReadFromFile($"{ret.myfile}.prj");
				} catch {
					GUI.WriteLn($"ERROR:\tI could not read {ret.myfile}.prj");
					QuickGTK.Error($"!!ERROR!!\n\nI could not read {ret.myfile}.prj");
					return null;
				} finally {
					GUI.WriteLn("Complete!");
				}
				if (ret.Data == null) {
					GUI.WriteLn($"ERROR:\tI could not read {ret.myfile}.prj");
					QuickGTK.Error($"!!ERROR!!\n\nI could not read {ret.myfile}.prj");
					return null;
				}
				// Load Indexes
				if (!File.Exists(ret.myfile)) {
					GUI.WriteLn($"Creating index file: {ret.myname}");
					var BT=QuickStream.WriteFile(ret.myfile);
					BT.Close();
				}
				var bix = QuickStream.ReadFile($"{ret.myfile}.Index");
				byte tag;
				dvIndex Index;
				GUI.WriteLn($"Loading indexes for project: {prjname}");
				while (!bix.EOF) {
					//if (bix.EOF) { Console.WriteLine($"ERROR! Record {want} not found"); goto closure; } // I hate the "goto" command but in this particular case it's the safest way to go! (No I do miss the "defer" keyword Go has!)
					tag = bix.ReadByte();
					if (tag != 0) { GUI.WriteLn($"ERROR! Unknown index command tag {tag}!"); ret = null; goto closure; }
					Index = new dvIndex();
					Index.id = bix.ReadInt();
					Index.size = bix.ReadLong();
					Index.offset = bix.ReadLong();
					if (ret.Indexes.ContainsKey(Index.id)) GUI.WriteLn($"WARNING!!! Duplicate index #{Index.id}. One of them will overwrite another");
					ret.Indexes[Index.id] = Index;
					//einde = offset + size;
				} //while (id != want);//(id < 0 || id < min || id > max);
				LoadedProjects[prjname] = ret;
				GUI.WriteLn($"Records:        {ret.CountRecords}");
				GUI.WriteLn($"Highest number: {ret.HighestRecordNumber}");
				ret.Data.CL("CDPREFIX");
				dvPrefix pref = null;
				foreach (string iline in ret.Data.List("CDPREFIX")) {
					var line = iline.Trim();
					if (line != "") {
						var p = line.IndexOf(':');
						if (p < 0) {
							GUI.WriteLn("Invalid Prefix definition!");
						} else {
							var key = line.Substring(0, p).Trim().ToUpper();
							var value = line.Substring(p + 1).Trim();
							if (key == "NEW") {
								pref = new dvPrefix();
								ret.Prefixes[value] = pref;
								GUI.WriteLn($"Prefix added:   {value}");
							} else if (pref == null) {
								GUI.WriteLn("Definition without prefix! Please check your prefix settings!");
							} else {
								pref.RawSet(key, value);
								Console.WriteLine($"Prefix[\"{key}\"]=\"{value}\";");
							}
						}
					}
				}
			closure:
				bix.Close();
				return ret;
			} catch (Exception E) {
				QuickGTK.Error(E.Message);
				return null;
			}
		}

		public void SaveIndexes(){
			var bix = QuickStream.WriteFile($"{myfile}.Index");
			foreach(dvIndex idx in Indexes.Values){
				bix.WriteByte(0);
				bix.WriteInt(idx.id);
				bix.WriteLong(idx.size);
				bix.WriteLong(idx.offset);
			}
			bix.Close();
		}

		public bool GotTag(string tag) => Data.List("TAGS").Contains(tag.Trim().ToUpper());

		public string EntryFile { get { return $"{myfile}.Entries"; }}
		public TGINI Data = new TGINI();
		public int CountRecords { get {
				/* old
				if (countrecords >= 0) return countrecords;
				var lines = QOpen.LoadString($"{myfile}.entries").Split('\n');
				foreach(string tline in lines){
					var line = tline.Trim();
					var sline = line.Split(':');
					if (sline.Length==2 && sline[0].Trim()=="NEW"){
						countrecords++;
						var rn = qstr.ToInt(sline[1].Trim());
						if (highestrecordnumber < rn) highestrecordnumber = rn;
					}
				}
				return countrecords;
				*/
				return Indexes.Count;
			}}
		public int HighestRecordNumber { get{
				var hi = 0;
				/*
				if (highestrecordnumber < 0) i = CountRecords;
				return highestrecordnumber;
				*/
				foreach (dvIndex i in Indexes.Values) if (hi < i.id) hi = i.id;
				return hi;
			}
		}

		public void DefData(string f, string v) {
			Data.D(f, v);
			SaveMe();
		}
		public string GetData(string f) => Data.C(f);
		public string GetDataDefault(string f, string defaultvalue, bool autosave = true) {
			var ret = Data.C(f);
			if (ret == "") {
				ret = defaultvalue;
				if (autosave) DefData(f, defaultvalue);
			}
			return ret;
		}

		public int GetDataInt(string f) => qstr.ToInt(GetData(f));
		public int GetDataDefaultInt(string f, int defaultvalue, bool autosave = true) => qstr.ToInt(GetDataDefault(f, $"{defaultvalue}", autosave));

		public string GitHub { get => Data.C("GITHUBREPOSITORY").Trim();  set { Data.D("GITHUBREPOSITORY", value); SaveMe(); } }
		public string Target { get => Data.C($"TARGET.{MainClass.Platform}").Trim();  set { Data.D($"TARGET.{MainClass.Platform}",value); SaveMe(); }}
		public string Template { get => Data.C($"TEMPLATE.{MainClass.Platform}").Trim(); set { Data.D($"TEMPLATE.{MainClass.Platform}", value); SaveMe(); }}

		static public void Hi(){
			MKL.Version("Development Log - dvProject.cs","21.08.28");
			MKL.Lic    ("Development Log - dvProject.cs","GNU General Public License 3");
		}

		public void SaveMe(){
			try {
				Console.WriteLine($"Saving: {myfile}");
				var CDPREFIX = Data.List("CDPREFIX");
				CDPREFIX.Clear();
				foreach (string key in Prefixes.Keys) {
					Prefixes[key].UnParse(key, CDPREFIX);
				}
				Data.SaveSource(Dirry.AD(myfile + ".prj"));
			} catch(Exception e) {
                QuickGTK.Error($"!!ERROR!!\n\nI could not write {myfile}.prj\n\n{e.Message}");
            }
		}

		public dvProject() {
			System.Diagnostics.Debug.WriteLine("Creating project record 'on-the-fly'");
		}

		public dvProject(string name) {
			myname = name;
			myfile = $"{MainClass.WorkSpace}/Projects/{myname}";
			System.Diagnostics.Debug.WriteLine($"Project record '{myname}' created!");
		}
	}
}