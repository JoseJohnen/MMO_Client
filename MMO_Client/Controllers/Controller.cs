using Stride.Core.Collections;
using Stride.Engine;
using Stride.Graphics;
using Stride.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using MMO_Client.Controllers;
using Stride.Core.IO;
using Stride.Rendering;
using Interfaz.Models;
using System.Threading.Tasks;
using Interfaz.Utilities;
using MMO_Client.Code.Models;
using Stride.Core.Mathematics;
using System.Text.RegularExpressions;

namespace MMO_Client.Code.Controllers
{
    public class Controller : SyncScript
    {
        public PlayerController playerController;
        public WorldController worldController;
        protected ConnectionManager connectionManager;
        public static Controller controller;
        private static string instrucciones;

        public IDatabaseFileProviderService dataFileProviderService;

        private Timer CadaCiertosSegundos;
        private Timer CadaMedioSegundo;

        private UInt16 segundosTranscurridos = 0; //Da para un máximo de 65535, que son 18.204167 horas.
        private UInt16 seguroSegundosTranscurridos = 0;

        private float medioSegundoTranscurridos = 0; //Da para un máximo de 65535, que son 18.204167 horas.
        private float seguroMedioSegundoTranscurridos = 0;

        UIComponent uIComponent;
        EditText txtEmail;
        EditText txtPassword;
        Button btnLogin;

        public List<Prefab> l_Prefabs = new List<Prefab>();
        public List<Model> l_Models = new List<Model>();
        public List<SpriteSheet> l_Sprites = new List<SpriteSheet>();
        public List<Texture> l_Textures = new List<Texture>();
        public List<Material> l_Materials = new List<Material>();
        public List<UIPage> l_UI = new List<UIPage>();

        public bool movementDisable = false;
        public bool autoMovement = false;
        public bool isHistoryRelatedUsage = false;
        public bool isPressedZToRun = false;

        bool ActivateOrDeactivateLastText = false;
        string LastText = string.Empty;

        public bool isLoginInProcess = false;
        public bool isLoginSuccessfull = false;
        public DateTime dtIsLoginInProcess = DateTime.Now;

        public static TaskStatus dataAnswer = TaskStatus.Created;
        public static TaskStatus dataContinous = TaskStatus.Created;

        public Entity CursorPos = null;

        private readonly FastList<CameraComponent> cameraDb = new FastList<CameraComponent>();

        public DateTime lastPositionBeforeMove = DateTime.Now;

        bool textoFueraDeCutscene = false;
        private DateTime dateTimeTextoFueraDeCutScene = DateTime.Now;

        public UIPage page;
        public SpriteSheet MainSceneImages { get; set; }
        public bool TextoFueraDeCutscene
        {
            get => textoFueraDeCutscene; set
            {
                textoFueraDeCutscene = value;
                if (textoFueraDeCutscene == true)
                {
                    dateTimeTextoFueraDeCutScene = DateTime.Now;
                }
            }
        }

        private string Token;

        //public Sound BackgroundMusic;
        //public Sound GhostMusic;

        //public Sound GhostLullaby;
        //public Sound GhostScream;
        //public Sound BabyCry;
        //public Sound SonidoChoque;
        //public Sound Thunder;

        //private SoundInstance music;
        //private SoundInstance effect;

        //private Dictionary<string, Sound> DicMusic = new Dictionary<string, Sound>();
        //private Dictionary<string, Sound> DicEffect = new Dictionary<string, Sound>();

