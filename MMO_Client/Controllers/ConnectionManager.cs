using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Linq;
using Interfaz.Models;
using SharpDX.MediaFoundation;
using static Stride.Audio.AudioLayer;

namespace MMO_Client.Code.Controllers
{
    public class ResultObject
    {
        public string result = string.Empty;
        public bool isCheked = false;
    }

    //It has to send and receive data internally, and return the received data back to the main Controller
    public class ConnectionManager : SyncScript
    {
        // Allow data procesing inside the while method.
        private static string inputCommand = string.Empty;
        public static ConnectionManager Instance;

        public bool isStarted = false;
        // State object for receiving data from remote device.  
        public static byte[] directions = new byte[1000];

        public static string url = "127.0.0.1"; //"192.168.0.5";
        public static int PortToSend = 22223; //8081;
        public static int PortToReceive = 22222; //8081;

        static GameSocketClient gameSocketClient = null;
        static AsyncCallback asncCallBack = null;
        static ManualResetEvent allDone = new ManualResetEvent(false);

        static bool retrySend = true;
        static bool retryRecv = true;
        private static bool? stopUpdateProcessing = false; //It change if preparations fail for some reason

        //To receive socket calls (Both Systems)
        static Socket listeningSocket = null;//new Socket(SocketType.Stream, ProtocolType.Tcp);
        public static List<string> l_instrucciones = new List<string>();

        #region Parameters Specific Old System
        public static string received = string.Empty;
        public static List<StateObject> l_stateObjects = new List<StateObject>();
        private static Thread receivingThread;
        private static bool receiving = false;
        private static Socket client = null;
        #endregion

        //Para llamadas Api/rest
        private static readonly HttpClient httpclient = new HttpClient();
        private static readonly string httpUrl = "http://127.0.0.1:8000/api/login";

        internal static bool isLoginSuccessfull = false;
        public override void Start()
        {
            Services.AddService(this);
            Instance = this;

            Task.Run(() => PrepareListeningSocket());

            //SendStartAsync();

            LogInSocket();
            Task.Run(() => SendAsync(url, PortToSend));
        }

        #region Listening Socket
        private async static void PrepareListeningSocket()
        {
            listeningSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            listeningSocket.Bind(new IPEndPoint(IPAddress.Any, PortToReceive));
            listeningSocket.Listen();
            asncCallBack = new AsyncCallback(AcceptCallback);

            DateTime startingTime = DateTime.Now;
            TimeSpan timeLapse = new TimeSpan(0, 0, 25); //in Seconds

            do
            {
                listeningSocket.BeginAccept(asncCallBack, listeningSocket);
                if (DateTime.Now - startingTime > timeLapse)
                {
                    startingTime = DateTime.Now;
                    Console.WriteLine("Its Listening!!!: " + DateTime.Now);
                }
            }
            while (true);
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                // Get the socket that handles the client request.
                Socket listener = (Socket)ar.AsyncState;

                if (gameSocketClient != null)
                {
                    gameSocketClient.ListenerSocket = listener.EndAccept(ar);
                    gameSocketClient.StreamSocket = new NetworkStream(gameSocketClient.ListenerSocket);
                }

                // Create the state object.
                //ReceiveStartAsync();
                Task.Run(() => ReceiveSteamAsync());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error AcceptCallback(IAsyncResult): " + ex.Message);
            }
        }
        #endregion

