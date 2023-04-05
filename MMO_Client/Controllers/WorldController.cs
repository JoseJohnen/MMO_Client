using System.Collections.Concurrent;
using System.Numerics;
using Interfaz.Models.Worlds;
using System;
using Stride.Engine;
using MMO_Client.Code.Controllers;
using MMO_Client.Models.TilesModels;
using Interfaz.Models;
using Tile = MMO_Client.Models.TilesModels.Tile;

namespace MMO_Client.Controllers
{
    public class WorldController : StartupScript
    {
        public static ConcurrentDictionary<string, World> dic_worlds = new ConcurrentDictionary<string, World>();

        static World TestWorld = null;

        public override void Start()
        {
            try
            {
                Services.AddService(this);
                Controller.controller.worldController = this;

                WorldController_OnStart();
            }
            catch (Exception ex)
            {
                Console.WriteLine("PlayerController Start() Error: " + ex.Message);
            }
        }

        public void WorldController_OnStart()
        {
            try
            {
                TestWorld = new MMO_Client.Models.WorldModels.BaseWorld();
                TestWorld.RegisterWorld("NombreDePrueba");
                TestWorld.FillWorld();
                Tile tl = new Grass("Tile_X_Y_Z", Vector3.One, new Vector3(2, 2, 2));
                string a = tl.ToJson();
                string b = TestWorld.ToJson();
                Tile d = Grass.CreateFromJson(a);
                d.InstanceTile();
                World c = World.CreateFromJson(b);
                Console.WriteLine(d.ToJson());
                Console.WriteLine(c.ToJson());
                //dic_worlds.Add("World1", BaseWorld.CreateWorld(prefab));
                //dic_worlds["World1"].Entity.Transform.Position = new Vector3(0, 0, 0);
                //dic_worlds["World1"].Save();
                //gmobj.Transform.parent = WorldController.Instance.Transform;
                //wrld.Empty();
                //wrld.Load();
            }
            catch (Exception ex)
            {
                Console.WriteLine("WorldController_OnStart() Error: " + ex.Message);
            }
        }

        public void WorldController_Tick()
        {
            try
            {
               
            }
            catch (Exception ex)
            {
                Console.WriteLine("WorldController_Tick() Error: " + ex.Message);
            }
        }

    }
}