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
using TrickyUnits;


namespace Devlog
{
    class MainClass
    {
        readonly static string configfile = Dirry.C("$AppSupport$/.DevlogConfig.GINI");
        static TGINI vConfig = new TGINI();
        static public string Platform { get => vConfig.C("PLATFORM"); }
        static public string WorkSpace { get => vConfig.C("WORKSPACE"); }

        static void InitSubClasses(){
            MKL.Version("Development Log - Devlog.cs","18.11.14");
            MKL.Lic    ("Development Log - Devlog.cs","GNU General Public License 3");
            CommandClass.Init();
            dvProject.Hi();
			TrickyUnits.GTK.ListBox.Hello();
        }

        static void SaveConfig() => vConfig.SaveSource(configfile);

        static void LoadConfig(){
            if (!System.IO.File.Exists(configfile)) SaveConfig();
            vConfig = GINI.ReadFromFile(configfile);
            var code = "UNK";
            var name = "UNKNOWN";
            if (vConfig.C("PLATFORM")=="")
            {
                int p = (int)Environment.OSVersion.Platform;
                if ((p == 4) || (p == 6) || (p == 128))
                {
                    //Console.WriteLine("Running on Unix");
                    if (p==4){
                        code = "MAC";
                        name = "a Mac";
                    } else {
                        code = "UNIX";
                        name = "Unix";
                    }
                }
                else
                {
                    //Console.WriteLine("NOT running on Unix");
                }
                TrickyUnits.GTK.QuickGTK.Info($"It appears you are running this on {name}. If this is not correct, please correct this in the file: {configfile}.\n\n(I will not ask this again. I had to note this, as this tool has been written in C# and C# has pretty poor platform recognition).");
                DefConfig("platform", code);
            }
        }

        static public void DefConfig(string f,string v){
            vConfig.D(f, v);
            SaveConfig();
        }

        static public string GetConfig(string f) => vConfig.C(f);

        static public string GetConfigDefault(string f,string defaultvalue, bool autosave=true){
            var ret = vConfig.C(f);
            if (ret=="") {
                ret = defaultvalue;
                if (autosave) DefConfig(f, defaultvalue);
            }
            return ret;
        }

        static public int GetConfigInt(string f) => qstr.ToInt(GetConfig(f));
        static public int GetConfigDefaultInt(string f, int defaultvalue, bool autosave = true) => qstr.ToInt(GetConfigDefault(f, $"{defaultvalue}", autosave));

        public static void Main(string[] args)
        {
            InitSubClasses();
            // TODO: args recognition!
            GUI.Init();
            LoadConfig();
            GUI.RenewProjects();
            GUI.Start();
        }
    }
}
