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
// Version: 18.11.10
// EndLic

﻿using System;
using System.Collections.Generic;
using TrickyUnits;

namespace Devlog
{
    class dvEntry{

        void ParseText(){
            // Actual parsing comes later!
        }

        public readonly Dictionary<string, string> core = new Dictionary<string, string>();
        public string Tag { get => core["TAG"]; set { core["TAG"] = value; }}
        public string Pure { get => core["PURE"]; set { core["PURE"] = value; ParseText(); }}
        public string Text { get => core["TEXT"]; } // No set, as this is the result as parsing from "pure".
        public string Date { get => core["DATE"]; } // No midifications needed here (yet)
        public string Time { get => core["TIME"]; }
    }

    class dvProject
    {
        public TGINI Data = new TGINI();

        public string GitHub { get => Data.C("GITHUBREPOSITORY");  set { Data.D("GITHUBREPOSITORY", value); SaveMe(); } }
        public string Target { get => Data.C($"TARGET.{MainClass.Platform}");  set { Data.D($"TARGET.{MainClass.Platform}",value); SaveMe(); }}
        public string Template { get => Data.C($"TARGET.{MainClass.Platform}"); set { Data.D($"TARGET.{MainClass.Platform}", value); SaveMe(); }}

        static public void Hi(){
            MKL.Version("Development Log - dvProject.cs","18.11.10");
            MKL.Lic    ("Development Log - dvProject.cs","GNU General Public License 3");
        }

        public void SaveMe(){} // TODO: Save me ;)

        public dvProject()
        {
        }
    }
}
