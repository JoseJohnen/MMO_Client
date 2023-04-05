using Stride.Core.Mathematics;
using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MMO_Client.Code.Assistants;
using MMO_Client.Code.Controllers;
using MMO_Client.Code.Interfaces;
using Newtonsoft.Json;
using Interfaz.Models;
using Stride.Graphics;
using Interfaz.Models.Shots;
using Interfaz.Models.Comms;

namespace MMO_Client.Code.Models
{
    public abstract partial class Puppet
    {
        private Dictionary<string, Pares<double, double>> direccionales = null;

        public virtual float HP { get; set; }
        public virtual float VelocityModifier { get; set; }
        public virtual float MPKillBox { get; set; }
        public virtual bool IsFlyer { get; set; }
        public virtual Entity Entity { get; set; }
        public virtual AnimacionSprite AnimSprite { get; set; }
        public virtual Dictionary<string, Pares<double, double>> Direccionales { get => direccionales; set => direccionales = value; }
        public virtual Entity RealEnt { get; set; }
        public virtual Entity Body { get; set; }
        public virtual Entity Weapon { get; set; }
        public virtual Entity LShoulder { get; set; }
        public virtual Entity RShoulder { get; set; }


        //TODO: Did puppets have Modes? or Should? Como para atacar/idle/escapar y definirlo por objeto, asignar un valor que defina comportamiento??.
        protected Puppet(Entity entity, Entity realEnt, float hp = 10, float velocityModifier = 0.05f, float mpKillBox = 0.08f, float mpKillBox1 = 0, bool isFlyer = true)
        {
            Entity = entity;
            RealEnt = realEnt;
            if (RealEnt != null)
            {
                RealEnt.Name = "RealEnt";
            }
            HP = hp;
            VelocityModifier = velocityModifier;
            IsFlyer = isFlyer;
            MPKillBox = mpKillBox;
        }

        protected Puppet(float hp = 10, float velocityModifier = 0.05f, float mpKillBox = 0.08f, bool isFlyer = true)
        {
            HP = hp;
            RealEnt = new Entity("RealEnt");
            VelocityModifier = velocityModifier;
            IsFlyer = isFlyer;
            MPKillBox = mpKillBox;
        }

        protected Puppet()
        {
        }

        public virtual Dictionary<string, Pares<double, double>> DirectionalsPerAngle(Dictionary<string, Pares<double, double>> arrString = null)
        {
            try
            {
                if (arrString != null)
                {
                    Direccionales = arrString;
                    return Direccionales;
                }

                if (Direccionales == null)
                {
                    Direccionales = new Dictionary<string, Pares<double, double>>
                    {
                        { "-WWalk", new Pares<double, double>() { Item1 = 0, Item2 = 45 } },
                        { "NWWalk", new Pares<double, double>() { Item1 = 45, Item2 = 90 } },
                        { "N-Walk", new Pares<double, double>() { Item1 = 90, Item2 = 135 } },
                        { "NEWalk", new Pares<double, double>() { Item1 = 135, Item2 = 180 } },
                        { "-EWalk", new Pares<double, double>() { Item1 = 180, Item2 = 225 } },
                        { "SEWalk", new Pares<double, double>() { Item1 = 225, Item2 = 270 } },
                        { "S-Walk", new Pares<double, double>() { Item1 = 270, Item2 = 315 } },
                        { "SWWalk", new Pares<double, double>() { Item1 = 315, Item2 = 360 } }
                    };
                }

                return Direccionales;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Puppet) DirectionalsPerAngle: " + ex.Message);
                return null;
            }
        }

        public bool Prepare(Vector3 position = new Vector3())
        {
            try
            {
                PrepareAnimations();
                InstancePuppet(position);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (MMO_Client.Code.Models.Puppet) Prepare: " + ex.Message);
                return false;
            }
        }

