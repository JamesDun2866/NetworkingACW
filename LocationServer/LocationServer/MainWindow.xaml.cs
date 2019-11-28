using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace LocationServer
{



    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

        }
    }

    public partial class MainWindow : Window
    {
        

        TcpListener listener; // initlize the listener here
        Socket connection;
        string pairs;



        public bool stop = false;
        public MainWindow()
        {
            InitializeComponent(); // start the UI 

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            txtServer.Text = "Server is running ";

            Thread A = new Thread(() => RunServer()); // Start the server
            A.Start();

        }


        public void RunServer()
        {

       
        Handler2 RequestHandler;
            
            try
            {
              
                
                listener = new TcpListener(IPAddress.Any, 43);
                listener.Start(); 

                while (true) // connection
                {
                    connection = listener.AcceptSocket();
                    RequestHandler = new Handler2();
                    Thread t = new Thread(() => RequestHandler.doRequest(connection)); //Start a thread
                    t.Start();

                }
  
            }
            catch (Exception e)
            {
                //Console.WriteLine("Exception: " + e.ToString());
                
            }
        }

        private void labelserver_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e) // if this button is clicked stop listening and stop the server
        {
            txtServer.Text = "Server is not running ";
            listener.Stop();
            connection.Close();
            
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            string log = Handler2.message;

            txtoutput.Text = log; // print log to textbox and refresh
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            
            SaveFileDialog dialog = new SaveFileDialog()
            {
                Filter = "Text Files(*.txt)|*.txt|All(*.*)|*"
            };
            if(dialog.ShowDialog() == true)
            {
                File.WriteAllText(dialog.FileName, Handler2.message);
            }

          
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            


            foreach (KeyValuePair<string, string> kvp in Handler2.ArgDict)
            {

                pairs +=  kvp.Key +" "+ kvp.Value + "\r\n";

            }
            SaveFileDialog dialog = new SaveFileDialog()
            {
                Filter = "Text Files(*.txt)|*.txt|All(*.*)|*"
            };
            if (dialog.ShowDialog() == true)
            {
                File.WriteAllText(dialog.FileName, pairs);
            }
        }
    }

    public class Handler2
    {
        public static string message { get; set; } // getter and setter to fetch message and use it in the other class 
        string time = DateTime.Now.ToShortTimeString(); // find the current time 
        string date = DateTime.Now.Date.ToString(); // find the current date


        public static Dictionary<string, string> ArgDict = new Dictionary<string, string>();
        public string doRequest(Socket connection)
        {
           
            NetworkStream socketStream;
            socketStream = new NetworkStream(connection);  // Connect to the server
            String Hostname = ((IPEndPoint)connection.RemoteEndPoint).Address.ToString(); // Get current IP address

            try
            {


                socketStream.WriteTimeout = 1000;  // Timeout for server
                socketStream.ReadTimeout = 1000;
                StreamWriter sw = new StreamWriter(socketStream); //setup stream reader and writer
                StreamReader sr = new StreamReader(socketStream);

                StringBuilder str = new StringBuilder();

                sw.AutoFlush = true; // Flush automatically
                string line = null;


                List<string> request = new List<string>();

                while (sr.Peek() >= 0) //Read all data from request
                {
                    str.Append(" ");
                    str.Append(sr.ReadLine().ToString());

                }
                line = str.ToString().Trim(); // trim the input


                string[] split = line.Split(' ');  // split input into args by space

                request.AddRange(split); // Add the args to the request list


                string location = null;
                String[] arg = line.Split(new char[] { ' ' }, 2); // Split the line by space into 2 args (Used for whois)

                if (request.Contains("HTTP/1.0")) // determines if the mode is HTTP 1.0
                {

                    List<string> refinedrequest = new List<string>(); // create a refined list

                    string[] refinedsplit = line.Split(' ');
                    refinedrequest.AddRange(refinedsplit);


                    string foundname = "";

                    if (line.StartsWith("GET") || line.StartsWith("?")) // remove all useless information provided by the request (GET)
                    {
                        refinedrequest.Remove("HTTP/1.0");
                        refinedrequest.Remove("GET");
                        foundname = refinedrequest[0];
                        if (line.StartsWith("GET"))
                            foundname = foundname.Remove(0, 2);
                    }
                    else // remove all useless information provided by the request (POST)
                    {
                        refinedrequest.Remove("");
                        refinedrequest.Remove("HTTP/1.0");
                        refinedrequest.Remove("POST");
                        refinedrequest.Remove("Content-Length:");
                        refinedrequest.RemoveAt(1);
                        foundname = refinedrequest[0];
                        foundname = foundname.Remove(0, 1);

                        for (int j = 1; j < refinedrequest.Count; j++)
                        {
                            location += ' ';
                            location += refinedrequest[j];
                        }
                    }

                    if (refinedrequest.Count == 1) // if only username provided do a lookup
                    {
                        if (ArgDict.ContainsKey(foundname))
                        {

                            sw.WriteLine("HTTP/1.0 200 OK" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n" + ArgDict[foundname] + "\r\n"); // if found send responce 

                            message += Hostname+ " - - " + '[' + date + " " + time + ']' +" " + '"' + "GET /?" + foundname +" HTTP/1.0" +'"' + " OK" +"\r\n"; // Log the server

                        }
                        else
                        {
                            sw.WriteLine("HTTP/1.0 404 Not Found" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n"); // if not found send responce

                            message += Hostname + " - - " + '[' + date + " "+ time + ']' + " " + '"' + "GET /?" + foundname + " HTTP/1.0" + '"' + " Unknown" +"\r\n"; // Log the server

                        }
                    }
                    else
                    {
                        location.Trim();
                        location = location.Remove(0, 1);
                        if (ArgDict.ContainsKey(foundname))
                        {
                            ArgDict[foundname] = location; // change location 
                            sw.WriteLine("HTTP/1.0 200 OK" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n");  // respond to the change of location

                            message += Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + "POST /" + foundname +" HTTP/1.0"+ '"' + " OK" + "\r\n"; // Log the server

                        }
                        else
                        {
                            ArgDict.Add(foundname, location);
                            sw.WriteLine("HTTP/1.0 200 OK" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n"); // respond to the addition of user

                            message += Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + "POST /" + foundname + " HTTP/1.0" + '"' + " OK" + "\r\n"; // Log the server



                        }
                    }
                } //1.0

                else if (request.Contains("HTTP/1.1")) // determines if the mode is HTTP 1.1
                {


                    List<string> refinedrequest = new List<string>(); // create a refined list


                    string[] refinedsplit = line.Split(' ');

                    refinedrequest.AddRange(refinedsplit);

                    string foundname = "";

                    string find = "";

                    bool gate = true; //set Gate to true



                    if (line.StartsWith("GET")) // remove all useless information provided by the request (GET)
                    {
                        find = refinedrequest[1];
                        find = find.Remove(0, 7);
                        foundname = find;
                    }
                    else //find the location and the username
                    {
                        char[] Locationchar = refinedrequest[7].ToCharArray();

                        for (int i = 5; i < Locationchar.Length; ++i)
                        {


                            while (gate == true) // if gate is true add the name chars to the username
                            {
                                if (Locationchar[i + 1] == '&') // if the character hits & it will set gate to false which stops any addition to the name variable
                                {
                                    gate = false;

                                }
                                foundname += Locationchar[i];
                                break;
                            }

                            if (i >= foundname.Length + 15) //if location is false adds to find the location
                            {
                                location += Locationchar[i];
                            }
                        }





                        for (int j = 8; j < refinedrequest.Count; j++)
                        {
                            location += ' ';
                            location += refinedrequest[j];

                        }
                    }
                    if (location == null) // if location is not supplied perform a lookup
                    {

                        if (ArgDict.ContainsKey(foundname)) // if name exists
                        {
                            sw.WriteLine("HTTP/1.1 200 OK" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n" + ArgDict[foundname] + "\r\n"); // if found send responce


                            message += Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + "GET /?name=" + foundname + " HTTP/1.1" + '"' + " OK" + "\r\n"; // Log the server

                        }
                        else //if name does not exist
                        {
                            sw.WriteLine("HTTP/1.1 404 Not Found" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n"); // if not found send responce
                            message += Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + "GET /?name=" + foundname + " HTTP/1.1" + '"' + " Unknown" + "\r\n"; // Log the server

                        }
                    }
                    else // if location is supplied
                    {
                        location.Trim();
                        if (ArgDict.ContainsKey(foundname))
                        {
                            ArgDict[foundname] = location; // change location 
                            sw.WriteLine("HTTP/1.1 200 OK" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n"); // respond to the change of location
                            message += Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + "POST / HTTP/1.1" + '"' + " OK" + "\r\n"; // Log the server

                        }
                        else
                        {
                            ArgDict.Add(foundname, location);
                            sw.WriteLine("HTTP/1.1 200 OK" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n"); // respond to the addition of user
                            message += Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + "POST / HTTP/1.1" + '"' + " OK" + "\r\n"; // Log the server
                        }

                    }

                } //1.1

                else if (!request.Contains("HTTP/1.1") && !request.Contains("HTTP/1.0") && request.Contains("GET") || !request.Contains("HTTP/1.1") && !request.Contains("HTTP/1.0") && request.Contains("PUT")) // if statement determines if request is a 0.9 request
                {

                    List<string> refinedrequest = new List<string>();
                    line = line.Remove(0, 5); // remove uselss information
                    string[] refinedsplit = line.Split(' '); // split by space
                    refinedrequest.AddRange(refinedsplit); // add range of items to list

                    for (int j = 1; j < refinedrequest.Count; j++)
                    {
                        location += ' ';
                        location += refinedrequest[j];

                    }

                    refinedrequest.Remove(""); // remove empty strings


                    if (refinedrequest.Count == 1) // if location not supplied
                    {
                        if (ArgDict.ContainsKey(refinedrequest[0])) // if the user exists perform a lookup
                        {
                            sw.Write("HTTP/0.9 200 OK" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n" + ArgDict[refinedrequest[0]]); // if found send responce
                            message += Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + "GET / " + refinedrequest[0] + '"' + " OK" + "\r\n"; // Log the server
                        }
                        else
                        {
                            sw.Write("HTTP/0.9 404 Not Found" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n"); // if not found send responce
                            message += Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + "GET / " + refinedrequest[0] + '"' + " Unknown" + "\r\n"; // Log the server
                        }
                    }
                    else // if location is supplied
                    {
                        location = location.Remove(0, 1);
                        if (ArgDict.ContainsKey(refinedrequest[0]))
                        {
                            ArgDict[refinedrequest[0]] = location; // change location 
                            sw.Write("HTTP/0.9 200 OK" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n"); // respond to the change of location
                            message += Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' +"PUT /" + refinedrequest[0] + '"' + " OK" + "\r\n"; // Log the server
                        }
                        else
                        {
                            ArgDict.Add(refinedrequest[0], location); //Add user
                            sw.Write("HTTP/0.9 200 OK" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n"); // respond to the addition of user
                            message += Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' +"PUT /" + refinedrequest[0] + '"' + " OK" + "\r\n"; // Log the server
                        }
                    }
                } //0.9

                else //whois

                if (arg.Length == 1) // if loation is not supplied perform a lookup
                {
                    if (ArgDict.ContainsKey(arg[0])) // if username found
                    {
                        sw.WriteLine(ArgDict[arg[0]]); // if found send responce

                        message += Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + arg[0] + '"' + " OK" + "\r\n"; // Log the server

                    }
                    else
                    {
                        sw.WriteLine("ERROR: no entries found"); // if not found send responce

                        message += Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + arg[0]+ '"'+ " Unknown" + "\r\n"; // Log the server
                    }
                }
                else if (arg.Length == 2) //If location supplied
                {
                    if (ArgDict.ContainsKey(arg[0]))
                    {
                        ArgDict[arg[0]] = arg[1];
                        sw.WriteLine("OK"); // respond to the change of location



                        message += Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + arg[1] + '"' +  " OK" + "\r\n"; // Log the server

                    }
                    else  // if user doesnt exist
                    {
                        ArgDict.Add(arg[0], arg[1]); //Add the user
                        sw.WriteLine("OK"); // respond to the addition of user
                        message += Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + arg[1] + '"' + " OK" + "\r\n"; // Log the server
                    }
                }
                return (message);
                
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                message += e; // Write exception
                return (message);
            }
            finally
            {

                socketStream.Close();
                connection.Close(); // close the connection
            }
        }
    }
}





