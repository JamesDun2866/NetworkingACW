using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LocationServer
{
    public static class Program
    {
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool FreeConsole();


        [STAThread]
        public static int Main(string[] args)
        {

            if (!args.Contains("-w")) // if - w is not supplied enter command line mode 
            {
                Thread A = new Thread(() => runServer()); // start thread A 
                A.Start();
                string argpick = null;


                for (int i = 0; i < args.Length; i++) 
                {
                    switch (args[i])
                    {

                        case "-l": // detect -l and -f1
                        case "-f1":


                            argpick = args[i];

                            break;
                        case "-t":
                            Handler1.Timeout = int.Parse(args[i + 1]); //set the timer

                            break;



                    }

                }

                switch (argpick)
                {
                    case "-l":
                        string filelocation = args[1];



                        Console.WriteLine("Server started listening"); 
                        System.IO.File.WriteAllText(@filelocation, "Log File" + "\r\n"); // Write the file to the location specified



                        while (A.IsAlive) // while the server is running 
                        {

                            if (Handler1.logcount > 0) // if handler1 gets to a output it adds 1 to log count and to stop it wring to the file contantly I added a counter
                            {

                                Handler1.logcount--;

                            }
                            if (Handler1.logcount == 0)
                            {
                                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@filelocation, true)) // use the file writing service
                                {
                                    if (Handler1.message == "") // if message is blank dont write
                                    {

                                    }
                                    else
                                    {
                                        file.WriteLine(Handler1.message); // write the message to the file
                                        Handler1.message = ""; //set message to blank so it doesnt add the same message. 
                                    }

                                }
                            }
                        }
                        return 0;

                    case "-f1": //--UNFINISHED-- Writes part of the file not all
                        string filelocation2 = args[1];
                        string pairs = null;
                        //  bool gate = true;



                        Console.WriteLine("Server started listening");
                        System.IO.File.WriteAllText(@filelocation2, "Log File" + "\r\n");


                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@filelocation2, true))
                        {

                            while (A.IsAlive)
                            {







                                if (pairs == "")
                                {
                                    pairs = null;
                                }
                                else
                                {





                                    foreach (KeyValuePair<string, string> kvp in Handler1.ArgDict)
                                    {
                                        pairs += kvp.Key + " " + kvp.Value + "\r\n";
                                    }


                                    file.WriteLine(pairs);


                                }





                            }






                        }

                        break;



                    default:

                        Console.WriteLine("Server started listening");

                        break;


                }
                return 0;
            }
            else
            {
                FreeConsole(); // use window mode 
                var app = new App();
                return app.Run();
            }




        }

        static void runServer()
        {
            TcpListener listener;
            Socket connection;
            Handler1 RequestHandler;

            try
            {
                listener = new TcpListener(IPAddress.Any, 43); // Extablish connection 
                listener.Start();

                while (true)
                {
                    connection = listener.AcceptSocket();
                    RequestHandler = new Handler1();
                    Thread t = new Thread(() => RequestHandler.doRequest(connection)); // start the server thread
                    t.Start();



                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.ToString()); // Write exceptions
            }
        }
    }
}
