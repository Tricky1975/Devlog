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
// Version: 18.11.17
// EndLic

using System;
using System.Collections.Generic;
using TrickyUnits;
using TrickyUnits.GTK;

namespace Devlog
{
	class dvIndex{
		public int id;
		public long size;
		public long offset;

	}

    class dvEntry{

        bool DoUpdate = true;
		public bool Loaded = true;
		readonly public int RecID;
		readonly dvProject project;

        void ParseText(){
            // Actual parsing comes later!
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
			core["TEXT"] = apure; // TODO: REMOVE THIS LINE ONCE ACTUAL TEXT PARSING'S DONE!!!!
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

		public dvEntry(object aprj, int want) { //}, int max) {
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
			var bcn = QOpen.ReadFile($"{prj.PrjFile}.Content");
			byte tag = 0;
			if (!prj.Indexes.ContainsKey(want)) { GUI.WriteLn($"ERROR! Index {want} not found!"); goto closure; }
			var Index = prj.Indexes[want];
			int id = Index.id; //-100;
			long size = Index.size;
			long offset = Index.offset;
			long einde = offset + size;
			bcn.Position = offset;
			while (bcn.Position < einde) {
				if (bcn.Position > einde) { GUI.WriteLn("ERROR! Index read pointer exceeds ending point!"); goto closure; }
				tag = bcn.ReadByte();
				if (tag != 0) { GUI.WriteLn($"ERRROR! Unknown content command tag {tag}"); goto closure; }
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
		public int autopush = 10;
		string myname;
		string myfile;
		//int countrecords = -1;
		//int highestrecordnumber = -1;

		public string PrjFile { get { return myfile; }}
		public string PrjName { get { return myname; }}

        static Dictionary<string, dvProject> LoadedProjects = new Dictionary<string, dvProject>();
        static public dvProject Get(string prjname){
            if (LoadedProjects.ContainsKey(prjname)) return LoadedProjects[prjname];
            GUI.WriteLn($"Loading project: {prjname}");
            var ret = new dvProject();
            ret.myname = prjname;
            ret.myfile = $"{MainClass.WorkSpace}/Projects/{ret.myname}";
            try {
                ret.Data = GINI.ReadFromFile($"{ret.myfile}.prj");
            } catch {
                GUI.WriteLn($"ERROR:\tI could not read {ret.myfile}.prj");
                QuickGTK.Error($"!!ERROR!!\n\nI could not read {ret.myfile}.prj");
                return null;
            } finally {
                GUI.WriteLn("Complete!");
            }
            if(ret.Data==null){
                GUI.WriteLn($"ERROR:\tI could not read {ret.myfile}.prj");
                QuickGTK.Error($"!!ERROR!!\n\nI could not read {ret.myfile}.prj");
                return null;
            }
			// Load Indexes
			var bix = QOpen.ReadFile($"{ret.myfile}.Index");
			byte tag;
			dvIndex Index;
			GUI.WriteLn($"Loading indexes for project: {prjname}");
			while (!bix.EOF) {
				//if (bix.EOF) { Console.WriteLine($"ERROR! Record {want} not found"); goto closure; } // I hate the "goto" command but in this particular case it's the safest way to go! (No I do miss the "defer" keyword Go has!)
				tag = bix.ReadByte();
				if (tag != 0) { GUI.WriteLn($"ERROR! Unknown index command tag {tag}!"); ret=null; goto closure; }
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
		closure:
			bix.Close();
			return ret;
        }

		public void SaveIndexes(){
			var bix = QOpen.WriteFile($"{myfile}.Index");
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
            MKL.Version("Development Log - dvProject.cs","18.11.17");
            MKL.Lic    ("Development Log - dvProject.cs","GNU General Public License 3");
        }

        public void SaveMe(){
			Console.WriteLine($"Saving: {myfile}");
			Data.SaveSource(myfile+".prj");
		} 

        public dvProject()
        {
        }
    }
}
