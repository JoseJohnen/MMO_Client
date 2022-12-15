using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Stride.Core.Mathematics;
using System.Text.Json;
using Newtonsoft.Json;
using Stride.Engine;
using Stride.Rendering.Sprites;
using MMO_Client.Code.Models;
using MMO_Client.Code.Controllers;
using System.Text.Json.Serialization;
using MMO_Client.Code.Assistants;
using Quaternion = Stride.Core.Mathematics.Quaternion;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Stride.Graphics;
using Stride.Rendering;
using MMO_Client.Models.FurnitureModels;
using Stride.Core.Collections;

namespace MMO_Client
{
    public class WorldController : StartupScript
    {
        // Declared public member fields and properties will show in the game studio

        List<Planet> l_planets = new List<Planet>();
        private List<Furniture> l_furniture = new List<Furniture>();

        private string[] l_strings = null; //To load the files founded, temporal space to know the names before the load of the assets
        public Prefab tileSquare;
        public Sprite l_sprites;

        public List<List<Entity>> l_l_instanciatedEntitys = new List<List<Entity>>();
        //TODO: For now, furniture will be his own thing, because Vehicules are gonna be consider furniture too
        public List<Pares<Furniture, Entity>> l_entitysFurnitures = new List<Pares<Furniture, Entity>>(); //Other Characters, Players or not

        //List the areas than exist in the map
        public List<Area> l_areaLimits = new List<Area>();
        public List<Area> l_areaTeleports = new List<Area>();
        public List<Area> l_areaFurnitures = new List<Area>();

        public override void Start()
        {
            Services.AddService(this);
            Controller.controller.worldController = this;

            //CreatePlanet(10, 10, 3, 3, 3);

            //FurnitureCreate("Ground", l_planets[0],Vector3.Zero);
            //List<Entity> instance6 = vase.Instantiate();
            ////instance4.First().Transform.Position = Vector3.Zero;
            //instance6.First().Get<SpriteComponent>().Enabled = false;
            //instance6.First().Transform.Position.X = -14.750f;
            //instance6.First().Transform.Position.Y = -1.228f;
            //Entity.Scene.Entities.AddRange(instance6);
            //l_l_instanciatedEntitys.Add(instance6);

            //PrepareAreas();
            //Load();
            //Save();
        }

