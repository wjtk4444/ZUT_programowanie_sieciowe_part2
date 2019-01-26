using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace lab3_SMTP
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length != 7)
            {
                Console.WriteLine($"USAGE: {System.Diagnostics.Process.GetCurrentProcess().ProcessName} <hostname> <port> <username> <password> <recipient> <subject> <body>");
                return;
            }

            string hostname = args[0];
            int port = int.Parse(args[1]);
            string username = args[2];
            string password = args[3];
            string recipient = args[4];
            string subject = args[5];
            string body = args[6];

            TcpClient tcpClient = new TcpClient(hostname, port);
            SslStream sslStream = new SslStream(tcpClient.GetStream());
            sslStream.AuthenticateAsClient(hostname);

            byte[] buffer = new byte[4096];
            string response;

            string END_MSG = "\r\n";

            try
            {
                sslStream.Write(Encoding.ASCII.GetBytes($"EHLO" + END_MSG));
                sslStream.Read(buffer, 0, buffer.Length);
                response = Encoding.ASCII.GetString(buffer).Replace("\0", "");
                //Console.WriteLine(response);

                sslStream.Write(Encoding.ASCII.GetBytes($"AUTH LOGIN" + END_MSG));
                sslStream.Read(buffer, 0, buffer.Length);
                response = Encoding.ASCII.GetString(buffer).Replace("\0", "");
                //Console.WriteLine(response);

                sslStream.Write(Encoding.ASCII.GetBytes(Convert.ToBase64String(Encoding.ASCII.GetBytes(username)) + END_MSG));
                sslStream.Read(buffer, 0, buffer.Length);
                response = Encoding.ASCII.GetString(buffer).Replace("\0", "");
                //Console.WriteLine(response);

                sslStream.Write(Encoding.ASCII.GetBytes(Convert.ToBase64String(Encoding.ASCII.GetBytes(password)) + END_MSG));
                sslStream.Read(buffer, 0, buffer.Length);
                response = Encoding.ASCII.GetString(buffer).Replace("\0", "");
                //Console.WriteLine(response);
                if (!response.StartsWith("334"))
                    throw new Exception("Authentication failed.");

                sslStream.Write(Encoding.ASCII.GetBytes($"MAIL From: {username}" + END_MSG));
                sslStream.Read(buffer, 0, buffer.Length);
                response = Encoding.ASCII.GetString(buffer).Replace("\0", "");
                //Console.WriteLine(response);

                sslStream.Write(Encoding.ASCII.GetBytes($"RCPT To: {recipient}" + END_MSG));
                sslStream.Read(buffer, 0, buffer.Length);
                response = Encoding.ASCII.GetString(buffer).Replace("\0", "");
                //Console.WriteLine(response);

                sslStream.Write(Encoding.ASCII.GetBytes($"DATA" + END_MSG));
                sslStream.Read(buffer, 0, buffer.Length);
                response = Encoding.ASCII.GetString(buffer).Replace("\0", "");
                //Console.WriteLine(response);

                sslStream.Write(Encoding.ASCII.GetBytes($"Subject: {subject}" + END_MSG));
                sslStream.Read(buffer, 0, buffer.Length);
                response = Encoding.ASCII.GetString(buffer).Replace("\0", "");
                //Console.WriteLine(response);

                sslStream.Write(Encoding.ASCII.GetBytes($"From: {username}" + END_MSG));
                /*sslStream.Read(buffer, 0, buffer.Length);
                response = Encoding.ASCII.GetString(buffer).Replace("\0", "");
                Console.WriteLine(response);*/

                sslStream.Write(Encoding.ASCII.GetBytes($"{body}" + END_MSG));
                /*sslStream.Read(buffer, 0, buffer.Length);
                response = Encoding.ASCII.GetString(buffer).Replace("\0", "");
                Console.WriteLine(response);*/

                sslStream.Write(Encoding.ASCII.GetBytes($"." + END_MSG));
                sslStream.Read(buffer, 0, buffer.Length);
                response = Encoding.ASCII.GetString(buffer).Replace("\0", "");
                //Console.WriteLine(response);
                if (!response.StartsWith("250"))
                    throw new Exception("Sending mail failed.");
                else
                    Console.WriteLine("Mail sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                try
                {
                    sslStream.Write(Encoding.ASCII.GetBytes($"QUIT" + END_MSG));
                }
                catch
                {
                    // ¯\_(ツ)_ /¯
                }
            }

            Console.ReadLine();
        }
    }
}
