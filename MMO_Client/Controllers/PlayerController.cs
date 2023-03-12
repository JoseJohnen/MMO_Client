using System;
using Stride.Core.Mathematics;
using Stride.Input;
using Stride.Engine;
using System.Collections.Generic;
using System.Linq;
using MMO_Client.Code.Models;
using System.Reflection;
using MMO_Client.Code.Interfaces;
using MMO_Client.Code.Assistants;
using MMO_Client.Code.Controllers;
using Stride.Rendering;
using Stride.Rendering.Sprites;
using Stride.Graphics;
using Controller = MMO_Client.Code.Controllers.Controller;
using Quaternion = Stride.Core.Mathematics.Quaternion;
using Interfaz.Models;
using Stride.Core;
using System.Collections.Concurrent;
using System.Windows.Interop;
using Interfaz.Utilities;
using UtilityAssistant = MMO_Client.Code.Assistants.UtilityAssistant;
using SerializedVector3 = MMO_Client.Code.Models.SerializedVector3;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http;
using Stride.Core.Extensions;

namespace MMO_Client.Controllers
{
    public class PlayerController : StartupScript
    {
        #region Atributos
        // Declared public member fields and properties will show in the game studio
        public List<Puppet> l_entitysCharacters = new List<Puppet>(); //Other Characters, Players or not

        public List<Pares<List<Entity>, Bullet>> l_bullets = new List<Pares<List<Entity>, Bullet>>();

        public static bool isQuestionAsked = false;

        [DataMemberIgnore]
        public static ConcurrentDictionary<string, Bullet> dic_bulletsOnline = new ConcurrentDictionary<string, Bullet>();
        [DataMemberIgnore]
        public ConcurrentQueue<Entity> q_NewEntitiesToScene = new ConcurrentQueue<Entity>();
        [DataMemberIgnore]
        public ConcurrentQueue<Entity> q_RemoveEntitiesFromScene = new ConcurrentQueue<Entity>();

        [DataMemberIgnore]
        public ConcurrentQueue<Shot> q_PendingShotsCreatedRun = new ConcurrentQueue<Shot>();
        [DataMemberIgnore]
        public ConcurrentQueue<ShotPosUpdate> q_PendingShotPosUpdateRun = new ConcurrentQueue<ShotPosUpdate>();
        [DataMemberIgnore]
        public ConcurrentQueue<ShotState> q_PendingShotStateToRun = new ConcurrentQueue<ShotState>();
        

        public Area ActiveArea;
        public List<Area> l_ActiveAreaFurniture;
        public Thread workerThread = null;

        public List<Trios<int, Puppet, TimeSpan>> l_AnimacionesEntitys = new List<Trios<int, Puppet, TimeSpan>>();
        static DateTime lastFrame = DateTime.Now;

        public Prefab shot;

        public CameraComponent OtherCamera;

        public Vector3 modifier = Vector3.Zero;

        public bool CameraFollowing = false;
        internal bool isStoppedMoving = false;
        internal bool isStoppedRotate = false;
        #endregion

