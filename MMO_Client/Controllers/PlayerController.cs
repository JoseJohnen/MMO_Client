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

namespace MMO_Client.Controllers
{
    public class PlayerController : StartupScript
    {
        #region Atributos
        // Declared public member fields and properties will show in the game studio
        public List<Puppet> l_entitysCharacters = new List<Puppet>(); //Other Characters, Players or not

        public List<Pares<List<Entity>, Bullet>> l_bullets = new List<Pares<List<Entity>, Bullet>>();

        [DataMemberIgnore]
        public List<Bullet> l_bulletsOnline = new List<Bullet>();

        public Area ActiveArea;
        public List<Area> l_ActiveAreaFurniture;

        public List<Trios<int, Puppet, TimeSpan>> l_AnimacionesEntitys = new List<Trios<int, Puppet, TimeSpan>>();
        DateTime lastFrame = DateTime.Now;

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

                lastFrame = DateTime.Now;

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

            Animacion();
        }

        /*internal static void SetInstrucciones(string returned)
        {
            instrucciones = returned;
        }*/
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
                    /*if (item.Name == "Sprite")
                    {
                        l_entitysCharacters[position].Sprite = item;
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
        public bool ProcessMovementFromServer(string item)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    if (item.Contains("MV"))
                    {
                        string tempString = UtilityAssistant.ExtractValues(item, "MV");
                        //item = item.Replace("MV:" + tempString, "");
                        //item = item.Trim();
                        //Console.WriteLine("Mov Extraído: " + tempString);
                        if (!string.IsNullOrWhiteSpace(tempString))
                        {
                            Vector3 v3MvInstr = new SerializedVector3(tempString).ConvertToVector3();
                            Player.PLAYER.Entity.Transform.Position = v3MvInstr;
                            //return moveInstructions;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error ProcessMovementFromServer(string): " + ex.Message);
                return false;
            }
        }

        //Solve Movement of player only (Offline)
        public void MovementSinglePlayer(UtilityAssistant.Axis AxistToIgnore = UtilityAssistant.Axis.Y)
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

        //Process the answer to the online shot interactions
        public bool ProcessShotFromServer(string item)
        {
            try
            {
                //Console.WriteLine("moveInstructions: "+ moveInstructions)
                if (!string.IsNullOrWhiteSpace(item))
                {
                    //Individual Answer Shots
                    /*if (item.Contains("ST:"))
                    {
                        string tempString = UtilityAssistant.ExtractValues(item, "ST");
                        if (!string.IsNullOrWhiteSpace(tempString))
                        {
                            Shot shot = Shot.CreateFromJson(tempString);

                            //Console.WriteLine("Shot: " + shot.ToJson());

                            int intbllt = l_bulletsOnline.Count;
                            l_bulletsOnline.Add(new Bullet(shot.Id, shot.LN, UtilityAssistant.ConvertVector3NumericToStride(shot.WPos), UtilityAssistant.ConvertVector3NumericToStride(shot.Mdf)));
                            List<Entity> l_ent = Controller.controller.GetPrefab("Bullet");
                            l_bulletsOnline[intbllt].ProyectileBody = l_ent[0];
                            l_bulletsOnline[intbllt].ProyectileBody.Transform.Position = l_bulletsOnline[intbllt].InitialPosition;
                            UtilityAssistant.RotateTo(l_bulletsOnline[intbllt].ProyectileBody, (l_bulletsOnline[intbllt].ProyectileBody.Transform.Position + l_bulletsOnline[intbllt].MovementModifier));
                            Entity.Scene.Entities.AddRange(l_ent);

                            //return shotInstructions;
                        }
                    }*/

                    //World Update Shots and those shot by others
                    if (item.Contains("SM:"))
                    {
                        string tempString = UtilityAssistant.ExtractValues(item, "SM");
                        if (!string.IsNullOrWhiteSpace(tempString))
                        {
                            ShotTotalState STS = ShotTotalState.CreateFromJson(tempString);

                            //Shot shot = Interfaz.Models.Shot.CreateFromJson(tempString);
                            //Console.WriteLine("Shot: " + shot.ToJson());
                            //l_bullets.Add(new Pares<List<Entity>, Bullet>(instance, new Bullet(entUse.Name, initialposition, moddif)));

                            if (STS.l_shots != null)
                            {
                                if (STS.l_shots.Count > 0)
                                {
                                    foreach (Shot shot in STS.l_shots)
                                    {
                                        int intbllt = l_bulletsOnline.Count;
                                        l_bulletsOnline.Add(new Bullet(shot.Id, shot.LN, UtilityAssistant.ConvertVector3NumericToStride(shot.WPos), UtilityAssistant.ConvertVector3NumericToStride(shot.Mdf)));
                                        List<Entity> l_ent = Controller.controller.GetPrefab("Bullet");
                                        l_bulletsOnline[intbllt].ProyectileBody = l_ent[0];
                                        l_bulletsOnline[intbllt].ProyectileBody.Transform.Position = l_bulletsOnline[intbllt].InitialPosition;
                                        UtilityAssistant.RotateTo(l_bulletsOnline[intbllt].ProyectileBody, (l_bulletsOnline[intbllt].ProyectileBody.Transform.Position + l_bulletsOnline[intbllt].MovementModifier));
                                        Entity.Scene.Entities.AddRange(l_ent);
                                    }
                                }
                            }

                            if (STS.l_shotsUpdates != null)
                            {
                                if (STS.l_shotsUpdates.Count > 0)
                                {
                                    foreach (ShotUpdate shtUp in STS.l_shotsUpdates)
                                    {
                                        foreach (Bullet bllt in l_bulletsOnline)
                                        {
                                            if (shtUp.Id == bllt.id)
                                            {
                                                bllt.Position = UtilityAssistant.ConvertVector3NumericToStride(shtUp.Pos);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        //return shotInstructions;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error ProcessShotFromServer(string): " + ex.Message);
                return false;
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
                int id = l_bullets.Count;
                l_bullets.Add(new Pares<List<Entity>, Bullet>(instance, new Bullet(id, entUse.Name, initialposition, moddif)));
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
                        l_bullets.Add(new Pares<List<Entity>, Bullet>(instance, new Bullet(id, Player.PLAYER.Entity.Name, Player.PLAYER.Weapon.Transform.WorldMatrix.TranslationVector, a)));
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

                    //Not needed for now, because return on the main function without a '2' already cause to jump to the next item
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
                foreach (Entity item in ppt.Entity.GetChildren())
                {
                    if (item.Name == "Camera")
                    {
                        Camera = item;
                        continue;
                    }
                    if (item.Name == "weapon")
                    {
                        weapon = item;
                        continue;
                    }
                    if (item.Name == "L-Shoulder")
                    {
                        LeftShoulder = item;
                        continue;
                    }
                    if (item.Name == "R-Shoulder")
                    {
                        RightShoulder = item;
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
