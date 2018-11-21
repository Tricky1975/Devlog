// Lic:
// 	Development Log
// 	Pure parser
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
// Version: 18.11.21
// EndLic

using System;
using System.Text.RegularExpressions;
using TrickyUnits;
namespace Devlog
{
	public class PureParse
	{

		public static string Go(string pure){
			var words = pure.Split(' ');
			var cp = dvProject.Get(GUI.CurrentProjectName);
			var content = "";
			if (cp == null) { GUI.WriteLn("Internal error! Parse attempt without a project!"); return ""; }
			foreach( string word in words ){
				var hashtag = word.IndexOf('#');
				var newword = word;
				if (qstr.Prefixed(word,"##")){
					newword = word.Substring(1).Replace("##","#");
				} else if (hashtag>=0 && qstr.ToInt(word.Substring(hashtag+1))>0){
					var rsplit = word.Split('#');
					if (Regex.Match(word, "##").Success) { newword = word.Replace("##", "#"); }
					else if (rsplit.Length>2) { newword = word.Replace("##", "#"); }
					else if (qstr.Left(word,1)=="#") {
						if (cp.GitHub!=""){
							newword = $"<a href='http://github.com/{cp.GitHub}/issues/{qstr.Right(word, qstr.Len(word) - 1)}'>{word}</a>";
						} else {
							newword = word;
							GUI.WriteLn("WARNING! No github repository to refer to!");
						}
					} else {
						newword = $"<a href='http://github.com/{rsplit[0]}/issues/{rsplit[1]}'>{word}</a>";
					} 
				} else if (qstr.Left(word,1)=="$"){
					newword = cp.Data.C($"VAR.{word.Substring(1)}");
				}
				if (content != "") content += " ";
				content += newword;
			}
			/* Original Blitz Code
		For word=EachIn words
			hashtag = word.find("#")
			If Prefixed(word,"##")
				newword = Right(word,(Len Word)-1)
			ElseIf hashtag>=0 And word[hashtag+1..].toint()
				rsplit = word.split("#")
				If word.find("##")>=0
					newword = Replace(word,"##","#")
				ElseIf Len(rsplit)>2
					newword = Replace(word,"##","#")
				ElseIf Left(word,1)="#"
					If project.C("githubrepository")
						newword = "<a href='http://github.com/"+Project.c("GitHubRepository")+"/issues/"+Right(word,Len(word)-1)+"'>"+word+"</a>"
						'commit:+"Devlog referred to: "+Project.c("GitHubRepository")+word+";~n"
					Else
						newword = word
						echo "WARNING! No github repository!",100,80,0
					EndIf	
				Else
					newword = "<a href='http://github.com/"+rsplit[0]+"/issues/"+rsplit[1]+"'>"+word+"</a>"
					'commit:+"Devlog referred to: "+Word+";~n "
				EndIf
			ElseIf Left(word,1)="$"
				newword = project.c("VAR."+Right(word,Len(word)-1))
				If Not newword newword=word
			Else
				newword=word	
			EndIf
			If content content:+" "
			content:+newword
		Next

						 */
			return content;

		}

		public PureParse()
		{
		}
	}
}