        public Entity thingy1;
        public Entity thingy2;
        public override void Start()
        {
            //Preparing to work itself
            Services.AddService(this);
            base.Start();
            controller = Entity.Get<Controller>();
            //Starting to prepare everything else

            InitTimer();

            //Prepare Service References
            dataFileProviderService = Services.GetService<IDatabaseFileProviderService>();

            playerController = Services.GetService<PlayerController>();
            worldController = Services.GetService<WorldController>();
            connectionManager = Services.GetService<ConnectionManager>();

            //Take active elements to be used
            l_Prefabs = GetItemsFromVirtualGameFolder<Prefab>("Prefabs");
            l_Models = GetItemsFromVirtualGameFolder<Model>("Models");
            l_Sprites = GetItemsFromVirtualGameFolder<SpriteSheet>("Sprites");
            l_Textures = GetItemsFromVirtualGameFolder<Texture>("Textures");
            l_Materials = GetItemsFromVirtualGameFolder<Material>("Materials");
            l_UI = GetItemsFromVirtualGameFolder<UIPage>("UI");


            //TODO: TEST: You just change the instanced sound in the right context, and maybie it start to sound.

            /*DicMusic.Add("BackgroundMusic",BackgroundMusic);
            DicMusic.Add("GhostMusic", GhostMusic);

            DicEffect.Add("GhostLullaby", GhostLullaby);
            DicEffect.Add("GhostScream", GhostScream);
            DicEffect.Add("BabyCry", BabyCry);
            DicEffect.Add("SonidoChoque", SonidoChoque);
            DicEffect.Add("Thunder", Thunder);

            ChangeMusic("BackgroundMusic");*/

            //effect = GhostLullaby.CreateInstance();
            /*audioEmitterComponent = Entity.Get<AudioEmitterComponent>();
            mySound1Controller = audioEmitterComponent["Background Music"];
            mySound2Controller = audioEmitterComponent["Ghost Music"];*/

            PreparingCameras();
            PrepareUI();


            //TODO: Delete when login is Added
        }

        public override void Update()
        {
            UpdatingCamera();
            worldController.WorldController_Tick();
            playerController.PlayerController_Tick();
            //ConnectionManager.Queue_Instrucciones.Clear();
            ActualizarConData();
        }

        #region Del Juego
        public bool ActualizarConData()
        {
            try
            {
                if (ConnectionManager.Queue_Answers.Count > 0)
                {
                    dataAnswer = Task.Run(() => ActualizarConDataDeRespuesta()).Status;
                }
                //else
                //{
                //    Console.WriteLine("ActualizarConDataDeRespuesta Status: "+dataAnswer + " Time: "+DateTime.Now.ToString());
                //}

                if (ConnectionManager.Queue_Instrucciones.Count > 0)
                {
                    dataContinous = Task.Run(() =>
                    {
                        ActualizarConDataDelServer();
                    }).Status;
                }
                //else
                //{
                //    Console.WriteLine("ActualizarConDataDelServer Status: " + dataContinous + " Time: " + DateTime.Now.ToString());
                //}

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error ActualizarConData: " + ex.Message);
                return false;
            }
        }

