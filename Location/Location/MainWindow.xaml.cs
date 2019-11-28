using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Net.Sockets;
using System.IO;

namespace Location
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
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
                int port = int.Parse (txtport.Text);
                string server = txtserver.Text;
                string username = txtname.Text;
                string location = txtlocation.Text;
                

            if (username == null)
            {
                MessageBox.Show("Too few arguments");

            }

            try
            {
                TcpClient client = new TcpClient(); //Connect to the server
                client.Connect(server, port);

                StreamWriter sw = new StreamWriter(client.GetStream()); //Set the streamreader andwriter to what it recieves from the client
                StreamReader sr = new StreamReader(client.GetStream());

                sw.AutoFlush = true; // Flushes the code automatically

                List<string> Arglist = new List<string>(); // A list of arguments

                client.SendTimeout = 1000; // Timeout for the client
                client.ReceiveTimeout = 1000;

                

                if (radwhois.IsChecked == true) // whois checked
                {
                    if (location == "") // if a location is not supplied does a lookup
                    {
                        sw.WriteLine(username);
                        MessageBox.Show(username + " is " + sr.ReadToEnd()); //Prints the location of the user
                    }
                    else
                    {
                        sw.WriteLine(username + " " + location); //Writes to server the user and location
                        String reply = sr.ReadLine(); //gets reply

                        if (reply == "OK")
                        {
                            MessageBox.Show(username + " location changed to be " + location); // Changes location
                        }
                        else
                        {
                            MessageBox.Show("ERROR: Unexpected response " + reply); //Error
                        }

                    }
                }

             
                if (rad9.IsChecked == true) // if 0.9 is checked
                {
                    if (location == "") // if location is not supplied do a lookup request
                    {
                        sw.WriteLine("GET /" + username); // requests location
                        string line1 = sr.ReadLine();
                        line1 = sr.ReadLine();
                        line1 = sr.ReadLine();
                        MessageBox.Show(username + " is " + sr.ReadLine()); // if found presents location recieved from server 
                    }
                    else
                    {
                        sw.WriteLine("PUT /" + username + "\r\n" + location); //adds user
                        String reply = sr.ReadLine(); //gets reply

                        if (reply.EndsWith("OK"))
                        {
                            MessageBox.Show(username + " location changed to be " + location); //changes location 
                        }
                        else
                        {
                            MessageBox.Show("ERROR: Unexpected response " + reply); //error
                        }
                    }
                }



                if (rad11.IsChecked == true) //if HTTP 1.1 is checked
                {
                    if (location == "") //if location is not supplied does a lookup request
                    {
                        sw.WriteLine("GET /?name=" + username + " HTTP/1.1" + "\r\n" + "Host: " + server + "\r\n" + "\r\n"); //requests infromation from server
                        string line2 = sr.ReadLine();
                        line2 = sr.ReadLine();
                        line2 = sr.ReadLine();

                        if (port == 80) //HTML website lookup
                        {
                            string s = "";
                            while (sr.Peek() >= 0) // reads in the lines
                                
                            {
                                s = sr.ReadLine().ToString();
                                Arglist.Add(s); // adds args to list

                            }
                            s = "";
                            int index = Arglist.IndexOf(""); // start at index where ""

                            for (int i = index + 1; i < Arglist.Count; i++)
                            {
                                s += Arglist[i]; // add the reqest to list
                                s += "\r\n"; // new line it 
                            }
                            MessageBox.Show(username + " is " + s);
                        }
                        else
                        {
                            MessageBox.Show(username + " is " + sr.ReadLine());
                        }

                    }
                    else
                    {
                        int lengthtotal = username.Length + location.Length + 15; // find the total Content length 
                        sw.WriteLine("POST / HTTP/1.1" + "\r\n" + "Host: " + server + "\r\n" + "Content-Length: " + lengthtotal + "\r\n" + "name=" + username + "&location=" + location + "\r\n"); // add user


                        String reply = sr.ReadLine();

                        if (reply.EndsWith("OK"))
                        {
                            MessageBox.Show(username + " location changed to be " + location); //change location
                        }
                        else
                        {
                            MessageBox.Show("ERROR: Unexpected response " + reply); //error
                        }
                    }
                }
               
                if (rad1.IsChecked == true) // if HTTP 1.0 is checked 
                {
                    if (location == "") // if location is empty do a get request
                    {
                        sw.WriteLine("GET /?" + username + " HTTP/1.0" + "\r\n" + "\r\n"); //request the information
                        string line2 = sr.ReadLine();
                        line2 = sr.ReadLine();
                        line2 = sr.ReadLine();
                       
                        MessageBox.Show(username + " is " + sr.ReadLine()); // print usernames location
                    }
                    else
                    {


                        sw.WriteLine("POST /" + username + " HTTP/1.0" + "\r\n" + "Content-Length: " + location.Length + "\r\n" + "\r\n" + location); // add user

                        String reply = sr.ReadLine();

                        if (reply.EndsWith("OK"))
                        {
                            MessageBox.Show(username + " location changed to be " + location); //change location
                        }
                        else
                        {
                            MessageBox.Show("ERROR: Unexpected response " + reply); //error
                        }
                    }
                }

            }
            catch (Exception f)
            {
                MessageBox.Show(f.ToString()); // print exceptions
            }
        }
    }
}
