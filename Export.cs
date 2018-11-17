// Lic:
// 	Devlog
// 	Export
// 	
// 	
// 	
// 	(c) Jeroen P. Broks, 2018, All rights reserved
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

#undef TempOutput


using System;
using TrickyUnits;
using TrickyUnits.GTK;
namespace Devlog
{
	public class Export
	{
		static dvProject CurrentProject { get { if (GUI.CurrentProjectName == "") return null; return dvProject.Get(GUI.CurrentProjectName); } }
#if TempOutput
		const string OutDir = "/Users/Rachel/Projects/VisualStudio/DevLog/MKL_TempExport";
#else
		static string OutDir { get { return CurrentProject.Target.Trim(); } }
#endif


		static public void Hello(){
			MKL.Version("Development Log - Export.cs","18.11.17");
			MKL.Lic    ("Development Log - Export.cs","GNU General Public License 3");
		}


		static public void Gen(){
			System.IO.Directory.CreateDirectory(OutDir);
			string template;
			var cp = CurrentProject; if (cp == null) { GUI.WriteLn("GEN: No project!"); return; }
			int pages = cp.CountRecords / 200;
			int page = 1;
			int pcountdown = 200;
			bool justnewpaged = false;
			string olddate = "____OLD____";
			string[] iconext = { "png", "gif", "svg", "jpg" };
			if (cp.CountRecords % 200 > 0) pages++;
			try{
				template = QOpen.LoadString(cp.Template.Trim());
			} catch {
				QuickGTK.Error($"Template {cp.Template} could not be properly loaded!");
				GUI.WriteLn($"GEN:Template {cp.Template} could not be properly loaded!");
				return;
			}
			GUI.Write("Exporting...");
			var pageline = "";
			for (int p = 1; p <= pages; p++) {
				if (page == p) pageline += $"<big><big>{p}</big><big> "; else pageline += $"<a href='{cp.PrjName}_DevLog_Page_{p}.html'>{p}</a> ";
			}
			pageline = pageline.Trim();
			var content = $"<table style=\"width:{cp.GetDataDefaultInt("EXPORT.TABLEWIDTH", 1200)}\">\n";
			content += $"<tr><td colspan=3 align=center>{pageline}</td></tr>\n";
			for (int i = cp.HighestRecordNumber; i > 0;i--){
				//if (i % 6 == 0) { GUI.Write("."); Console.Write($".{i}."); }
				justnewpaged = false;
				var rec = new dvEntry(cp, i);
				if (rec.Date != olddate) content += $"<tr><td align=center colspan=3 style='font-size:30pt;'>- = {rec.Date} = -</td></tr>\n"; olddate = rec.Date;
				string headstyle = cp.Data.C($"HEAD.{rec.Tag.ToUpper()}");
				string contentstyle = cp.Data.C($"INHD.{rec.Tag.ToUpper()}");
				content += $"<tr valign=top><td align=left><a id='dvRec_{rec.RecID}'></a>{rec.Time}</td><td style=\"{headstyle}\">{rec.Tag}</td><td style='width: { cp.GetDataDefaultInt("EXPORT.CONTENTWIDTH", 800)}; {contentstyle}'><div style=\"width: { cp.GetDataDefaultInt("EXPORT.CONTENTWIDTH", 800)}; overflow-x:auto;\">";
				var icon = $"{OutDir}/Icons/{rec.Tag.ToLower()}";
				icon = icon.Replace("#", "hashtag");
				foreach(string pfmt in iconext) {
					var iconfile = $"{icon}.{pfmt}";
					iconfile = iconfile.Replace("#", "%23");
					if (System.IO.File.Exists(iconfile)) { content += $"<img style='float:{cp.GetDataDefault("EXPORT.ICONFLOATPOSITION", "Right")}; height:{cp.GetDataDefaultInt("EXPORT.ICONHEIGHT", 50)}' src='{iconfile}' alt='{rec.Tag}'>"; break; }
				}
				content += $"{rec.Text}</div></td></tr>\n";
				pcountdown--;
				if (pcountdown<=0){
					pcountdown = 200;
					justnewpaged = true;
					content += $"<tr><td colspan=3 align=center>{pageline}</td></tr>\n</table>\n\n";
					QOpen.SaveString($"{OutDir}/{cp.PrjName}_DevLog_Page_{page}.html",template.Replace("@CONTENT@",content));
					page++;
					pageline = "";
					for (int p = 1; p <= pages; p++) {
						if (page == p) pageline += $"<big><big>{p}</big><big> "; else pageline += $"<a href='{cp.PrjName}_DevLog_Page_{p}.html'>{p}</a> ";
					}
					content = $"<table style=\"width:{cp.GetDataDefaultInt("EXPORT.TABLEWIDTH", 1200)}\">\n";
					content += $"<tr><td colspan=3 align=center>{pageline}</td></tr>\n";
				}
			}
			if (!justnewpaged) {
				content += $"<tr><td colspan=3 align=center>{pageline}</td></tr>\n</table>";
				QOpen.SaveString($"{OutDir}/{cp.PrjName}_DevLog_Page_{page}.html", template.Replace("@CONTENT@", content));
			}
			GUI.WriteLn(" Done");
			Console.WriteLine(" GENERATED");
		}
	}
}
