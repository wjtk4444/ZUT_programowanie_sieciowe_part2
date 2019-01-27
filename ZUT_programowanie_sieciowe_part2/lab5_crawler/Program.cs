using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace lab5_crawler
{
    class Program
    {
        static string emailPattern = @"([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" + @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)";

        static HashSet<string> visitedPages = new HashSet<string>();
        static WebClient webClient = new WebClient();
        static XElement xRoot = null;

        static Regex imageRegex = new Regex("<img(.*?)src=\"(?<IMAGE>.+?)\"");
        static Regex emailRegex = new Regex($"(.*?)(?<EMAIL>{emailPattern})(.*?)");
        static Regex fileRegex = new Regex("<a(.*?)href=\"(?<FILE>" + @".+?\." + "html?)\"");

        static int MAX_DEPTH = 3;

        static void Main(string[] args)
        {
            if (args.Length < 2 || args.Length > 3)
            {
                Console.WriteLine($"USAGE: {System.Diagnostics.Process.GetCurrentProcess().ProcessName} <url> <out.xml> [max_depth = 3]");
                Console.ReadLine();
                return;
            }

            string url = args[0];
            string outFile = args[1];
            if (args.Length == 3)
                MAX_DEPTH = int.Parse(args[2]);

            xRoot = new XElement("SITE", new XAttribute("url", url));

            crawl(url, xRoot);

            xRoot.Save(outFile);
            Console.WriteLine("XML file written.");
            Console.ReadLine();
        }

        static void crawl(string url, XElement xRoot, int depth = 0)
        {
            if (depth == MAX_DEPTH)
                return;

            if (visitedPages.Contains(url))
                return;

            if (depth != 0)
                System.Threading.Thread.Sleep(1000);

            visitedPages.Add(url);

            for(int i = 0; i < depth; i++)
                Console.Write("\t");
            Console.WriteLine("crawling: " + url);

            string pageContents = "";
            try
            {
                pageContents = webClient.DownloadString(url);
            }
            catch
            {
                return;
            }

            foreach (string line in pageContents.Split('\n'))
            {
                var result = imageRegex.Match(line);
                if (result.Groups["IMAGE"].Success)
                    xRoot.Add(new XElement("IMAGE", result.Groups["IMAGE"].Value));

                result = emailRegex.Match(line);
                if (result.Groups["EMAIL"].Success)
                    xRoot.Add(new XElement("EMAIL", result.Groups["EMAIL"].Value));

                result = fileRegex.Match(line);
                if (result.Groups["FILE"].Success)
                {
                    string val = result.Groups["FILE"].Value;
                    if (!val.StartsWith("http"))
                    {
                        if (url[url.Length - 1] == '/')
                            val = url + val;
                        else
                            val = url + '/' + val;
                    }

                    XElement xElement = new XElement("SITE", new XAttribute("url", val));
                    xRoot.Add(xElement);
                    crawl(val, xElement, depth + 1);
                }
            }
        }
    }
}
