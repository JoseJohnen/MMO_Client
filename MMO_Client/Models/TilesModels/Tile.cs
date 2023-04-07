using Interfaz.Models;
using Interfaz.Utilities;
using MMO_Client.Code.Assistants;
using MMO_Client.Code.Controllers;
using Stride.Engine;
using Stride.Graphics;
using Stride.Rendering.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using UtilityAssistant = Interfaz.Utilities.UtilityAssistant;
using Quaternion = Stride.Core.Mathematics.Quaternion;
using System.Xml.Linq;
using MMO_Client.Controllers;
using MMO_Client.Code.Models;

namespace MMO_Client.Models.TilesModels
{
    public abstract class Tile : Interfaz.Models.Tile
    {
        public virtual new Area Area { get => area; set => area = value; }

        private Area area = new Area(new List<AreaDefiner>() {
            new AreaDefiner(),
            new AreaDefiner(),
            new AreaDefiner(),
            new AreaDefiner(),
        });

        public override Vector3 Position
        {
            get => base.Position;
            set
            {
                base.Position = value;
                if (entity != null)
                {
                    entity.Transform.Position = MMO_Client.Code.Assistants.UtilityAssistant.ConvertVector3NumericToStride(value);
                }
            }
        }

        public override string Name
        {
            get => base.Name;
            set
            {
                base.Name = value;
                if (entity != null)
                {
                    entity.Name = value;
                }

            }
        }

        private Entity entity = null;
        public Entity Entity
        {
            get
            {
                if (entity == null)
                {
                    if (string.IsNullOrWhiteSpace(base.Name))
                    {
                        return null;
                    }
                    SceneInstance sceneInstance = WorldController.game.SceneSystem.SceneInstance;
                    this.entity = sceneInstance.RootScene.Entities.Where(c => c.Name == base.Name).FirstOrDefault();
                }
                return entity;
            }
            set => entity = value;
        }

        public Tile()
        {

        }

        public Tile(string name = "", Vector3 position = new(), Vector3 inworldpos = new()) : base(name, position, inworldpos)
        {
            Name = name;
            this.entity = null;
            base.Position = position;
            InWorldPos = inworldpos;
        }

        #region Auxiliares
        //public virtual void GetEntityByName(Game game, string name)
        //{
        //    Get the scene instance
        //    var sceneInstance = new Entity().Sc game.SceneSystem.SceneInstance;

        //    Search for the entity by name

        //   var entity = sceneInstance.RootScene.Entities.Find(name);

        //    Use the entity reference as needed
        //    if (entity != null)
        //    {
        //        Do something with the entity
        //    }
        //}

        public virtual new string ToJson()
        {
            try
            {
                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new TileConverter(),
                        new EntityConverter(),
                    }
                };