        public bool PrepareAnimations(bool AgregarComponenteSiNoEsta = true)
        {
            try
            {
                List<string> l_string = new List<string>();
                SpriteSheet spSheet = Controller.controller.GetSpriteSheet(this.GetType().Name);
                Dictionary<string, List<Sprite>> dic_sprites = spSheet.Sprites.Where(c => c.Name.Contains("_")).GroupBy(c => c.Name.Substring(0, c.Name.IndexOf("_"))).ToDictionary(g => g.Key, g => g.ToList());

                AnimSprite = new AnimacionSprite(new Animacion[dic_sprites.Count]);
                DirectionalsPerAngle(); //Si se seteo a mano, lo deja como esta, si no, pone 8 direccionales por defecto

                int i = 0;
                int j = 0;
                int firstValue = 0;
                int lastValue = 0;
                bool isFirstWork = true;
                string nameSection = string.Empty;
                List<Sprite> l_sprites = spSheet.Sprites.Where(c => c.Name.Contains("_")).ToList();
                foreach (Sprite spr in l_sprites)
                {
                    if (j == l_sprites.Count - 1)
                    {
                        lastValue = (j - 1);
                        //Añadidos valores antiguos porque ESOS SON LOS CORRECTOS para la animación, dado que ya tienes los datos de cierre también Y QUE ESTE SOLO SE EJECUTA EN LA ÚLTIMA ITERACIÓN.
                        AnimSprite.DesdeHastaFrames[i] = new Animacion(firstValue, lastValue, nameSection);
                        continue;
                    }

                    if ((spr.Name.Substring(0, spr.Name.IndexOf("_")) != nameSection) || (j == l_sprites.Count))
                    {
                        if (isFirstWork)
                        {
                            firstValue = j;
                            isFirstWork = false;
                        }
                        else
                        {
                            lastValue = (j - 1);
                            //Añadidos valores antiguos porque ESOS SON LOS CORRECTOS para la animación, dado que ya tienes los datos de cierre también.
                            AnimSprite.DesdeHastaFrames[i] = new Animacion(firstValue, lastValue, nameSection);
                            //Preparing values for next round
                            i++;
                            firstValue = j;
                        }
                        nameSection = spr.Name.Substring(0, spr.Name.IndexOf("_"));
                    }
                    j++;
                }

                //Si falta uno de los lados, agregar con los del otro lado y marcar que es inverso
                List<string> l_missingAnimations = Direccionales.Keys.Where(p => AnimSprite.DesdeHastaFrames.All(p2 => p2.Nombre != p)).ToList();
                int k = AnimSprite.DesdeHastaFrames.Length;
                Animacion[] tempAnimArray = AnimSprite.DesdeHastaFrames;
                Array.Resize<Animacion>(ref tempAnimArray, (AnimSprite.DesdeHastaFrames.Length + l_missingAnimations.Count));
                string nameOfTheLost = string.Empty;
                Animacion anim = default(Animacion);
                foreach (string item in l_missingAnimations)
                {
                    if (item.Contains("-W"))
                    {
                        anim = AnimSprite.DesdeHastaFrames.Where(c => c.Nombre.Substring(0, 2).Contains("-E") && c.Nombre.Contains("Walk")).First();
                    }
                    else if (item.Contains("-E"))
                    {
                        anim = AnimSprite.DesdeHastaFrames.Where(c => c.Nombre.Substring(0, 2).Contains("-W") && c.Nombre.Contains("Walk")).First();
                    }
                    else if (item.Contains("NW"))
                    {
                        anim = AnimSprite.DesdeHastaFrames.Where(c => c.Nombre.Substring(0, 2).Contains("NE") && c.Nombre.Contains("Walk")).First();
                    }
                    else if (item.Contains("NE"))
                    {
                        anim = AnimSprite.DesdeHastaFrames.Where(c => c.Nombre.Substring(0, 2).Contains("NW") && c.Nombre.Contains("Walk")).First();
                    }
                    else if (item.Contains("SW"))
                    {
                        anim = AnimSprite.DesdeHastaFrames.Where(c => c.Nombre.Substring(0, 2).Contains("SE") && c.Nombre.Contains("Walk")).First();
                    }
                    else if (item.Contains("SE"))
                    {
                        anim = AnimSprite.DesdeHastaFrames.Where(c => c.Nombre.Substring(0, 2).Contains("SW") && c.Nombre.Contains("Walk")).First();
                    }

                    if (!string.IsNullOrWhiteSpace(anim.Nombre))
                    {
                        tempAnimArray[k] = new Animacion(anim.DesdeFrame, anim.HastaFrame, item);
                        tempAnimArray[k].isReversible = true;
                        k++;
                    }
                }
                AnimSprite.DesdeHastaFrames = tempAnimArray;

                //si no tiene componente, agregarlo asumiendo es comportamiento estandar
                if (AgregarComponenteSiNoEsta)
                {
                    if (Entity == null)
                    {
                        int position = Controller.controller.playerController.l_entitysCharacters.Count();
                        Entity = new Entity(position.ToString() + " " + this.GetType().Name);
                        Controller.controller.playerController.l_entitysCharacters.Add(this);
                        AnimacionSprite.RegistrarEntidadEnAnimacionSprite(Controller.controller.playerController.l_entitysCharacters[position], new TimeSpan(0, 0, 0, 0, 250));
                    }

                    if (Entity.Get<SpriteComponent>() == null)
                    {
                        Entity.Components.Add(new SpriteComponent());
                        Entity.Get<SpriteComponent>().ChangeSpriteSheet(spSheet);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Puppet) bool PrepareAnimations: " + ex.Message);
                return false;
            }
        }

        public void InstancePuppet(Vector3 position = default(Vector3))
        {
            try
            {
                Vector3 Position = Vector3.Zero;
                if (position != default(Vector3))
                {
                    Position = position;
                }

                List<Entity> instance = Controller.controller.GetPrefab("RealEnt"); //RealEnt.Instantiate();
                instance.First().Transform.Position = Position;
                this.Entity.Transform.Position = Position;
                this.RealEnt = instance[0];
                Controller.controller.Entity.Scene.Entities.AddRange(instance);
                Controller.controller.Entity.Scene.Entities.Add(this.Entity);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (MMO_Client.Code.Models.Puppet) InstancePuppet: " + ex.Message);
            }
        }

        //TODO: Metodo virtual que activa activación ataque melee (Meramente visual o de hecho real con efecto sobre los datos) recibe el objetivo sobre el que ataca que orienta básicamente la rotación y animación

        public virtual void ShootingToOnline(Vector3 target, string shot)
        {
            try
            {
                UtilityAssistant.RotateTo(this.Entity, target);
                UtilityAssistant.RotateTo(this.RealEnt, target);

                //TODO: Activar animación relevante

                Message msgOut = new Message();
                Controller.controller.playerController.CreateShot(shot, msgOut, out msgOut);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Puppet) ShootingToOffline(Puppet, Shot): " + ex.Message);
            }
        }

