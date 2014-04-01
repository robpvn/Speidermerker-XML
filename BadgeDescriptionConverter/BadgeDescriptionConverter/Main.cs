using System;
using System.Net;
using System.Linq;
using System.Xml.Linq;
using HtmlAgilityPack;

namespace BadgeDescriptionConverter
{
	/// <summary>
	/// Simple app for downloading badge descriptions from Speiderbasen and converting them to XML segments
	/// </summary>
	class MainClass
	{
		public static void Main (string[] args)
		{
			//Quick and dirty, no concurrency or error handling!
			Console.WriteLine ("Downloading web page");

			WebClient client = new WebClient();
//

//			string url = args[0];
			string url = "http://speiderbasen.no/?side=bever.programmet.programmerker.friluftsliv";
			string page = client.DownloadString(url);


			HtmlDocument pageHTML = new HtmlDocument();
			pageHTML.OptionFixNestedTags = true;
			pageHTML.LoadHtml(page);


			Console.WriteLine ("Opening XML file");

			XDocument merkerXML = XDocument.Load("Speidermerker.xml");
			XNamespace ns = "http://www.bygdoyspeider.net/Speidermerker";

			Console.WriteLine ("Processing web page");

			//Remove http://speiderbasen.no/?side=
			string level = url.Remove(0, 29).Split('.')[0];

			Console.WriteLine("Level:");
			Console.WriteLine(level);

			string programBadgeRootName;

			switch (level) {
				case "bever":
					programBadgeRootName = "BeverMerker";
					break;
				default:
					throw new ArgumentException("No known level");
			}

			XNode programBadgeRoot = merkerXML.Root.Descendants(ns + programBadgeRootName).Descendants(ns + "ProgramMerker").First();

			//Betting heavy that all the pages are similar!
			string badgeTitle =	pageHTML.DocumentNode.Descendants("div")
				.Where(x => x.Attributes["class"].Value == "sb_sideoverskrift").First().InnerText;

			Console.WriteLine("Badge Title:");
			Console.WriteLine(badgeTitle);

			string badgeImg =	pageHTML.DocumentNode.Descendants("div")
				.Where(x => x.Attributes["class"].Value == "sb_sideinnhold").First().Descendants("img").First().Attributes["src"].Value;

			Console.WriteLine("Badge Image:");
			Console.WriteLine(badgeImg);
			client.DownloadFile("http://speiderbasen.no" + badgeImg, "images/" + badgeImg.Split('/')[3]);

			Console.WriteLine("Parsing requirements");

			HtmlNode reqTable = pageHTML.DocumentNode.Descendants("table").Where(x => x.Attributes["class"].Value == "maal_liste").First();

			//TODO: Use these values to construct XML snippets
			foreach (var row in reqTable.ChildNodes) {
				if (row.FirstChild.Attributes["class"].Value == "maal_gruppering") {
					Console.WriteLine("Group: " + row.FirstChild.InnerText);
				} else if (row.FirstChild.Name == "td" && row.FirstChild.Attributes["class"].Value == "maal_liste") {
					string code = row.ChildNodes.ElementAt(0).InnerText;
					Console.WriteLine(code);
					string desc = row.ChildNodes.ElementAt(1).InnerText;
					Console.WriteLine(desc);
					Console.WriteLine("Tasks: ");
					foreach (var task in row.ChildNodes.ElementAt(2).FirstChild.Descendants("li")) {
						string teskDesc = task.InnerText;
						Console.WriteLine(teskDesc);
					}


				}
			}

		}
	}
}
