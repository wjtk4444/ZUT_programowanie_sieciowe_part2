using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace lab4_FTP
{
    class FTPClient
    {
        public FTPClient(string hostname, int port, string username, string password, string initialDirectory = "/")
        {
            this.hostname = hostname;
            this.port = port;
            this.initialDirectory = initialDirectory;
            credentials = new NetworkCredential(username, password);
        }

        /// <summary>
        /// Returns directory contents as a list of strings
        /// directories come first, files come last
        /// directories are separated from files by a single "-" entry
        /// Example output:
        /// dir1
        /// dir2
        /// dir3
        /// -
        /// file1
        /// file2
        /// file3
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public List<string> getDirectoryContents(string directory = null)
        {
            if (directory == null)
                directory = initialDirectory;

            //        whatever comes first  +  modification date MMM DD HH:MM           + directory / file name
            Regex regex = new Regex("(.+?)" + "[A-Za-z]{3} [0-9]{2} [0-9]{2}:[0-9]{2} " + "(?<DIRECTORY>.+)");

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{hostname}:{port}{directory}");
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            request.Credentials = credentials;
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            List<string> directories = new List<string>();
            List<string> files = new List<string>();
            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                    if (!string.IsNullOrWhiteSpace(line) && line != "." && line != "..")
                    {
                        Match match = regex.Match(line);
                        if (!match.Groups["DIRECTORY"].Success)
                            continue;

                        if (match.Groups["DIRECTORY"].Value == "." || match.Groups["DIRECTORY"].Value == "..")
                            continue;

                        if (line[0] == 'd')
                            directories.Add(match.Groups["DIRECTORY"].Value);
                        else
                            files.Add(match.Groups["DIRECTORY"].Value);
                    }
            }

            directories.Add("-");
            directories.AddRange(files);
            return directories;
        }

        private string hostname;
        private int port;
        private string initialDirectory;
        NetworkCredential credentials;
    }
}
