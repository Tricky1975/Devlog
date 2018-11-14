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
// Version: 18.11.14
// EndLic

using System;
using System.Collections.Generic;
using TrickyUnits;
using TrickyUnits.GTK;

namespace Devlog
{
    class dvEntry{

        bool DoUpdate = true;
		public bool Loaded = true;

        void ParseText(){
            // Actual parsing comes later!
        }

        void UpdateMe(){
            if (!DoUpdate) return;
            // TODO: Actual updating
        }

        public readonly Dictionary<string, string> core = new Dictionary<string, string>();
        public string Tag { get => core["TAG"]; set { core["TAG"] = value; UpdateMe(); }}
        public string Pure { get => core["PURE"]; set { core["PURE"] = value; ParseText(); UpdateMe(); }}
        public string Text { get => core["TEXT"]; } // No set, as this is the result as parsing from "pure".
        public string Date { get => core["DATE"]; } // No midifications needed here (yet)
        public string Time { get => core["TIME"]; }
        public string Modified { get { if (core.ContainsKey("MODIFIED")) return core["MODIFIED"]; return ""; } set { if (!core.ContainsKey("MODIFIED")) core["MODIFIED"] = value; else core["MODIFIED"] += $"; {value}"; } }

        // Creates new entry
        public dvEntry(string atag,string apure, string byProject){
            DoUpdate = false; // must be first
            Tag = atag;
            Pure = apure;
            DoUpdate = true;  // must be last
        }

        // Obtains entry from .Entries file
        public dvEntry(int id,string byProject){
            DoUpdate = false; // must be first
            int tid = -10;
            foreach(string fline in QOpen.LoadString($"{MainClass.WorkSpace}/Entries/{byProject}.Entries").Split('\n')){
                var line = fline.Trim();
                if (line != ""){
                    if (line == "PUSH"){
                        if (tid == id) { return; }
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

		public dvEntry(QuickStream bt,int min, int max) {
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
					id = qstr.ToInt(sl[1]);
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
								var pos = line.IndexOf(':');
								if (pos < 0) Console.WriteLine("WARNING! Malformed line in entry file!"); else {
									var key = eline.Substring(0, pos).ToUpper().Trim();
									var value = eline.Substring(pos);
									core[key] = value;
								}
							}
						}
					} while (eline != "PUSH");
				}
			} while (id >= 0 && id > min && id < max);
		}
    }

    class dvProject
    {
        string myname;
        string myfile;
		int countrecords = -1;
		int highestrecordnumber = -1;

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
            LoadedProjects[prjname] = ret;
			GUI.WriteLn($"Records:        {ret.CountRecords}");
			GUI.WriteLn($"Highest number: {ret.HighestRecordNumber}");
            return ret;
        }

        public TGINI Data = new TGINI();
		public int CountRecords { get {
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
			}}
		public int HighestRecordNumber { get{
				var i = 0;
				if (highestrecordnumber < 0) i = CountRecords;
				return highestrecordnumber;
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

		public string GitHub { get => Data.C("GITHUBREPOSITORY");  set { Data.D("GITHUBREPOSITORY", value); SaveMe(); } }
        public string Target { get => Data.C($"TARGET.{MainClass.Platform}");  set { Data.D($"TARGET.{MainClass.Platform}",value); SaveMe(); }}
        public string Template { get => Data.C($"TARGET.{MainClass.Platform}"); set { Data.D($"TARGET.{MainClass.Platform}", value); SaveMe(); }}

        static public void Hi(){
            MKL.Version("Development Log - dvProject.cs","18.11.14");
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