        private void PrepareAreas()
        {
            //l_areaTeleports.Add(new Area("Habitacion -> Pasillo"));
            //l_areaTeleports.Add(new Area("Cocina -> Pasillo"));
            //l_areaTeleports.Add(new Area("Pasillo -> Habitacion"));

            //l_areaTeleports.Add(new Area("Pasillo -> Cocina"));
            //l_areaTeleports.Add(new Area("Pasillo (Left) -> Pasillo (Right)"));
            //l_areaTeleports.Add(new Area("Pasillo (Right) -> Pasillo (Left)"));

            //l_areaTeleports.Add(new Area("\"GuestRoom\""));
            //l_areaTeleports.Add(new Area("Bathroom"));
            //l_areaTeleports.Add(new Area("Fusebox"));

            //l_areaTeleports.Add(new Area("Vase Area"));
            //l_areaTeleports.Add(new Area("Bed (Activate)"));

            //l_areaLimits.Add(new Area("Bedroom"));
            //l_areaLimits.Add(new Area("Hallway"));
            //l_areaLimits.Add(new Area("Kitchen"));

            //l_areaFurnitures.Add(new Area("Bedroom"));
            //l_areaFurnitures.Add(new Area("Bedroom"));
            //l_areaFurnitures.Add(new Area("Hallway"));
            //l_areaFurnitures.Add(new Area("Kitchen"));
            //l_areaFurnitures.Add(new Area("Kitchen"));
            //l_areaFurnitures.Add(new Area("Kitchen"));
            //l_areaFurnitures.Add(new Area("Bedroom"));

            ////TODO: CORRIGE ESTE Y OESTE, ESTAN INVERTIDOS EN EL X AXIS
            ////Teleports and History Actions
            ////Habitacion -> Pasillo
            //l_areaTeleports[0].L_AreaDefiners[0].Point = new Pares<string, SerializedVector3>("NW", new SerializedVector3(-1.934f, 0.198f, 0));
            //l_areaTeleports[0].L_AreaDefiners[1].Point = new Pares<string, SerializedVector3>("NE", new SerializedVector3(-1.050f, 0.198f, 0));
            //l_areaTeleports[0].L_AreaDefiners[2].Point = new Pares<string, SerializedVector3>("SW", new SerializedVector3(-1.934f, -1.170f, 0)); //SW
            //l_areaTeleports[0].L_AreaDefiners[3].Point = new Pares<string, SerializedVector3>("SE", new SerializedVector3(-1.050f, -1.170f, 0)); //SE

            ////Cocina -> Pasillo
            //l_areaTeleports[1].L_AreaDefiners[0].Point = new Pares<string, SerializedVector3>("NW", new SerializedVector3(5.303f, -0.867f, 0));
            //l_areaTeleports[1].L_AreaDefiners[1].Point = new Pares<string, SerializedVector3>("NE", new SerializedVector3(6.205f, -0.867f, 0));
            //l_areaTeleports[1].L_AreaDefiners[2].Point = new Pares<string, SerializedVector3>("SW", new SerializedVector3(5.303f, -1.334f, 0)); //SW
            //l_areaTeleports[1].L_AreaDefiners[3].Point = new Pares<string, SerializedVector3>("SE", new SerializedVector3(6.205f, -1.334f, 0)); //SE

            ////Pasillo -> Habitacion
            //l_areaTeleports[2].L_AreaDefiners[0].Point = new Pares<string, SerializedVector3>("NW", new SerializedVector3(-13.319f, -0.110f, 0));
            //l_areaTeleports[2].L_AreaDefiners[1].Point = new Pares<string, SerializedVector3>("NE", new SerializedVector3(-12.600f, -0.110f, 0));
            //l_areaTeleports[2].L_AreaDefiners[2].Point = new Pares<string, SerializedVector3>("SW", new SerializedVector3(-13.319f, -0.330f, 0)); //SW
            //l_areaTeleports[2].L_AreaDefiners[3].Point = new Pares<string, SerializedVector3>("SE", new SerializedVector3(-12.600f, -0.330f, 0)); //SE

            ////Pasillo -> Cocina
            //l_areaTeleports[3].L_AreaDefiners[0].Point = new Pares<string, SerializedVector3>("NW", new SerializedVector3(-5.987f, -0.110f, 0));
            //l_areaTeleports[3].L_AreaDefiners[1].Point = new Pares<string, SerializedVector3>("NE", new SerializedVector3(-4.682f, -0.110f, 0));
            //l_areaTeleports[3].L_AreaDefiners[2].Point = new Pares<string, SerializedVector3>("SW", new SerializedVector3(-5.987f, -0.270f, 0)); //SW
            //l_areaTeleports[3].L_AreaDefiners[3].Point = new Pares<string, SerializedVector3>("SE", new SerializedVector3(-4.682f, -0.270f, 0)); //SE

            ////Pasillo (Left) -> Pasillo (Right)
            //l_areaTeleports[4].L_AreaDefiners[0].Point = new Pares<string, SerializedVector3>("NW", new SerializedVector3(-25.000f, -0.220f, 0));
            //l_areaTeleports[4].L_AreaDefiners[1].Point = new Pares<string, SerializedVector3>("NE", new SerializedVector3(-18.236f, -0.220f, 0));
            //l_areaTeleports[4].L_AreaDefiners[2].Point = new Pares<string, SerializedVector3>("SW", new SerializedVector3(-25.000f, -0.923f, 0)); //SW
            //l_areaTeleports[4].L_AreaDefiners[3].Point = new Pares<string, SerializedVector3>("SE", new SerializedVector3(-18.236f, -0.923f, 0)); //SE

            ////Pasillo (Right) -> Pasillo (Left)
            //l_areaTeleports[5].L_AreaDefiners[0].Point = new Pares<string, SerializedVector3>("NW", new SerializedVector3(-5.250f, -0.220f, 0));
            //l_areaTeleports[5].L_AreaDefiners[1].Point = new Pares<string, SerializedVector3>("NE", new SerializedVector3(-1.682f, -0.220f, 0));
            //l_areaTeleports[5].L_AreaDefiners[2].Point = new Pares<string, SerializedVector3>("SW", new SerializedVector3(-5.250f, -0.923f, 0)); //SW
            //l_areaTeleports[5].L_AreaDefiners[3].Point = new Pares<string, SerializedVector3>("SE", new SerializedVector3(-1.682f, -0.923f, 0)); //SE

            ////"GuestRoom"
            //l_areaTeleports[6].L_AreaDefiners[0].Point = new Pares<string, SerializedVector3>("NW", new SerializedVector3(-17.264f, -0.110f, 0));
            //l_areaTeleports[6].L_AreaDefiners[1].Point = new Pares<string, SerializedVector3>("NE", new SerializedVector3(-15.868f, -0.110f, 0));
            //l_areaTeleports[6].L_AreaDefiners[2].Point = new Pares<string, SerializedVector3>("SW", new SerializedVector3(-17.264f, -0.270f, 0)); //SW
            //l_areaTeleports[6].L_AreaDefiners[3].Point = new Pares<string, SerializedVector3>("SE", new SerializedVector3(-15.868f, -0.270f, 0)); //SE

            ////Bathroom
            //l_areaTeleports[7].L_AreaDefiners[0].Point = new Pares<string, SerializedVector3>("NW", new SerializedVector3(-9.734f, -0.110f, 0));
            //l_areaTeleports[7].L_AreaDefiners[1].Point = new Pares<string, SerializedVector3>("NE", new SerializedVector3(-8.203f, -0.110f, 0));
            //l_areaTeleports[7].L_AreaDefiners[2].Point = new Pares<string, SerializedVector3>("SW", new SerializedVector3(-9.734f, -0.270f, 0)); //SW
            //l_areaTeleports[7].L_AreaDefiners[3].Point = new Pares<string, SerializedVector3>("SE", new SerializedVector3(-8.203f, -0.270f, 0)); //SE

            ////Fusebox
            //l_areaTeleports[8].L_AreaDefiners[0].Point = new Pares<string, SerializedVector3>("NW", new SerializedVector3(4.534f, 1.340f, 0));
            //l_areaTeleports[8].L_AreaDefiners[1].Point = new Pares<string, SerializedVector3>("NE", new SerializedVector3(5.260f, 1.340f, 0));
            //l_areaTeleports[8].L_AreaDefiners[2].Point = new Pares<string, SerializedVector3>("SW", new SerializedVector3(4.534f, 1.000f, 0)); //SW
            //l_areaTeleports[8].L_AreaDefiners[3].Point = new Pares<string, SerializedVector3>("SE", new SerializedVector3(5.260f, 1.000f, 0)); //SE

            ////Vase Area
            //l_areaTeleports[9].L_AreaDefiners[0].Point = new Pares<string, SerializedVector3>("NW", new SerializedVector3(-6.045f, -0.110f, 0));
            //l_areaTeleports[9].L_AreaDefiners[1].Point = new Pares<string, SerializedVector3>("NE", new SerializedVector3(-5.045f, -0.110f, 0));
            //l_areaTeleports[9].L_AreaDefiners[2].Point = new Pares<string, SerializedVector3>("SW", new SerializedVector3(-6.045f, -1.100f, 0)); //SW
            //l_areaTeleports[9].L_AreaDefiners[3].Point = new Pares<string, SerializedVector3>("SE", new SerializedVector3(-5.045f, -1.100f, 0)); //SE

            ////Bed (Activate)
            //l_areaTeleports[10].L_AreaDefiners[0].Point = new Pares<string, SerializedVector3>("NW", new SerializedVector3(0.235f, 1.110f, 0));
            //l_areaTeleports[10].L_AreaDefiners[1].Point = new Pares<string, SerializedVector3>("NE", new SerializedVector3(0.400f, 1.110f, 0));
            //l_areaTeleports[10].L_AreaDefiners[2].Point = new Pares<string, SerializedVector3>("SW", new SerializedVector3(0.235f, -0.350f, 0)); //SW
            //l_areaTeleports[10].L_AreaDefiners[3].Point = new Pares<string, SerializedVector3>("SE", new SerializedVector3(0.400f, -0.350f, 0)); //SE
            ////End Teleports

            ////-- LIMITS --
            ////Bedroom
            //l_areaLimits[0].L_AreaDefiners[0].Point = new Pares<string, SerializedVector3>("NW", new SerializedVector3(-1.230f, 0.930f, 0));
            //l_areaLimits[0].L_AreaDefiners[1].Point = new Pares<string, SerializedVector3>("NE", new SerializedVector3(1.500f, 0.930f, 0));
            //l_areaLimits[0].L_AreaDefiners[2].Point = new Pares<string, SerializedVector3>("SW", new SerializedVector3(-1.230f, -0.929f, 0)); //SW
            //l_areaLimits[0].L_AreaDefiners[3].Point = new Pares<string, SerializedVector3>("SE", new SerializedVector3(1.500f, -0.929f, 0)); //SE

            ////Hallway
            //l_areaLimits[1].L_AreaDefiners[0].Point = new Pares<string, SerializedVector3>("NW", new SerializedVector3(-18.720f, -0.210f, 0));
            //l_areaLimits[1].L_AreaDefiners[1].Point = new Pares<string, SerializedVector3>("NE", new SerializedVector3(-5.115f, -0.210f, 0));
            //l_areaLimits[1].L_AreaDefiners[2].Point = new Pares<string, SerializedVector3>("SW", new SerializedVector3(-18.720f, -0.924f, 0)); //SW
            //l_areaLimits[1].L_AreaDefiners[3].Point = new Pares<string, SerializedVector3>("SE", new SerializedVector3(-5.115f, -0.924f, 0)); //SE

            ////Kitchen
            //l_areaLimits[2].L_AreaDefiners[0].Point = new Pares<string, SerializedVector3>("NW", new SerializedVector3(4.253f, 1.105f, 0));
            //l_areaLimits[2].L_AreaDefiners[1].Point = new Pares<string, SerializedVector3>("NE", new SerializedVector3(6.296f, 1.105f, 0));
            //l_areaLimits[2].L_AreaDefiners[2].Point = new Pares<string, SerializedVector3>("SW", new SerializedVector3(4.253f, -0.929f, 0)); //SW
            //l_areaLimits[2].L_AreaDefiners[3].Point = new Pares<string, SerializedVector3>("SE", new SerializedVector3(6.296f, -0.929f, 0)); //SE
            ////-- END LIMITS --

            ////-- FURNITURE AREAS --
            ////Bedroom
            ////Bed
            //l_areaFurnitures[0].L_AreaDefiners[0].Point = new Pares<string, SerializedVector3>("NW", new SerializedVector3(0.395f, 1.080f, 0));
            //l_areaFurnitures[0].L_AreaDefiners[1].Point = new Pares<string, SerializedVector3>("NE", new SerializedVector3(1.366f, 1.080f, 0));
            //l_areaFurnitures[0].L_AreaDefiners[2].Point = new Pares<string, SerializedVector3>("SW", new SerializedVector3(0.395f, -0.520f, 0)); //SW
            //l_areaFurnitures[0].L_AreaDefiners[3].Point = new Pares<string, SerializedVector3>("SE", new SerializedVector3(1.366f, -0.520f, 0)); //SE

            ////Cajonera
            //l_areaFurnitures[1].L_AreaDefiners[0].Point = new Pares<string, SerializedVector3>("NW", new SerializedVector3(-0.980f, 1.085f, 0));
            //l_areaFurnitures[1].L_AreaDefiners[1].Point = new Pares<string, SerializedVector3>("NE", new SerializedVector3(-1.260f, 1.085f, 0));
            //l_areaFurnitures[1].L_AreaDefiners[2].Point = new Pares<string, SerializedVector3>("SW", new SerializedVector3(-0.980f, -0.000f, 0)); //SW
            //l_areaFurnitures[1].L_AreaDefiners[3].Point = new Pares<string, SerializedVector3>("SE", new SerializedVector3(-1.260f, -0.000f, 0)); //SE

            ////Mueble Lampara
            //l_areaFurnitures[6].L_AreaDefiners[0].Point = new Pares<string, SerializedVector3>("NW", new SerializedVector3(-0.299f, 1.045f, 0));
            //l_areaFurnitures[6].L_AreaDefiners[1].Point = new Pares<string, SerializedVector3>("NE", new SerializedVector3(0.104f, 1.045f, 0));
            //l_areaFurnitures[6].L_AreaDefiners[2].Point = new Pares<string, SerializedVector3>("SW", new SerializedVector3(-0.299f, 1.001f, 0)); //SW
            //l_areaFurnitures[6].L_AreaDefiners[3].Point = new Pares<string, SerializedVector3>("SE", new SerializedVector3(0.104f, 1.001f, 0)); //SE
            ////End Bedroom

            ////---|-|--

            ////Hallway
            ////Mesa Florero
            //l_areaFurnitures[2].L_AreaDefiners[0].Point = new Pares<string, SerializedVector3>("NW", new SerializedVector3(-7.800f, -0.115f, 0));
            //l_areaFurnitures[2].L_AreaDefiners[1].Point = new Pares<string, SerializedVector3>("NE", new SerializedVector3(-7.015f, -0.115f, 0));
            //l_areaFurnitures[2].L_AreaDefiners[2].Point = new Pares<string, SerializedVector3>("SW", new SerializedVector3(-7.800f, -0.400f, 0)); //SW
            //l_areaFurnitures[2].L_AreaDefiners[3].Point = new Pares<string, SerializedVector3>("SE", new SerializedVector3(-7.015f, -0.400f, 0)); //SE
            ////End Hallway

            ////---|-|--

            ////Kitchen
            ////Kitchen Furniture
            //l_areaFurnitures[3].L_AreaDefiners[0].Point = new Pares<string, SerializedVector3>("NW", new SerializedVector3(6.420f, 1.034f, 0));
            //l_areaFurnitures[3].L_AreaDefiners[1].Point = new Pares<string, SerializedVector3>("NE", new SerializedVector3(7.000f, 1.034f, 0));
            //l_areaFurnitures[3].L_AreaDefiners[2].Point = new Pares<string, SerializedVector3>("SW", new SerializedVector3(6.420f, -0.929f, 0)); //SW
            //l_areaFurnitures[3].L_AreaDefiners[3].Point = new Pares<string, SerializedVector3>("SE", new SerializedVector3(7.000f, -0.929f, 0)); //SE

            ////Table and chairs
            //l_areaFurnitures[4].L_AreaDefiners[0].Point = new Pares<string, SerializedVector3>("NW", new SerializedVector3(2.000f, 0.737f, 0));
            //l_areaFurnitures[4].L_AreaDefiners[1].Point = new Pares<string, SerializedVector3>("NE", new SerializedVector3(4.800f, 0.737f, 0));
            //l_areaFurnitures[4].L_AreaDefiners[2].Point = new Pares<string, SerializedVector3>("SW", new SerializedVector3(2.000f, -0.929f, 0)); //SW
            //l_areaFurnitures[4].L_AreaDefiners[3].Point = new Pares<string, SerializedVector3>("SE", new SerializedVector3(4.800f, -0.929f, 0)); //SE

            ////Table and chairs2
            //l_areaFurnitures[5].L_AreaDefiners[0].Point = new Pares<string, SerializedVector3>("NW", new SerializedVector3(2.000f, 0.438f, 0));
            //l_areaFurnitures[5].L_AreaDefiners[1].Point = new Pares<string, SerializedVector3>("NE", new SerializedVector3(5.125f, 0.438f, 0));
            //l_areaFurnitures[5].L_AreaDefiners[2].Point = new Pares<string, SerializedVector3>("SW", new SerializedVector3(2.000f, -0.929f, 0)); //SW
            //l_areaFurnitures[5].L_AreaDefiners[3].Point = new Pares<string, SerializedVector3>("SE", new SerializedVector3(5.125f, -0.929f, 0)); //SE
            //-- END FURNITURE AREAS --
        }

