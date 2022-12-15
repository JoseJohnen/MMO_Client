using Stride.Engine;
using MMO_Client.Code.Controllers;

namespace MMO_Client.Code.Models
{
    public class Player : Puppet
    {
        public static string WP = string.Empty;
        public static string LS = string.Empty;
        public static string RS = string.Empty;
        public static string PS = string.Empty;
        public static string RT = string.Empty;
        public static string GNPS = string.Empty;
        public static string GNRT = string.Empty;

        public static Entity CAM = null;
        public static Player PLAYER = null;

        public Entity Camera { get => CAM; set { CAM = value; } }
        public override Entity Weapon { get; set; }
        public Entity LeftShoulder { get; set; }
        public Entity RightShoulder { get; set; }
        public Entity Gun { get; set; }

        public override float HP { get => hpplayer; set => hpplayer = value; }

        private float hpplayer = 15;
        public override bool IsFlyer { get => isflyer; set => isflyer = value; }

        private bool isflyer = false;
        public override Entity Entity
        {
            get
            {
                /*if(entity == null)
                {
                    entity = Controller.controller.playerController.player;
                }*/
                return entity;
            }
            set => entity = value;
        }

        private Entity entity = null;
        public Player()
        {
            entity = new Entity("Player"); //Controller.controller.playerController.player;
            PLAYER = this;
        }

        public Player(Entity ent)
        {
            entity = ent;
            PLAYER = this;
        }

        public Player(Entity ent, AnimacionSprite anmSpr)
        {
            entity = ent;
            base.AnimSprite = anmSpr;
            PLAYER = this;
        }

        public override void RunIA()
        {
            //Es el jugador, no corre IA
            return;
        }

        //For local purposes, it's a "card", hence, is static.
        //TODO: Data can and will be modified through a method by controller when online.
    }
}