        #region Executing Functions
        public override void Start()
        {
            try
            {
                Services.AddService(this);
                Controller.controller.playerController = this;

                PlayerCharacterStart();
                //player.Transform.position = new Vector3(7, 0, 2);

                //Prefab model = Content.Load<Prefab>("Prefabs/DoomGuy");
                //Quaternion qtrn = new Quaternion();
                //Quaternion.RotationY(90, out qtrn);
                EnemyNPCStart("DoomGuy");
                //l_entitysCharacters[0].RealEnt.Transform.Rotation = qtrn;
                //Message messageOut = new Message();
                //Shot shot = new Shot();
                //shot.Id = "\"aaa\"";
                //CreateBullet(shot, messageOut, out messageOut);

                lastFrame = DateTime.Now;
                /*workerThread = new Thread(new ThreadStart(PreguntarWhileHttp));
                workerThread.IsBackground = true;
                workerThread.Start();*/
                //Parallel.Invoke(PreguntarWhileHttp);

                // Load a model (replace URL with valid URL)

                //DirectoryInfo d = new DirectoryInfo(@"Prefabs/"); //Assuming Test is your Folder

                //FileInfo[] Files = d.GetFiles("*.sdprefab"); //Getting Text files
                //string str = "";

                //foreach (FileInfo file in Files)
                //{
                //  Console.WriteLine("FileInfo: "+ file);
                //SceneSystem.SceneInstance.RootScene.Entities.Add(new Entity(,));
                //}

                // Create a new entity to add to the scene
                //Entity entity = new Entity(Vector3.Zero, "Entity Added by Script");

                // Add a new entity to the scene
                //SceneSystem.SceneInstance.RootScene.Entities.Add(entity);

                // It is possible that our entity does not have a parent. We therefore check if the parent is not null.
                if (Player.PLAYER.Entity != null)
                {
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("PlayerController Start() Error: " + ex.Message);
            }
        }

        public void PlayerController_Tick()
        {
            try
            {
                if (Player.PLAYER == default(Player))
                {
                    return;
                }
                if (Player.PLAYER.Weapon == null)
                {
                    return;
                }

                if (!Controller.controller.isLoginSuccessfull && Controller.controller.isLoginInProcess)
                {
                    if (DateTime.Now - Controller.controller.dtIsLoginInProcess > new TimeSpan(0, 0, 7))
                    {
                        Controller.controller.isLoginInProcess = false;
                    }
                }

                MovementOnline();
                ShotOnline();
                //ShotAndProyectileProcessing();
                Animacion();
                Limpieza();
                //Parallel.Invoke(Preguntar);
                //Parallel.Invoke(PreguntarWhileHttp);
                //Preguntar();
            }
            catch (Exception ex)
            {
                Console.WriteLine("PlayerController_Tick() Error: " + ex.Message);
            }
        }

        private void Limpieza()
        {
            try
            {
                Entity ent = null;
                while(q_RemoveEntitiesFromScene.TryDequeue(out ent))
                {
                    Entity.Scene.Entities.Remove(ent);
                }

                foreach (KeyValuePair<string,Bullet> item in dic_bulletsOnline.Reverse())
                {
                    if(DateTime.Now - item.Value.LastUpdate >= new TimeSpan(0,0,3))
                    {
                        if (dic_bulletsOnline.TryRemove(item))
                        {
                            Entity.Scene.Entities.Remove(item.Value.ProyectileBody);
                        }
                    }
                }

                foreach (Entity item in Entity.Scene.Entities.Where(c => dic_bulletsOnline.Values.All(c2 => ("Bullet_"+c2.id) != c.Name) && c.Name.Contains("Bullet_")).Reverse()) // Entity.Scene.Entities.Where(c => !excludedIDs.Contains(c.Name) && c.Name.Contains("Bullet")).Reverse())
                {
                    Entity.Scene.Entities.Remove(item);
                }

                while(q_NewEntitiesToScene.TryDequeue(out ent))
                {
                    Entity.Scene.Entities.Add(ent);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Limpieza() Error: " + ex.Message);
            }
        }

        private void Preguntar()
        {
            try
            {
                PreguntaObj prtObj = new PreguntaObj();
                Console.Out.WriteLineAsync("\nPreguntando...\n");
                MissingMessages mMsg = new MissingMessages();
                if (MissingMessages.q_MissingMessages.Count > 0)
                {
                    while (MissingMessages.q_MissingMessages.TryDequeue(out mMsg))
                    {
                        ConnectionManager.gameSocketClient.l_SendQueueMessages.TryAdd("MM:" + mMsg.ToJson());
                        //ConnectionManager.gameSocketClient.l_SendQueueMessages.Enqueue("MM:" + mMsg.ToJson());
                    }
                    return;
                }

                if (isQuestionAsked)
                {
                    if (DateTime.Now - lastFrame > new TimeSpan(0, 0, 0, 1, 50))
                    {
                        isQuestionAsked = false;
                        lastFrame = DateTime.Now;
                    }
                    return;
                }
                isQuestionAsked = true;

                //TODO: ¿Cuál es el criterio para enviar las preguntas?
                //- Tiempo de las balas
                //- Que no hayan balas, y si es así, cada medio segundo
                if (dic_bulletsOnline.Count <= 0)
                {
                    if (DateTime.Now - lastFrame > new TimeSpan(0, 0, 0, 0, 50))
                    {
                        ConnectionManager.gameSocketClient.l_SendBigMessages.TryAdd("PR:" + prtObj.ToJson());
                        //ConnectionManager.gameSocketClient.l_SendBigMessages.Enqueue("PR:" + prtObj.ToJson());
                        lastFrame = DateTime.Now;
                    }
                }
                else
                {
                    prtObj.l_id_bullets_preguntando.AddRange(dic_bulletsOnline.Keys.ToList());
                    foreach (Bullet item in dic_bulletsOnline.Values)
                    {
                        if (DateTime.Now - lastFrame > new TimeSpan(0, 0, 0, 0, 50))
                        {
                            if (DateTime.Now - item.LastUpdate >= item.Velocity)
                            {
                                ConnectionManager.gameSocketClient.l_SendBigMessages.TryAdd("PR:" + prtObj.ToJson());
                                //ConnectionManager.gameSocketClient.l_SendBigMessages.Enqueue("PR:" + prtObj.ToJson());
                                return;
                            }
                            lastFrame = DateTime.Now;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.Out.WriteLineAsync("Error Preguntar(): " + ex.Message);
                Console.ResetColor();
            }
        }

        private void PreguntarWhile()
        {
            try
            {
                do
                {
                    PreguntaObj prtObj = new PreguntaObj();
                    Console.Out.WriteLineAsync("\nPreguntando...\n");
                    MissingMessages mMsg = new MissingMessages();
                    if (MissingMessages.q_MissingMessages.Count > 0)
                    {
                        while (MissingMessages.q_MissingMessages.TryDequeue(out mMsg))
                        {
                            ConnectionManager.gameSocketClient.l_SendQueueMessages.TryAdd("MM:" + mMsg.ToJson());
                            //ConnectionManager.gameSocketClient.l_SendQueueMessages.Enqueue("MM:" + mMsg.ToJson());
                        }
                        return;
                    }

                    if (isQuestionAsked)
                    {
                        if (DateTime.Now - lastFrame > new TimeSpan(0, 0, 0, 1, 50))
                        {
                            isQuestionAsked = false;
                            lastFrame = DateTime.Now;
                        }
                        return;
                    }
                    isQuestionAsked = true;

                    //TODO: ¿Cuál es el criterio para enviar las preguntas?
                    //- Tiempo de las balas
                    //- Que no hayan balas, y si es así, cada medio segundo
                    if (dic_bulletsOnline.Count <= 0)
                    {
                        if (DateTime.Now - lastFrame > new TimeSpan(0, 0, 0, 0, 50))
                        {
                            ConnectionManager.gameSocketClient.l_SendBigMessages.TryAdd("PR:" + prtObj.ToJson());
                            //ConnectionManager.gameSocketClient.l_SendBigMessages.Enqueue("PR:" + prtObj.ToJson());
                            lastFrame = DateTime.Now;
                        }
                    }
                    else
                    {
                        prtObj.l_id_bullets_preguntando.AddRange(dic_bulletsOnline.Keys.ToList());
                        foreach (Bullet item in dic_bulletsOnline.Values)
                        {
                            if (DateTime.Now - lastFrame > new TimeSpan(0, 0, 0, 0, 50))
                            {
                                if (DateTime.Now - item.LastUpdate >= item.Velocity)
                                {
                                    ConnectionManager.gameSocketClient.l_SendBigMessages.TryAdd("PR:" + prtObj.ToJson());
                                    //ConnectionManager.gameSocketClient.l_SendBigMessages.Enqueue("PR:" + prtObj.ToJson());
                                    return;
                                }
                                lastFrame = DateTime.Now;
                            }
                        }
                    }
                }
                while (true);
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.Out.WriteLineAsync("Error Preguntar(): " + ex.Message);
                Console.ResetColor();
            }
        }

        private static bool isRunning = false;
        internal static async void PreguntarWhileHttp()
        {
            HttpClient client = new();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
            client.DefaultRequestHeaders.Add("User-Agent", "MMoClient");
            HttpResponseMessage response = null;
            do
            {
                try
                {
                    PreguntaObj prtObj = new PreguntaObj();
                    Console.Out.WriteLineAsync("\nPreguntando...\n");
                    MissingMessages mMsg = new MissingMessages();
                    if (MissingMessages.q_MissingMessages.Count > 0)
                    {
                        while (MissingMessages.q_MissingMessages.TryDequeue(out mMsg))
                        {
                            ConnectionManager.gameSocketClient.l_SendQueueMessages.TryAdd("MM:" + mMsg.ToJson());
                        }
                        continue;
                    }

                    if (isQuestionAsked)
                    {
                        if (DateTime.Now - lastFrame > new TimeSpan(0, 0, 0, 1, 50))
                        {
                            isQuestionAsked = false;
                            lastFrame = DateTime.Now;
                        }
                        continue;
                    }
                    isQuestionAsked = true;

                    //TODO: ¿Cuál es el criterio para enviar las preguntas?
                    //- Tiempo de las balas
                    //- Que no hayan balas, y si es así, cada medio segundo
                    if (dic_bulletsOnline.Count <= 0)
                    {
                        if (DateTime.Now - lastFrame > new TimeSpan(0, 0, 0, 0, 25))
                        {
                            string a = "PR:" + prtObj.ToJson();
                            FormUrlEncodedContent stringContent = new(new[]
                            {
                                new KeyValuePair<string, string>("token","a"),
                                new KeyValuePair<string, string>("strItem",a),
                            });

                            response = await client.PostAsync("https://localhost:7109/api/ContextDeliverer", stringContent);
                            lastFrame = DateTime.Now;
                        }
                    }
                    else
                    {
                        prtObj.l_id_bullets_preguntando.AddRange(dic_bulletsOnline.Keys.ToList());
                        foreach (Bullet item in dic_bulletsOnline.Values)
                        {
                            if (DateTime.Now - lastFrame > new TimeSpan(0, 0, 0, 0, 25))
                            {
                                if (DateTime.Now - item.LastUpdate >= item.Velocity)
                                {
                                    FormUrlEncodedContent stringContent = new(new[]
                                    {
                                        new KeyValuePair<string, string>("token","a"),
                                        new KeyValuePair<string, string>("strItem","PR:" + prtObj.ToJson()),
                                    });

                                    response = await client.PostAsync("https://localhost:7109/api/ContextDeliverer", stringContent);
                                }
                                lastFrame = DateTime.Now;
                            }
                        }
                    }

                    if (response != null)
                    {
                        //TODO: Esta variable debería ser de la clase padre (o algo así), cosa tal de que la información
                        //pase para su procesamiento a su siguiente fase
                        string resp = await response.Content.ReadAsStringAsync(); //" + resp + "
                        if (resp.Equals("1"))
                        {
                            return;
                        }
                        //resp = "MS:{\"Length\":6000,\"IdRef\":0,\"IdMsg\":0,\"text\":\"Q086eyJsX2J1bGxldHNfdG9fY3JlYXRlIiA6IFt7IklkIjoieENrVmR4dE1IcyIsIkxOIjoiUGxheWVyIiwiVHlwZSI6Ik5CIiwiT3JQb3MiOiI8MHwwfC0wLjU\\u002BIiwiV1BvcyI6IjwwfDB8LTAuNT4iLCJNZGYiOiI8MHwwfC0wLjU\\u002BIn0seyJJZCI6IlUySXM5cFVKNHQiLCJMTiI6IlBsYXllciIsIlR5cGUiOiJOQiIsIk9yUG9zIjoiPDB8MHwtMC41PiIsIldQb3MiOiI8MHwwfC0wLjU\\u002BIiwiTWRmIjoiPDB8MHwtMC41PiJ9LHsiSWQiOiI0Y3BMVHJZVDhuIiwiTE4iOiJQbGF5ZXIiLCJUeXBlIjoiTkIiLCJPclBvcyI6IjwwfDB8LTAuNT4iLCJXUG9zIjoiPDB8MHwtMC41PiIsIk1kZiI6IjwwfDB8LTAuNT4ifSx7IklkIjoiVDZOV2FjbXl3MCIsIkxOIjoiUGxheWVyIiwiVHlwZSI6Ik5CIiwiT3JQb3MiOiI8MHwwfC0wLjU\\u002BIiwiV1BvcyI6IjwwfDB8LTAuNT4iLCJNZGYiOiI8MHwwfC0wLjU\\u002BIn0seyJJZCI6IlpBaG5mdWZCQ3AiLCJMTiI6IlBsYXllciIsIlR5cGUiOiJOQiIsIk9yUG9zIjoiPDB8MHwtMC41PiIsIldQb3MiOiI8MHwwfC0wLjU\\u002BIiwiTWRmIjoiPDB8MHwtMC41PiJ9LHsiSWQiOiJkRk1GY0hxZkE5IiwiTE4iOiJQbGF5ZXIiLCJUeXBlIjoiTkIiLCJPclBvcyI6IjwwfDB8LTAuNT4iLCJXUG9zIjoiPDB8MHwtMC41PiIsIk1kZiI6IjwwfDB8LTAuNT4ifSx7IklkIjoiV1ZkOUVMekVLUSIsIkxOIjoiUGxheWVyIiwiVHlwZSI6Ik5CIiwiT3JQb3MiOiI8MHwwfC0wLjU\\u002BIiwiV1BvcyI6IjwwfDB8LTAuNT4iLCJNZGYiOiI8MHwwfC0wLjU\\u002BIn0seyJJZCI6ImRXVDV1OFdKbEIiLCJMTiI6IlBsYXllciIsIlR5cGUiOiJOQiIsIk9yUG9zIjoiPDB8MHwtMC41PiIsIldQb3MiOiI8MHwwfC0wLjU\\u002BIiwiTWRmIjoiPDB8MHwtMC41PiJ9LHsiSWQiOiJRdDM4dlpXbmJUIiwiTE4iOiJQbGF5ZXIiLCJUeXBlIjoiTkIiLCJPclBvcyI6IjwwfDB8LTAuNT4iLCJXUG9zIjoiPDB8MHwtMC41PiIsIk1kZiI6IjwwfDB8LTAuNT4ifSx7IklkIjoiN2dVcWtmNDNTYiIsIkxOIjoiUGxheWVyIiwiVHlwZSI6Ik5CIiwiT3JQb3MiOiI8MHwwfC0wLjU\\u002BIiwiV1BvcyI6IjwwfDB8LTAuNT4iLCJNZGYiOiI8MHwwfC0wLjU\\u002BIn0seyJJZCI6IlFwTjhaMHZUTmMiLCJMTiI6IlBsYXllciIsIlR5cGUiOiJOQiIsIk9yUG9zIjoiPDB8MHwtMC41PiIsIldQb3MiOiI8MHwwfC0wLjU\\u002BIiwiTWRmIjoiPDB8MHwtMC41PiJ9LHsiSWQiOiJNOXJ0cGFyZjNpIiwiTE4iOiJQbGF5ZXIiLCJUeXBlIjoiTkIiLCJPclBvcyI6IjwwfDB8LTAuNT4iLCJXUG9zIjoiPDB8MHwtMC41PiIsIk1kZiI6IjwwfDB8LTAuNT4ifSx7IklkIjoiQ2Fpa29qejJhbiIsIkxOIjoiUGxheWVyIiwiVHlwZSI6Ik5CIiwiT3JQb3MiOiI8MHwwfC0wLjU\\u002BIiwiV1BvcyI6IjwwfDB8LTAuNT4iLCJNZGYiOiI8MHwwfC0wLjU\\u002BIn0seyJJZCI6ImNNT0R0ZmFWWGgiLCJMTiI6IlBsYXllciIsIlR5cGUiOiJOQiIsIk9yUG9zIjoiPDB8MHwtMC41PiIsIldQb3MiOiI8MHwwfC0wLjU\\u002BIiwiTWRmIjoiPDB8MHwtMC41PiJ9LHsiSWQiOiJrUzJObkJ2T2pVIiwiTE4iOiJQbGF5ZXIiLCJUeXBlIjoiTkIiLCJPclBvcyI6IjwwfDB8LTAuNT4iLCJXUG9zIjoiPDB8MHwtMC41PiIsIk1kZiI6IjwwfDB8LTAuNT4ifSx7IklkIjoid2RSYTVUMUtweiIsIkxOIjoiUGxheWVyIiwiVHlwZSI6Ik5CIiwiT3JQb3MiOiI8MHwwfC0wLjU\\u002BIiwiV1BvcyI6IjwwfDB8LTAuNT4iLCJNZGYiOiI8MHwwfC0wLjU\\u002BIn0seyJJZCI6IjNpbTlyY1NTeWgiLCJMTiI6IlBsYXllciIsIlR5cGUiOiJOQiIsIk9yUG9zIjoiPDB8MHwtMC41PiIsIldQb3MiOiI8MHwwfC0wLjU\\u002BIiwiTWRmIjoiPDB8MHwtMC41PiJ9LHsiSWQiOiJtQkR0VW1FbkNvIiwiTE4iOiJQbGF5ZXIiLCJUeXBlIjoiTkIiLCJPclBvcyI6IjwwfDB8LTAuNT4iLCJXUG9zIjoiPDB8MHwtMC41PiIsIk1kZiI6IjwwfDB8LTAuNT4ifSx7IklkIjoiOVVOQ09pSEVXViIsIkxOIjoiUGxheWVyIiwiVHlwZSI6Ik5CIiwiT3JQb3MiOiI8MHwwfC0wLjU\\u002BIiwiV1BvcyI6IjwwfDB8LTAuNT4iLCJNZGYiOiI8MHwwfC0wLjU\\u002BIn0seyJJZCI6ImhobWFaMTRCZ00iLCJMTiI6IlBsYXllciIsIlR5cGUiOiJOQiIsIk9yUG9zIjoiPDB8MHwtMC41PiIsIldQb3MiOiI8MHwwfC0wLjU\\u002BIiwiTWRmIjoiPDB8MHwtMC41PiJ9LHsiSWQiOiJ6NkliRGxwY3ZTIiwiTE4iOiJQbGF5ZXIiLCJUeXBlIjoiTkIiLCJPclBvcyI6IjwwfDB8LTAuNT4iLCJXUG9zIjoiPDB8MHwtMC41PiIsIk1kZiI6IjwwfDB8LTAuNT4ifSx7IklkIjoiVklmVVM4VmRiTCIsIkxOIjoiUGxheWVyIiwiVHlwZSI6Ik5CIiwiT3JQb3MiOiI8MHwwfC0wLjU\\u002BIiwiV1BvcyI6IjwwfDB8LTAuNT4iLCJNZGYiOiI8MHwwfC0wLjU\\u002BIn0seyJJZCI6InZsV2VYQzVhTFEiLCJMTiI6IlBsYXllciIsIlR5cGUiOiJOQiIsIk9yUG9zIjoiPDB8MHwtMC41PiIsIldQb3MiOiI8MHwwfC0wLjU\\u002BIiwiTWRmIjoiPDB8MHwtMC41PiJ9LHsiSWQiOiJkUk5pNWljNlpDIiwiTE4iOiJQbGF5ZXIiLCJUeXBlIjoiTkIiLCJPclBvcyI6IjwwfDB8LTAuNT4iLCJXUG9zIjoiPDB8MHwtMC41PiIsIk1kZiI6IjwwfDB8LTAuNT4ifSx7IklkIjoiUTJQM2dXZ2FrcSIsIkxOIjoiUGxheWVyIiwiVHlwZSI6Ik5CIiwiT3JQb3MiOiI8MHwwfC0wLjU\\u002BIiwiV1BvcyI6IjwwfDB8LTAuNT4iLCJNZGYiOiI8MHwwfC0wLjU\\u002BIn0seyJJZCI6ImhacW5tU1FDRzgiLCJMTiI6IlBsYXllciIsIlR5cGUiOiJOQiIsIk9yUG9zIjoiPDB8MHwtMC41PiIsIldQb3MiOiI8MHwwfC0wLjU\\u002BIiwiTWRmIjoiPDB8MHwtMC41PiJ9LHsiSWQiOiJpc2R4emJSVlJIIiwiTE4iOiJQbGF5ZXIiLCJUeXBlIjoiTkIiLCJPclBvcyI6IjwwfDB8LTAuNT4iLCJXUG9zIjoiPDB8MHwtMC41PiIsIk1kZiI6IjwwfDB8LTAuNT4ifSx7IklkIjoieFR0WlA3dVU2UCIsIkxOIjoiUGxheWVyIiwiVHlwZSI6Ik5CIiwiT3JQb3MiOiI8MHwwfC0wLjU\\u002BIiwiV1BvcyI6IjwwfDB8LTAuNT4iLCJNZGYiOiI8MHwwfC0wLjU\\u002BIn0seyJJZCI6IkNRcEdGMVp3dFUiLCJMTiI6IlBsYXllciIsIlR5cGUiOiJOQiIsIk9yUG9zIjoiPDB8MHwtMC41PiIsIldQb3MiOiI8MHwwfC0wLjU\\u002BIiwiTWRmIjoiPDB8MHwtMC41PiJ9LHsiSWQiOiJpamFUeHo2Z0tCIiwiTE4iOiJQbGF5ZXIiLCJUeXBlIjoiTkIiLCJPclBvcyI6IjwwfDB8LTAuNT4iLCJXUG9zIjoiPDB8MHwtMC41PiIsIk1kZiI6IjwwfDB8LTAuNT4ifSx7IklkIjoiMmRtcnlNZEsyVSIsIkxOIjoiUGxheWVyIiwiVHlwZSI6Ik5CIiwiT3JQb3MiOiI8MHwwfC0wLjU\\u002BIiwiV1BvcyI6IjwwfDB8LTAuNT4iLCJNZGYiOiI8MHwwfC0wLjU\\u002BIn0seyJJZCI6IkdTa2RwUVRKcnoiLCJMTiI6IlBsYXllciIsIlR5cGUiOiJOQiIsIk9yUG9zIjoiPDB8MHwtMC41PiIsIldQb3MiOiI8MHwwfC0wLjU\\u002BIiwiTWRmIjoiPDB8MHwtMC41PiJ9LHsiSWQiOiJObWFVcThXNzhNIiwiTE4iOiJQbGF5ZXIiLCJUeXBlIjoiTkIiLCJPclBvcyI6IjwwfDB8LTAuNT4iLCJXUG9zIjoiPDB8MHwtMC41PiIsIk1kZiI6IjwwfDB8LTAuNT4ifSx7IklkIjoibkxzUXNaYU1NYiIsIkxOIjoiUGxheWVyIiwiVHlwZSI6Ik5CIiwiT3JQb3MiOiI8MHwwfC0wLjU\\u002BIiwiV1BvcyI6IjwwfDB8LTAuNT4iLCJNZGYiOiI8MHwwfC0wLjU\\u002BIn0seyJJZCI6IlBieldFUHRoeTgiLCJMTiI6IlBsYXllciIsIlR5cGUiOiJOQiIsIk9yUG9zIjoiPDB8MHwtMC41PiIsIldQb3MiOiI8MHwwfC0wLjU\\u002BIiwiTWRmIjoiPDB8MHwtMC41PiJ9LHsiSWQiOiJZNnMyYTF3UERBIiwiTE4iOiJQbGF5ZXIiLCJUeXBlIjoiTkIiLCJPclBvcyI6IjwwfDB8LTAuNT4iLCJXUG9zIjoiPDB8MHwtMC41PiIsIk1kZiI6IjwwfDB8LTAuNT4ifSx7IklkIjoiMXZveldNbWtvRSIsIkxOIjoiUGxheWVyIiwiVHlwZSI6Ik5CIiwiT3JQb3MiOiI8MHwwfC0wLjU\\u002BIiwiV1BvcyI6IjwwfDB8LTAuNT4iLCJNZGYiOiI8MHwwfC0wLjU\\u002BIn0seyJJZCI6IlFqTUJ1aDNWZ0ciLCJMTiI6IlBsYXllciIsIlR5cGUiOiJOQiIsIk9yUG9zIjoiPDB8MHwtMC41PiIsIldQb3MiOiI8MHwwfC0wLjU\\u002BIiwiTWRmIjoiPDB8MHwtMC41PiJ9LHsiSWQiOiJOUFE3bDh2MGlTIiwiTE4iOiJQbGF5ZXIiLCJUeXBlIjoiTkIiLCJPclBvcyI6IjwwfDB8LTAuNT4iLCJXUG9zIjoiPDB8MHwtMC41PiIsIk1kZiI6IjwwfDB8LTAuNT4ifSx7IklkIjoiRFdmVEtsVFJIMSIsIkxOIjoiUGxheWVyIiwiVHlwZSI6Ik5CIiwiT3JQb3MiOiI8MHwwfC0wLjU\\u002BIiwiV1BvcyI6IjwwfDB8LTAuNT4iLCJNZGYiOiI8MHwwfC0wLjU\\u002BIn0seyJJZCI6IlZlSTExZjg4Mm4iLCJMTiI6IlBsYXllciIsIlR5cGUiOiJOQiIsIk9yUG9zIjoiPDB8MHwtMC41PiIsIldQb3MiOiI8MHwwfC0wLjU\\u002BIiwiTWRmIjoiPDB8MHwtMC41PiJ9XSwibF9idWxsZXRzX3RvX3VwZGF0ZSIgOiBbXSwibF9idWxsZXRzX3RvX2NoYW5nZV9zdGF0ZSIgOiBbeyJJZCI6IjIiLCJTdGF0ZSI6MX0seyJJZCI6IjMiLCJTdGF0ZSI6MX0seyJJZCI6IjQiLCJTdGF0ZSI6MX1dfQ==\"}";
                        Message nwMsg = Message.CreateFromJson(resp);

                        if (!isRunning)
                        {
                            PlayerController.ProcesarConversarObj(nwMsg.TextOriginal, nwMsg, out nwMsg);
                            //ConnectionManager.Queue_Instrucciones.Enqueue(resp);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.BackgroundColor = ConsoleColor.DarkGreen;
                            Console.Out.WriteLineAsync(DateTime.Now.ToString() + " Preguntando response: " + resp);
                            //Console.WriteLine(DateTime.Now.ToString() + " Preguntando response: " + resp);
                            Console.ResetColor();
                            isRunning = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.Out.WriteLineAsync("Error Preguntar(): " + ex.Message);
                    Console.ResetColor();
                }
            }
            while (true);
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.Out.WriteLineAsync("\n\nCERRADO EL BUCLE POR ALGUN MOTIVO (PREGUNTAR())\n\n");
            Console.ResetColor();
        }

        #region UpCrElBullets
        /*public void ShotAndProyectileProcessing()
        {
            try
            {
                int result = 0;
                if (UpdateBullets())
                {
                    result++;
                }

                if (CreateBullets())
                {
                    result++;
                }

                if (EliminateBullets())
                {
                    result++;
                }

                //If the message was processed on a perfect way
                if (result == 3)
                {
                    foreach (KeyValuePair<int, Message> item in Message.dic_ActiveMessages)
                    {
                        ConnectionManager.gameSocketClient.l_SendQueueMessages.Enqueue(item.Value.ToJson());
                    }
                }

            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("Error UpdateBullets(): " + ex.Message);
                Console.ResetColor();
            }
        }*/

        private bool UpdateBullets(List<ShotPosUpdate> l_shotPosUpdates)
        {
            try
            {
                bool result = false;
                List<ShotPosUpdate> l_shtUpd = l_shotPosUpdates.ToList();
                List<Trios<string, Bullet, ShotPosUpdate>> l_tempBulletOnline = new();
                ConcurrentDictionary<string, Bullet> dictionaryFrom = new ConcurrentDictionary<string, Bullet>();
                ConcurrentDictionary<string, Bullet> dictionaryTo = new ConcurrentDictionary<string, Bullet>(dic_bulletsOnline);
                foreach (ShotPosUpdate shtUpd in l_shtUpd)
                {
                    foreach (KeyValuePair<string, Bullet> bllt in dictionaryTo)
                    {
                        if (shtUpd.Id == bllt.Key)
                        {
                            l_tempBulletOnline.Add(new Trios<string, Bullet, ShotPosUpdate>(shtUpd.Id, bllt.Value, shtUpd));
                            //bllt.Value.Position = UtilityAssistant.ConvertVector3NumericToStride(shtUpd.Pos);
                        }
                    }
                }
                l_shotPosUpdates.Clear();
                foreach (Trios<string, Bullet, ShotPosUpdate> item in l_tempBulletOnline)
                {
                    item.Item2.Position = UtilityAssistant.ConvertVector3NumericToStride(item.Item3.Pos);
                }

                dictionaryFrom = new ConcurrentDictionary<string, Bullet>(l_tempBulletOnline.ToDictionary(c => c.Item1, c => c.Item2));
                foreach (KeyValuePair<string, Bullet> item in dictionaryFrom.ToList())
                {
                    dictionaryTo.AddOrUpdate(item.Key,
                        addValueFactory: (ky) =>
                    {
                        ky = item.Key; //dic_bulletsOnline.Count();
                        return item.Value;
                    },
                        updateValueFactory: (ky, oldVle) =>
                    {
                        oldVle = item.Value;
                        return item.Value;
                    });
                }
                dic_bulletsOnline.Clear();
                dic_bulletsOnline = new ConcurrentDictionary<string, Bullet>(dictionaryTo);
                /*ShotPosUpdate shtUpd = new ShotPosUpdate();
                while (q_PendingShotPosUpdateRun.TryDequeue(out shtUpd))
                {
                    foreach (KeyValuePair<int, Bullet> bllt in dic_bulletsOnline)
                    {
                        if (shtUpd.Id == bllt.Key)
                        {
                            bllt.Value.Position = UtilityAssistant.ConvertVector3NumericToStride(shtUpd.Pos);
                        }
                    }
                }*/
                result = true;
                return result;
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("Error UpdateBullets(): " + ex.Message);
                Console.ResetColor();
                return false;
            }
        }

        private bool CreateBullets(List<Shot> l_shots)
        {
            try
            {
                bool result = false;
                //Shot sht = new Shot();
                //while (q_PendingShotsCreatedRun.TryDequeue(out sht))
                //{
                foreach (Shot sht in l_shots)
                {
                    Bullet bullet = new Bullet(sht.Id, sht.LN, UtilityAssistant.ConvertVector3NumericToStride(sht.WPos), UtilityAssistant.ConvertVector3NumericToStride(sht.Mdf));
                    List<Entity> l_ent = Controller.controller.GetPrefab("Bullet");
                    bullet.ProyectileBody = l_ent[0];
                    bullet.ProyectileBody.Transform.Position = bullet.InitialPosition;
                    UtilityAssistant.RotateTo(bullet.ProyectileBody, (bullet.ProyectileBody.Transform.Position + bullet.MovementModifier));
                    dic_bulletsOnline.TryAdd(sht.Id, bullet);
                    //dic_bulletsOnline[intbllt].ProyectileBody.Transform.Position = dic_bulletsOnline[intbllt].InitialPosition;
                    //UtilityAssistant.RotateTo(dic_bulletsOnline[intbllt].ProyectileBody, (dic_bulletsOnline[intbllt].ProyectileBody.Transform.Position + dic_bulletsOnline[intbllt].MovementModifier));
                    Entity.Scene.Entities.Add(bullet.ProyectileBody);
                }
                result = true;
                return result;
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("Error CreateBullets(): " + ex.Message);
                Console.ResetColor();
                return false;
            }
        }

        private bool EliminateBullets(List<ShotState> l_shotStates)
        {
            try
            {
                bool result = false;
                float evaluatorX = 0;
                float evaluatorY = 0;
                float evaluatorZ = 0;
                float distance = 25;
                //ShotState sst = null;
                //while (q_PendingShotStateToRun.TryDequeue(out sst))
                //{
                foreach (ShotState sst in l_shotStates)
                {
                    foreach (KeyValuePair<string, Bullet> bllt in dic_bulletsOnline)
                    {
                        if (sst.Id == bllt.Key)
                        {
                            if (sst.State == StateOfTheShot.Destroyed)
                            {
                                if (bllt.Value != null)
                                {
                                    Entity.Scene.Entities.Remove(bllt.Value.ProyectileBody);
                                    dic_bulletsOnline.TryRemove(bllt);
                                    //dic_bulletsOnline.Select(c => c.Value).ToList().RemoveAll(v => v.id == sst.Id);
                                }
                            }
                        }

                        evaluatorX = UtilityAssistant.DistanceComparitorByAxis(bllt.Value.InitialPosition.X, bllt.Value.ProyectileBody.Transform.Position.X);
                        evaluatorY = UtilityAssistant.DistanceComparitorByAxis(bllt.Value.InitialPosition.Y, bllt.Value.ProyectileBody.Transform.Position.Y);
                        evaluatorZ = UtilityAssistant.DistanceComparitorByAxis(bllt.Value.InitialPosition.Z, bllt.Value.ProyectileBody.Transform.Position.Z);

                        if (evaluatorX >= distance || evaluatorY >= distance || evaluatorZ >= distance)
                        {
                            Entity.Scene.Entities.Remove(bllt.Value.ProyectileBody);
                        }
                    }

                }
                result = true;
                return result;

                /*List<Entity> l_entitys = Entity.Scene.Entities.Where(c => c.Name == "Bullet").ToList();
                if (l_entitys.Count > dic_bulletsOnline.Count)
                {
                    List<Entity> l_t_entitiesToDelete = l_entitys.Where(c => dic_bulletsOnline.All(c2 => c2.ProyectileBody.Id != c.Id)).ToList();
                    foreach (Entity item in l_t_entitiesToDelete)
                    {
                        foreach (Entity itm in l_entitys)
                        {
                            if (item.Id == itm.Id)
                            {
                                Entity.Scene.Entities.Remove(itm);
                            }
                        }
                    }
                }*/
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("Error EliminateBullets(): " + ex.Message);
                Console.ResetColor();
                return false;
            }
        }

        /*internal static void SetInstrucciones(string returned)
        {
            instrucciones = returned;
        }*/
        #endregion
        #endregion

        #region Animaciones
        public void Animacion()
        {
            try
            {
                foreach (Trios<int, Puppet, TimeSpan> item in l_AnimacionesEntitys)
                {
                    if (item.Item2.AnimSprite != null)
                    {
                        if (Player.PLAYER.Entity != null)
                        {
                            if (item.Item2.Entity.Name == Player.PLAYER.Entity.Name)
                            {
                                continue;
                            }
                        }

                        if (DateTime.Now - item.Item2.AnimSprite.LastTime > item.Item3)
                        {
                            item.Item2.AnimSprite.LastTime = DateTime.Now;
                            if (item.Item2.Entity.Get<SpriteComponent>() != null)
                            {
                                item.Item2.AnimSprite.Animar(item.Item2.Entity, item.Item1);
                                item.Item2.AnimSprite.OrientarAPlayer(item.Item2, Controller.controller.GetActiveCamera().Entity);
                                item.Item2.AnimSprite.CambiarSpritePorPerspectivaPlayer(item.Item2, Controller.controller.GetActiveCamera().Entity);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error Animacion(): " + ex.Message);
                return;
            }
        }
        #endregion

        #region Characters & Creatures functions
        //Solve the existance of the player in the game, it create it when it is required
        public void PlayerCharacterStart(string strTypePrefab = "Player")
        {
            try
            {
                //if player already exist
                if (Player.PLAYER != null)
                {
                    return;
                }

                List<Entity> instance = Controller.controller.GetPrefab(strTypePrefab); //playerPrefab.Instantiate();
                instance.First().Transform.Position = Vector3.Zero;
                Entity.Scene.Entities.AddRange(instance);
                //player = instance[0];

                //Fija acá animaciones en un nuevo objeto de AnimacionSprite para pasar en el método "RegistrarEntidad..:" de abajo
                Animacion[] arrAnimacion = new Animacion[1]
                {
                    new Animacion (0,1,"Idle") //Idle
                };
                AnimacionSprite animacionSprite = new AnimacionSprite(arrAnimacion);
                Player.PLAYER = new Player(instance[0], animacionSprite);
                AnimacionSprite.RegistrarEntidadEnAnimacionSprite(Player.PLAYER, new TimeSpan(0, 0, 0, 0, 250));
                //InitTimer(player).Start();

                foreach (Entity item in Player.PLAYER.Entity.GetChildren())
                {
                    if (item.Name == "Camera")
                    {
                        Player.CAM = item;
                        //Registering the player Camera
                        Services.GetService<Controller>().RegisterCamera(item.Get<CameraComponent>());
                        Services.GetService<Controller>().ActivateCamera(item.Get<CameraComponent>().Name);
                        continue;
                    }
                    if (item.Name == "weapon")
                    {
                        Player.PLAYER.Weapon = item;
                        //ConnectionManager.AddInstruction("WP:" + weapon.Transform.position.ToString());
                        Player.WP = Player.PLAYER.Weapon.Transform.Position.ToString();
                        continue;
                    }
                    if (item.Name == "L-Shoulder")
                    {
                        Player.PLAYER.LeftShoulder = item;
                        //ConnectionManager.AddInstruction("LS:" + LeftShoulder.Transform.position.ToString());
                        Player.LS = Player.PLAYER.LeftShoulder.Transform.Position.ToString();
                        continue;
                    }
                    if (item.Name == "R-Shoulder")
                    {
                        Player.PLAYER.RightShoulder = item;
                        //ConnectionManager.AddInstruction("RS:" + RightShoulder.Transform.position.ToString());
                        Player.RS = Player.PLAYER.RightShoulder.Transform.Position.ToString();
                        continue;
                    }

                    if (item.Name == "Gun")
                    {
                        Player.PLAYER.Gun = item;
                        //ConnectionManager.AddInstruction("WP:" + weapon.Transform.position.ToString());
                        Player.GNPS = Player.PLAYER.Gun.Transform.Position.ToString();
                        Player.GNRT = Player.PLAYER.Gun.Transform.Rotation.ToString();
                        continue;
                    }
                }

                //ConnectionManager.AddInstruction("PS:" + player.Transform.position.ToString());
                //ConnectionManager.AddInstruction("RT:" + player.Transform.Rotation.ToString());
                Player.PS = Player.PLAYER.Entity.Transform.Position.ToString();
                //player.Transform.Rotation.X = -90;
                Player.RT = Player.PLAYER.Entity.Transform.Rotation.ToString();
                //ConnectionManager.AddInstruction("PST:" + JsonConvert.SerializeObject(pldt));
                //ConnectionManager.PrepareData();

            }
            catch (Exception ex)
            {
                Log.Error("Error PlayerCharacterStart(): " + ex.Message);
                return;
            }
        }

        //Create an enemy (NPC) on the field, it register the enemy in the corresponding list too, but also return it.
        public Puppet EnemyNPCStart(String TypeOfPuppetName, Vector3 Position = default, Quaternion Rotation = default)
        {
            try
            {
                //var a = Controller.controller.GetModel("Cone");
                //var a = Controller.controller.GetTexture("PSK_MBT_G1_Camo_darkg");
                //var b = a;

                //Determine if it is a model or a Sprite and add it to the model
                Model model = Content.Load<Model>("Models/" + TypeOfPuppetName);
                //Prefab prefab = Content.Load<Prefab>("Prefabs/" + TypeOfPuppetName);
                List<Entity> prefab = Controller.controller.GetPrefab("TypeOfPuppetName"); //Content.Load<Prefab>("Prefabs/" + TypeOfPuppetName);
                //Prefab RealEnt = Content.Load<Prefab>("Prefabs/RealEnt");

                Type typ = Puppet.TypesOfMonsters().Where(c => c.Name == TypeOfPuppetName).FirstOrDefault();
                if (typ == null)
                {
                    typ = Puppet.TypesOfMonsters().Where(c => c.FullName == TypeOfPuppetName).FirstOrDefault();
                }

                object obtOfType = Activator.CreateInstance(typ); //Requires parameterless constructor.
                                                                  //TODO: System to determine the type of enemy to make the object, prepare stats and then add it to the list
                int position = l_entitysCharacters.Count();
                l_entitysCharacters.Add(((Puppet)obtOfType));

                Entity sprt = new Entity(TypeOfPuppetName);

                Vector3 pos = Vector3.Zero;
                Quaternion rot = Quaternion.Identity;
                if (Position != default)
                {
                    pos = Position;
                }
                if (Rotation != default)
                {
                    rot = Rotation;
                }

                if (prefab.Count > 0)
                {
                    //List<Entity> ents = prefab.Instantiate();
                    prefab.First().Transform.Position = pos;
                    //Entity.Scene.Entities.AddRange(instance);
                    ((Puppet)obtOfType).Entity = prefab[0];
                }
                else if (model == null)
                {
                    SpriteSheet spritesheet = Controller.controller.GetSpriteSheet(TypeOfPuppetName); //Content.Load<SpriteSheet>("Sprites/" + TypeOfPuppetName);
                    sprt.GetOrCreate<SpriteComponent>().SpriteProvider = SpriteFromSheet.Create(spritesheet, spritesheet[0].Name);
                    ((Puppet)obtOfType).Entity = sprt;
                }
                else
                {
                    sprt.GetOrCreate<ModelComponent>().Model = model;
                    ((Puppet)obtOfType).Entity = sprt;
                }

                if (typ == default(Type))
                {
                    Console.WriteLine(" Entity EnemyNPCStart Error: Type not found, maybie a typo in the name of the type?");
                    return null;
                }

                ((Puppet)obtOfType).Entity.Name = position.ToString() + " " + TypeOfPuppetName;
                ((Puppet)obtOfType).Entity.Transform.Position = pos;
                ((Puppet)obtOfType).Entity.Transform.Rotation = rot;

                List<Entity> instance = Controller.controller.GetPrefab("RealEnt"); //RealEnt.Instantiate();
                instance.First().Transform.Position = Position;
                Entity.Scene.Entities.AddRange(instance);
                ((Puppet)obtOfType).RealEnt = instance[0];

                ((Puppet)obtOfType).RealEnt.Transform.RotationEulerXYZ = new Vector3(0f, -90f, 0f);

                /*Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("l_ent count: " + l_ent.Count());
                foreach (Entity ent in l_ent)
                {
                    Console.WriteLine("Component: "+ent.Name);
                }
                Console.ResetColor();*/

                Entity.Scene.Entities.Add(((Puppet)obtOfType).Entity);

                AnimacionSprite.RegistrarEntidadEnAnimacionSprite(l_entitysCharacters[position], new TimeSpan(0, 0, 0, 0, 250));

                return ((Puppet)obtOfType);
            }
            catch (Exception ex)
            {
                Log.Error("Error EnemyNPCStart(): " + ex.Message);
                return null;
            }
        }

        //Create an enemy (NPC) on the field, it register the enemy in the corresponding list too, but also return it.
        public Entity EnemyNPCStart(Prefab Enemy, Puppet type = default, Vector3 Position = default, AnimacionSprite animacionSprite = default, int HP = 32)
        {
            try
            {
                //Need to send instruction of shoot here in case of internet Here, instead of the next code:
                List<Entity> instance = Enemy.Instantiate();
                instance.First().Transform.Position = Position;
                Entity.Scene.Entities.AddRange(instance);
                int position = l_entitysCharacters.Count;
                instance[0].Name = position.ToString() + " " + Enemy.Entities[0].Name;

                Type typ;
                object obtOfType = null;
                if (type == default(Puppet) || type == null)
                {
                    //Here is filtered already, NO va a ser nada distinto a un descendiente del tipo 'Puppet'
                    typ = Puppet.TypesOfMonsters().Where(c => c.Name == Enemy.Entities[0].Name).FirstOrDefault();
                    obtOfType = Activator.CreateInstance(typ, instance[0]); //Requires parameterless constructor.
                                                                            //TODO: System to determine the type of enemy to make the object, prepare stats and then add it to the list
                    l_entitysCharacters.Add((Puppet)obtOfType);
                }
                else
                {
                    typ = type.GetType();
                }

                if (obtOfType == null)
                {
                    //This implys already than type != null && Puppet (Probably)
                    type.Entity = instance[0];
                    l_entitysCharacters.Add(type);
                }

                //TODO: Fijar acá animaciones en un nuevo objeto de AnimacionSprite para pasar en el método "RegistrarEntidad..:" de abajo

                //AnimacionSprite an = animacionSprite != default(AnimacionSprite) ? animacionSprite : new AnimacionSprite();
                //LoadSkeletonPrefab(l_entitysCharacters[position]);

                //RegistrarEntidadEnAnimacionSprite(instance[0]);
                //InitTimer(instance[0]).Start();

                // We retrieve the parent entity by using the GetParent() command.
                foreach (Entity item in l_entitysCharacters[position].Entity.GetChildren())
                {
                    if (item.Name == "Body")
                    {
                        l_entitysCharacters[position].Body = item;
                        continue;
                    }
                    if (item.Name == "Weapon")
                    {
                        l_entitysCharacters[position].Weapon = item;
                        continue;
                    }
                    if (item.Name == "L-Shoulder")
                    {
                        l_entitysCharacters[position].LShoulder = item;
                        continue;
                    }
                    if (item.Name == "R-Shoulder")
                    {
                        l_entitysCharacters[position].RShoulder = item;
                        continue;
                    }
                    /*if (itemParam.Name == "Sprite")
                    {
                        l_entitysCharacters[position].Sprite = itemParam;
                        continue;
                    }*/
                }

                AnimacionSprite.RegistrarEntidadEnAnimacionSprite(l_entitysCharacters[position], new TimeSpan(0, 0, 0, 0, 250));

                return instance[0];
            }
            catch (Exception ex)
            {
                Log.Error("Error EnemyNPCStart(): " + ex.Message);
                return new Entity();
            }
        }
        #endregion

        #region Player Movement
        //Solve Movement of player only (Online) 
        public string MovementOnline()
        {
            try
            {
                DebugText.Print("Posicion (TranslationVector) thingy1: " + Controller.controller.thingy1.Transform.WorldMatrix.TranslationVector, new Int2(200, 130));
                DebugText.Print("Posicion (TranslationVector) thingy2: " + Controller.controller.thingy2.Transform.WorldMatrix.TranslationVector, new Int2(200, 150));
                DebugText.Print("Difference: " + (Controller.controller.thingy1.Transform.WorldMatrix.TranslationVector - Controller.controller.thingy2.Transform.WorldMatrix.TranslationVector), new Int2(200, 170));
                // We display the entity's name and its local and world position on screen
                DebugText.Print("Posicion (TranslationVector) Player: " + Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector, new Int2(200, 430));
                DebugText.Print("Posicion (Local) Player: " + Player.PLAYER.Entity.Transform.Position, new Int2(200, 450));
                DebugText.Print("Posicion Arma: " + Player.PLAYER.Weapon.Transform.WorldMatrix.TranslationVector, new Int2(200, 470));
                DebugText.Print("Posicion Camara: " + Player.PLAYER.Camera.Transform.WorldMatrix.TranslationVector, new Int2(200, 490));

                DebugText.Print("Posicion Hombro Izq: " + Player.PLAYER.LeftShoulder.Transform.WorldMatrix.TranslationVector, new Int2(200, 530));
                DebugText.Print("Posicion Hombro Der: " + Player.PLAYER.RightShoulder.Transform.WorldMatrix.TranslationVector, new Int2(200, 550));

                if (Input.HasKeyboard)
                {
                    // Key down is used for when a key is being held down.
                    string byteData = string.Empty;
                    if (Input.IsKeyDown(Keys.Up) || Input.IsKeyDown(Keys.W))
                    {
                        byteData += "10";
                    }
                    else if (Input.IsKeyDown(Keys.Down) || Input.IsKeyDown(Keys.S))
                    {
                        byteData += "01";
                    }
                    else
                    {
                        byteData += "00";
                    }

                    if (Input.IsKeyDown(Keys.Left) || Input.IsKeyDown(Keys.A))
                    {
                        byteData += "10";
                    }
                    else if (Input.IsKeyDown(Keys.Right) || Input.IsKeyDown(Keys.D))
                    {
                        byteData += "01";
                    }
                    else
                    {
                        byteData += "00";
                    }

                    //Console.WriteLine("byteData: " + byteData);

                    if (!string.IsNullOrWhiteSpace(byteData))
                    {
                        if (byteData.Equals("0000"))
                        {
                            goto nosend;
                        }

                        /*if (!byteData.Equals("0000") && isStoppedMoving == true)
                        {
                            isStoppedMoving = false;
                        }
                        //Si no se desplaza hacia ninguna parte
                        if (byteData.Equals("0000") && isStoppedMoving == true)
                        {
                            return String.Empty;
                        }

                        if (!isStoppedMoving)
                        {
                            if (byteData.Equals("0000") && isStoppedMoving == false)
                            {
                                isStoppedMoving = true;
                            }
                            ConnectionManager.AddInstruction("MV:" + byteData);
                        }*/
                        ConnectionManager.AddInstruction("MV:" + byteData);
                    }
                }
            nosend:

                //Console.WriteLine("moveInstructions: "+ moveInstructions);
                return string.Empty;
            }
            catch (Exception ex)
            {
                Log.Error("Error string MovementOnline(): " + ex.Message);
                return string.Empty;
            }
        }

        //Process the answer to the online movement interactions
        public bool ProcessMovementFromServer(string item, Message message, out Message messageOut)
        {
            try
            {
                messageOut = message;
                if (!string.IsNullOrWhiteSpace(item))
                {
                    if (item.Contains("MV"))
                    {
                        messageOut.Status = StatusMessage.Delivered;
                        string tempString = UtilityAssistant.ExtractValues(item, "MV");
                        //itemParam = itemParam.Replace("MV:" + tempString, "");
                        //itemParam = itemParam.Trim();
                        //Console.WriteLine("Mov Extraído: " + tempString);
                        if (!string.IsNullOrWhiteSpace(tempString))
                        {
                            Vector3 v3MvInstr = new SerializedVector3(tempString).ConvertToVector3();
                            Player.PLAYER.Entity.Transform.Position = v3MvInstr;
                            //return moveInstructions;
                        }
                        messageOut.Status = StatusMessage.Executed;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.Out.WriteLineAsync("Error ProcessMovementFromServer(string): " + ex.Message);
                messageOut = new Message();
                messageOut.Status = StatusMessage.Error;
                return false;
            }
        }

        //Solve Movement of player only (Offline)
        public void MovementSinglePlayer(Code.Assistants.UtilityAssistant.Axis AxistToIgnore = Code.Assistants.UtilityAssistant.Axis.Y)
        {
            try
            {
                if (Player.PLAYER.Entity == null)
                {
                    return;
                }

                if (Services.GetService<Controller>().movementDisable)
                {
                    if (!Services.GetService<Controller>().autoMovement)
                    {
                        Trios<int, Puppet, TimeSpan> cms = l_AnimacionesEntitys.Where(c => c.Item2.Entity.Name == Player.PLAYER.Entity.Name).First();
                        cms.Item1 = cms.Item2.AnimSprite.LugarAnimacionEspecificaPorNombre("Idle");
                    }
                    else
                    {
                        Trios<int, Puppet, TimeSpan> cms = l_AnimacionesEntitys.Where(c => c.Item2.Entity.Name == Player.PLAYER.Entity.Name).First();
                        cms.Item1 = cms.Item2.AnimSprite.LugarAnimacionEspecificaPorNombre("Moving");
                    }
                    return;
                }

                // We display the entity's name and its local and world position on screen
                DebugText.Print("Posicion Player: " + Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector, new Int2(200, 450));
                DebugText.Print("Posicion Arma: " + Player.PLAYER.Weapon.Transform.WorldMatrix.TranslationVector, new Int2(200, 470));
                DebugText.Print("Posicion Camara: " + Player.PLAYER.Camera.Transform.WorldMatrix.TranslationVector, new Int2(200, 490));

                //DebugText.Print("Posicion Hombro Izq: " + LeftShoulder.Transform.WorldMatrix.TranslationVector, new Int2(200, 530));
                //DebugText.Print("Posicion Hombro Der: " + RightShoulder.Transform.WorldMatrix.TranslationVector, new Int2(200, 550));
                DebugText.Print("Rotacion: " + Player.PLAYER.Entity.Transform.Rotation.ToString(), new Int2(200, 570));

                float speedf = DeltaTimizar(0.9f);
                Area area = new Area();

                if (Input.IsKeyPressed(Keys.C))
                {
                    speedf = DeltaTimizar(1.8f);
                    Services.GetService<Controller>().isPressedZToRun = true;
                }
                else
                {
                    if (Services.GetService<Controller>().isPressedZToRun)
                    {
                        Services.GetService<Controller>().isPressedZToRun = false;
                    }
                }

                modifier = new Vector3();
                if (Input.HasKeyboard)
                {
                    // Key down is used for when a key is being held down.
                    string byteData = string.Empty;

                    if (Input.IsKeyDown(Keys.Up) || Input.IsKeyDown(Keys.W))
                    {
                        Vector3 modifier = Player.PLAYER.Weapon.Transform.WorldMatrix.TranslationVector - Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector;
                        if (AxistToIgnore == UtilityAssistant.Axis.Y)
                        {
                            modifier = new Vector3(modifier.X, modifier.Z, modifier.Y);
                        }
                        BasicSinglePlayerMovement(modifier);
                        byteData += "10";
                    }
                    else if (Input.IsKeyDown(Keys.Down) || Input.IsKeyDown(Keys.S))
                    {
                        Vector3 modifier = Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector - Player.PLAYER.Weapon.Transform.WorldMatrix.TranslationVector;
                        if (AxistToIgnore == UtilityAssistant.Axis.Y)
                        {
                            modifier = new Vector3(modifier.X, modifier.Z, modifier.Y);
                        }
                        BasicSinglePlayerMovement(modifier);
                        byteData += "01";
                    }
                    else
                    {
                        byteData += "00";
                    }

                    if (Input.IsKeyDown(Keys.Left) || Input.IsKeyDown(Keys.A))
                    {
                        Vector3 modifier = Player.PLAYER.LeftShoulder.Transform.WorldMatrix.TranslationVector - Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector;
                        BasicSinglePlayerMovement(modifier);
                        byteData += "10";
                    }
                    else if (Input.IsKeyDown(Keys.Right) || Input.IsKeyDown(Keys.D))
                    {
                        Vector3 modifier = Player.PLAYER.RightShoulder.Transform.WorldMatrix.TranslationVector - Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector;
                        BasicSinglePlayerMovement(modifier);
                        byteData += "01";
                    }
                    else
                    {
                        byteData += "00";
                    }

                    //Rotacion (Solo manda modificadores)
                    if (Input.IsKeyDown(Keys.Q))
                    {
                        Player.PLAYER.Entity.Transform.Rotation *= Quaternion.RotationY(0.1f);
                    }
                    else if (Input.IsKeyDown(Keys.E))
                    {
                        Player.PLAYER.Entity.Transform.Rotation *= Quaternion.RotationY(-0.1f);
                    }

                    //Rotacion (Solo manda modificadores)
                    if (Input.IsKeyDown(Keys.Space))
                    {
                        Player.PLAYER.Entity.Transform.Rotation *= Quaternion.RotationZ(-0.1f);
                    }
                    else if (Input.IsKeyDown(Keys.C))
                    {
                        Player.PLAYER.Entity.Transform.Rotation *= Quaternion.RotationZ(0.1f);
                    }

                    ConnectionManager.AddInstruction("MV:" + byteData);
                }

            }
            catch (Exception ex)
            {
                Log.Error("Error Movement(): " + ex.Message);
            }
        }

        //Allow the character to move in one direction respecting limits and borders and also through a diferential
        public void BasicSinglePlayerMovement(Vector3 modifier)
        {
            Services.GetService<Controller>().lastPositionBeforeMove = DateTime.Now;
            if (ActiveArea == null)
            {
                //Vector3 a = weapon.Transform.WorldMatrix.TranslationVector - player.Transform.WorldMatrix.TranslationVector;
                Vector3 a = modifier;
                a.Y = 0;
                Player.PLAYER.Entity.Transform.Position += a;
                return;
            }

            if (ActiveArea.L_AreaDefiners.Count > 0)
            {
                if (Area.AvoidEntityLeave(ActiveArea.L_AreaDefiners, Player.PLAYER.Entity, modifier))
                {
                    Vector3 originalPosition = Player.PLAYER.Entity.Transform.Position;
                    Player.PLAYER.Entity.Transform.Position = Area.MoveMeThereOnlyIfValidCheckEverything(l_ActiveAreaFurniture, Player.PLAYER.Entity, modifier);
                    Trios<int, Puppet, TimeSpan> cms = l_AnimacionesEntitys.Where(c => c.Item2.Entity.Name == Player.PLAYER.Entity.Name).First();
                    if (Player.PLAYER.Entity.Transform.Position != originalPosition)
                    {
                        cms.Item1 = cms.Item2.AnimSprite.LugarAnimacionEspecificaPorNombre("Moving");
                        return;
                    }
                    //player.Transform.position = Area.MoveMeThereOnlyIfValid(l_ActiveAreaFurniture[0].L_AreaDefiners, player, modifier);
                    return;
                }
            }
        }

        //TODO: Solve Rotation of player only (Online)
        private string Rotation(string rotateInstructions = "")
        {
            try
            {
                // We display the entity's name and its local and world position on screen
                //DebugText.Print("Posicion Player: " + player.Transform.WorldMatrix.TranslationVector, new Int2(200, 450));
                //DebugText.Print("Posicion Arma: " + weapon.Transform.WorldMatrix.TranslationVector, new Int2(200, 470));
                //DebugText.Print("Posicion Camara: " + Camera.Transform.WorldMatrix.TranslationVector, new Int2(200, 490));

                //DebugText.Print("Posicion Hombro Izq: " + LeftShoulder.Transform.WorldMatrix.TranslationVector, new Int2(200, 530));
                //DebugText.Print("Posicion Hombro Der: " + RightShoulder.Transform.WorldMatrix.TranslationVector, new Int2(200, 550));

                if (Input.HasKeyboard)
                {
                    //Rotacion (Solo manda modificadores)
                    string byteData = string.Empty;
                    //L/R Yaw
                    if (Input.IsKeyDown(Keys.Q))
                    {
                        byteData += "10";
                    }
                    else if (Input.IsKeyDown(Keys.E))
                    {
                        byteData += "01";
                    }
                    else
                    {
                        byteData += "00";
                    }

                    //U/D Pitch
                    if (Input.IsKeyDown(Keys.Space))
                    {
                        byteData += "10";
                    }
                    else if (Input.IsKeyDown(Keys.C))
                    {
                        byteData += "01";
                    }
                    else
                    {
                        byteData += "00";
                    }

                    //L/R Roll
                    if (Input.IsKeyDown(Keys.O))
                    {
                        byteData += "10";
                    }
                    else if (Input.IsKeyDown(Keys.P))
                    {
                        byteData += "01";
                    }
                    else
                    {
                        byteData += "00";
                    }

                    if (!string.IsNullOrWhiteSpace(byteData))
                    {
                        if (!byteData.Equals("000000") && isStoppedRotate == true)
                        {
                            isStoppedRotate = false;
                        }
                        //Si no se desplaza hacia ninguna parte
                        if (byteData.Equals("000000") && isStoppedRotate == true)
                        {
                            return string.Empty;
                        }

                        if (!isStoppedRotate)
                        {
                            if (byteData.Equals("000000") && isStoppedRotate == false)
                            {
                                isStoppedRotate = true;
                                return string.Empty;
                            }
                            ConnectionManager.AddInstruction("RT:" + byteData);
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(rotateInstructions))
                {
                    if (rotateInstructions.Contains("RT"))
                    {
                        string tempString = UtilityAssistant.ExtractValues(rotateInstructions, "RT");
                        rotateInstructions = rotateInstructions.Replace("RT:" + tempString, "");
                        rotateInstructions = rotateInstructions.Trim();
                        if (!string.IsNullOrWhiteSpace(tempString))
                        {
                            //Quaternion qtRtInstr = UtilityAssistant.StringToQuaternion(tempString.Replace("{", "").Replace("}", ""));
                            Vector3 v3MvInstr = new SerializedVector3(tempString).ConvertToVector3();
                            Player.PLAYER.Entity.Transform.Rotation = UtilityAssistant.ToQuaternion(v3MvInstr);
                            return rotateInstructions;
                        }
                    }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                Log.Error("Error string Rotation(string): " + ex.Message);
                return string.Empty;
            }
        }
        #endregion

        #region AI
        /// <summary>
        /// Make every non-player puppet behave like it should acording to his own conditions.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void IA()
        {
            foreach (Puppet pppt in l_entitysCharacters)
            {
                pppt.RunIA();
            }
        }

        //Solve other results with external automatic input (i.e. [NPC AI/Other Players] Actions, Damage*, Enviroment, etcetera) (* = Online only)
        public void Results()
        {
            //TODO: Fusionar MovementPuppet con Movement
            //MovementPuppet();

            IA();
            //ShotPuppet();
        }

        //Solve Movement of other chars (NPC's or other players) (Online and Offline)
        public void MovementPuppet()
        {
            try
            {
                if (Player.PLAYER.Entity == null)
                {
                    return;
                }

                foreach (Puppet charmanfs in l_entitysCharacters)
                {
                    if (DetectPlayer(charmanfs))
                    {
                        MoveInPlayerDirection(charmanfs);
                        /*Vector3 originalPosition = charmanfs.Entity.Transform.position;
                        Cuarteto<int, AnimacionSprite, Entity, TimeSpan> cms = l_AnimacionesEntitys.Where(c => c.Item3.Name == charmanfs.Entity.Name).First();
                        if (charmanfs.Entity.Transform.position != originalPosition)
                        {
                            cms.Item1 = cms.Item2.LugarAnimacionEspecificaPorNombre("Moving");
                            return;
                        }
                        cms.Item1 = cms.Item2.LugarAnimacionEspecificaPorNombre("Idle");*/
                    }
                    else
                    {
                        MoveRandom(charmanfs);
                        /*Vector3 originalPosition = charmanfs.Item2.Transform.position;
                        Cuarteto<int, AnimacionSprite, Entity, TimeSpan> cms = l_AnimacionesEntitys.Where(c => c.Item3.Name == charmanfs.Item2.Name).First();
                        if (charmanfs.Item2.Transform.position != originalPosition)
                        {
                            cms.Item1 = cms.Item2.LugarAnimacionEspecificaPorNombre("Moving");
                            return;
                        }
                        cms.Item1 = cms.Item2.LugarAnimacionEspecificaPorNombre("Idle");*/
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error MovementNPC(): " + ex.Message);
            }
        }

        #region NPC Movements Types Functions
        //Allow the puppets with the required attribute to detect if the player is nearby and act in base of that
        public bool DetectPlayer(Puppet chr)
        {
            try
            {
                if (Player.PLAYER.Entity == null || chr == null)
                {
                    return false;
                }

                PropertyInfo p = chr.GetType().GetProperty("DetectionArea");
                if (p == null)
                {
                    return false;
                }

                //Could be anyone of those clases than actualy have detectionArea, however, it needs to be corrected
                float detectArea = ((IPuppetWithDetector)chr).DetectionArea;
                if (
                    Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector.X <= chr.Entity.Transform.Position.X + detectArea &&
                    Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector.X >= chr.Entity.Transform.Position.X - detectArea &&
                    Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector.Y <= chr.Entity.Transform.Position.Y + detectArea &&
                    Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector.Y >= chr.Entity.Transform.Position.Y - detectArea &&
                    Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector.Z <= chr.Entity.Transform.Position.Z + detectArea &&
                    Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector.Z >= chr.Entity.Transform.Position.Z - detectArea
                    )
                {
                    return ((IPuppetWithDetector)chr).IfDetectPlayer();
                }

                return false;
            }
            catch (Exception ex)
            {
                Log.Error("Error DetectPlayer(Puppet ent): " + ex.Message);
                return false;
            }
        }

        //Make the Puppet (NPC or other Player) move in the direction of the player
        public void MoveInPlayerDirection(Puppet chr)
        {
            try
            {
                if (Player.PLAYER.Entity == null || chr == null)
                {
                    return;
                }

                Vector3 a = Vector3.Zero;
                if (chr.Entity.Transform.Position.X < Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector.X)
                {
                    a.X = chr.VelocityModifier;
                    if (chr.Entity.Transform.Scale.X < 0)
                    {
                        chr.Entity.Transform.Scale.X *= -1;
                    }

                }
                else if (chr.Entity.Transform.Position.X > Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector.X)
                {
                    a.X = chr.VelocityModifier * -1;
                    if (chr.Entity.Transform.Scale.X > 0)
                    {
                        chr.Entity.Transform.Scale.X *= -1;
                    }
                }

                //Only if fly
                if (chr.IsFlyer)
                {
                    if (chr.Entity.Transform.Position.Y < Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector.Y)
                    {
                        a.Y = chr.VelocityModifier;
                    }
                    else if (chr.Entity.Transform.Position.Y > Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector.Y)
                    {
                        a.Y = chr.VelocityModifier * -1;
                    }
                }

                if (chr.Entity.Transform.Position.Z < Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector.Z)
                {
                    a.Z = chr.VelocityModifier;
                }
                else if (chr.Entity.Transform.Position.Z > Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector.Z)
                {
                    a.Z = chr.VelocityModifier * -1;
                }

                chr.Entity.Transform.Position += a;
            }
            catch (Exception ex)
            {
                Log.Error("Error MoveInPlayerDirection(): " + ex.Message);
            }
        }

        //TODO: Check if usefull, maybie unnecesary
        //Make the Puppet (NPC or other Player) walk in that direction
        public void MoveOneStep(Pares<Puppet, Entity> chr, Vector3 vector3)
        {
            try
            {
                if (Player.PLAYER.Entity == null || chr == null)
                {
                    return;
                }

                Vector3 a = Vector3.Zero;
                if (chr.Item2.Transform.Position.X < vector3.X)
                {
                    a.X = chr.Item1.VelocityModifier;
                }
                else if (chr.Item2.Transform.Position.X > vector3.X)
                {
                    a.X = chr.Item1.VelocityModifier * -1;
                }

                //Only if fly
                if (chr.Item1.IsFlyer)
                {
                    if (chr.Item2.Transform.Position.Y < vector3.Y)
                    {
                        a.Y = chr.Item1.VelocityModifier;
                    }
                    else if (chr.Item2.Transform.Position.Y > vector3.Y)
                    {
                        a.Y = chr.Item1.VelocityModifier * -1;
                    }
                }

                if (chr.Item2.Transform.Position.Z < vector3.Z)
                {
                    a.Z = chr.Item1.VelocityModifier;
                }
                else if (chr.Item2.Transform.Position.Z > vector3.Z)
                {
                    a.Z = chr.Item1.VelocityModifier * -1;
                }

                chr.Item2.Transform.Position += a;
            }
            catch (Exception ex)
            {
                Log.Error("Error MoveOneStep(): " + ex.Message);
            }
        }

        //Move puppets in a random direction, a random distance, each certain random ammount of time
        public async void MoveRandom(Puppet chr)
        {
            try
            {
                Random rand = new Random();
                int maxNumberForChance = 19;
                //If choose the turn to move this Puppet
                int random1 = rand.Next(1, maxNumberForChance);
                int random2 = rand.Next(1, maxNumberForChance);

                if (random1 != random2)
                {
                    return;
                }

                if (Player.PLAYER.Entity == null || chr == null)
                {
                    return;
                }

                Vector3 a = Vector3.Zero;
                int range = rand.Next(0, 51);
                a.X = rand.Next(range * -1, range);
                if (chr.IsFlyer)
                {
                    a.Y = rand.Next(range * -1, range);
                }
                a.Z = rand.Next(range * -1, range);

                chr.Entity.Transform.Position += UtilityAssistant.DistanceModifierByVectorComparison(chr.Entity.Transform.Position, a);
            }
            catch (Exception ex)
            {
                Log.Error("Error MoveRandom(): " + ex.Message);
            }
        }
        #endregion
        #endregion

        #region Shot & Combat Functions
        //Solve shot interactions (Online)
        public string ShotOnline()
        {
            try
            {
                if (Input.HasMouse)
                {
                    // Key down is used for when a key is being held down.
                    string byteData = string.Empty;
                    if (Input.IsMouseButtonDown(MouseButton.Left))
                    {

                        Shot shot = new Shot();
                        shot.LN = Player.PLAYER.Entity.Name;
                        shot.WPos = new SerializedVector3(Player.PLAYER.Weapon.Transform.WorldMatrix.TranslationVector).ConvertToVector3SN();
                        byteData += shot.ToJson(); //+= "1";

                        //Vector3 result = UtilityAssistant.ScreenToMapPosition(Input.MousePosition, UtilityAssistant.LockDimension.Y, 0f);
                        //byteData += result;

                    }

                    if (!string.IsNullOrWhiteSpace(byteData))
                    {
                        //goto continueshot;
                        //if (!byteData.Equals("0")) //&& isStoppedMoving == true)
                        //{
                        //    //isStoppedMoving = false;
                        //}
                        //Si no se desplaza hacia ninguna parte
                        /*if (byteData.Equals("0") && isStoppedMoving == true)
                        {
                            return string.Empty;
                        }

                        if (!isStoppedMoving)
                        {
                            if (byteData.Equals("0") && isStoppedMoving == false)
                            {
                                isStoppedMoving = true;
                            }
                        }*/
                        ConnectionManager.AddInstruction("ST:" + byteData);
                        //isQuestionAsked = false;
                    }
                }
                //continueshot:

                return string.Empty;
            }
            catch (Exception ex)
            {
                Log.Error("Error string ShotOnline(): " + ex.Message);
                return string.Empty;
            }
        }

        public static bool ProcesarConversarObj(string text, Message nwMsg, out Message messageOut)
        {
            messageOut = nwMsg;
            isRunning = true;
            try
            {
                Console.Out.WriteLineAsync("ProcesarConversarObj");
                bool result = false;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    messageOut.Status = StatusMessage.Delivered;
                    string tempString = Interfaz.Utilities.UtilityAssistant.ExtractValues(text, "CO");
                    tempString = Interfaz.Utilities.UtilityAssistant.CleanJSON(tempString);
                    //string strTemp = string.Empty;

                    //Reduntante, porque ahora el return devuelve una variable y por tanto no necesita dos returns
                    /*if (string.IsNullOrWhiteSpace(tempString))
                    {
                        return false;
                    }*/

                    if (!string.IsNullOrWhiteSpace(tempString))
                    {
                        if (tempString.Equals("PONG"))
                        {
                            //En este caso la idea es verificar que todo esta en orden, por eso se setea el isQuestionAsked y solo se retorna
                            //true, porque el servidor ha dicho que no ha detectado novedades necesarias de comunicar
                            //isQuestionAsked = false;
                            return true;
                        }

                        ConversacionObj convObj = ConversacionObj.CreateFromJson(tempString);
                        foreach (Shot btc in convObj.L_Bullets_to_create)
                        {
                            Controller.controller.playerController.CreateBullet(btc, messageOut, out messageOut);
                        }

                        foreach (ShotPosUpdate btc in convObj.L_Bullets_to_update)
                        {
                            UpdateBullet(btc, messageOut, out messageOut);
                        }

                        foreach (ShotState btc in convObj.L_Bullets_to_change_state)
                        {
                            DestroyBullet(btc, messageOut, out messageOut);
                        }
                        result = true;
                    }
                }
                isQuestionAsked = false;
                return result;
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("Error ProcesarConversarObj(string, Message, out Message): " + ex.Message);
                Console.ResetColor();
                messageOut = new Message();
                messageOut.Status = StatusMessage.Error;
                isQuestionAsked = false;
                isRunning = false;
                return false;
            }
        }

        #region Create Update y Destroy Shot Methods (Para Pregunta)
        public bool CreateBullet(Shot shot, Message message, out Message messageOut)
        {
            try
            {
                messageOut = message;
                if (shot.Equals(default(Shot)))
                {
                    return false;
                }

                string shotId = shot.Id.Replace("\"", "");
                Bullet bullet = new Bullet(shotId, shot.LN, UtilityAssistant.ConvertVector3NumericToStride(shot.WPos), UtilityAssistant.ConvertVector3NumericToStride(shot.Mdf));
                List<Entity> l_ent = Controller.controller.GetPrefab("Bullet");
                bullet.ProyectileBody = l_ent[0];
                bullet.ProyectileBody.Transform.Position = bullet.InitialPosition;
                UtilityAssistant.RotateTo(bullet.ProyectileBody, (bullet.ProyectileBody.Transform.Position + bullet.MovementModifier));
                dic_bulletsOnline.TryAdd(bullet.id, bullet);
                //dic_bulletsOnline[intbllt].ProyectileBody.Transform.Position = dic_bulletsOnline[intbllt].InitialPosition;
                //UtilityAssistant.RotateTo(dic_bulletsOnline[intbllt].ProyectileBody, (dic_bulletsOnline[intbllt].ProyectileBody.Transform.Position + dic_bulletsOnline[intbllt].MovementModifier));
                Entity.Scene.Entities.Add(bullet.ProyectileBody);

                //Entity.Scene.Entities.AddRange(instance);
                //Entity.Scene.Entities.Add(((Puppet)obtOfType).Entity);
                //Entity.Scene.Entities.AddRange(l_ent);
                return true;
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Error CreateBullet(string): " + ex.Message);
                Console.ResetColor();
                messageOut = new Message();
                messageOut.Status = StatusMessage.Error;
                return false;
            }
        }

        //Process the answer to the online shot interactions (Ex ProcessShotFromServer)
        public static bool UpdateBullet(ShotPosUpdate itemParameter, Message message, out Message message1)
        {
            string[] strArray = null;
            message1 = message;
            message1.Status = StatusMessage.Delivered;
            try
            {
                if (string.IsNullOrWhiteSpace(itemParameter.Id) && itemParameter.Pos == System.Numerics.Vector3.Zero)
                {
                    return false;
                }

                Bullet bllt = null;
                Bullet blltNew = null;

                dic_bulletsOnline.TryGetValue(itemParameter.Id, out bllt);
                if (bllt != null)
                {
                    blltNew = bllt;
                    blltNew.Position = UtilityAssistant.ConvertVector3NumericToStride(itemParameter.Pos);
                    if (dic_bulletsOnline.TryUpdate(itemParameter.Id, blltNew, bllt))
                    {
                        Console.WriteLine("\n\nUpdate of ShotPosUpdate Successfull!");
                    }
                }

                message1.Status = StatusMessage.Executed;
                if (message1.Status != StatusMessage.Executed)
                {
                    StateMessage stMsg = new StateMessage(message1.IdMsg, message1.Status);
                    return false;
                }
                //If it is Executed
                return true;
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("Error UpdateBullet(string): " + ex.Message);
                Console.ResetColor();
                message1 = new Message();
                message1.Status = StatusMessage.Error;
                return false;
            }
        }

        public static void DestroyBullet(ShotState sst, Message message, out Message messageOut)
        {
            try
            {
                float distance = 25; //TODO: Reeplace with a value inside the proyectile Someday
                messageOut = message;

                messageOut.Status = StatusMessage.Delivered;
                //foreach (KeyValuePair<int, Bullet> bllt in dic_bulletsOnline)
                Bullet bllt = null;
                string index = string.Empty;
                while (dic_bulletsOnline.TryGetValue(sst.Id, out bllt))
                {
                    if (bllt != null)
                    {
                        if (sst.State != StateOfTheShot.JustCreated)
                        {
                            /*if (sst.Id == bllt.id) //Redundante
                            {*/
                            index = sst.Id;
                            Bullet bullet = null;
                            if (sst.State == StateOfTheShot.Destroyed)
                            {
                                messageOut.Status = StatusMessage.Delivered;
                                //KeyValuePair<int, Bullet> kvp = dic_bulletsOnline.Where(C => C.Key == index).First();
                                if (dic_bulletsOnline.TryRemove(index, out bullet))
                                {
                                    if (bullet != null)
                                    {
                                        Controller.controller.Entity.Scene.Entities.Reverse().RemoveDisposeBy(bullet.ProyectileBody); //(bullet.ProyectileBody);
                                    }
                                }
                                //dic_bulletsOnline.Select(c => c.Value).ToList().RemoveAll(v => v.id == sst.Id);
                            }
                            /*else
                            {
                                float evaluatorX = UtilityAssistant.DistanceComparitorByAxis(bllt.InitialPosition.X, bllt.ProyectileBody.Transform.Position.X);
                                float evaluatorY = UtilityAssistant.DistanceComparitorByAxis(bllt.InitialPosition.Y, bllt.ProyectileBody.Transform.Position.Y);
                                float evaluatorZ = UtilityAssistant.DistanceComparitorByAxis(bllt.InitialPosition.Z, bllt.ProyectileBody.Transform.Position.Z);

                                if (evaluatorX >= distance || evaluatorY >= distance || evaluatorZ >= distance)
                                {
                                    //KeyValuePair<int, Bullet> kvp = dic_bulletsOnline.Where(C => C.Key == index).First();
                                    if (dic_bulletsOnline.TryRemove(index, out bullet))
                                    {
                                        if (bullet != null)
                                        {
                                            Controller.controller.Entity.Scene.Entities.Remove(bullet.ProyectileBody);
                                        }
                                    }
                                }
                            }*/
                            //}
                            messageOut.Status = StatusMessage.Executed;
                        }
                    }

                    /*if (index < dic_bulletsOnline.Count)
                    {
                        index++;
                    }*/
                }
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("Error DestroyBullet(string): " + ex.Message);
                Console.ResetColor();
                messageOut = new Message();
                messageOut.Status = StatusMessage.Error;
            }
        }
        #endregion

        #region Create Update y Destroy Shot Methods (Para Socket)
        public bool CreateShot(string itemParameter, Message message, out Message messageOut)
        {
            try
            {
                messageOut = message;
                if (!string.IsNullOrWhiteSpace(itemParameter))
                {
                    if (itemParameter.Contains("CS:"))
                    {
                        messageOut.Status = StatusMessage.Delivered;
                        string tempString = UtilityAssistant.ExtractValues(itemParameter, "CS");
                        //Console.WriteLine("Evaluate if it's Null or Whitespace");
                        if (!string.IsNullOrWhiteSpace(tempString))
                        {
                            Shot shot = Shot.CreateFromJson(tempString);
                            Bullet bullet = new Bullet(shot.Id, shot.LN, UtilityAssistant.ConvertVector3NumericToStride(shot.WPos), UtilityAssistant.ConvertVector3NumericToStride(shot.Mdf));
                            List<Entity> l_ent = Controller.controller.GetPrefab("Bullet");
                            bullet.ProyectileBody = l_ent[0];
                            bullet.ProyectileBody.Transform.Position = bullet.InitialPosition;
                            bullet.ProyectileBody.Name = "Bullet_"+shot.Id;
                            UtilityAssistant.RotateTo(bullet.ProyectileBody, (bullet.ProyectileBody.Transform.Position + bullet.MovementModifier));
                            bullet.LastUpdate = DateTime.Now;
                            dic_bulletsOnline.TryAdd(bullet.id, bullet);
                            //dic_bulletsOnline[intbllt].ProyectileBody.Transform.Position = dic_bulletsOnline[intbllt].InitialPosition;
                            //UtilityAssistant.RotateTo(dic_bulletsOnline[intbllt].ProyectileBody, (dic_bulletsOnline[intbllt].ProyectileBody.Transform.Position + dic_bulletsOnline[intbllt].MovementModifier));


                            //Entity.Scene.Entities.Add(bullet.ProyectileBody);
                            q_NewEntitiesToScene.Enqueue(bullet.ProyectileBody);
                        }
                        messageOut.Status = StatusMessage.Executed;

                        if (messageOut.Status != StatusMessage.Executed)
                        {
                            StateMessage stMsg = new StateMessage(messageOut.IdMsg, messageOut.Status);
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("Error CreateShot(string): " + ex.Message);
                Console.ResetColor();
                messageOut = new Message();
                messageOut.Status = StatusMessage.Error;
                return false;
            }
        }

        public bool UpdateShot(string itemParameter, Message message, out Message message1)
        {
            message1 = message;
            message1.Status = StatusMessage.Delivered;
            try
            {
                if (!string.IsNullOrWhiteSpace(itemParameter))
                {
                    if (itemParameter.Contains("US:"))
                    {
                        string tempString = UtilityAssistant.ExtractValues(itemParameter, "US");
                        if (!string.IsNullOrWhiteSpace(tempString))
                        {
                            ShotPosUpdate shtPosUpd = ShotPosUpdate.CreateFromJson(tempString);
                            if (string.IsNullOrWhiteSpace(shtPosUpd.Id) && shtPosUpd.Pos == System.Numerics.Vector3.Zero)
                            {
                                return false;
                            }

                            Bullet bllt = null;
                            Bullet blltNew = null;

                            if (dic_bulletsOnline.TryGetValue("\"" + shtPosUpd.Id + "\"", out bllt))
                            {
                                if (bllt != null)
                                {
                                    blltNew = bllt;
                                    blltNew.Position = UtilityAssistant.ConvertVector3NumericToStride(shtPosUpd.Pos);
                                    blltNew.LastUpdate = DateTime.Now;
                                    if (dic_bulletsOnline.TryUpdate("\"" + shtPosUpd.Id + "\"", blltNew, bllt))
                                    {
                                        Console.WriteLine("\n\nUpdate of ShotPosUpdate Successfull!");
                                    }
                                }
                            }

                            message1.Status = StatusMessage.Executed;
                            if (message1.Status != StatusMessage.Executed)
                            {
                                StateMessage stMsg = new StateMessage(message1.IdMsg, message1.Status);
                                return false;
                            }
                            //If it is Executed
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("Error UpdateShot(string): " + ex.Message);
                Console.ResetColor();
                message1 = new Message();
                message1.Status = StatusMessage.Error;
                return false;
            }
        }

        public void DestroyShot(string itemParameter, Message message, out Message messageOut)
        {
            messageOut = message;
            try
            {
                if (!string.IsNullOrWhiteSpace(itemParameter))
                {
                    if (itemParameter.Contains("DS:"))
                    {
                        string tempString = UtilityAssistant.ExtractValues(itemParameter, "DS");
                        if (!string.IsNullOrWhiteSpace(tempString))
                        {
                            ShotState sst = ShotState.CreateFromJson(itemParameter);
                            float distance = 25; //TODO: Reeplace with a value inside the proyectile Someday
                            messageOut = message;

                            messageOut.Status = StatusMessage.Delivered;
                            Bullet bllt = null;
                            string index = string.Empty;
                            while (dic_bulletsOnline.TryGetValue("\"" + sst.Id + "\"", out bllt))
                            {
                                if (bllt != null)
                                {
                                    if (sst.State != StateOfTheShot.JustCreated)
                                    {
                                        /*if (sst.Id == bllt.id) //Redundante
                                        {*/
                                        index = sst.Id;
                                        Bullet bullet = null;
                                        if (sst.State == StateOfTheShot.Destroyed)
                                        {
                                            messageOut.Status = StatusMessage.Delivered;
                                            if (dic_bulletsOnline.TryRemove("\"" + index + "\"", out bullet))
                                            {
                                                if (bullet != null)
                                                {
                                                    if (Entity.Scene.Entities.Contains(bullet.ProyectileBody))
                                                    {
                                                        //Entity.Scene.Entities.Remove(bullet.ProyectileBody);
                                                        q_RemoveEntitiesFromScene.Enqueue(bullet.ProyectileBody);
                                                    }
                                                }
                                            }
                                        }
                                        messageOut.Status = StatusMessage.Executed;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("Error DestroyShot(string): " + ex.Message);
                Console.ResetColor();
                messageOut = new Message();
                messageOut.Status = StatusMessage.Error;
            }
        }
        #endregion

        #region Varios Shots no usados
        public void ProcessShotTotalState(ShotTotalState STS)
        {
            try
            {
                /*if (STS.l_shotsCreated != null)
                {
                    if (STS.l_shotsCreated.Count > 0)
                    {
                        foreach (Shot shot in STS.l_shotsCreated)
                        {
                            Bullet bullet = new Bullet(shot.Id, shot.LN, UtilityAssistant.ConvertVector3NumericToStride(shot.WPos), UtilityAssistant.ConvertVector3NumericToStride(shot.Mdf));
                            List<Entity> l_ent = Controller.controller.GetPrefab("Bullet");
                            bullet.ProyectileBody = l_ent[0];
                            bullet.ProyectileBody.Transform.Position = bullet.InitialPosition;
                            UtilityAssistant.RotateTo(bullet.ProyectileBody, (bullet.ProyectileBody.Transform.Position + bullet.MovementModifier));
                            dic_bulletsOnline.TryAdd(dic_bulletsOnline.Count, bullet);
                            //dic_bulletsOnline[intbllt].ProyectileBody.Transform.Position = dic_bulletsOnline[intbllt].InitialPosition;
                            //UtilityAssistant.RotateTo(dic_bulletsOnline[intbllt].ProyectileBody, (dic_bulletsOnline[intbllt].ProyectileBody.Transform.Position + dic_bulletsOnline[intbllt].MovementModifier));
                            Entity.Scene.Entities.Add(bullet.ProyectileBody);
                        }
                    }
                }*/

                //Console.WriteLine("the l_shotsPosUpdates");
                if (STS.l_shotsPosUpdates != null)
                {
                    if (STS.l_shotsPosUpdates.Count > 0)
                    {
                        foreach (ShotPosUpdate shtUp in STS.l_shotsPosUpdates)
                        {
                            foreach (KeyValuePair<string, Bullet> bllt in dic_bulletsOnline)
                            {
                                if (shtUp.Id == bllt.Key)
                                {
                                    bllt.Value.Position = UtilityAssistant.ConvertVector3NumericToStride(shtUp.Pos);
                                }
                            }
                        }
                    }
                }

                //Console.WriteLine("the l_shotsStates");
                /*if (STS.l_shotsStates != null)
                {
                    if (STS.l_shotsStates.Count > 0)
                    {
                        foreach (ShotState shtSt in STS.l_shotsStates)
                        {
                            q_PendingShotStateToRun.Enqueue(shtSt);
                        }
                    }
                }*/
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("Error ProcessShotTotalState(string): " + ex.Message);
                Console.ResetColor();
            }
        }

        //Solve shot interactions (Offline)
        public void ShotOffline(Puppet pppt = null, Puppet target = null)
        {
            try
            {
                if (Player.PLAYER.Entity == null)
                {
                    return;
                }

                Entity entUse = null;
                bool isFlyer = false;
                if (pppt == null)
                {
                    entUse = Player.PLAYER.Entity;
                    isFlyer = Player.PLAYER.IsFlyer;
                }
                else
                {
                    entUse = pppt.Entity;
                    isFlyer = pppt.IsFlyer;
                }

                //if (Input.HasMouse)
                //{
                // Key down is used for when a key is being held down.
                string byteData = string.Empty;

                //if (Input.IsMouseButtonDown(MouseButton.Left))
                //{
                Vector3 moddif = Vector3.Zero;

                //Need to send instruction of shoot here in case of internet Here, instead of the next code:
                List<Entity> instance = shot.Instantiate();
                Vector3 initialposition = Vector3.Zero;
                if (pppt == null)
                {
                    instance.First().Transform.Position = Player.PLAYER.Weapon.Transform.WorldMatrix.TranslationVector;
                    initialposition = Player.PLAYER.Weapon.Transform.WorldMatrix.TranslationVector;
                    moddif = Player.PLAYER.Weapon.Transform.WorldMatrix.TranslationVector - Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector;
                }
                else
                {
                    instance.First().Transform.Position = pppt.Entity.Transform.Position;
                    initialposition = pppt.Entity.Transform.WorldMatrix.TranslationVector;
                    moddif = pppt.Weapon.Transform.WorldMatrix.TranslationVector - pppt.Entity.Transform.WorldMatrix.TranslationVector;
                    //Vector3 trgt = target != null ? UtilityAssistant.DistanceModifierByCartesianVectorComparison(target.Entity.Transform.position, pppt.Entity.Transform.position) : UtilityAssistant.DistanceModifierByCartesianVectorComparison(player.Transform.position, pppt.Entity.Transform.position);
                    //moddif = trgt;
                }

                if (!isFlyer)
                {
                    moddif.Y = entUse.Transform.Position.Y;
                }

                Entity.Scene.Entities.AddRange(instance);
                //int id = l_bullets.Count;
                l_bullets.Add(new Pares<List<Entity>, Bullet>(instance, new Bullet("", entUse.Name, initialposition, moddif)));
                //}
                //}
            }
            catch (Exception ex)
            {
                Log.Error("Error Shot(): " + ex.ToString());
            }
        }

        //Controls the Puppet to shot targets (Other Puppets (NPC/Other players), Furniture or the Player)
        private void ShotPuppet()
        {
            try
            {
                if (Player.PLAYER.Entity == null)
                {
                    return;
                }

                foreach (Puppet ent in l_entitysCharacters)
                {
                    if (DetectPlayer(ent))
                    {
                        Vector3 a = ent.Entity.Transform.WorldMatrix.TranslationVector;
                        if (!ent.IsFlyer)
                        {
                            a.Y = 0.0f;
                        }

                        //Need to send instruction of shoot here in case of internet Here, instead of the next code:
                        List<Entity> instance = shot.Instantiate();
                        instance.First().Transform.Position = Player.PLAYER.Weapon.Transform.WorldMatrix.TranslationVector;
                        Entity.Scene.Entities.AddRange(instance);

                        int id = l_bullets.Count;
                        l_bullets.Add(new Pares<List<Entity>, Bullet>(instance, new Bullet("", Player.PLAYER.Entity.Name, Player.PLAYER.Weapon.Transform.WorldMatrix.TranslationVector, a)));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error ShotPuppet(): " + ex.Message);
            }
            //If in area, check if in range, if not, get closer
            //If in range, lock target (i.e. Make a delay and/or rotation to point against him)
            //Generate a Proyectile of some description in the target direction from the gun.

            //Let "Shot", and "Damage" deal the rest :)
        }

        //Checks and execute the movement of the Bullets, Missile and everything else.
        public void ProyectileTurn()
        {
            try
            {
                if (l_bullets.Count > 0)
                {
                    ProyectileCheck(l_bullets);
                    // Here it process the received data from inet as result of bullet creation on server, Because here is where data of the bullets is updated anyway.
                    foreach (Pares<List<Entity>, Bullet> item in l_bullets)
                    {
                        item.Item1.First().Transform.Position += item.Item2.MovementModifier;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error ProyectileTurn(): " + ex.Message);
            }
        }

        //Check if the bullets impact the player.
        /*private int BulletCharacterEvaluation(Pares<List<Entity>, Bullet> bllt)
        {
            try
            {
                if (bllt == null || player == null)
                {
                    return -1; //it means problems
                }
                float Radius = 0.5f;
                if ((bllt.Item1[0].Transform.position.Z <= player.Transform.position.Z + Radius) && (bllt.Item1[0].Transform.position.Z >= player.Transform.position.Z - Radius))
                {
                    if ((bllt.Item1[0].Transform.position.X <= player.Transform.position.X + Radius) && (bllt.Item1[0].Transform.position.X >= player.Transform.position.X - Radius))
                    {
                        if ((bllt.Item1[0].Transform.position.Y <= player.Transform.position.Y + Radius) && (bllt.Item1[0].Transform.position.Y >= player.Transform.position.Y - Radius))
                        {
                            if (bllt.Item2.NameLauncher != player.Name)
                            {
                                if (Player.HpPlayer <= 0)
                                {
                                    Entity.Scene.Entities.Remove(player);
                                    l_entitysCharacters = l_entitysCharacters.Where(c => c.Entity.Name != player.Name).ToList();
                                    Log.Info(player.Name + " DIES!");
                                    return 2; //we make than the other function understand that as continue
                                }
                                if (l_entitysCharacters.Count <= 0)
                                {
                                    return 3; //we make than the other function understand than most make return the father function too
                                }
                                l_entitysCharacters.Where(c => c.Entity.Name == player.Name).First().HP -= 10;
                                Log.Info(player.Name + " DAMAGED! Now only have " + l_entitysCharacters.Where(c => c.Entity.Name == player.Name).First().HP + " of HP!!!");
                            }
                        }
                    }
                }
                return 1; //Everything normal.
            }
            catch (Exception ex)
            {
                Log.Error("Error BulletEvaluation(): " + ex.Message);
                return -1;
            }
        }*/

        //Check if the bullets impact the targets (non player).
        private int BulletEvaluation(Pares<List<Entity>, Bullet> bllt, Puppet enmy)
        {
            try
            {
                if (bllt == null || enmy == null || Player.PLAYER.Entity == null)
                {
                    return -1; //it means problems
                }
                float Radius = enmy.MPKillBox / 2;
                //if ((bllt.Item1[0].Transform.position.Z <= enmy.Entity.Transform.position.Z + Radius) && (bllt.Item1[0].Transform.position.Z >= enmy.Entity.Transform.position.Z - Radius))
                //{
                //    if ((bllt.Item1[0].Transform.position.X <= enmy.Entity.Transform.position.X + Radius) && (bllt.Item1[0].Transform.position.X >= enmy.Entity.Transform.position.X - Radius))
                //    {
                //        if ((bllt.Item1[0].Transform.position.Y <= enmy.Entity.Transform.position.Y + Radius) && (bllt.Item1[0].Transform.position.Y >= enmy.Entity.Transform.position.Y - Radius))
                //        {

                //Si la distancia es menos que la mitad del radio, entonces esta en el área de impacto
                float evaluatorX = UtilityAssistant.DistanceComparitorByAxis(bllt.Item1[0].Transform.Position.X, enmy.Entity.Transform.Position.X);
                float evaluatorY = UtilityAssistant.DistanceComparitorByAxis(bllt.Item1[0].Transform.Position.Y, enmy.Entity.Transform.Position.Y);
                float evaluatorZ = UtilityAssistant.DistanceComparitorByAxis(bllt.Item1[0].Transform.Position.Z, enmy.Entity.Transform.Position.Z);

                if (evaluatorX <= Radius && evaluatorY <= Radius && evaluatorZ <= Radius)
                {
                    if (bllt.Item2.NameLauncher != enmy.Entity.Name)
                    {
                        if (enmy.HP <= 0)
                        {
                            Entity.Scene.Entities.Remove(enmy.Entity);
                            l_entitysCharacters = l_entitysCharacters.Where(c => c.Entity.Name != enmy.Entity.Name).ToList();
                            Log.Info(enmy.Entity.Name + " DIES!");
                            return 2; //we make than the other function understand that as continue
                        }
                        if (l_entitysCharacters.Count <= 0)
                        {
                            return 3; //we make than the other function understand than must make return to the father function too
                        }
                        l_entitysCharacters.Where(c => c.Entity.Name == enmy.Entity.Name).First().HP -= bllt.Item2.Damage;
                        Log.Info(enmy.Entity.Name + " DAMAGED! Now only have " + l_entitysCharacters.Where(c => c.Entity.Name == enmy.Entity.Name).First().HP + " of HP!!!");
                        return 1; //Everything normal, Target Impacted;
                    }
                }
                //        }
                //    }
                //}
                return 0; //Everything normal, No Target Impacted.
            }
            catch (Exception ex)
            {
                Log.Error("Error BulletEvaluation(): " + ex.Message);
                return -1;
            }
        }

        //Damage resolution for all the things damaged, (i.e. damage comprobation and aplication)
        public void Damage()
        {
            try
            {
                if (l_entitysCharacters.Count <= 0 || l_bullets.Count <= 0 || Player.PLAYER.Entity == null)
                {
                    return;
                }

                //int chrsTarget = l_entitysCharacters.Count - 1;
                //int bltsTarget = l_bullets.Count - 1;

                //int i = 0, j = 0, result = 0;
                List<Puppet> l_tempList = l_entitysCharacters;
                l_tempList.Add(Player.PLAYER);
                foreach (Puppet chr in l_tempList)
                {
                    foreach (Pares<List<Entity>, Bullet> P_bllt in l_bullets)
                    {
                        BulletEvaluation(P_bllt, chr);
                        //BulletCharacterEvaluation(P_bllt);
                    }
                }
                /*do
                {
                    result = BulletEvaluation(l_bullets[j], l_entitysCharacters[i]);
                    BulletCharacterEvaluation(l_bullets[j]);
                    if (result == 3)
                    {
                        return;
                    }

                    //Not needed for now, because return on the main function without a '2' already cause to jump to the next itemParam
                    //i leave it here however as a reminder, if for some reason in the future i needed to make explicit the continue thing
                    //if(result == 2)
                    //{
                    //    continue;
                    //}

                    if (i == chrsTarget)
                    {
                        j++;
                        i = 0;
                    }
                    else
                    {
                        i++;
                    }
                } while (i < chrsTarget && j < bltsTarget);*/

                l_entitysCharacters = l_entitysCharacters.Where(c => c != null).ToList();
            }
            catch (Exception ex)
            {
                Log.Error("Error Damage(): " + ex.Message);
            }
        }

        //Check different elements of different proyectiles in order to have special effects or despawn them.
        private void ProyectileCheck(List<Pares<List<Entity>, Bullet>> l_blts)
        {
            try
            {
                float distance = 25;
                if (l_blts.Count <= 0)
                {
                    return;
                }

                float evaluatorX = 0;
                float evaluatorY = 0;
                float evaluatorZ = 0;
                foreach (Pares<List<Entity>, Bullet> item in l_blts.ToList())
                {
                    evaluatorX = UtilityAssistant.DistanceComparitorByAxis(item.Item2.InitialPosition.X, item.Item1[0].Transform.Position.X);
                    evaluatorY = UtilityAssistant.DistanceComparitorByAxis(item.Item2.InitialPosition.Y, item.Item1[0].Transform.Position.Y);
                    evaluatorZ = UtilityAssistant.DistanceComparitorByAxis(item.Item2.InitialPosition.Z, item.Item1[0].Transform.Position.Z);

                    if (evaluatorX >= distance || evaluatorY >= distance || evaluatorZ >= distance)
                    {
                        Entity.Scene.Entities.Remove(item.Item1[0]);
                        l_blts.Remove(item);
                    }
                }
                l_bullets = l_blts.Where(c => c != null).ToList();
            }
            catch (Exception ex)
            {
                Log.Error("Error ProyectileCheck(): " + ex.Message);
            }
        }
        #endregion
        #endregion

        #region Camera Specific Functions
        //Allow to change the next camera in the CameraList
        public void ChangeCamera()
        {
            try
            {
                if (Input.HasKeyboard)
                {
                    if (Input.IsKeyPressed(Keys.Tab))
                    {
                        Services.GetService<Controller>().NextCamera();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error ChangeCamera(): " + ex.Message);
            }
        }
        #endregion

        #region Suplementary Functions
        //Carga el prefab que corresponda al monstruo
        /*private void LoadSkeletonPrefab(Puppet ppt)
        {
            try
            {
                ppt.PrefabSkeleton = Content.Load<Prefab>("Prefabs/" + ppt.GetType().Name.ToString());
                foreach (Entity itemParam in ppt.Entity.GetChildren())
                {
                    if (itemParam.Name == "Camera")
                    {
                        Camera = itemParam;
                        continue;
                    }
                    if (itemParam.Name == "weapon")
                    {
                        weapon = itemParam;
                        continue;
                    }
                    if (itemParam.Name == "L-Shoulder")
                    {
                        LeftShoulder = itemParam;
                        continue;
                    }
                    if (itemParam.Name == "R-Shoulder")
                    {
                        RightShoulder = itemParam;
                        continue;
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Error("Error LoadSkeletonPrefab(Puppet): " + ex.Message);
            }
        }*/

        //Rotate the position of the entity in the direction of the destination
        //It tends to become gimbal locked
        private void RotateToEuler(Entity obj, Vector3 destination)
        {
            try
            {
                Vector3 direction = destination - obj.Transform.Position;
                Quaternion rotation = Quaternion.BetweenDirections(obj.Transform.WorldMatrix.TranslationVector, direction);
                obj.Transform.Rotation = Quaternion.Lerp(obj.Transform.Rotation, rotation, 1);
            }
            catch (Exception ex)
            {
                Log.Error("Error: RotateToEuler(Entity, Vector3): " + ex.Message);
            }
        }

        //Rotate the position of the entity in the direction of the destination
        private void RotateTo(Entity obj, Vector3 destination)
        {
            try
            {
                Vector3 towardsPlayer = obj.Transform.Position - destination;
                towardsPlayer.Y = 0; //avoid up/down rotation
                towardsPlayer.Normalize();
                obj.Transform.Rotation = Quaternion.BetweenDirections(Vector3.UnitZ, towardsPlayer);
            }
            catch (Exception ex)
            {
                Log.Error("Error: RotateTo(Entity, Vector3): " + ex.Message);
            }
        }

        //Aplica deltatime a cualquier velocidad, en función de facilitar su normalización con respecto a los updates. 
        public float DeltaTimizar(float baseMov = 0.9f)
        {
            float deltaTime = (float)Game.UpdateTime.Elapsed.TotalSeconds;
            float speed = deltaTime * baseMov;
            return speed;
        }
        #endregion
    }
}
