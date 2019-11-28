using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;

namespace Location
{
    public static class Program
    {
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool FreeConsole();

        [STAThread]
        public static int Main(string[] args)
        {
            if (args != null && args.Length > 0) // if arguments supplied use console mode else use UI mode
            {
                int port = 43; // standard port 43
                string protocol = "whois"; // Default protocol whois
                string username = null;
                string location = null;

                String server = "whois.net.dcs.hull.ac.uk"; //Brians server
                int timouttime = 1000;


                for (int i = 0; i < args.Length; ++i) // finds which tags (-) are present then does the relevent task
                {
                    switch (args[i])
                    {
                        case "-h":
                            server = args[++i]; //sets hostname
                            break;
                        case "-p":
                            port = int.Parse(args[++i]); //sets port
                            break;
                        case "-h9": //sets protocol to 0.9
                        case "-h0": //sets protocol to 1.0
                        case "-h1": protocol = args[i]; break; //sets protocol to HTTP 1.1
                        default: //if no protocol was set use whois
                            if (username == null)
                            {
                                username = args[i]; //set username to the firt arg
                            }
                            else if (location == null) // set location to second arg
                            {
                                location = args[i];
                            }
                            else
                            {
                                Console.WriteLine("Too many arguments"); // if there are too many args error
                            }
                            break;

                    }
                    if (args[i] =="-t")
                    {
                        timouttime = int.Parse(args[i + 1]);
                    }
                }

                if (username == null) // if there are too few args error
                {
                    Console.WriteLine("Too few arguments");

                }


                try
                {
                    TcpClient client = new TcpClient(); // connect to server
                    client.Connect(server, port);

                    StreamWriter sw = new StreamWriter(client.GetStream()); //set the streamreads and writers to what the server gets
                    StreamReader sr = new StreamReader(client.GetStream());

                    sw.AutoFlush = true; // Flushes the code automatically

                    List<string> Arglist = new List<string>();// A list of arguments
                    
                    client.SendTimeout = timouttime; // Timeout for the client
                    client.ReceiveTimeout = timouttime;





                    switch (protocol) // switches around the protocols decided by the first switch
                    {

                        case "whois": //default

                            if (location == null)  // if a location is not supplied does a lookup
                            {
                                sw.WriteLine(username);
                                Console.WriteLine(username + " is " + sr.ReadToEnd());  //Prints the location of the user
                            }
                            else
                            {
                                sw.WriteLine(username + " " + location); //Writes to server the user and location
                                String reply = sr.ReadLine();//gets reply


                                if (reply == "OK")
                                {
                                    Console.WriteLine(username + " location changed to be " + location); // Changes location

                                }
                                else
                                {
                                    Console.WriteLine("ERROR: Unexpected response " + reply);//Error
                                }
                                
                            }
                            break;



                        case "-h9": //if -h9 is provided preforms 0.9 requests
                            if (location == null) // if location is not supplied do a lookup request
                            {
                                sw.WriteLine("GET /" + username); // requests location
                                string line1 = sr.ReadLine();
                                line1 = sr.ReadLine();
                                line1 = sr.ReadLine();
                                Console.WriteLine(username + " is " + sr.ReadLine()); // if found presents location recieved from server
                            }
                            else
                            {
                                sw.WriteLine("PUT /" + username + "\r\n" + location); //adds user
                                String reply = sr.ReadLine();//gets reply

                                if (reply.EndsWith("OK"))
                                {
                                    Console.WriteLine(username + " location changed to be " + location);//changes location
                                }
                                else
                                {
                                    Console.WriteLine("ERROR: Unexpected response " + reply); //error

                                }
                            }
                            break;



                        case "-h1": //if HTTP 1.1 is checked

                            if (location == null) //if location is not supplied does a lookup request
                            {
                                sw.WriteLine("GET /?name=" + username + " HTTP/1.1" + "\r\n" + "Host: " + server + "\r\n" + "\r\n"); //requests infromation from server
                                string line2 = sr.ReadLine();
                                line2 = sr.ReadLine();
                                line2 = sr.ReadLine();

                                if (port == 80) //HTML website lookup
                                {
                                    string s = "";
                                    while (sr.Peek() >= 0)  // reads in the lines
                                    {
                                        s = sr.ReadLine().ToString(); // adds args to list
                                        Arglist.Add(s);

                                    }
                                    s = "";
                                    int index = Arglist.IndexOf(""); // start at index where ""

                                    for (int i = index + 1; i < Arglist.Count; i++)
                                    {
                                        s += Arglist[i]; // add the reqest to list
                                        s += "\r\n";// new line it 
                                    }
                                    Console.WriteLine(username + " is " + s);
                                }
                                else
                                {
                                    Console.WriteLine(username + " is " + sr.ReadLine());
                                }

                            }
                            else
                            {
                                int lengthtotal = username.Length + location.Length + 15; // find the total Content length
                                sw.WriteLine("POST / HTTP/1.1" + "\r\n" + "Host: " + server + "\r\n" + "Content-Length: " + lengthtotal + "\r\n" + "name=" + username + "&location=" + location + "\r\n"); // add user


                                String reply = sr.ReadLine();

                                if (reply.EndsWith("OK"))
                                {
                                    Console.WriteLine(username + " location changed to be " + location); //change location
                                }
                                else
                                {
                                    Console.WriteLine("ERROR: Unexpected response " + reply); //error
                                }
                            }
                            break;



                        case "-h0": // if HTTP 1.0 is checked 

                            if (location == null) // if location is empty do a get request
                            {
                                sw.WriteLine("GET /?" + username + " HTTP/1.0" + "\r\n" + "\r\n"); //request the information
                                string line2 = sr.ReadLine();
                                line2 = sr.ReadLine();
                                line2 = sr.ReadLine();
                                Console.WriteLine(username + " is " + sr.ReadLine()); // print usernames location
                            }
                            else
                            {

                                sw.WriteLine("POST /" + username + " HTTP/1.0" + "\r\n" + "Content-Length: " + location.Length + "\r\n" + "\r\n" + location); // add user


                                String reply = sr.ReadLine();

                                if (reply.EndsWith("OK"))
                                {
                                    Console.WriteLine(username + " location changed to be " + location); //change location
                                }
                                else
                                {
                                    Console.WriteLine("ERROR: Unexpected response " + reply); //error
                                }
                            }



                            break;
                    }


                }
                catch (Exception e)
                {
                    Console.WriteLine(e); // print exceptions
                }
                return 0;
            }
            else // Launch the UI
            {
                FreeConsole();
                var app = new App();

                return app.Run();

            }
        }
    }
}
