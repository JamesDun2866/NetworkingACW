using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LocationServer
{
    class Handler1
    {
        public static Dictionary<string, string> ArgDict = new Dictionary<string, string>();
        public static string message { get; set; } // getter and setter to fetch message and use it in the other class 
        public static int logcount { get; set; }
        public static int Timeout { get; set; } = 1000;


        public void doRequest(Socket connection)
        {

            NetworkStream socketStream;
            socketStream = new NetworkStream(connection); // Connect to the server
            String Hostname = ((IPEndPoint)connection.RemoteEndPoint).Address.ToString(); // Get current IP address

            

            try
            {
               
                
                    socketStream.WriteTimeout = Timeout; // Timeout for server
                    socketStream.ReadTimeout = Timeout;
                    
                

                

                StreamWriter sw = new StreamWriter(socketStream);
                StreamReader sr = new StreamReader(socketStream); //setup stream reader and writer 

                string time = DateTime.Now.ToShortTimeString(); // Get the time
                string date = DateTime.Now.Date.ToString(); // Get the date

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


                string[] split = line.Split(' '); // split input into args by space

                request.AddRange(split); // Add the args to the request list


                string location = null;
                String[] arg = line.Split(new char[] { ' ' }, 2);// Split the line by space into 2 args (Used for whois)

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
                    else// remove all useless information provided by the request (POST)
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
                            Console.WriteLine(Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + "GET /?" + foundname + " HTTP/1.0" + '"' + " OK" + "\r\n"); // Log the server
                            message = (Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + "GET /?" + foundname + " HTTP/1.0" + '"' + " OK" + "\r\n"); // Log the server
                            logcount++;
                        }
                        else
                        {
                            sw.WriteLine("HTTP/1.0 404 Not Found" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n"); // if not found send responce
                            Console.WriteLine(Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + "GET /?" + foundname + " HTTP/1.0" + '"' + " Unknown" + "\r\n");// Log the server
                            message = (Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + "GET /?" + foundname + " HTTP/1.0" + '"' + " Unknown" + "\r\n");// Log the server
                            logcount++;
                        }
                    }
                    else
                    {
                        location.Trim();
                        location = location.Remove(0, 1);
                        if (ArgDict.ContainsKey(foundname))
                        {
                            ArgDict[foundname] = location; // change location 
                            sw.WriteLine("HTTP/1.0 200 OK" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n"); // respond to the change of location
                            Console.WriteLine(Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + "POST /" + foundname + " HTTP/1.0" + '"' + " OK" + "\r\n");// Log the server
                            message= (Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + "POST /" + foundname + " HTTP/1.0" + '"' + " OK" + "\r\n");// Log the server
                            logcount++;
                        }
                        else
                        {
                            ArgDict.Add(foundname, location);
                            sw.WriteLine("HTTP/1.0 200 OK" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n"); // respond to the addition of user
                            Console.WriteLine(Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + "POST /" + foundname + " HTTP/1.0" + '"' + " OK" + "\r\n");// Log the server
                            message=(Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + "POST /" + foundname + " HTTP/1.0" + '"' + " OK" + "\r\n");// Log the server
                            logcount++;
                        }
                    }
                } //1.0

                else if (request.Contains("HTTP/1.1"))// determines if the mode is HTTP 1.1
                {


                    List<string> refinedrequest = new List<string>();  // create a refined list

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
                            sw.WriteLine("HTTP/1.1 200 OK" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n" + ArgDict[foundname] + "\r\n"); // respond to the lookup
                            Console.WriteLine(Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + "GET /?name=" + foundname + " HTTP/1.1" + '"' + " OK" + "\r\n"); //log server
                            message =(Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + "GET /?name=" + foundname + " HTTP/1.1" + '"' + " OK" + "\r\n"); //log server
                            logcount++;
                        }
                        else //if name does not exist
                        {
                            sw.WriteLine("HTTP/1.1 404 Not Found" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n");// respond to the lookup 
                            Console.WriteLine(Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + "GET /?name=" + foundname + " HTTP/1.1" + '"' + " Unknown" + "\r\n");//log server
                            message = (Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + "GET /?name=" + foundname + " HTTP/1.1" + '"' + " Unknown" + "\r\n");//log server
                            logcount++;
                        }
                    }
                    else // if location is supplied
                    {
                        location.Trim();
                        if (ArgDict.ContainsKey(foundname))
                        {
                            ArgDict[foundname] = location; // change location 
                            sw.WriteLine("HTTP/1.1 200 OK" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n"); // respond to the change of location
                            Console.WriteLine(Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + "POST / HTTP/1.1" + '"' + " OK" + "\r\n"); //log server
                            message = (Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + "POST / HTTP/1.1" + '"' + " OK" + "\r\n"); //log server
                            logcount++;
                        }
                        else
                        {
                            ArgDict.Add(foundname, location); // add the user
                            sw.WriteLine("HTTP/1.1 200 OK" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n"); // respond to the addition of a user 
                            Console.WriteLine(Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + "POST / HTTP/1.1" + '"' + " OK" + "\r\n");//log server
                            message = (Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + "POST / HTTP/1.1" + '"' + " OK" + "\r\n");//log server
                            logcount++;
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
                            sw.Write("HTTP/0.9 200 OK" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n" + ArgDict[refinedrequest[0]]); //respond to lookup
                            Console.WriteLine(Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + "GET / " + refinedrequest[0] + '"' + " OK" + "\r\n"); //Log server
                            message=(Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + "GET / " + refinedrequest[0] + '"' + " OK" + "\r\n"); //Log server
                            logcount++;
                        }
                        else
                        {
                            sw.Write("HTTP/0.9 404 Not Found" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n"); //respond to lookup
                            Console.WriteLine(Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + "GET / " + refinedrequest[0] + '"' + " Unknown" + "\r\n");//Log server
                            message=(Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + "GET / " + refinedrequest[0] + '"' + " Unknown" + "\r\n");//Log server
                            logcount++;
                        }
                    }
                    else // if location is supplied
                    {
                        location = location.Remove(0, 1); // Remove useless information
                        if (ArgDict.ContainsKey(refinedrequest[0])) // if user exits change location
                        {
                            ArgDict[refinedrequest[0]] = location; // change location 
                            sw.Write("HTTP/0.9 200 OK" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n"); // respond to location change 
                            Console.WriteLine(Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + "PUT /" + refinedrequest[0] + '"' + " OK" + "\r\n"); //Log server
                            message=(Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + "PUT /" + refinedrequest[0] + '"' + " OK" + "\r\n"); //Log server
                            logcount++;
                        }
                        else
                        {
                            ArgDict.Add(refinedrequest[0], location); // add user 
                            sw.Write("HTTP/0.9 200 OK" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n"); // respond to user addition
                            Console.WriteLine(Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + "PUT /" + refinedrequest[0] + '"' + " OK" + "\r\n");//Log server
                            message = (Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + "PUT /" + refinedrequest[0] + '"' + " OK" + "\r\n");//Log server
                            logcount++;
                        }
                    }
                } //0.9

                else //whois

                if (arg.Length == 1) // if loation is not supplied perform a lookup
                {
                    if (ArgDict.ContainsKey(arg[0])) // if username found
                    {
                        sw.WriteLine(ArgDict[arg[0]]); // respond to lookup
                        Console.WriteLine(Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + arg[0] + '"' + " OK" + "\r\n"); // Log the sever
                        message = (Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + arg[0] + '"' + " OK" + "\r\n"); // Log the sever
                        logcount++;

                    }
                    else
                    {
                        sw.WriteLine("ERROR: no entries found"); // Respond to Lookup
                        Console.WriteLine(Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + arg[0] + '"' + " Unknown" + "\r\n"); //Log the server
                        message = (Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + arg[0] + '"' + " Unknown" + "\r\n"); //Log the server
                        logcount++;
                    }
                }
                else if (arg.Length == 2) //If location supplied
                {
                    if (ArgDict.ContainsKey(arg[0])) // if user exists
                    {
                        ArgDict[arg[0]] = arg[1]; // change location
                        sw.WriteLine("OK"); // Respond to location Change
                        Console.WriteLine(Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + arg[1] + '"' + " OK" + "\r\n"); //Log the server
                        message =(Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + arg[1] + '"' + " OK" + "\r\n"); //Log the server
                        logcount++;
                    }
                    else // if user doesnt exist
                    {
                        ArgDict.Add(arg[0], arg[1]); // add the user 
                        sw.WriteLine("OK"); // respond to addition
                        Console.WriteLine(Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + arg[1] + '"' + " OK" + "\r\n"); //log the server
                        message = (Hostname + " - - " + '[' + date + " " + time + ']' + " " + '"' + arg[1] + '"' + " OK" + "\r\n"); //log the server
                        logcount++;
                    }
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e); // Write exception
            }
            finally
            {
                socketStream.Close(); // close the connection
                connection.Close();

            }
        }
    }
}