                string strResult = JsonSerializer.Serialize(this, serializeOptions);
                return strResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Puppet) String ToJson(): " + ex.Message);
                return string.Empty;
            }
        }

        public virtual new Tile FromJson(string Text)
        {
            string txt = Text;
            try
            {
                txt = UtilityAssistant.CleanJSON(txt.Replace("\u002B", "+"));

                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new TileConverter(),
                        new EntityConverter(),
                    }
                };

                Tile strResult = JsonSerializer.Deserialize<Tile>(txt, serializeOptions);

                //TODO: VER QUE EL OBJETO AL HACER TO JSON SALVE EL NOMBRE DE LA CLASE TAMBIÉN
                //TODO2: RECUERDA QUE DEBES EXTRAER EL OBJETO

                if (strResult != null)
                {
                    this.Name = strResult.Name;
                    this.Position = strResult.Position;
                    this.InWorldPos = strResult.InWorldPos;
                }
                return strResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Tile) FromJson: " + ex.Message + " Text: " + txt);
                return null;
            }
        }

        public static new Tile CreateFromJson(string json)
        {
            try
            {
                string clase = UtilityAssistant.CleanJSON(json);
                clase = UtilityAssistant.ExtractAIInstructionData(clase, "Class").Replace("\"", "");

                Type typ = Tile.TypesOfTiles().Where(c => c.Name == clase).FirstOrDefault();
                if (typ == null)
                {
                    typ = Tile.TypesOfTiles().Where(c => c.FullName == clase).FirstOrDefault();
                }

                object obtOfType = Activator.CreateInstance(typ); //Requires parameterless constructor.
                                                                  //TODO: System to determine the type of enemy to make the object, prepare stats and then add it to the list

                Tile prgObj = ((Tile)obtOfType);
                return prgObj.FromJson(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Tile) CreateFromJson(): " + ex.Message);
                return null;
            }
        }

        public static new List<Type> TypesOfTiles()
        {
            List<Type> myTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => type.IsSubclassOf(typeof(Tile)) && !type.IsAbstract).ToList();
            return myTypes;
        }

        public virtual void InstanceTile(string name = "", Vector3 position = default(Vector3), Vector3 inworldpos = default(Vector3))
        {
            try
            {
                Vector3 Pos = Vector3.Zero;
                if (position != default(Vector3))
                {
                    Pos = position;
                }

                if (inworldpos != default(Vector3))
                {
                    InWorldPos = inworldpos;
                }

                if (!string.IsNullOrEmpty(name))
                {
                    this.Name = name;
                }

                SpriteSheet spritesheet = null;
                string nameSprite = string.Empty;
                foreach (SpriteSheet spSht in Controller.controller.l_Tileset)
                {
                    foreach (Sprite sprite in spSht.Sprites)
                    {
                        if (sprite.Name == this.GetType().Name)
                        {
                            spritesheet = spSht;
                            nameSprite = sprite.Name;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(nameSprite))
                {
                    if (entity == null)
                    {
                        this.entity = new Entity(this.Name);
                        Controller.controller.Entity.Scene.Entities.Add(this.entity);
                        //SceneInstance sceneInstance = WorldController.game.SceneSystem.SceneInstance;
                        //this.entity = sceneInstance.RootScene.Entities.Where(c => c.Name == base.Name).FirstOrDefault();
                        entity.GetOrCreate<SpriteComponent>().SpriteProvider = SpriteFromSheet.Create(spritesheet, nameSprite);
                    }

                    this.Position = Pos;

                    // Get the size of the sprite
                    spriteSize = MMO_Client.Code.Assistants.UtilityAssistant.ConvertVector2StrideToNumeric(entity.GetOrCreate<SpriteComponent>().CurrentSprite.Size);

                    // Calculate the corners of the sprite
                    Vector3 topLeft = MMO_Client.Code.Assistants.UtilityAssistant.ConvertVector3StrideToNumeric(entity.Transform.WorldMatrix.TranslationVector) + new Vector3(-spriteSize.X / 2, spriteSize.Y / 2, 0);
                    Vector3 topRight = MMO_Client.Code.Assistants.UtilityAssistant.ConvertVector3StrideToNumeric(entity.Transform.WorldMatrix.TranslationVector) + new Vector3(spriteSize.X / 2, spriteSize.Y / 2, 0);
                    Vector3 bottomLeft = MMO_Client.Code.Assistants.UtilityAssistant.ConvertVector3StrideToNumeric(entity.Transform.WorldMatrix.TranslationVector) + new Vector3(-spriteSize.X / 2, -spriteSize.Y / 2, 0);
                    Vector3 bottomRight = MMO_Client.Code.Assistants.UtilityAssistant.ConvertVector3StrideToNumeric(entity.Transform.WorldMatrix.TranslationVector) + new Vector3(spriteSize.X / 2, -spriteSize.Y / 2, 0);
                    this.Area.L_AreaDefiners[0].Point = new Pares<string, Code.Models.SerializedVector3>() { Item1 = "NW", Item2 = new Code.Models.SerializedVector3(topLeft) };
                    this.Area.L_AreaDefiners[1].Point = new Pares<string, Code.Models.SerializedVector3>() { Item1 = "NE", Item2 = new Code.Models.SerializedVector3(topRight) };
                    this.Area.L_AreaDefiners[2].Point = new Pares<string, Code.Models.SerializedVector3>() { Item1 = "SW", Item2 = new Code.Models.SerializedVector3(bottomLeft) };
                    this.Area.L_AreaDefiners[3].Point = new Pares<string, Code.Models.SerializedVector3>() { Item1 = "SE", Item2 = new Code.Models.SerializedVector3(bottomRight) };

                    //SceneInstance sceneInstance = WorldController.game.SceneSystem.SceneInstance;
                    //this.entity = sceneInstance.RootScene.Entities.Where(c => c.Name == base.Name).FirstOrDefault();

                    //Correct system rotation
                    //Entity.Transform.Rotation *= Quaternion.RotationX(Convert.ToSingle(MMO_Client.Code.Assistants.UtilityAssistant.ConvertDegreesToRadiants(90)));

                    //More precise rotation
                    entity.Transform.Rotation *= MMO_Client.Code.Assistants.UtilityAssistant.ConvertSystemNumericsToStrideQuaternion(System.Numerics.Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathF.PI / 2));

                    //Entity.Transform.Position = Code.Assistants.UtilityAssistant.ConvertVector3NumericToStride(Pos);
                    return;
                }
                Console.WriteLine("Error (MMO_Client.Models.TilesModels.Tile) InstanceTile: SPRITE NO ENCONTRADO PARA CLASE " + this.GetType().FullName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (MMO_Client.Models.TilesModels.Tile) InstanceTile: " + ex.Message);
            }
        }
        #endregion

    }

    public class TileConverter : System.Text.Json.Serialization.JsonConverter<Tile>
    {
        public override Tile Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string strJson = string.Empty;
            try
            {
                //TODO: Corregir, testear y terminar
                JsonDocument jsonDoc = JsonDocument.ParseValue(ref reader);
                strJson = jsonDoc.RootElement.GetRawText();
                //strJson = reader.GetString();

                string clase = UtilityAssistant.CleanJSON(strJson);
                clase = UtilityAssistant.ExtractValue(clase, "Class").Replace("\"", "");

                Type typ = Tile.TypesOfTiles().Where(c => c.Name == clase).FirstOrDefault();
                if (typ == null)
                {
                    typ = Tile.TypesOfTiles().Where(c => c.FullName == clase).FirstOrDefault();
                }

                object obtOfType = Activator.CreateInstance(typ); //Requires parameterless constructor.
                                                                  //TODO: System to determine the type of enemy to make the object, prepare stats and then add it to the list

                Tile prgObj = ((Tile)obtOfType);

                string pst = UtilityAssistant.ExtractValue(strJson, "Pos");
                prgObj.Position = UtilityAssistant.Vector3Deserializer(pst);
                pst = UtilityAssistant.ExtractValue(strJson, "InWorldPos");
                prgObj.InWorldPos = UtilityAssistant.Vector3Deserializer(pst);
                prgObj.Name = UtilityAssistant.ExtractValue(strJson, "Name");

                return prgObj;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (TileConverter) Read(): {0} Message: {1}", strJson, ex.Message);
                return default;
            }
        }

        public override void Write(Utf8JsonWriter writer, Tile tle, JsonSerializerOptions options)
        {
            try
            {
                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new Vector3Converter()
                        ,new NullConverter()
                    },
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true,
                    IgnoreNullValues = true
                };


                //Para deserealizar los vector3 serializados: UtilityAssistant.Vector3Deserializer(tle);

                //TODO: Corregir, testear y terminar
                string Name = string.IsNullOrWhiteSpace(tle.Name) ? "null" : tle.Name;
                string Position = System.Text.Json.JsonSerializer.Serialize(tle.Position, serializeOptions);
                string InWorldPos = System.Text.Json.JsonSerializer.Serialize(tle.InWorldPos, serializeOptions);
                string Class = tle.GetType().Name;

                char[] a = { '"' };

                string wr = string.Concat("{ ", new string(a), "Name", new string(a), ":", new string(a), Name, new string(a),
                    ", ", new string(a), "Class", new string(a), ":", new string(a), Class, new string(a),
                    ", ", new string(a), "Pos", new string(a), ":", Position,
                    ", ", new string(a), "InWorldPos", new string(a), ":", InWorldPos,
                    "}");

                string resultJson = Regex.Replace(wr, "(\"(?:[^\"\\\\]|\\\\.)*\")|\\s+", "$1");
                //string resultJson = "{Id:" + Id + ", LN:" + LauncherName + ", Type:" + Type + ", OrPos:" + LauncherPos + ", WPos:" + WeaponPos + ", Mdf:" + Moddif + "}";
                writer.WriteStringValue(resultJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (TileConverter) Write(): " + ex.Message);
            }
        }
    }

}