        public bool ActualizarConDataDelServer()
        {
            try
            {
                string item = string.Empty;
                while (ConnectionManager.Queue_Instrucciones.TryDequeue(out item))
                {
                    //uint index = 0;
                    //bool indexWasSuccessfull = false;

                    #region Region: Por si hay "Lag Spikes" y llega mas de un mensaje juntos
                    if (Regex.Matches(item, "MS:").Count > 1)
                    {
                        string[] tempStrArray = item.Split("MS:");
                        string[] strLessZero = new string[(tempStrArray.Length - 1)];

                        item = tempStrArray[0];
                        for (int i = 0; i < tempStrArray.Length; i++)
                        {
                            if (i > 0)
                            {
                                strLessZero[(i - 1)] = tempStrArray[i];
                            }
                        }

                        foreach (string tmpString in strLessZero)
                        {
                            ConnectionManager.Queue_Instrucciones.Enqueue(tmpString);
                        }
                    }

                    if (Regex.Matches(item, "}{").Count > 0)
                    {
                        item = item.Replace("}{", "}|°|MS:{");
                        string[] tempStrArray = item.Split("|°|");
                        item = tempStrArray[0];

                        if(tempStrArray.Length <= 1)
                        {
                            goto msg;
                        }

                        string[] strLessZero = new string[(tempStrArray.Length - 1)];

                        for (int i = 0; i < tempStrArray.Length; i++)
                        {
                            if (i > 0)
                            {
                                strLessZero[(i - 1)] = tempStrArray[i];
                            }
                        }

                        foreach (string tmpString in strLessZero)
                        {
                            ConnectionManager.Queue_Instrucciones.Enqueue(tmpString);
                        }
                    }
                    #endregion

                    msg:
                    item = UtilityAssistant.ExtractValues(item, "MS");
                    Message nwMsg = Message.CreateFromJson(item);

                    //Por si llegan de a pedacitos: TODO: Requiere testear y pulir bien
                    //R: Ya no va, en su lugar ConsolidateMessage es la clase que hace todo esto y lo trabaja
                    //fuera del loop regular de mensajes, cuando termina incorpora el mensaje armado al loop regular
                    /*if (nwMsg.IdRef > 0)
                    {
                        nwMsg = Message.ConsolidateMessages(nwMsg);
                        if(nwMsg == null)
                        {
                            return false;
                        }
                    }*/
                    //END TODO

                    //do
                    //{
                    //    index = (uint)Message.dic_ActiveMessages.Count;
                    //    indexWasSuccessfull = Message.dic_ActiveMessages.TryAdd(index, nwMsg);
                    //}
                    //while (indexWasSuccessfull == false);
                    //bool resultOfSearchDic = Message.dic_ActiveMessages.TryGetValue(index, out nwMsg);
                    //if (resultOfSearchDic == false)
                    //{
                    //    return false;
                    //}

                    if (string.IsNullOrWhiteSpace(nwMsg.TextOriginal))
                    {
                        return false;
                    }

                    string typeOf = nwMsg.TextOriginal.Substring(0, 2);
                    switch (typeOf)
                    {
                        case "LO":
                            Console.WriteLine("LOGIN_TRUE: FOR NOW: TODO!!!");
                            //playerController.ProcessMovementFromServer(item);
                            break;
                        case "CO":
                            playerController.ProcesarConversarObj(nwMsg.TextOriginal, nwMsg, out nwMsg);
                            Console.WriteLine(" ");
                            break;
                        /*case "SM":
                          if(playerController.ProcessShotFromServer(nwMsg, out nwMsg))
                           {
                               StateMessage stMsg = new StateMessage(nwMsg.IdMsg, nwMsg.Status);
                               ConnectionManager.gameSocketClient.l_SendQueueMessages.Enqueue(new Message("SM:" + stMsg.ToJson()).ToJson());
                           }
                        break;*/
                        default:
                            break;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error ActualizarConDataDelServer: " + ex.Message);
                return false;
            }
            //finally
            //{
            //    Console.WriteLine("Finally ActualizarConDataDelServer");
            //}
        }

        public bool ActualizarConDataDeRespuesta()
        {
            try
            {
                string item = string.Empty;
                while (ConnectionManager.Queue_Answers.TryDequeue(out item))
                {
                    //uint index = 0;
                    //bool indexWasSuccessfull = false;
                    if (Regex.Matches(item, "MS:").Count > 1)
                    {
                        string[] tempStrArray = item.Split("MS:");
                        item = tempStrArray[0];
                    }

                    if (Regex.Matches(item, "}{").Count > 0)
                    {
                        item = item.Replace("}{", "}|°|{");
                        string[] tempStrArray = item.Split("|°|");
                        item = tempStrArray[0];
                    }

                    string tempString = UtilityAssistant.ExtractValues(item, "MS");
                    Message nwMsg = Message.CreateFromJson(tempString);
                    //do
                    //{
                    //    index = (uint)Message.dic_ActiveMessages.Count;
                    //    indexWasSuccessfull = Message.dic_ActiveMessages.TryAdd(index, nwMsg);
                    //}
                    //while (indexWasSuccessfull == false);
                    //bool resultOfSearchDic = Message.dic_ActiveMessages.TryGetValue(index, out nwMsg);
                    //if (resultOfSearchDic == false)
                    //{
                    //    return false;
                    //}
                    if (string.IsNullOrWhiteSpace(nwMsg.TextOriginal))
                    {
                        return false;
                    }

                    string typeOf = nwMsg.TextOriginal.Substring(0, 2);
                    switch (typeOf)
                    {
                        case "MV":
                            playerController.ProcessMovementFromServer(nwMsg.TextOriginal, nwMsg, out nwMsg);
                            break;
                        case "ST":
                            /*if (playerController.CreateShot(nwMsg.Text, nwMsg, out nwMsg))
                            {
                                StateMessage stMsg1 = new StateMessage(nwMsg.IdMsg, nwMsg.Status);
                                ConnectionManager.gameSocketClient.l_SendQueueMessages.Enqueue("MS:" + stMsg1.ToJson());
                            }*/
                            break;
                        case "SS":
                            //playerController.DestroyShot(nwMsg.Text, nwMsg, out nwMsg);
                            //StateMessage stMsg2 = new StateMessage(nwMsg.IdMsg, nwMsg.Status);
                            //ConnectionManager.gameSocketClient.l_SendQueueMessages.Enqueue("MS:" + stMsg2.ToJson());
                            break;
                        case "PY":
                            if (typeOf.Equals("PYST"))
                            {
                                Console.WriteLine("PYST Received!!: " + nwMsg.Text);
                            }
                            break;
                        default:
                            break;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error ActualizarConDataDeRespuesta: " + ex.Message);
                return false;
            }
            //finally
            //{
            //    Console.WriteLine("Finally ActualizarConDataDeRespuesta");
            //}
        }
        #endregion

        #region Utilitarios
        #region Preparativos
        public List<T> GetItemsFromVirtualGameFolder<T>(string path, string filterByName = "") where T : class
        {
            try
            {
                string strPrepared = "*";
                if (!String.IsNullOrWhiteSpace(filterByName))
                {
                    strPrepared = "*" + filterByName + "*";
                }

                string[] l_strings = Controller.controller.dataFileProviderService.FileProvider.ListFiles(path, strPrepared, VirtualSearchOption.AllDirectories);
                List<T> l_temp = new List<T>();

                string strTemp = string.Empty;
                int i = 0;
                List<string> l_ignoreStrings = new List<string>();
                bool state = false;
                foreach (string item in l_strings)
                {
                    if (item.Contains("/gen/") || item.Contains("__ATLAS_TEXTURE__0") || item.Contains("_Data"))
                    {
                        if (item.Contains("/gen/"))
                        {
                            strTemp = item.Substring(item.IndexOf("/gen/"));
                            strTemp = item.Replace(strTemp, "");
                        }
                        else if (item.Contains("__ATLAS_TEXTURE__0"))
                        {
                            strTemp = item.Substring(item.IndexOf("__ATLAS_TEXTURE__0"));
                            strTemp = item.Replace(strTemp, "");
                        }
                        else if (item.Contains("_Data"))
                        {
                            strTemp = item.Substring(item.IndexOf("_Data"));
                            strTemp = item.Replace(strTemp, "");
                        }

                        foreach (string itm in l_ignoreStrings)
                        {
                            if (strTemp.ToUpper().Equals(itm.ToUpper()))
                            {
                                state = true;
                                break;
                            }
                        }

                        if (state == true)
                        {
                            state = false;
                            continue;
                        }

                        //Si no ha sido registrado antes y pasa el break
                        T tmp = Content.Load<T>(strTemp);
                        l_temp.Add(tmp);
                        i++;
                        l_ignoreStrings.Add(item);
                        continue;
                    }
                    T temp = Content.Load<T>(item);
                    l_temp.Add(temp);
                    i++;
                    l_ignoreStrings.Add(item);
                }
                return l_temp;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: string GetItemsFromVirtualGameFolder: " + ex.Message);
                return default(List<T>);
            }
        }
        #endregion

        #region Sonido y Musica
        //public void ChangeMusic(string nameSoundToPlay)
        //{
        //    return;
        //    if (music != null)
        //    {
        //        music.Stop();
        //    }
        //    music = null;
        //    music = DicMusic.GetValueOrDefault(nameSoundToPlay).CreateInstance();
        //    music.IsLooping = true;
        //    music.Play();
        //}

        //public void ChangeEffect(string nameSoundToPlay)
        //{
        //    return;
        //    if (effect != null)
        //    {
        //        effect.Stop();
        //    }
        //    effect = null;
        //    effect = DicEffect.GetValueOrDefault(nameSoundToPlay).CreateInstance();
        //    effect.Play();
        //}
        #endregion

        #region Contadores de Tiempo
        private void ContadorDeSegundos(object sender, EventArgs e)
        {
            if (segundosTranscurridos == 65000)
            {
                segundosTranscurridos = 0;
                //TODO: Recuerda que el contador debe regresar al mínimo conveniente, esto quiere decir
                //que debe volver al valor siguiente al valor del segundo de la última acción que ya no se debe realizar
                //que este fijada por este reloj.
            }

            segundosTranscurridos++;
        }

        private void ContadorDeMediosSegundos(object sender, EventArgs e)
        {
            if (medioSegundoTranscurridos == 65000)
            {
                medioSegundoTranscurridos = 0;
                //TODO: Recuerda que el contador debe regresar al mínimo conveniente, esto quiere decir
                //que debe volver al valor siguiente al valor del segundo de la última acción que ya no se debe realizar
                //que este fijada por este reloj.
            }

            medioSegundoTranscurridos += 0.5f;
        }

        public void InitTimer()
        {
            CadaCiertosSegundos = new Timer();
            CadaCiertosSegundos.Elapsed += new ElapsedEventHandler(ContadorDeSegundos);
            CadaCiertosSegundos.Interval = 1000; // in miliseconds

            CadaMedioSegundo = new Timer();
            CadaMedioSegundo.Elapsed += new ElapsedEventHandler(ContadorDeMediosSegundos);
            CadaMedioSegundo.Interval = 500; // in miliseconds
        }
        #endregion

        #region UI
        public void PrepareUI()
        {
            try
            {
                uIComponent = SceneSystem.SceneInstance.RootScene.Entities.FirstOrDefault(a => a.Name == "IntroCamera")?.Get<UIComponent>();

                if (uIComponent != null)
                {
                    UIPage re = uIComponent.Page;
                    txtEmail = (EditText)re.RootElement.FindName("txtEmail");
                    txtPassword = (EditText)re.RootElement.FindName("txtPassword");
                    btnLogin = (Button)re.RootElement.FindName("btnLogin");

                    btnLogin.Click += BtnLogin_Click;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error PrepareUI: " + ex.Message);
            }
        }

        public async void Login()
        {
            Message repositories = new Message();
            try
            {
                repositories = await ConnectionManager.PostLogin(txtEmail.Text, txtPassword.Text);

                Token = repositories.Text;
                Log.Info($"{repositories.Text} Received");

                isLoginInProcess = false;
                dtIsLoginInProcess = DateTime.Now;

                //Login Succeed
                if (!Token.Contains("Bad Credentials"))
                {
                    isLoginSuccessfull = true;
                    uIComponent.Enabled = false;
                    //ConnectionManager.l_stateObjects.Clear();
                    playerController.PlayerCharacterStart();
                    ConnectionManager.Instance.WhenLoginIsSuccessfullAsync();
                    NextCamera();
                    return;
                }
                isLoginSuccessfull = false;
            }
            catch (Exception ex)
            {
                isLoginInProcess = false;
                isLoginSuccessfull = false;
                if (string.IsNullOrWhiteSpace(repositories.Text))
                {
                    repositories = new Message();
                    repositories.Text = "it hasn't been possible to establish a connection with the server";
                }
                Log.Info($"Error Login(): {ex.Message} the message received back was {repositories.Text}");
                return;
            }
        }

        private void BtnLogin_Click(object sender, Stride.UI.Events.RoutedEventArgs e)
        {
            ConnectionManager.Instance.WhenLoginIsSuccessfullAsync();
            if (!isLoginInProcess)
            {
                isLoginInProcess = true;
                dtIsLoginInProcess = DateTime.Now;
                Login();
            }
        }
        #endregion

        #region Cameras
        //Prepare the Original suit of Cameras to work, you may want to add all the starting cameras in this method at start unless that cameras will be added at runtime, in such a case Register camera is the way.
        public void PreparingCameras()
        {
            foreach (Entity item in SceneSystem.SceneInstance.RootScene.Entities)
            {
                CameraComponent a = item.Get<CameraComponent>();
                if (a != null)
                {
                    Services.GetService<Controller>().RegisterCamera(a);
                }
            }
            //Services.GetService<Controller>().RegisterCamera(SceneSystem.SceneInstance.RootScene.Entities.FirstOrDefault(a => a.Name == "IntroCamera")?.Get<CameraComponent>());
        }

        public void UpdatingCamera()
        {

        }

        //Make the Camera procesed by this method the only Active Camera
        public void ActivateCamera(CameraComponent cameraComponent)
        {
            if (cameraComponent == null)
            {
                Log.Error("Entity attempted to activate a camera when no camera is attached.");
                return;
            }

            foreach (var camera in cameraDb)
            {
                camera.Enabled = false;
            }

            cameraComponent.Enabled = true;
            Log.Info($"{cameraComponent.Entity.Name} has been activated.");
        }

        //Make a Camera searched by his name the only Active Camera
        public void ActivateCamera(string cameraComponentName)
        {
            if (string.IsNullOrWhiteSpace(cameraComponentName))
            {
                Log.Error("Camera couldn't be found, are you certain it was registered using the RegisterCamera method?");
                return;
            }

            foreach (var camera in cameraDb)
            {
                if (string.IsNullOrWhiteSpace(camera.Name))
                {
                    camera.Name = "";
                }

                if (camera.Name.ToUpper() == cameraComponentName.ToUpper())
                {
                    camera.Enabled = true;
                    break;
                }
            }

            foreach (var camera in cameraDb)
            {
                if (camera.Name.ToUpper() != cameraComponentName.ToUpper())
                {
                    camera.Enabled = false;
                }
            }
            //Log.Info($"{cameraComponent.Entity.Name} has been activated.");
        }

        //Change the active camera to the next Camera in the list, also return the new active Camera.
        public CameraComponent NextCamera()
        {
            try
            {
                CameraComponent cameraComponent = default(CameraComponent);
                int i = 0;
                foreach (var camera in cameraDb)
                {
                    if (camera.Enabled == true)
                    {
                        camera.Enabled = false;
                        if (i >= (cameraDb.Count - 1))
                        {
                            i = 0;
                            cameraDb[i].Enabled = true;
                            cameraComponent = cameraDb[i];
                            Log.Info($"{cameraDb[i].Entity.Name} has been activated.");
                            break;
                        }
                        cameraDb[i + 1].Enabled = true;
                        cameraComponent = cameraDb[i + 1];
                        Log.Info($"{cameraDb[i + 1].Entity.Name} has been activated.");
                        break;
                    }
                    i++;
                }
                return cameraComponent;
            }
            catch (Exception ex)
            {
                Log.Error("Error NextCamera(): " + ex.Message);
                return default(CameraComponent);
            }
        }

        //internal static void SetInstrucciones(string returned)
        //{
        //    instrucciones = returned;

        //    //TODO: Acá se separan las instrucciones para cada controlador para después repartirlas entre ellos
        //    PlayerController.SetInstrucciones(instrucciones);
        //}

        //Return the Active Camera from the list of Cameras, if non, it will return a default.
        public CameraComponent GetActiveCamera()
        {
            try
            {
                CameraComponent cameraComponent = default(CameraComponent);
                foreach (var camera in cameraDb)
                {
                    if (camera.Enabled == true)
                    {
                        return camera;
                    }
                }
                return cameraComponent;
            }
            catch (Exception ex)
            {
                Log.Error("Error GetActiveCamera(): " + ex.Message);
                return default(CameraComponent);
            }
        }

        //It allow to add a Camera to the list of Cameras, avoid repetitions.
        public void RegisterCamera(CameraComponent cameraComponent)
        {
            if (cameraComponent == null)
            {
                Log.Error("Entity attempted to register a camera when no camera is attached.");
                return;
            }

            if (!cameraDb.Contains(cameraComponent))
            {
                if (cameraDb.Count > 0)
                {
                    if (GetActiveCamera().Name != cameraComponent.Name)
                    {
                        cameraComponent.Enabled = false;
                    }
                }

                cameraDb.Add(cameraComponent);
                Log.Info($"{cameraComponent?.Name} camera has been registered with camera db.");
            }
        }
        #endregion

        #region Auto-Extractores
        //    l_Models = GetItemsFromVirtualGameFolder<Model>("Models");
        //    l_Textures = GetItemsFromVirtualGameFolder<Texture>("Textures");
        //    l_Materials = GetItemsFromVirtualGameFolder<Material>("Materials");

        public List<Entity> GetPrefab(string prefabName)
        {
            try
            {
                Prefab prefab = null;
                prefab = this.l_Prefabs.Where(D => D.Entities[0].Name.ToUpper() == prefabName.ToUpper()).FirstOrDefault();
                if (prefab != default(Prefab))
                {
                    return prefab.Instantiate();
                }
                return new List<Entity>();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetPrefab(string): " + ex.Message);
                return new List<Entity>();
            }
        }

        /*public List<Entity> GetModel(string modelName)
        {
            try
            {
                //Model model = null;
                //model = this.l_Models.Where(D => D.Instantiate()..ToUpper() == modelName.ToUpper()).FirstOrDefault();
                //if (model != default(Model))
                //{
                //    return prefab.Instantiate();
                //}
                foreach (var item in l_Models)
                {
                    Console.WriteLine("-----");
                    Console.WriteLine(item);
                    Console.WriteLine("-----");
                }
                return new List<Entity>();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetPrefab(string): " + ex.Message);
                return new List<Entity>();
            }
        }*/

        public SpriteSheet GetSpriteSheet(string spriteSheetName)
        {
            try
            {
                SpriteSheet spriteSheet = null;
                spriteSheet = this.l_Sprites.Where(D => D.Sprites[(D.Sprites.Count - 1)].Name.ToUpper() == spriteSheetName.ToUpper()).FirstOrDefault();

                if (spriteSheet != default(SpriteSheet))
                {
                    return spriteSheet;
                }
                return new SpriteSheet();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetSpriteSheet(string): " + ex.Message);
                return new SpriteSheet();
            }
        }

        /*public Texture GetTexture(string textureName)
        {
            try
            {
                //Model model = null;
                //model = this.l_Models.Where(D => D.Instantiate()..ToUpper() == modelName.ToUpper()).FirstOrDefault();
                //if (model != default(Model))
                //{
                //    return prefab.Instantiate();
                //}
                foreach (var item in l_Materials)
                {
                    Console.WriteLine("-----");
                    Console.WriteLine(item);
                    Console.WriteLine("-----");
                }
                return new Texture();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetPrefab(string): " + ex.Message);
                return new Texture();
            }
        }*/
        #endregion
        #endregion

    }
}
