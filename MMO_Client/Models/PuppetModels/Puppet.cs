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
using MMO_Client.Models.FurnitureModels;

namespace MMO_Client.Code.Models
{
    public abstract class Puppet
    {
        public virtual float HP { get; set; }
        public virtual float VelocityModifier { get; set; }
        public virtual float MPKillBox { get; set; }
        public virtual bool IsFlyer { get; set; }
        public virtual Entity Entity { get; set; }
        public virtual AnimacionSprite AnimSprite { get; set; }
        public virtual Entity RealEnt { get; set; }
        public virtual Entity Body { get; set; }
        public virtual Entity Weapon { get; set; }
        public virtual Entity LShoulder { get; set; }
        public virtual Entity RShoulder { get; set; }


        //TODO: Did puppets have Modes? or Should? Como para atacar/idle/escapar y definirlo por objeto, asignar un valor que defina comportamiento??.
        protected Puppet(Entity entity, Entity realEnt, float hp = 10, float velocityModifier = 0.05f, float mpKillBox = 0.08f, bool isFlyer = true)
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

        public virtual string ToJson()
        {
            try
            {
                JsonSerializerSettings serializeOptions = new JsonSerializerSettings
                {
                    Converters =
                    {
                        new EntityConverterJSON(),
                        new FurnitureConverterJSON(),
                    }
                };

                string strResult = JsonConvert.SerializeObject(this, serializeOptions);
                return strResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error String ToJson(): "+ex.Message);
                return string.Empty;
            }
        }

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
            if(base.RealEnt != null)
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

        bool IPuppetWithDetector.IfDetectPlayer()
        {
            Console.WriteLine("PLAYER DETECTED BY tankexport!!!");
            return true;
        }

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

        bool IPuppetWithDetector.IfDetectPlayer()
        {
            Console.WriteLine("PLAYER DETECTED BY SPIDER!!!");
            return true;
        }

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