        //Convert entities in scene to actual entitys in the project, so they will be able to be serialized and saved in the file
        private void EntitysToObjects()
        {
            try
            {
                TrackingCollection<Entity> tc_entities = Entity.Scene.Entities;
                foreach (Entity item in tc_entities.ToList())
                {
                    if(item.Name == "Player")
                    {
                        continue;
                    }

                    foreach (Type type in Puppet.TypesOfMonsters())
                    {
                        if (item.Name == type.Name)
                        {
                            Controller.controller.playerController.l_entitysCharacters.Add(Controller.controller.playerController.EnemyNPCStart(item.Name, item.Transform.Position, item.Transform.Rotation));
                        }
                    }

                    foreach (Type type in Furniture.TypesOfFurniture())
                    {
                        if (item.Name == type.Name)
                        {
                            l_furniture.Add(Controller.controller.worldController.FurnitureCreate(item.Name, Position: item.Transform.Position, Rotation: item.Transform.Rotation));
                        }
                    }
                }

                if (Controller.controller.worldController.l_planets.Count > 0)
                {
                    if (Controller.controller.worldController.l_furniture.Count > 0)
                    {
                        Controller.controller.worldController.l_planets[0].L_Furnitures.AddRange(Controller.controller.worldController.l_furniture);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error EntitysToObjects(): " + ex.Message);
                return;
            }
        }

        public void WorldController_Tick(List<Pares<List<Entity>, Bullet>> l_bullets)
        {
            if (l_bullets.Count > 0 && this.l_entitysFurnitures.Count > 0)
            {
                this.Damage(l_bullets);
            }
        }

        public void WorldController_Tick()
        {
        }

        #region Each FPS Functions
        //Damage resolution for all the things damaged, (i.e. damage comprobation and aplication)
        public void Damage(List<Pares<List<Entity>, Bullet>> l_bullets)
        {
            try
            {
                if (l_entitysFurnitures.Count <= 0 || l_bullets.Count <= 0)
                {
                    return;
                }

                float Radius = 2f;
                foreach (Pares<Furniture, Entity> enmy in l_entitysFurnitures)
                {
                    foreach (Pares<List<Entity>, Bullet> bllt in l_bullets)
                    {
                        if ((bllt.Item1[0].Transform.Position.Z <= enmy.Item2.Transform.Position.Z + Radius) && (bllt.Item1[0].Transform.Position.Z >= enmy.Item2.Transform.Position.Z - Radius))
                        {
                            if ((bllt.Item1[0].Transform.Position.X <= enmy.Item2.Transform.Position.X + Radius) && (bllt.Item1[0].Transform.Position.X >= enmy.Item2.Transform.Position.X - Radius))
                            {
                                if ((bllt.Item1[0].Transform.Position.Y <= enmy.Item2.Transform.Position.Y + Radius) && (bllt.Item1[0].Transform.Position.Y >= enmy.Item2.Transform.Position.Y - Radius))
                                {
                                    if (enmy.Item1.HP <= 0)
                                    {
                                        Entity.Scene.Entities.Remove(enmy.Item2);
                                        l_entitysFurnitures = l_entitysFurnitures.Where(c => c.Item2.Name != enmy.Item2.Name).ToList();
                                        Log.Info(enmy.Item2.Name + " DIES!");
                                        continue;
                                    }
                                    if (l_entitysFurnitures.Count <= 0)
                                    {
                                        return;
                                    }
                                    l_entitysFurnitures.Where(c => c.Item2.Name == enmy.Item2.Name).First().Item1.HP -= 10;
                                    Log.Info(enmy.Item2.Name + " DAMAGED! Now only have " + l_entitysFurnitures.Where(c => c.Item2.Name == enmy.Item2.Name).First().Item1 + " of HP!!!");
                                }
                            }
                        }
                    }
                }
                l_entitysFurnitures = l_entitysFurnitures.Where(c => c != null).ToList();

            }
            catch (Exception ex)
            {
                Log.Error("Error Damage(): " + ex.Message);
            }
        }
        #endregion

        #region TerrainMethods

        //Create a new Planet (i.e. a collection of worlds in a 3d grid) with the required size.
        public Planet CreatePlanet(int WorldXDimension, int WorldZDimension, int Xdim, int Zdim, int Ydim)
        {
            int iX = 0;
            int iZ = 0;
            int iY = 0;
            Planet plnt = new Planet();
            plnt.L_worlds = new World[Xdim, Zdim, Ydim];
            bool continueLoop = true;
            do
            {
                plnt.L_worlds[iX, iZ, iY] = CreateWorld(WorldXDimension, WorldZDimension, iX, iZ, iY);
                int TempX = Convert.ToInt32(Math.Round(Convert.ToDecimal(WorldXDimension / 2)));
                int TempZ = Convert.ToInt32(Math.Round(Convert.ToDecimal(WorldZDimension / 2)));
                Tile centralTile = plnt.L_worlds[iX, iZ, iY].L_tiles.Where(c => c.X == TempX && c.Z == TempZ).First();

                plnt.L_worlds[iX, iZ, iY].SetPosition(centralTile.Entity.Transform.Position);
                //TODO: Check if it's worth it, and if it is, how to do than the tiles have the world as a parent, it doesn't seem necesary, at least for now.
                //plnt.l_worlds[iX, iZ, iY].Entity.Transform.Position.X = iX;
                //plnt.l_worlds[iX, iZ, iY].Entity.Transform.Position.X = (iX + 7);
                //plnt.l_worlds[iX, iZ, iY].Entity.Transform.Position.Y = iY;
                //plnt.l_worlds[iX, iZ, iY].Entity.Transform.Position.Y = (iY + 7);
                //plnt.l_worlds[iX, iZ, iY].Entity.Transform.Position.Z = iZ;
                //plnt.l_worlds[iX, iZ, iY].Entity.Transform.Position.Z = (iZ + 7);

                iX++;
                if (iX == Xdim)
                {
                    iX = 0;
                    iZ++;
                    if (iZ >= Zdim)
                    {
                        iZ = 0;
                        iY++;
                        if (iY >= Ydim)
                        {
                            continueLoop = false;
                        }
                    }
                }

            }
            while (continueLoop);
            this.l_planets.Add(plnt);
            return plnt;
        }

        //Create a new world (i.e. a plane or tiled floor) with the required size.
        public World CreateWorld(int Xdim, int Zdim, int iXp, int iZp, int iYp)
        {
            int iX = 0;
            int iZ = 0;
            List<Entity> l_entity;
            World wrld = new World(xP: iXp, zP: iZp, yP: iYp);
            wrld.Entity = new Entity();
            bool continueLoop = true;
            //List<Tile> listP = null,float xP = 0, float yP = 0, float zP = 0, string nombreP = "World_X_Y_Z", Entity ent = null
            do
            {
                l_entity = tileSquare.Instantiate();
                Entity.Scene.Entities.AddRange(l_entity);
                Tile tle = new Tile(iX, iZ, "Tile_" + iX + "_" + iZ, ent: l_entity[0]);

                wrld.L_tiles.Add(tle);
                wrld.Entity.SetParent(tle.Entity);
                iX++;

                if (iX == Xdim)
                {
                    iX = 0;
                    iZ++;
                    if (iZ >= Zdim)
                    {
                        continueLoop = false;
                    }
                }

            }
            while (continueLoop);
            return wrld;
        }
        #endregion

        #region Furniture Functions
        public Furniture FurnitureCreate(String TypeOfFurnitureName, Planet planet = null, Vector3 Position = default, Quaternion Rotation = default)
        {
            try
            {
                //Determine if it is a model or a Sprite and add it to the model
                Model model = Content.Load<Model>("Models/" + TypeOfFurnitureName);
                Prefab prefab = Content.Load<Prefab>("Prefabs/" + TypeOfFurnitureName);

                Type typ = Furniture.TypesOfFurniture().Where(c => c.Name == TypeOfFurnitureName).FirstOrDefault();
                if (typ == null)
                {
                    typ = Furniture.TypesOfFurniture().Where(c => c.FullName == TypeOfFurnitureName).FirstOrDefault();
                }

                object obtOfType = Activator.CreateInstance(typ); //Requires parameterless constructor.
                                                                  //TODO: System to determine the type of enemy to make the object, prepare stats and then add it to the list
                int position = 0;
                if (planet != null)
                {
                    position = planet.L_Furnitures.Count();
                    planet.L_Furnitures.Add(((Furniture)obtOfType));
                }
                else
                {
                    if (Controller.controller.worldController.l_planets.Count > 0)
                    {
                        position = Controller.controller.worldController.l_planets[0].L_Furnitures.Count();
                        Controller.controller.worldController.l_planets[0].L_Furnitures.Add(((Furniture)obtOfType));
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine("Warning, the furniture cannot be loaded to any planet because none was found");
                        Console.ResetColor();
                    }
                }

                Entity sprt = new Entity(TypeOfFurnitureName);

                Vector3 pos = Vector3.Zero;
                Quaternion rot = Quaternion.Identity;
                if (Position == default)
                {
                    pos = Position;
                }
                if (Rotation == default)
                {
                    rot = Rotation;
                }

                if (prefab != null)
                {
                    List<Entity> instance = prefab.Instantiate();
                    instance.First().Transform.Position = pos;
                    //Entity.Scene.Entities.AddRange(instance);
                    ((Furniture)obtOfType).Entity = instance[0];
                }
                else if (model == null)
                {
                    SpriteSheet spritesheet = Content.Load<SpriteSheet>("Sprites/" + TypeOfFurnitureName);
                    sprt.GetOrCreate<SpriteComponent>().SpriteProvider = SpriteFromSheet.Create(spritesheet, spritesheet[0].Name);
                    ((Furniture)obtOfType).Entity = sprt;
                }
                else
                {
                    sprt.GetOrCreate<ModelComponent>().Model = model;
                    ((Furniture)obtOfType).Entity = sprt;
                }

                if (typ == default(Type))
                {
                    Console.WriteLine(" Entity FurnitureCreate Error: Type not found, maybie a typo in the name of the type?");
                    return null;
                }

                ((Furniture)obtOfType).Entity.Name = position.ToString() + " " + TypeOfFurnitureName;
                ((Furniture)obtOfType).Entity.Transform.Position = pos;
                ((Furniture)obtOfType).Entity.Transform.Rotation = rot;

                //((Furniture)obtOfType).Entity.Transform.RotationEulerXYZ = new Vector3(0f, -90f, 0f);

                /*Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("l_ent count: " + l_ent.Count());
                foreach (Entity ent in l_ent)
                {
                    Console.WriteLine("Component: "+ent.Name);
                }
                Console.ResetColor();*/

                Entity.Scene.Entities.Add(((Furniture)obtOfType).Entity);

                //AnimacionSprite.RegistrarEntidadEnAnimacionSprite(l_entitysCharacters[position], new TimeSpan(0, 0, 0, 0, 250));

                return ((Furniture)obtOfType);
            }
            catch (Exception ex)
            {
                Log.Error("Error FurnitureCreate(): " + ex.Message);
                return null;
            }
        }
        #endregion

        #region Suplementary Functions
        //Allow to change the next camera in the CameraList
        public Area FindArea(string NameArea)
        {
            try
            {
                foreach (Area ara in l_areaLimits)
                {
                    if (ara.Name.ToUpper() == NameArea.ToUpper())
                    {
                        return ara;
                    }
                }
                return new Area();
            }
            catch (Exception ex)
            {
                Log.Error("Error FindArea(): " + ex.Message);
                return new Area();
            }
        }

        public List<Area> FindAreaFurniture(string NameAreaFurniture)
        {
            try
            {
                List<Area> l_areas = new List<Area>();
                foreach (Area ara in l_areaFurnitures)
                {
                    if (ara.Name.ToUpper() == NameAreaFurniture.ToUpper())
                    {
                        l_areas.Add(ara);
                    }
                }
                return l_areas;
            }
            catch (Exception ex)
            {
                Log.Error("Error FindAreaFurniture(): " + ex.Message);
                return new List<Area>();
            }
        }

        public bool Save()
        {
            //For now, it only save Areas
            try
            {
                EntitysToObjects();

                WorldControllerSave wcs = new WorldControllerSave();
                wcs.L_areaLimits = this.l_areaLimits;
                wcs.L_areaTeleports = this.l_areaTeleports;
                wcs.L_areaFurnitures = this.l_areaFurnitures;
                wcs.L_planets = this.l_planets;
                //wcs.L_puppets = Controller.controller.playerController.l_entitysCharacters;

                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    ReferenceHandler = ReferenceHandler.Preserve,
                    Converters =
                    {
                        new WorldControllerSaveConverter()
                    }
                };

                /*Entity nEnt = new Entity("New Entity");
                nEnt.Transform.Position = new Vector3(3f, 5f, 0f);
                nEnt.Transform.RotationEulerXYZ = new Vector3(3f, 5f, 0f);
                Quaternion aa = nEnt.Transform.Rotation;
                Quaternion b = aa;
                string saveEntity = JsonSerializer.Serialize(nEnt, serializeOptions);
                Entity result = JsonSerializer.Deserialize<Entity>(saveEntity, serializeOptions);*/

                string result = JsonSerializer.Serialize<WorldControllerSave>(wcs, serializeOptions);

                File.WriteAllText("Areas.json", result);
                //Log.Info(File.ReadAllText("Areas.json"));
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Error: WorldController.Save(): " + ex.Message);
                return false;
            }
        }

        public string Load(string fromWhereToLoad = "Areas.json")
        {
            //For now, it only load Areas
            try
            {
                //Prepare result
                string result = File.ReadAllText(fromWhereToLoad);
                //WorldControllerSave wcs = System.Text.Json.JsonSerializer.Deserialize<WorldControllerSave>(result);
                //WorldControllerSave wcs = JsonConvert.DeserializeObject<WorldControllerSave>(result);

                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Converters =
                    {
                        new WorldControllerSaveConverter()
                    }
                };

                WorldControllerSave wcs = JsonSerializer.Deserialize<WorldControllerSave>(result, serializeOptions);

                //Cargar data en el WorldController
                this.l_areaLimits = wcs.L_areaLimits;
                this.l_areaTeleports = wcs.L_areaTeleports;
                this.l_areaFurnitures = wcs.L_areaFurnitures;
                this.l_planets = wcs.L_planets;

                return result;
            }
            catch (Exception ex)
            {
                Log.Error("Error: WorldController.Load(): " + ex.Message);
                return string.Empty;
            }
        }
        #endregion
    }

