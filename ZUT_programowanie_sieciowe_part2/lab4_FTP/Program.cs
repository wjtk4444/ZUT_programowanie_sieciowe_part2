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
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine($"USAGE: {System.Diagnostics.Process.GetCurrentProcess().ProcessName} <hostname[/initial_path]> <port> <username> <password>");
                return;
            }

            int port = int.Parse(args[1]);
            string username = args[2];
            string password = args[3];

            string hostname;
            string initialDirectory;
            if(args[0].Contains('/'))
            {
                hostname = args[0].Substring(0, args[0].IndexOf('/'));
                initialDirectory = args[0].Substring(hostname.Length, args[0].Length - (hostname.Length));
            }
            else
            {
                hostname = args[0];
                initialDirectory = "/";
            }

            FTPClient ftpClient = new FTPClient(hostname, port, username, password, initialDirectory);
            Console.WriteLine($"Connecting to {hostname} as {username}");
            menuLoop(ftpClient, hostname, username);
        }

        static void menuLoop(FTPClient ftpClient, string hostname, string username)
        {
            string currentDirectory = "/";

            while (true)
            {
                List<string> directoryContents;
                try
                {
                    directoryContents = ftpClient.getDirectoryContents(currentDirectory);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Unable to get directory contents.");
                    Console.WriteLine(ex.Message);
                    Console.ReadLine();
                    return;
                }

                Console.WriteLine($"Connected to {hostname} as {username}");
                Console.WriteLine($"Directory contents: ({hostname}{currentDirectory})");
                int dirCounter = 1;
                bool files = false;
                foreach (string s in directoryContents)
                {
                    if (s == "-")
                    {
                        files = true;
                        continue;
                    }

                    if (!files) // directories
                    {
                        Console.WriteLine($"{dirCounter}: {s}");
                        dirCounter++;
                    }
                    else
                    {
                        Console.WriteLine(s);
                    }
                }
                Console.WriteLine();

                Console.WriteLine("Pick a directory number to enter, -1 to exit the program, 0 to go one directory up," + Environment.NewLine
                    + "type \"tree\" to display directory tree starting in current directory");

                string input = Console.ReadLine().Trim().ToLower();

                if (input == "tree")
                {
                    printTree(ftpClient, currentDirectory);
                    Console.WriteLine();

                    Console.WriteLine("Press any key to return to the menu");
                    Console.ReadLine();
                }
                else
                {
                    int selection = int.Parse(input);
                    if (selection == -1)
                        return;

                    if (selection == 0)
                    {
                        if (currentDirectory != "/")
                            currentDirectory = currentDirectory.Substring(0, currentDirectory.Substring(0, currentDirectory.Length - 1).LastIndexOf('/') + 1);
                    }
                    else
                    {
                        currentDirectory += directoryContents[selection - 1] + "/";
                    }
                }

                Console.Clear();
            }
        }

        static void printTree(FTPClient ftpClient, string currentDirectory, string prefix = "")
        {
            List<string> directoryContents;
            try
            {
                directoryContents = ftpClient.getDirectoryContents(currentDirectory);
            }
            catch (Exception ex)
            {
                Console.WriteLine(prefix + "Unable to get directory contents.");
                Console.WriteLine(prefix + ex.Message);
                Console.ReadLine();
                return;
            }

            bool files = false;
            foreach (string s in directoryContents)
            {
                if (s == "-")
                {
                    files = true;
                    continue;
                }

                if (!files) // directories
                {
                    Console.WriteLine($"{prefix}directory: {s}");
                    System.Threading.Thread.Sleep(1000);
                    printTree(ftpClient, currentDirectory + s + "/", prefix + "\t");
                }
                else
                {
                    Console.WriteLine($"{prefix}file: {s}");
                }
            }

        }
    }

    class FTPClient
    {
        public FTPClient(string hostname, int port, string username, string password, string initialDirectory = "/")
        {
            this.hostname = hostname;
            this.port = port;
            this.initialDirectory = initialDirectory;
            credentials = new NetworkCredential(username, password);
        }

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