        #region Send - Receive Operations
        static async Task ReceiveSteamAsync()
        {
            try
            {
                byte[] responseBytes = new byte[1024];
                char[] responseChars = new char[1024];

                retryRecv = true;
                int size = 1000;

                if (gameSocketClient.StreamSocket == null)
                {
                    gameSocketClient.StreamSocket = new NetworkStream(gameSocketClient.ListenerSocket);
                }

                while (true)
                {
                    if (listeningSocket.Available > size)
                    {
                        size = listeningSocket.Available;
                        responseBytes = new byte[size];
                        responseChars = new char[size];
                    }

                    List<byte> allData = new List<byte>();
                    int numBytesRead = 0;
                    if (gameSocketClient.StreamSocket.DataAvailable && gameSocketClient.StreamSocket.CanRead)
                    {
                        do
                        {
                            numBytesRead = gameSocketClient.StreamSocket.Read(responseBytes, 0, responseBytes.Length);

                            if (numBytesRead == responseBytes.Length)
                            {
                                allData.AddRange(responseBytes);
                            }
                            else if (numBytesRead > 0)
                            {
                                allData.AddRange(responseBytes.Take(numBytesRead));
                            }
                        } while (gameSocketClient.StreamSocket.DataAvailable);
                    }

                    // Convert byteCount bytes to ASCII characters using the 'responseChars' buffer as destination
                    int charCount = Encoding.ASCII.GetChars(allData.ToArray(), 0, numBytesRead, responseChars, 0);

                    if (charCount == 0) continue;


                    if (responseChars.AsSpan(0, size).SequenceEqual("LOGIN_TRUE"))
                    {
                        retrySend = false;
                        isLoginSuccessfull = true;
                    }
                    
                    if (charCount > 0)
                    {
                        ConnectionManager.l_instrucciones.Add(new string(responseChars).Replace("\0", ""));
                        await Console.Out.WriteAsync("Received (StreamReader): " + responseChars.AsMemory(0, charCount));
                    }

                    responseBytes = new byte[1024];
                    responseChars = new char[1024];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error ReceiveAsync: " + ex.Message);
            }
            finally
            {
                if (gameSocketClient != null)
                {
                    gameSocketClient.CloseConnection();
                }
            }
        }

        static async Task SendAsync(string remoteHost, int remotePort)
        {
            try
            {
                if (gameSocketClient == null)
                {
                    gameSocketClient = new GameSocketClient();
                }

                if (gameSocketClient != null)
                {
                    if (gameSocketClient.SenderSocket == null)
                    {
                        bool makeSenderSocket = false;
                        TaskStatus tstatus = TaskStatus.Created;
                        gameSocketClient.SenderSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                        await gameSocketClient.SenderSocket.ConnectAsync(remoteHost, remotePort);
                    }

                    if (!gameSocketClient.SenderSocket.Connected)
                    {
                        await gameSocketClient.SenderSocket.ConnectAsync(remoteHost, remotePort);
                    }
                }

                retrySend = false;
                while (stopUpdateProcessing == false)
                {
                    if (gameSocketClient != null)
                    {
                        if (gameSocketClient.SenderSocket != null)
                        {
                            if (l_stateObjects.Count > 0)
                            {
                                foreach (StateObject item in l_stateObjects.ToList())
                                {
                                    if (item == null)
                                    {
                                        break;
                                    }

                                    if (item.sb == null)
                                    {
                                        stopUpdateProcessing = true;
                                    }

                                    //inputCommand += item.sb.ToString();
                                    inputCommand = item.sb.ToString().Trim();

                                    //Example of "Brute Processing" to close connection with the server
                                    if (inputCommand.Equals("<EXIT>"))
                                    {
                                        stopUpdateProcessing = true;
                                    }

                                    byte[] requestBytes = Encoding.ASCII.GetBytes(inputCommand);

                                    int bytesSent = 0;

                                    while (bytesSent < requestBytes.Length)
                                    {
                                        bytesSent += await gameSocketClient.SenderSocket.SendAsync(requestBytes.AsMemory(bytesSent), SocketFlags.None);
                                    }

                                    Console.WriteLine("Sending..." + inputCommand + " count: " + requestBytes.Length);
                                    //await Task.Delay(TimeSpan.FromSeconds(1));
                                    inputCommand = String.Empty;
                                }
                                l_stateObjects.Clear();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error SendAsync: " + ex.Message);
            }
            finally
            {
                if (gameSocketClient != null)
                {
                    if (gameSocketClient.SenderSocket.Connected)
                    {
                        gameSocketClient.CloseConnection();
                    }
                }
            }
        }

        static async Task ReceiveAsync()
        {
            try
            {
                byte[] responseBytes = new byte[1000];
                char[] responseChars = new char[1000];

                retryRecv = true;
                int size = 1000;
                while (true)
                {
                    if (listeningSocket.Available > size)
                    {
                        size = listeningSocket.Available;
                        responseBytes = new byte[size];
                        responseChars = new char[size];
                    }

                    int bytesReceived = await gameSocketClient.ListenerSocket.ReceiveAsync(responseBytes, SocketFlags.None);

                    // Receiving 0 bytes means EOF has been reached
                    if (bytesReceived == 0) break;

                    // Convert byteCount bytes to ASCII characters using the 'responseChars' buffer as destination
                    int charCount = Encoding.ASCII.GetChars(responseBytes, 0, bytesReceived, responseChars, 0);

                    //Console.WriteLine("new String(responseChars): " + new String(responseChars).Replace("\0",""));
                    //Console.WriteLine(new String(responseChars).Replace("\0", "") == "LOGIN_TRUE");
                    if (new String(responseChars).Replace("\0", "") == "LOGIN_TRUE")
                    {
                        retrySend = false;
                        isLoginSuccessfull = true;
                    }

                    ConnectionManager.l_instrucciones.Add(new string(responseChars).Replace("\0", ""));

                    // Print the contents of the 'responseChars' buffer to Console.Out
                    await Console.Out.WriteAsync("Received: " + responseChars.AsMemory(0, charCount));
                    responseBytes = new byte[1000];
                    responseChars = new char[1000];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error ReceiveAsync: " + ex.Message);
            }
            finally
            {
                if (gameSocketClient != null)
                {
                    gameSocketClient.CloseConnection();
                }
            }
        }

        #region Suplementary (Unused)
        static async void SendStartAsync()
        {
            TaskStatus tsst = TaskStatus.Canceled;
            TaskStatus lst_tsst = TaskStatus.Canceled;
            do
            {
                if (LogInSocket())
                {
                    if (tsst != TaskStatus.Running)
                    {
                        await Task.Run(() => tsst = SendAsync(url, PortToSend).Status);
                        if (tsst != lst_tsst)
                        {
                            lst_tsst = tsst;
                            Console.WriteLine("SendStartAsync Status: " + tsst.ToString());
                        }
                    }
                    //retrySend = false;
                }
            } while (retrySend);
        }

        static async void ReceiveStartAsync()
        {
            try
            {
                TaskStatus tsst = TaskStatus.Canceled;
                TaskStatus lst_tsst = TaskStatus.Canceled;
                do
                {
                    if (tsst != TaskStatus.Running)
                    {
                        await Task.Run(() => tsst = ReceiveAsync().Status).ConfigureAwait(false);
                        if (tsst != lst_tsst)
                        {
                            lst_tsst = tsst;
                            Console.WriteLine("ReceiveStartAsync Status: " + tsst.ToString());
                        }
                    }
                } while (retryRecv);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error ReceiveStartAsync: " + ex.Message);
            }
        }
        #endregion
        #endregion

        #region Others
        static bool LogInSocket()
        {
            try
            {
                if (isLoginSuccessfull)
                {
                    return true;
                }

                retrySend = false;
                Message msg = new Message();
                msg.Text = "some@email.com";
                string strMsg = msg.ToJson();
                StateObject stObj = new StateObject();
                stObj.addData(strMsg);
                l_stateObjects.Add(stObj);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error SendAsync: " + ex.Message);
                return false;
            }
        }
        #endregion

        #region Old System
        //Requiere AsyncScript
        /*public override Task Execute()
        {
            Services.AddService(this);
            Instance = this;

            StartClient();
            WhileRunning();
            return Task.CompletedTask;
        }*/

        public static async Task<Message> PostLogin(string email, string password)
        {
            httpclient.DefaultRequestHeaders.Accept.Clear();
            httpclient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded")
                );

            HttpContent httpContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("email", email),
                new KeyValuePair<string, string>("password", password),
            });

            HttpResponseMessage streamTask = await httpclient.PostAsync(httpUrl, httpContent).ConfigureAwait(true);
            string result = await streamTask.Content.ReadAsStringAsync();
            //TODO: Cambiar el tipo de "Message" recibido, aunque como vas a también cambiar lo que usarás de sistema de login
            //no debería ser demasiado inconveniente
            Message repositories = JsonSerializer.Deserialize<Message>(result);
            return repositories;
        }

        #region [Working code if it is converted back to SyncScript again (for some reason)
        /*public override void Start()
        {
            base.Start();
            StartClient();
        }

        public override void Update()
        {
            //It stop the code from executing if preparations fail for some reason
            if (stopUpdateProcessing == false)
            {
                try
                {
                    //Starting Sending capabilities

                    //Here must go the part than bring the textline to send, after processing TODO
                    string inputCommand = string.Empty;

                    inputCommand = Console.ReadLine();

                    //Example of "Brute Processing" to close connection with the server
                    if (inputCommand.Equals("<EXIT>"))
                    {
                        stopUpdateProcessing = true;
                    }

                    byte[] buffSend = Encoding.ASCII.GetBytes(inputCommand);
                    //Se puede hacer de una línea con la siguiente, pero mejor así porque así es mas fácil de debugear
                    client.Send(buffSend);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ConnectionManager Update: " + ex.Message);
                }
            }
            else if(stopUpdateProcessing == true)
            {
                if (client != null)
                {
                    if (client.Connected)
                    {
                        client.Shutdown(SocketShutdown.Both);
                    }

                    client.Close();
                    client.Dispose();
                    stopUpdateProcessing = null;
                }
            }
        }*/
        #endregion

        public async Task WhenLoginIsSuccessfullAsync()
        {
            StartClient();
            //l_stateObjects.Clear(); //Just start connection, therefore need a clean buffer.
            await Task.Run(WhileRunning);
        }

        public void StartClient()
        {
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPAddress ipaddr = null;
            int nPortInput = 0;

            try
            {
                if (!IPAddress.TryParse(url, out ipaddr))
                {
                    Console.WriteLine("Invalid server IP supplied.");
                    return;
                }
                if (!int.TryParse(PortToReceive.ToString(), out nPortInput))
                {
                    Console.WriteLine("Invalid port number supplied, return.");
                    return;
                }

                if (nPortInput <= 0 || nPortInput > 65535)
                {
                    Console.WriteLine("Port number must be between 0 and 65535.");
                    return;
                }

                System.Console.WriteLine(string.Format("IPAddress: {0} - Port: {1}", ipaddr.ToString(), nPortInput));

                client.Connect(ipaddr, nPortInput);
                //Connect is a blocking method, until is successfull or fail the connection

                Console.WriteLine("Connected to the server, type text and press enter to send it to the server, type <EXIT> to close.");

                //Starting Receiving Thread
                if (!receiving)
                {
                    receiving = ReceiveByOtherThread(client);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error StartClient(): " + ex.Message, ConsoleColor.Red);
                if (client != null)
                {
                    if (client.Connected)
                    {
                        client.Shutdown(SocketShutdown.Both);
                    }

                    client.Close();
                    client.Dispose();
                    stopUpdateProcessing = true;
                }
            }
        }

        public static bool AddInstruction(string instruction)
        {
            try
            {
                StateObject nwStObj = new StateObject();
                nwStObj.addData(instruction);
                l_stateObjects.Add(nwStObj);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error bool AddInstruction(string): " + ex.Message, ConsoleColor.Red);
                return false;
            }
        }

        public static StateObject PrepareData()
        {
            try
            {
                string result = string.Empty;
                foreach (StateObject item in l_stateObjects.ToList())
                {
                    result += item.sb.ToString();
                }
                l_stateObjects.Clear();
                StateObject nwStObj = new StateObject();
                nwStObj.addData(result);
                l_stateObjects.Add(nwStObj);
                return nwStObj;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error StateObject PrepareData(): " + ex.Message, ConsoleColor.Red);
                return new StateObject();
            }
        }

        public static StateObject PrepareData(out string result)
        {
            try
            {
                result = string.Empty;
                foreach (StateObject item in l_stateObjects.ToList())
                {
                    result += item.sb.ToString();
                }
                l_stateObjects.Clear();
                StateObject nwStObj = new StateObject();
                nwStObj.addData(result);
                l_stateObjects.Add(nwStObj);
                return nwStObj;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error StateObject PrepareData(out string): " + ex.Message, ConsoleColor.Red);
                result = string.Empty;
                return new StateObject();
            }
        }

        public async void WhileRunning()
        {
            while (Game.IsRunning)
            {
                //It stop the code from executing if preparations fail for some reason
                if (stopUpdateProcessing == false)
                {
                    try
                    {
                        //Starting Sending capabilities

                        //Here must go the part than bring the textline to send, after processing TODO
                        if (l_stateObjects.Count > 0)
                        {
                            foreach (StateObject item in l_stateObjects.ToList())
                            {
                                if (item == null)
                                {
                                    break;
                                }

                                if (item.sb == null)
                                {
                                    stopUpdateProcessing = true;
                                }

                                //inputCommand += item.sb.ToString();
                                inputCommand = item.sb.ToString().Trim();

                                //Example of "Brute Processing" to close connection with the server
                                if (inputCommand.Equals("<EXIT>"))
                                {
                                    stopUpdateProcessing = true;
                                }

                                byte[] buffSend = Encoding.ASCII.GetBytes(inputCommand);
                                inputCommand = String.Empty;
                                //Se puede hacer de una línea con la siguiente, pero mejor así porque así es mas fácil de debugear
                                client.Send(buffSend);

                                item.sb.Clear();
                            }
                            l_stateObjects.Clear();
                            //inputCommand = Console.ReadLine();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("ConnectionManager Update: " + ex.Message);
                        stopUpdateProcessing = true;
                    }
                }
                else if (stopUpdateProcessing == true)
                {
                    if (client != null)
                    {
                        if (client.Connected)
                        {
                            client.Shutdown(SocketShutdown.Both);
                        }

                        client.Close();
                        client.Dispose();
                        stopUpdateProcessing = false; //TODO: Cambiar a null para hacer que lo saque del sistema al romperse, false hace que vuelva a empezar.
                        StartClient();
                    }
                }
            }
        }

        public bool ReceiveByOtherThread(Socket client)
        {
            //Starting Receiving Thread
            receivingThread = new Thread(delegate ()
            {
                while (true)
                {
                    try
                    {
                        while (true)
                        {
                            byte[] buffReceived = new byte[1028];
                            int nRecv = client.Receive(buffReceived);
                            string returned = Encoding.ASCII.GetString(buffReceived, 0, nRecv).Trim();

                            //Controller.SetInstrucciones(returned);

                            Console.WriteLine("Data received: {0}", returned, ConsoleColor.Blue);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error ReceivingThread: " + ex.Message);
                        Console.ReadLine();
                    }
                    finally
                    {
                        Console.WriteLine("ReceivingThread: Finalized");
                        receiving = false;
                        //receivingThread.Abort();
                    }
                }
            });
            receivingThread.Start();
            return true;
        }

        public override void Update()
        {
            //throw new NotImplementedException();
        }
        #endregion
    }
}
