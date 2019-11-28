Files included:

- Location (the location tracking client) project folder
- LocationServer (The location Tracking Server) Project folder
- This readme file

Other Information:
- To the run programs the executables are in the /bin/debug file 
- When solution built the zip file will fail the zip tests as it detects another executable in the obj file 
- Failed 29 advanced tests on labs 1 - 5 
- Whois, HTTP 0.9, HTTP 1.0 and HTTP 1.1 have been implmented both server and client side 
- to run a specific protocol type -h1 (for 1.1) -h0 (for 1.0) -h9 (for 0.9) and leave blank for whois
- Autoflush was enabled to cut code and make it easier to flush
- -h for changing of hostname implemnted
- -p implmented for changing ports on client
- The server is on port 43 
- Threading implemnted to around 200 - 300 threads
- UI implemented on client and server
- Hybrid UI was implmented so you can use the software in UI mode and console mode
- To launch UI on the server a -w argument needs to be supplied to the server in the command line console. If you want to use the client in console mode simply provide it arguments in the console.
- UI has stop server button for server 
- Client UI has preset port and server
- Server UI for exporting a file has a save file dialog
- Code commenting was completed
 

Optional Features:
- Command line optional features do not all work together (EG: 555426 London -t 400 -l G:\NetworkingLabs\LocationServer\LocationServer\bin\Debug ) it will take the first argument an execute that 
- Optional feature -t implemented on both client and server 
- Optional feature logging implemented on both client and server
- Optional feature logging exporting works on server UI and command line
- Optional Feature exporting database/Dictonary works fully on UI however on command line it writes the first entry (-f1)
- Logging format not exact due to the DateTime (0s are first rather than last) 






