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
// Version: 18.11.06
// EndLic
﻿using System;
using System.Collections.Generic;
using TrickyUnits;
using TrickyUnits.GTK;

namespace Devlog
{

    delegate void MyCommand(string arg);

    static class CommandClass
    {
        static public readonly Dictionary<string, MyCommand> Commands = new Dictionary<string, MyCommand>();

        static void Annoy(string arg) => QuickGTK.Info(arg); // This is just a test ;)

        static public void Init(){
            MKL.Lic    ("Development Log - Command.cs","GNU General Public License 3");
            MKL.Version("Development Log - Command.cs","18.11.06");
            Commands["ANNOY"] = Annoy;
        }

    }
}