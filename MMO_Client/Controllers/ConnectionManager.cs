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
using System.Collections.Concurrent;
using MMO_Client.Code.Models;
using Interfaz.Utilities;
using System.Text.RegularExpressions;
using Interfaz.Models.Comms;

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

        public static GameSocketClient gameSocketClient = null;
        static AsyncCallback asncCallBack = null;
        static ManualResetEvent allDone = new ManualResetEvent(false);

        static bool retrySend = true;
        static bool retryRecv = true;
        private static bool? stopUpdateProcessing = false; //It change if preparations fail for some reason

        //To receive socket calls (Both Systems)
        static Socket listeningSocket = null;//new Socket(SocketType.Stream, ProtocolType.Tcp);
        public static ConcurrentQueue<string> Queue_Instrucciones = new ConcurrentQueue<string>();
        public static ConcurrentQueue<string> Queue_Answers = new ConcurrentQueue<string>();

        private static bool bolReceiveAsync = false;
        private static bool bolReceiveSteamAsync = false;

        #region Parameters Specific Old System
        public static string received = string.Empty;
        //public static List<StateObject> l_stateObjects = new List<StateObject>();
        private static Thread receivingThread;
        private static bool receiving = false;
        private static Socket client = null;
        #endregion

        //Para llamadas Api/rest
        private static readonly HttpClient httpclient = new HttpClient();
        private static readonly string httpUrl = "http://127.0.0.1:8000/api/login";

        internal static bool isLoginSuccessfull = false;

        //Variables a borrar después
        private static string MailTest = "some@email.com";
        //--FIN-- Variables a borrar después

        public override void Start()
        {
            Services.AddService(this);
            Instance = this;

            //Task.Run(() => PrepareListeningSocketHttpAsync());
            Task.Run(() => PrepareListeningSocketAsync());
            //SendStartAsync()

            LogInSocket();
            Task.Run(() => SendAsync(url, PortToSend));
            //Task.Run(() => ConsolidateMessage.CheckMissingMessages());
        }

        #region Listening Socket
        private static async Task PrepareListeningSocketHttpAsync()
        {
            try
            {
                listeningSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                listeningSocket.Bind(new IPEndPoint(IPAddress.Any, PortToReceive));
                listeningSocket.Listen();

                bool isListening = true;
                bool isConnected = false;
                Socket MisteriousSocket = null;
                do
                {
                    MisteriousSocket = await listeningSocket.AcceptAsync();
                    if(MisteriousSocket != null)
                    {
                        if(gameSocketClient.ListenerSocket == null)
                        {
                            if(!isConnected)
                            {
                                gameSocketClient.ListenerSocket = MisteriousSocket;
                                Task.Run(() => ReceiveAsync());
                                isConnected = true;
                            }
                            else
                            {
                                gameSocketClient.StreamSocket = MisteriousSocket;
                                Task.Run(() => ReceiveSteamAsync());
                            }
                        }
                    }

                    if (listeningSocket.Connected && gameSocketClient.StreamSocket != null)
                    {
                        listeningSocket.Close();
                        isListening = false;
                        Console.Out.WriteLineAsync("2 Connections Confirmed, listening Socket connection status: " + listeningSocket.Connected);
                    }
                }
                while (isListening);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error PrepareListeningSocketHttpAsync: " + ex.Message);
                Task.Run(() => PrepareListeningSocketHttpAsync());
            }
        }

        private static async Task PrepareListeningSocketAsync()
        {
            try
            {
                listeningSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                listeningSocket.Bind(new IPEndPoint(IPAddress.Any, PortToReceive));
                listeningSocket.Listen(2);

                bool isListening = true;

                Socket MisteriousSocket = null;
                do
                {
                    MisteriousSocket = await listeningSocket.AcceptAsync();
                    if (MisteriousSocket != null)
                    {
                        if (gameSocketClient.ReceiveAccepted == 0)
                        {
                            if (gameSocketClient.StreamSocket == null)
                            {
                                gameSocketClient.StreamSocket = MisteriousSocket;
                                if (gameSocketClient.StreamNetwork == null)
                                {
                                    gameSocketClient.StreamNetwork = new NetworkStream(gameSocketClient.StreamSocket);
                                    Console.WriteLine("gameSocketClient.StreamNetwork is set?: " + (gameSocketClient.StreamNetwork != null));
                                }
                                Task.Run(() => ReceiveSteamAsync());
                                gameSocketClient.ReceiveAccepted++;
                                Console.WriteLine("ReceivedAccepted (Stream): {0} Socket Received Type:  {1}", gameSocketClient.ReceiveAccepted, MisteriousSocket.SocketType);
                            }
                        }
                        else
                        {
                            if (gameSocketClient.ListenerSocket == null)
                            {
                                gameSocketClient.ListenerSocket = MisteriousSocket;
                                Console.WriteLine("gameSocketClient.ListenerSocket is set?: " + (gameSocketClient.ListenerSocket != null));
                            }
                            Task.Run(() => ReceiveAsync());
                            gameSocketClient.ReceiveAccepted++;
                            Console.WriteLine("ReceivedAccepted (Normal): {0} Socket Received Type:  {1}", gameSocketClient.ReceiveAccepted, MisteriousSocket.SocketType);
                        }

                        if (gameSocketClient.ReceiveAccepted == 2)
                        {
                            if (listeningSocket != null)
                            {
                                //Task.Run(() => SendSteamAsync(url, PortToSend));
                                if (listeningSocket.Connected)
                                {
                                    listeningSocket.Close();
                                    isListening = false;
                                    Console.WriteLine("2 Connections Confirmed, listening Socket connection status: " + listeningSocket.Connected);
                                }
                            }
                        }
                    }
                } while (isListening);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error PrepareListeningSocketAsync: " + ex.Message);
                Task.Run(() => PrepareListeningSocketAsync());
            }
        }
        #endregion

        #region Send - Receive Operations
        static async Task SendSteamAsync(string remoteHost, int remotePort)
        {
            try
            {
                int baseSize = 1024;
                byte[] responseBytes = new byte[baseSize];
                char[] responseChars = new char[baseSize];

                retryRecv = true;
                int size = 1000;

                if (gameSocketClient == null)
                {
                    gameSocketClient = new GameSocketClient();
                }

                if (gameSocketClient != null)
                {
                    if (gameSocketClient.StreamSocket == null)
                    {
                        bool makeSenderSocket = false;
                        TaskStatus tstatus = TaskStatus.Created;
                        gameSocketClient.StreamSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                        await gameSocketClient.StreamSocket.ConnectAsync(remoteHost, remotePort);
                    }

                    if (gameSocketClient.StreamNetwork == null)
                    {
                        if (!gameSocketClient.StreamSocket.Connected)
                        {
                            await gameSocketClient.StreamSocket.ConnectAsync(remoteHost, remotePort);
                        }
                        gameSocketClient.StreamNetwork = new NetworkStream(gameSocketClient.StreamSocket);
                    }
                }

                bool stopUpdateProcessing = false;
                //Task.Run(() => ReceiveSteamAsync());
                while (stopUpdateProcessing == false)
                {
                    if (gameSocketClient != null)
                    {
                        if (gameSocketClient.StreamSocket != null)
                        {
                            if (gameSocketClient.l_SendBigMessages.Count > 0)
                            {
                                string item = string.Empty;
                                while (gameSocketClient.l_SendBigMessages.TryTake(out item))
                                //while (gameSocketClient.l_SendBigMessages.TryDequeue(out item))
                                {
                                    if (string.IsNullOrWhiteSpace(item))
                                    {
                                        continue;
                                    }

                                    //Example of "Brute Processing" to close connection with the server
                                    if (item.Equals("<EXIT>"))
                                    {
                                        stopUpdateProcessing = true;
                                    }

                                    byte[] requestBytes = Encoding.ASCII.GetBytes(item);

                                    await gameSocketClient.StreamNetwork.WriteAsync(requestBytes, 0, requestBytes.Length);

                                    Console.Out.WriteLineAsync("\n\n " + DateTime.Now.ToString() + " Sending (Stream)..." + item + " count: " + requestBytes.Length);
                                    //gameSocketClient.l_SendQueueMessages.Remove(item);
                                    //await Task.Delay(TimeSpan.FromSeconds(1));
                                }
                                //gameSocketClient.l_SendQueueMessages.Clear();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLineAsync("Error SendSteamAsync: " + ex.Message);
            }
            finally
            {
                if (gameSocketClient != null)
                {
                    if (gameSocketClient.StreamSocket.Connected)
                    {
                        gameSocketClient.CloseConnection();
                    }
                }
            }
        }

        static async Task ReceiveSteamAsync()
        {
            int size = 1000;
            int charCount = 0;
            try
            {
                int baseSize = 1024;
                byte[] responseBytes = new byte[baseSize];
                char[] responseChars = new char[baseSize];

                if (gameSocketClient.StreamSocket == null)
                {
                    return;
                    //This will refuse to start until there is a StreamSocket ready
                }

                if (gameSocketClient.StreamNetwork == null)
                {
                    gameSocketClient.StreamNetwork = new NetworkStream(gameSocketClient.StreamSocket);
                }

                bolReceiveSteamAsync = true;
                List<byte> allData = new List<byte>();
                while (true)
                {
                    if (gameSocketClient.StreamSocket.Available > size)
                    {
                        size = gameSocketClient.StreamSocket.Available;
                        responseBytes = new byte[size];
                        responseChars = new char[size];
                    }

                    int numBytesRead = 0;
                    if (gameSocketClient.StreamNetwork.DataAvailable && gameSocketClient.StreamNetwork.CanRead)
                    {
                        do
                        {
                            numBytesRead = await gameSocketClient.StreamNetwork.ReadAsync(responseBytes, 0, responseBytes.Length);

                            if (numBytesRead == responseBytes.Length)
                            {
                                allData.AddRange(responseBytes);
                                break;
                            }
                            else if (numBytesRead > 0 && numBytesRead < responseBytes.Length)
                            {
                                allData.AddRange(responseBytes.Take(numBytesRead));
                                break;
                            }
                            else if (numBytesRead > responseBytes.Length)
                            {
                                allData.AddRange(responseBytes.Take(numBytesRead));
                            }
                        } while (gameSocketClient.StreamNetwork.DataAvailable && numBytesRead != 0);
                    }

                    // Convert byteCount bytes to ASCII characters using the 'responseChars' buffer as destination
                    charCount = Encoding.ASCII.GetChars(allData.ToArray(), 0, numBytesRead, responseChars, 0);

                    // Convert byteCount DIRECTLY TO A CLASS
                    //BinaryFormatter formatter = new BinaryFormatter();
                    //formatter.Serialize(fs, addresses);
                    //charCount = Encoding.ASCII.GetChars(allData.ToArray(), 0, numBytesRead, responseChars, 0);

                    if (charCount == 0) continue;

                    string responseString = new String(responseChars).Replace("\0", "");
                    string first3Char = string.Empty;
                    if (responseString.IndexOf(":") <= 6)
                    {
                        first3Char = responseString.Substring(0, responseString.IndexOf(":") + 1);

                        //Limpiando un poco por cosas que el CleanJSON no tiene responsabilidad de limpiar (como repeticiones)
                        List<string> l_strings = new List<string>();
                        if (responseString.Contains("MS:"))
                        {
                            //Entro aqui? entonces quiere decir que quedan mas MS: aparte del inicial, estos ya solo deberían ser limpiados
                            responseString = responseString.Replace("MS:", "");
                        }
                        if (responseString.Contains("}{"))
                        {
                            responseString = responseString.Replace("}{", "}|°|{");
                        }
                        string[] strArray = responseString.Split("|°|", StringSplitOptions.RemoveEmptyEntries);
                        l_strings.AddRange(strArray);
                        l_strings = l_strings.Distinct().ToList();
                        //END Special Cleaning

                        foreach (string strPreClean in l_strings)
                        {
                            responseString = UtilityAssistant.CleanJSON(strPreClean);

                            //En desuso
                            //if (responseChars.AsSpan(0, responseChars.Length).SequenceEqual("LOGIN_TRUE"))
                            //{
                            //    Player.PLAYER.Entity.Name = MailTest;
                            //    retrySend = false;
                            //    isLoginSuccessfull = true;
                            //}
                            //Fin en desuso

                            if (charCount > 0)
                            {
                                string[] strResp = new string[1];
                                string answer = string.Empty;

                                string tstString = string.Empty;
                                if (responseString.Contains("IdMsg"))
                                {
                                    Message msgResult = new Message();
                                    if (Regex.Matches("IdMsg", responseString).Count >= 2)
                                    {
                                        if (responseString.Contains("}{"))
                                        {
                                            responseString = responseString.Replace("}{", "}|°|{");
                                            strResp = responseString.Split("|°|", StringSplitOptions.RemoveEmptyEntries);
                                            foreach (string item in strResp)
                                            {
                                                if (first3Char.Contains(":") && !first3Char.Contains("{") && !responseString.Contains(first3Char))
                                                {
                                                    answer = first3Char + item;
                                                }

                                                if (!ConsolidateMessage.CheckJSONMessageIfMatch(responseString, out msgResult))
                                                {
                                                    if (Message.ValidTextFromJsonMsg(responseString))
                                                    {
                                                        ConnectionManager.Queue_Instrucciones.Enqueue(answer);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            answer = responseString;
                                            if (first3Char.Contains(":") && !first3Char.Contains("{") && !responseString.Contains(first3Char))
                                            {
                                                answer = first3Char + responseString;
                                            }

                                            if (!ConsolidateMessage.CheckJSONMessageIfMatch(responseString, out msgResult))
                                            {
                                                if (Message.ValidTextFromJsonMsg(responseString))
                                                {
                                                    ConnectionManager.Queue_Instrucciones.Enqueue(answer);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        answer = responseString;
                                        if (first3Char.Contains(":") && !first3Char.Contains("{") && !responseString.Contains(first3Char))
                                        {
                                            answer = first3Char + responseString;
                                        }

                                        if (!ConsolidateMessage.CheckJSONMessageIfMatch(responseString, out msgResult))
                                        {
                                            if (Message.ValidTextFromJsonMsg(responseString))
                                            {
                                                ConnectionManager.Queue_Instrucciones.Enqueue(answer);
                                            }
                                        }
                                    }

                                    if (msgResult != null)
                                    {
                                        if (UtilityAssistant.TryBase64Decode(msgResult.text, out tstString))
                                        {
                                            if (!string.IsNullOrWhiteSpace(tstString))
                                            {
                                                ConnectionManager.Queue_Instrucciones.Enqueue("MS:" + msgResult.ToJson());
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    answer = responseString;
                                    if (first3Char.Contains(":") && !first3Char.Contains("{") && !responseString.Contains(first3Char))
                                    {
                                        if (!string.IsNullOrWhiteSpace(responseString))
                                        {
                                            answer = first3Char + responseString;
                                        }
                                    }

                                    ConnectionManager.Queue_Instrucciones.Enqueue(answer);
                                }
                                Console.BackgroundColor = ConsoleColor.Blue;
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.Out.WriteLineAsync("\n\n " + DateTime.Now.ToString() + " Size of the Receivede (Stream) message is: " + answer.Length + " total");
                                Console.ResetColor();
                                //await Console.Out.WriteAsync("\n\nReceived (StreamReader): size: " + size + " charCount: " + charCount + " responseChar: " + responseChars.AsMemory(0, charCount));
                                await Console.Out.WriteAsync("\n\n " + DateTime.Now.ToString() + " Received (StreamReader): size: " + size + " charCount: " + charCount + " responseString: first3Char: " + first3Char + " \n\n " + responseString);
                            }

                        }
                    }

                    allData.Clear();
                    responseBytes = new byte[baseSize];
                    responseChars = new char[baseSize];
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLineAsync("Error ReceiveSteamAsync: size: " + size + " charCount: " + charCount + " Message: " + ex.Message);
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
            string rmHst = remoteHost;
            int rmPort = remotePort;
            int positionDondeCae = 0;
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
                string item = string.Empty;
                while (stopUpdateProcessing == false)
                {
                    if (gameSocketClient != null)
                    {
                        if (gameSocketClient.SenderSocket != null)
                        {
                            if (gameSocketClient.l_SendQueueMessages.Count > 0)
                            {
                                while (gameSocketClient.l_SendQueueMessages.TryTake(out item))
                                //while (gameSocketClient.l_SendQueueMessages.TryDequeue(out item))
                                {
                                    if (string.IsNullOrWhiteSpace(item))
                                    {
                                        break;
                                    }

                                    //inputCommand += item.sb.ToString().Trim();
                                    inputCommand = item.Trim();

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

                                    Console.Out.WriteLineAsync("\n\n " + DateTime.Now.ToString() + " Sending..." + inputCommand + " count: " + requestBytes.Length);
                                    //await Task.Delay(TimeSpan.FromSeconds(1));
                                    inputCommand = String.Empty;
                                }
                                //l_stateObjects.Clear();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLineAsync("Error SendAsync: " + ex.Message);
            }
            finally
            {
                if (gameSocketClient != null)
                {
                    if (gameSocketClient.SenderSocket.Connected)
                    {
                        gameSocketClient.CloseConnection();
                    }
                    else
                    {
                        Task.Run(() => SendAsync(rmHst, rmPort));
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
                bolReceiveAsync = true;
                while (true)
                {
                    if (gameSocketClient.ListenerSocket.Available > size)
                    {
                        size = gameSocketClient.ListenerSocket.Available;
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

                    string responseString = new String(responseChars).Replace("\0", "");
                    string first3Char = string.Empty;
                    if (responseString.IndexOf(":") <= 6)
                    {
                        first3Char = responseString.Substring(0, responseString.IndexOf(":") + 1);
                        responseString = UtilityAssistant.CleanJSON(responseString);
                    }

                    //En desuso
                    //if (responseString.Replace("\0", "") == "LOGIN_TRUE")
                    //{
                    //    Models.Player.PLAYER.Entity.Name = MailTest;
                    //    retrySend = false;
                    //    isLoginSuccessfull = true;
                    //}
                    //Fin en desuso

                    if (responseString.Contains("ST") && !responseString.Contains("MS:"))
                    {
                        Console.WriteLine("ENtro, COMO CHUCHA A ACA!");
                    }

                    string answer = responseString;
                    if (first3Char.Contains(":") && !first3Char.Contains("{") && !responseString.Contains(first3Char))
                    {
                        answer = first3Char + responseString;
                    }
                    ConnectionManager.Queue_Answers.Enqueue(answer);

                    // Print the contents of the 'responseChars' buffer to Console.Out
                    //await Console.Out.WriteAsync("\n\nReceived: " + responseChars.AsMemory(0, charCount));
                    await Console.Out.WriteAsync("\n\n " + DateTime.Now.ToString() + " Received: first3Char: " + first3Char + " \n\n " + answer);
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
                msg.Text = "EM:" + MailTest;
                string strMsg = msg.ToJson();
                //StateObject stObj = new StateObject();
                //stObj.addData(strMsg);

                //Descomentar cuando se use sistema Send de socket normal y no Stream
                //l_stateObjects.Add(stObj);

                //Comentar cuando no se user sistema Stream y si socket normal
                if (gameSocketClient == null)
                {
                    gameSocketClient = new GameSocketClient();
                }

                if (gameSocketClient.l_SendQueueMessages == null)
                {
                    gameSocketClient.l_SendQueueMessages = new BlockingCollection<string>();
                }

                gameSocketClient.l_SendQueueMessages.TryAdd("MS:" + strMsg);
                //gameSocketClient.l_SendQueueMessages.Enqueue(strMsg);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LogInSocket: " + ex.Message);
                return false;
            }
        }
        #endregion

        #region Send - Receive Suplementary Methods (Unused)
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

        #region Old Listening Socket
        private static void PrepareListeningSocketEvent()
        {
            try
            {
                listeningSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                listeningSocket.Bind(new IPEndPoint(IPAddress.Any, PortToReceive));
                listeningSocket.Listen();
                asncCallBack = new AsyncCallback(AcceptCallback);

                listeningSocket.BeginAccept(asncCallBack, listeningSocket);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error PrepareListeningSocketEvent: " + ex.Message);
                PrepareListeningSocketEvent();
            }
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                // Get the socket that handles the client request.
                //Socket listener = (Socket)ar.AsyncState;

                if (gameSocketClient != null)
                {
                    if (gameSocketClient.ListenerSocket == null)
                    {
                        Console.WriteLine("ListenerSocket is not null now i assume: " + (gameSocketClient.ListenerSocket != null));
                        gameSocketClient.ListenerSocket = ((Socket)ar.AsyncState).EndAccept(ar);
                        gameSocketClient.ReceiveAccepted++;
                        Console.WriteLine("AcceptCallback ReceivedAccepted: " + gameSocketClient.ReceiveAccepted);
                        Task.Run(() => ReceiveAsync());

                        listeningSocket.BeginAccept(asncCallBack, listeningSocket);
                    }
                    else
                    {
                        if (gameSocketClient.StreamSocket == null)
                        {
                            gameSocketClient.StreamSocket = ((Socket)ar.AsyncState).EndAccept(ar);
                            if (gameSocketClient.StreamNetwork == null)
                            {
                                gameSocketClient.StreamNetwork = new NetworkStream(gameSocketClient.StreamSocket);
                            }
                            gameSocketClient.ReceiveAccepted++;
                            Console.WriteLine("AcceptCallback ReceivedAccepted: " + gameSocketClient.ReceiveAccepted);
                            Task.Run(() => ReceiveSteamAsync());

                            listeningSocket.BeginAccept(asncCallBack, listeningSocket);
                        }
                    }
                }

                // Create the state object.
                //ReceiveStartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error AcceptCallback(IAsyncResult): " + ex.Message);
                gameSocketClient.ReceiveAccepted = 0;
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
            //await Task.Run(WhileRunning);
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
                //Comentado - Descomentar si se vuelve a StateObject
                //StateObject nwStObj = new StateObject();
                //nwStObj.addData(instruction);Gundam Wing Endless Waltz ending
                //l_stateObjects.Add(nwStObj);
                Message msg = null;
                if (!instruction.Contains("MS:"))
                {
                    msg = Message.CreateMessage(instruction, true);
                    instruction = "MS:" + msg.ToJson();
                }

                //Dejar descomentado si se esta utilizando gameSocketClient
                gameSocketClient.l_SendQueueMessages.TryAdd(instruction);
                //gameSocketClient.l_SendQueueMessages.Enqueue(instruction);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error bool AddInstruction(string): " + ex.Message, ConsoleColor.Red);
                return false;
            }
        }

        /*public static StateObject PrepareData()
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
                            //l_stateObjects.Clear();
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
        }*/

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