    public class WorldControllerSave
    {
        private List<Area> l_areaLimits = new List<Area>();
        private List<Area> l_areaTeleports = new List<Area>();
        private List<Area> l_areaFurnitures = new List<Area>();
        private List<Planet> l_planets = new List<Planet>();
        private List<Puppet> l_puppets = new List<Puppet>();

        //[JsonProperty("L_areaLimits")]
        public List<Area> L_areaLimits { get => l_areaLimits; set => l_areaLimits = value; }
        //[JsonProperty("L_areaTeleports")]
        public List<Area> L_areaTeleports { get => l_areaTeleports; set => l_areaTeleports = value; }
        //[JsonProperty("L_areaFurnitures")]
        public List<Area> L_areaFurnitures { get => l_areaFurnitures; set => l_areaFurnitures = value; }

        //[JsonProperty("L_planets")]
        public List<Planet> L_planets { get => l_planets; set => l_planets = value; }
        public List<Puppet> L_puppets { get => l_puppets; set => l_puppets = value; }

        public WorldControllerSave(List<Area> L_areaLimits = null, List<Area> L_areaTeleports = null, List<Area> L_areaFurnitures = null, List<Planet> L_planets = null)
        {
            this.L_areaLimits = L_areaLimits;
            this.L_areaTeleports = L_areaTeleports;
            this.L_areaFurnitures = L_areaFurnitures;
            this.L_planets = L_planets;
        }
    }