        public virtual void ShootingToOnline(Vector3 target, Shot shot)
        {
            try
            {
                UtilityAssistant.RotateTo(this.Entity, target);
                UtilityAssistant.RotateTo(this.RealEnt, target);

                //TODO: Activar animación relevante

                Message msgOut = new Message();
                Controller.controller.playerController.CreateShot("CS:" + shot.ToJson(), msgOut, out msgOut);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Puppet) ShootingToOffline(Vector3, Shot): " + ex.Message);
            }
        }

        public virtual void ShootingToOffline(Puppet target, string type = "NB")
        {
            try
            {
                Vector3 trgPos = target.Entity.Transform.Position;
                UtilityAssistant.RotateTo(this.Entity, trgPos);
                UtilityAssistant.RotateTo(this.RealEnt, trgPos);

                //TODO: Activar animación relevante

                Shot sht = new Shot();
                sht.LN = this.RealEnt.Name;
                sht.Type = type;
                Message msgOut = new Message();
                sht.OrPos = UtilityAssistant.ConvertVector3StrideToNumeric(this.Entity.Transform.WorldMatrix.TranslationVector);
                sht.WPos = UtilityAssistant.ConvertVector3StrideToNumeric(this.RealEnt.FindChild("Fwd").Transform.WorldMatrix.TranslationVector);
                Controller.controller.playerController.CreateBullet(sht, msgOut, out msgOut);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Puppet) ShootingToOffline(Puppet, string): " + ex.Message);
            }
        }

        public virtual void MoveTo(Vector3 targetPosition, bool transition = false)
        {
            try
            {
                if (!transition)
                {
                    this.Entity.Transform.Position = targetPosition;
                    if (this.RealEnt != null)
                    {
                        this.RealEnt.Transform.Position = this.Entity.Transform.Position;
                    }
                    return;
                }

                Vector3 a = Vector3.Zero;
                if (this.Entity.Transform.Position.X < targetPosition.X)//Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector.X)
                {
                    a.X = this.VelocityModifier;
                    if (this.Entity.Transform.Scale.X < 0)
                    {
                        this.Entity.Transform.Scale.X *= -1;
                    }

                }
                else if (this.Entity.Transform.Position.X > targetPosition.X)//Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector.X)
                {
                    a.X = this.VelocityModifier * -1;
                    if (this.Entity.Transform.Scale.X > 0)
                    {
                        this.Entity.Transform.Scale.X *= -1;
                    }
                }

                //Only if fly
                if (this.IsFlyer)
                {
                    if (this.Entity.Transform.Position.Y < targetPosition.Y)//Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector.Y)
                    {
                        a.Y = this.VelocityModifier;
                    }
                    else if (this.Entity.Transform.Position.Y > targetPosition.Y)//Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector.Y)
                    {
                        a.Y = this.VelocityModifier * -1;
                    }
                }

                if (this.Entity.Transform.Position.Z < targetPosition.Z)//Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector.Z)
                {
                    a.Z = this.VelocityModifier;
                }
                else if (this.Entity.Transform.Position.Z > targetPosition.Z)//Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector.Z)
                {
                    a.Z = this.VelocityModifier * -1;
                }

                this.Entity.Transform.Position += a;
                if (this.RealEnt != null)
                {
                    this.RealEnt.Transform.Position = this.Entity.Transform.Position;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error MoveTo(): " + ex.Message);
            }

        }

        public virtual bool DetectarPlayer()
        {
            try
            {
                if (Player.PLAYER.Entity == null)
                {
                    return false;
                }

                PropertyInfo p = this.GetType().GetProperty("DetectionArea");
                if (p == null)
                {
                    return false;
                }

                //Could be anyone of those clases than actualy have detectionArea, however, it needs to be corrected
                float detectArea = ((IPuppetWithDetector)this).DetectionArea;
                if (
                    Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector.X <= this.Entity.Transform.Position.X + detectArea &&
                    Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector.X >= this.Entity.Transform.Position.X - detectArea &&
                    Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector.Y <= this.Entity.Transform.Position.Y + detectArea &&
                    Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector.Y >= this.Entity.Transform.Position.Y - detectArea &&
                    Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector.Z <= this.Entity.Transform.Position.Z + detectArea &&
                    Player.PLAYER.Entity.Transform.WorldMatrix.TranslationVector.Z >= this.Entity.Transform.Position.Z - detectArea
                    )
                {
                    if (detectArea != 0)
                    {
                        return IfDetectPlayer(detectArea);
                    }
                    return IfDetectPlayer();
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Puppet) DetectarPlayer(): " + ex.Message);
                return false;
            }
        }

        public virtual bool DetectEntityInRange(Entity ent, float DetectionArea = 15)
        {
            try
            {
                if (Player.PLAYER.Entity == null)
                {
                    return false;
                }

                float a = UtilityAssistant.DistanceComparitorByAxis(((Puppet)this).Entity.Transform.Position.X, ent.Transform.Position.X);
                float b = UtilityAssistant.DistanceComparitorByAxis(((Puppet)this).Entity.Transform.Position.Y, ent.Transform.Position.Y);
                float c = UtilityAssistant.DistanceComparitorByAxis(((Puppet)this).Entity.Transform.Position.Z, ent.Transform.Position.Z);

                if (
                    (a < (DetectionArea / 2)) &&
                    (b < (DetectionArea / 2)) &&
                    (c < (DetectionArea / 2))
                    )
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Puppet) DetectEntityInRange(Entity, float): " + ex.Message);
                return false;
            }
        }

        public virtual bool IfDetectPlayer(float DetectionArea = 15)
        {
            try
            {
                if (Player.PLAYER.Entity == null)
                {
                    return false;
                }

                float a = UtilityAssistant.DistanceComparitorByAxis(((Puppet)this).Entity.Transform.Position.X, Player.PLAYER.Entity.Transform.Position.X);
                float b = UtilityAssistant.DistanceComparitorByAxis(((Puppet)this).Entity.Transform.Position.Y, Player.PLAYER.Entity.Transform.Position.Y);
                float c = UtilityAssistant.DistanceComparitorByAxis(((Puppet)this).Entity.Transform.Position.Z, Player.PLAYER.Entity.Transform.Position.Z);

                if (
                    (a < (DetectionArea / 2)) &&
                    (b < (DetectionArea / 2)) &&
                    (c < (DetectionArea / 2))
                    )
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Puppet) IfDetectPlayer(float): " + ex.Message);
                return false;
            }
        }

        public virtual string ToJson()
        {
            try
            {
                JsonSerializerSettings serializeOptions = new JsonSerializerSettings
                {
                    Converters =
                    {
                        new EntityConverterJSON(),
                        //new FurnitureConverterJSON(),
                    }
                };

                string strResult = JsonConvert.SerializeObject(this, serializeOptions);
                return strResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Puppet) String ToJson(): " + ex.Message);
                return string.Empty;
            }
        }

        /*public virtual Puppet FromJson(string Text)
        {
            string txt = Text;
            try
            {
                txt = Interfaz.Utilities.UtilityAssistant.CleanJSON(txt.Replace("\u002B", "+"));

                JsonSerializerSettings serializeOptions = new JsonSerializerSettings
                {
                    Converters =
                    {
                        new EntityConverterJSON(),
                        new FurnitureConverterJSON(),
                    }
                };

                Puppet strResult = JsonConvert.DeserializeObject(txt, serializeOptions);


                //TODO: VER QUE EL OBJETO AL HACER TO JSON SALVE EL NOMBRE DE LA CLASE TAMBIÉN
                //TODO2: RECUERDA QUE DEBES EXTRAER EL OBJETO
                string TypeOfPuppetName = string.Empty;
                Type typ = Puppet.TypesOfMonsters().Where(c => c.Name == TypeOfPuppetName).FirstOrDefault();
                if (typ == null)
                {
                    typ = Puppet.TypesOfMonsters().Where(c => c.FullName == TypeOfPuppetName).FirstOrDefault();
                }

                object obtOfType = Activator.CreateInstance(typ); //Requires parameterless constructor.
                                                                  //TODO: System to determine the type of enemy to make the object, prepare stats and then add it to the list
                int position = Controller.controller.playerController.l_entitysCharacters.Count();
                Controller.controller.playerController.l_entitysCharacters.Add(((Puppet)obtOfType));

                Puppet nwMsg = Controller.controller.playerController.l_entitysCharacters[position];
                if (plDt != null)
                {
                    nwMsg.Weapon = new Interfaz.Utilities.SerializedVector3(plDt.WP).ConvertToVector3();
                    this.Weapon = nwMsg.Weapon;
                    nwMsg.Leftarm = new Interfaz.Utilities.SerializedVector3(plDt.LS).ConvertToVector3();
                    this.Leftarm = nwMsg.Leftarm;
                    nwMsg.Rightarm = new Interfaz.Utilities.SerializedVector3(plDt.RS).ConvertToVector3();
                    this.Rightarm = nwMsg.Rightarm;
                    nwMsg.Position = new Interfaz.Utilities.SerializedVector3(plDt.PS).ConvertToVector3();
                    this.Position = nwMsg.Position;
                    nwMsg.Rotation = Interfaz.Utilities.UtilityAssistant.StringToQuaternion(plDt.RT);
                    this.Rotation = nwMsg.Rotation;
                }
                return nwMsg;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Player) FromJson: " + ex.Message + " Text: " + txt);
                return new Player();
            }
        }*/

        public static List<Type> TypesOfMonsters()
        {
            List<Type> myTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => type.IsSubclassOf(typeof(Puppet)) && !type.IsAbstract).ToList();
            /*Type[] types = Assembly.GetExecutingAssembly().GetTypes().Where(type => type.IsSubclassOf(typeof(Puppet)) && !type.IsAbstract).ToArray();
            List<Type> myTypes = new List<Type>();
            foreach (Type t in types)
            {
                if (t.Namespace == "MMO_Client.Code.Models")
                    myTypes.Add(t);
            }

            var derived_types = new List<Type>();
            foreach (var domain_assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var assembly_types = domain_assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(Puppet)) && !type.IsAbstract);

                derived_types.AddRange(assembly_types);
            }*/
            return myTypes;
        }

        protected virtual void Power()
        {
            return;
        }

        public abstract void RunIA();

        public virtual void RunIAInstructions(string Instructions)
        {
            try
            {
                //TODO: Pon un switch que indique instrucciones especificas de IA
                switch ("")
                {
                    case "":
                        this.MoveTo(Vector3.Zero);
                        break;
                    case "1":
                        this.ShootingToOnline(Vector3.Zero, new Shot());
                        break;
                    case "2":
                        this.MeleeAttack();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Puppet) RunIAInstructions(string): " + ex.Message);
            }
        }

        public virtual void MeleeAttack()
        {
            try
            {
                //TODO: TODO EL MÉTODO
                //Tiene que, en un principio, cambiar la animación, la versión offline necesita determinar la distancia con el blanco también.
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Puppet) MeleeAttack(): " + ex.Message);
            }
        }
    }

    public abstract class PuppetShooter : Puppet, IPuppetWithDetector
    {
        public float Range { get; set; }
        private float detectionArea = 0;
        public float DetectionArea { get { if (detectionArea == 0) { detectionArea = 50; } return detectionArea; } set => detectionArea = value; }


        protected PuppetShooter(Entity entity, Entity realEnt, float range = 10, float hp = 10, float velocityModifier = 0.05f, float mpKillBox = 0.08f, bool isFlyer = true)
        {
            Range = range;
            base.Entity = entity;
            base.RealEnt = realEnt;
            if (base.RealEnt != null)
            {
                base.RealEnt.Name = "RealEnt";
            }
            base.HP = hp;
            base.VelocityModifier = velocityModifier;
            base.IsFlyer = isFlyer;
            base.MPKillBox = mpKillBox;
        }

        protected PuppetShooter()
        {

        }

        public void Shot()
        {
            Controller.controller.playerController.ShotOffline(this);
        }
    }

    public class Ghost : Puppet, IPuppetWithDetector
    {
        public float DetectionArea { get; set; }
        private DateTime dtCounter = DateTime.Now;
        private bool isInAreaPlayer = false;
        public Ghost(float hp = 1, float velocityModifier = 0.05F, float mpKillBox = 1.2f, bool isFlyer = false, float detectionArea = 3f) : base(hp, velocityModifier, mpKillBox, isFlyer)
        {
            base.HP = hp;
            base.VelocityModifier = velocityModifier;
            base.MPKillBox = mpKillBox;
            base.IsFlyer = isFlyer;
            DetectionArea = detectionArea;
            AnimSprite = new AnimacionSprite(new Animacion[4]
                {
                    new Animacion (0,26,"Idle"),  //Idle
                    new Animacion (27,31,"Start Chasing"), //Start Chasing
                    new Animacion (34,35,"Moving"), //Chasing the player
                    new Animacion (32,33,"Calm Down") //Calm down baby
                });
        }

        public Ghost(Entity ent, float hp = 1, float velocityModifier = 0.05F, float mpKillBox = 1.2f, bool isFlyer = false, float detectionArea = 3f) : base(hp, velocityModifier, mpKillBox, isFlyer)
        {
            base.Entity = ent;
            base.HP = hp;
            base.VelocityModifier = velocityModifier;
            base.MPKillBox = mpKillBox;
            base.IsFlyer = isFlyer;
            DetectionArea = detectionArea;
            AnimSprite = new AnimacionSprite(new Animacion[4]
                {
                    new Animacion (0,26,"Idle"),  //Idle
                    new Animacion (27,31,"Start Chasing"), //Start Chasing
                    new Animacion (34,35,"Moving"), //Chasing the player
                    new Animacion (32,33,"Calm Down") //Calm down baby
                });
        }

        public Ghost(Vector3 position, float hp = 1, float velocityModifier = 0.05F, float mpKillBox = 1.2f, bool isFlyer = false, float detectionArea = 3f) : base(hp, velocityModifier, mpKillBox, isFlyer)
        {
            base.Entity = new Entity("Ghost");
            Entity.Transform.Position = position;
            base.HP = hp;
            base.VelocityModifier = velocityModifier;
            base.MPKillBox = mpKillBox;
            base.IsFlyer = isFlyer;
            DetectionArea = detectionArea;
            AnimSprite = new AnimacionSprite(new Animacion[4]
                {
                    new Animacion (0,26,"Idle"),  //Idle
                    new Animacion (27,31,"Start Chasing"), //Start Chasing
                    new Animacion (34,35,"Moving"), //Chasing the player
                    new Animacion (32,33,"Calm Down") //Calm down baby
                });
        }

        public Ghost(Entity ent)
        {
            base.Entity = ent;
            base.HP = 1;
            base.VelocityModifier = 0.05F;
            base.MPKillBox = 1.2f;
            base.IsFlyer = false;
            DetectionArea = 3f;
            AnimSprite = new AnimacionSprite(new Animacion[4]
                {
                    new Animacion (0,26,"Idle"),  //Idle
                    new Animacion (27,31,"Start Chasing"), //Start Chasing
                    new Animacion (32,33,"Calm Down"), //Calm down baby
                    new Animacion (34,35,"Moving") //Chasing the player
                });
        }

        public bool IfDetectPlayer()
        {
            try
            {
                if (!isInAreaPlayer)
                {
                    dtCounter = DateTime.Now;
                    isInAreaPlayer = true;
                    AnimacionSprite.CambiarAnimacion(Entity, "Start Chasing");
                    //Controller.controller.ChangeEffect("GhostScream");
                }

                if (((DateTime.Now - dtCounter) >= new TimeSpan(0, 0, 0, 2, 0)) && isInAreaPlayer)
                {
                    Puppet ppet = Controller.controller.playerController.l_entitysCharacters.Where(c => c.Entity.Name.Equals(this.Entity.Name)).First();
                    AnimacionSprite.CambiarAnimacion(Entity, "Moving");
                    Controller.controller.playerController.MoveInPlayerDirection(ppet);
                    isInAreaPlayer = false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error " + this.Entity.Name + ".ifDetectPlayer(): " + ex.Message);
                return false;
            }
        }

        public override void RunIA()
        {
            throw new NotImplementedException();
        }
    }

    public class tankexport : Puppet, IPuppetWithDetector
    {
        public float DetectionArea { get; set; }
        public tankexport(float hp = 10, float velocityModifier = 0.05F, float mpKillBox = 1.2f, bool isFlyer = false, float detectionArea = 11f) : base(hp, velocityModifier, mpKillBox, isFlyer)
        {
            DetectionArea = detectionArea;
        }

        public tankexport()
        {
            base.Entity = new Entity();
            base.HP = 35;
            base.VelocityModifier = .9f;
            base.MPKillBox = 3;
            base.IsFlyer = false;
        }

        public tankexport(Entity ent)
        {
            base.Entity = ent;
            base.HP = 35;
            base.VelocityModifier = .9f;
            base.MPKillBox = 3;
            base.IsFlyer = false;
        }

        /*bool IPuppetWithDetector.IfDetectPlayer()
        {
            Console.WriteLine("PLAYER DETECTED BY tankexport!!!");
            return true;
        }*/

        public override void RunIA()
        {

        }
    }

    public class Spider : Puppet, IPuppetWithDetector
    {
        public float DetectionArea { get; set; }
        public Spider(float hp = 10, float velocityModifier = 0.05F, float mpKillBox = 1.2f, bool isFlyer = false, float detectionArea = 11f) : base(hp, velocityModifier, mpKillBox, isFlyer)
        {
            DetectionArea = detectionArea;
        }

        public Spider(Entity ent)
        {
            base.Entity = ent;
            base.HP = 35;
            base.VelocityModifier = .9f;
            base.MPKillBox = 3;
            base.IsFlyer = false;
        }

        /*bool IPuppetWithDetector.IfDetectPlayer()
        {
            Console.WriteLine("PLAYER DETECTED BY SPIDER!!!");
            return true;
        }*/

        public override void RunIA()
        {

        }
    }

    public class Hunter : Ship
    {
        public Hunter(float hp = 10, float velocityModifier = 0.05F, float mpKillBox = 0.08F, bool isFlyer = true, float DetectionAreaP = 15, List<Entity> l_turretsP = null) : base(hp, velocityModifier, mpKillBox, isFlyer, DetectionAreaP, l_turretsP)
        {
        }

        public override List<Entity> L_turrets
        {
            get
            {
                if (base.L_turrets == null)
                {
                    base.L_turrets = new List<Entity>();
                }
                return base.L_turrets;
            }
            set
            {
                base.L_turrets = value;
                //TODO: Add here the preprogramed movement of the weapons to position them respect of the rest of the ship
            }
        }

        public override void RunIA()
        {
            throw new NotImplementedException();
        }
    }
}
