using System.Collections.Concurrent;
using System;
using Stride.Engine;
using MMO_Client.Code.Controllers;
using MMO_Client.Models.WorldModels;
using System.Collections.Generic;
using MMO_Client.Code.Assistants;

namespace MMO_Client.Controllers
{
    public class WorldController : StartupScript
    {
        public static ConcurrentDictionary<string, World> dic_worlds = new ConcurrentDictionary<string, World>();
        public static Game game = null;

        static World TestWorld = null;

        public static void LoadWorld(string textOriginal)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(textOriginal))
                {
                    if (textOriginal.Contains("WM:"))
                    {
                        string tempString = UtilityAssistant.ExtractValues(textOriginal, "WM");
                        if (!string.IsNullOrWhiteSpace(tempString))
                        {
                            World world = World.CreateFromJson(textOriginal);
                            world.InstanceWorld();
                            dic_worlds.TryAdd(world.Name, World.CreateFromJson(textOriginal));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("WorldController LoadWorld(string) Error: " + ex.Message);
            }
        }

        public override void Start()
        {
            try
            {
                Services.AddService(this);
                Controller.controller.worldController = this;
                game = (Game)this.Game;

                WorldController_OnStart();
            }
            catch (Exception ex)
            {
                Console.WriteLine("WorldController Start() Error: " + ex.Message);
            }
        }

        public void WorldController_OnStart()
        {
            try
            {
                TestWorld = new MMO_Client.Models.WorldModels.BaseWorld();
                TestWorld.RegisterWorld("NombreDePrueba");
                TestWorld.FillWorld("Grass");
                string b = TestWorld.ToJson();
                World c = World.CreateFromJson(b);
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