    public class WorldControllerSaveConverter : System.Text.Json.Serialization.JsonConverter<WorldControllerSave>
    {
        public override WorldControllerSave Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                string strEntity = reader.GetString();

                JsonSerializerSettings serializeOptions = new JsonSerializerSettings
                {
                    Converters =
                    {
                        new EntityConverterJSON(),
                        new FurnitureConverterJSON(),
                    }
                };

                WorldControllerSave wcs = JsonConvert.DeserializeObject<WorldControllerSave>(strEntity, serializeOptions);
                return wcs;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (WorldControllerSaveConverter) Read(): " + ex.Message);
                return null;
            }
        }

        public override void Write(Utf8JsonWriter writer, WorldControllerSave worldControllerSave, JsonSerializerOptions options)
        {
            try
            {
                string l_teleports = JsonSerializer.Serialize<List<Area>>(worldControllerSave.L_areaTeleports);
                string l_limits = JsonSerializer.Serialize<List<Area>>(worldControllerSave.L_areaLimits);
                string l_furnitures = JsonSerializer.Serialize<List<Area>>(worldControllerSave.L_areaFurnitures);

                string l_planets = "[";
                int i = 0;
                foreach (Planet plnt in worldControllerSave.L_planets)
                {
                    l_planets += plnt.ToJson();
                    if ((worldControllerSave.L_planets.Count() - 1) > i)
                    {
                        l_planets += ",";
                    }
                }
                l_planets += "]";

                string resultJson = "{L_areaTeleports:" + l_teleports + ", " + "L_areaLimits:" + l_limits + ", L_areaFurnitures:" + l_furnitures + ", L_planets:" + l_planets + "}";
                writer.WriteStringValue(resultJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (WorldControllerSaveConverter) Write(): " + ex.Message);
            }
        }
    }

}
