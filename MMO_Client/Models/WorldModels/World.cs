using Interfaz.Models.Puppets;
using Interfaz.Utilities;
using MMO_Client.Controllers;
using MMO_Client.Models.TilesModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace MMO_Client.Models.WorldModels
{
    public abstract class World : Interfaz.Models.Worlds.World
    {
        public new ConcurrentDictionary<string, Tile> dic_worldTiles = new ConcurrentDictionary<string, Tile>();

        public World(int westEast = 3, int height = 1, int frontBack = 3, string name = "")
        {
            WestEast = westEast;
            Height = height;
            FrontBack = frontBack;
            Name = name;
        }

        public virtual new World RegisterWorld(string nameOfTheWorld = "")
        {
            try
            {
                string name = "World_" + WorldController.dic_worlds.Count;
                if (nameOfTheWorld != "")
                {
                    name = nameOfTheWorld;
                }
                WorldController.dic_worlds.TryAdd(name, this);
                return this;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error RegisterWorld(string): " + ex.Message);
                return null;
            }
        }

        //public virtual World FillWorld()
        //{
        //    try
        //    {
        //        float x = 0, y = 0, z = 0;
        //        do
        //        {
        //            //world.worldTiles[x, y, z].Entity.Transform.parent = world.Instance.Transform;
        //            //worldTiles[x, y, z] = new Tile_Primus("Tile_"+this.Name+"_"+x+"_"+y+"_"+z);
        //            dic_worldTiles.TryAdd("Tile_" + x + "_" + y + "_" + z, new Grass("Tile_" + x + "_" + y + "_" + z));
        //            if (x == WestEast - 1)
        //            {
        //                x = 0;
        //                y++;

        //                if (y == Height)
        //                {
        //                    y = 0;
        //                    z++;

        //                    if (z == FrontBack)
        //                    {
        //                        break;
        //                    }
        //                }
        //            }
        //            else if (x < WestEast)
        //            {
        //                x++;
        //            }
        //        }
        //        while (x <= WestEast - 1 && y <= Height - 1 && z <= FrontBack - 1);
        //        return this;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Error FillWorld: " + ex.Message);
        //        return null;
        //    }
        //}

        public static List<Type> TypesOfWorlds()
        {
            List<Type> myTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => type.IsSubclassOf(typeof(World)) && !type.IsAbstract).ToList();
            return myTypes;
        }

        #region Métodos JSON
        public virtual string ToJson()
        {
            try
            {

                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new WorldConverter()
                    },
                };
                //ReadCommentHandling = JsonCommentHandling.Skip,
                //    AllowTrailingCommas = true,

                return JsonSerializer.Serialize(this, serializeOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (World) ToJson(): " + ex.Message);
                return string.Empty;
            }
        }

        public virtual World FromJson(string json)
        {
            string txt = json;
            try
            {
                txt = Interfaz.Utilities.UtilityAssistant.CleanJSON(txt.Replace("\u002B", "+"));

                //json = UtilityAssistant.CleanJSON(json);

                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new WorldConverter()
                    },
                };

                //AllowTrailingCommas = true,
                //ReadCommentHandling = JsonCommentHandling.Skip,
                //json = UtilityAssistant.CleanJSON(json);
                World wrldObj = JsonSerializer.Deserialize<World>(txt, serializeOptions);//, serializeOptions);
                //this = prgObj;

                if (wrldObj != null)
                {
                    this.WestEast = wrldObj.WestEast;
                    this.Height = wrldObj.Height;
                    this.FrontBack = wrldObj.FrontBack;
                    this.dic_worldTiles = wrldObj.dic_worldTiles;
                }

                return wrldObj;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\nError (World) FromJson(): " + json + " ex.Message: " + ex.Message);
                return null;
            }
        }

        public static World CreateFromJson(string json)
        {
            try
            {
                string clase = UtilityAssistant.CleanJSON(json);
                clase = UtilityAssistant.ExtractAIInstructionData(clase, "Class").Replace("\"", "");

                Type typ = World.TypesOfWorlds().Where(c => c.Name == clase).FirstOrDefault();
                if (typ == null)
                {
                    typ = World.TypesOfWorlds().Where(c => c.FullName == clase).FirstOrDefault();
                }

                object obtOfType = Activator.CreateInstance(typ); //Requires parameterless constructor.
                                                                  //TODO: System to determine the type of enemy to make the object, prepare stats and then add it to the list

                World prgObj = ((World)obtOfType);
                return prgObj.FromJson(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (World) CreateFromJson(): " + ex.Message);
                return null;
            }
        }
        #endregion
    }

    public class WorldConverter : System.Text.Json.Serialization.JsonConverter<World>
    {
        public override World Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string[] strJsonArray = new string[1];
            string[] strStrArr = new string[1];
            //string[] strStrArr2 = new string[1];
            //string[] strStrArr3 = new string[1];
            //string readerReceiver = string.Empty;
            try
            {
                //TODO: Corregir, testear y terminar
                //readerReceiver = reader.GetString();
                JsonDocument jsonDoc = JsonDocument.ParseValue(ref reader);
                string tempString = jsonDoc.RootElement.GetRawText();

                string clase = UtilityAssistant.CleanJSON(tempString);
                clase = UtilityAssistant.ExtractValue(clase, "Class").Replace("\"", "");

                Type typ = World.TypesOfWorlds().Where(c => c.Name == clase).FirstOrDefault();
                if (typ == null)
                {
                    typ = World.TypesOfWorlds().Where(c => c.FullName == clase).FirstOrDefault();
                }

                object obtOfType = Activator.CreateInstance(typ); //Requires parameterless constructor.
                                                                  //TODO: System to determine the type of enemy to make the object, prepare stats and then add it to the list

                World wrldObj = ((World)obtOfType);

                string strValue = UtilityAssistant.ExtractValue(tempString, "WestEast");
                wrldObj.WestEast = Convert.ToInt32(strValue);
                strValue = UtilityAssistant.ExtractValue(tempString, "Height");
                wrldObj.Height = Convert.ToInt32(strValue);
                strValue = UtilityAssistant.ExtractValue(tempString, "FrontBack");
                wrldObj.FrontBack = Convert.ToInt32(strValue);
                wrldObj.Name = UtilityAssistant.ExtractValue(tempString, "Name");

                /*if (string.IsNullOrWhiteSpace(readerReceiver) || readerReceiver.Equals("\"{\""))
                {
                    return null;
                }
                
                strJsonArray = tempString.Split("],");
                if (strJsonArray.Length > 1)
                {
                    strJsonArray[0] += "]";
                    strJsonArray[1] += "]";
                }*/

                strJsonArray[0] = tempString;

                string strTemp = strJsonArray[0].Substring(strJsonArray[0].IndexOf("dic_worldTiles")).Replace("dic_worldTiles", "");
                Tile tile = null;
                List<string> l_string = new List<string>(strTemp.Split("},{", StringSplitOptions.RemoveEmptyEntries));
                foreach (string item in l_string)
                {
                    //strTemp = UtilityAssistant.ExtractValue(item, "Value");
                    strTemp = item.Substring(item.IndexOf("\"Value\""));
                    strTemp = strTemp.Replace("\"Value\":", "").Replace("}}]}", "}");
                    tile = Tile.CreateFromJson(strTemp);
                    wrldObj.dic_worldTiles.TryAdd(tile.Name, tile);
                }
                //strTemp = strTemp.Substring(4).Replace("[", "").Replace("]", "").Replace("}}", "}");

                /*string str_tiles_to_create = string.Empty;
                if(!strTemp.Equals("}"))
                {
                    str_tiles_to_create = strTemp;
                }

                if (!string.IsNullOrWhiteSpace(str_tiles_to_create))
                {
                    //Array.Clear(strStrArr, 0, strStrArr.Length);
                    str_tiles_to_create = str_tiles_to_create.Replace("},{", "}|°|{");
                    strStrArr = str_tiles_to_create.Split("|°|");
                    foreach (string item1 in strStrArr)
                    {
                        //wwrldObj.dic_worldTiles.TryAdd(item1);
                    }

                    //wrldObj.LoadShots();
                }*/

                return wrldObj;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (WorldConverter) Read(): {0} Message: {1}", strJsonArray[0], ex.Message);
                return null;
            }
        }

        public override void Write(Utf8JsonWriter writer, World wldObj, JsonSerializerOptions options)
        {
            try
            {
                string strTemp = string.Empty;//"{";
                int i = 0;
                int last = 0;
                strTemp += "\"dic_worldTiles\" : [";
                last = wldObj.dic_worldTiles.Count;
                foreach (KeyValuePair<string, Tile> item in wldObj.dic_worldTiles)
                {
                    strTemp += "{\"Key\":\"" + item.Key + "\",\"Value\":\"" + item.Value.ToJson() + "\"}";
                    if (i < last - 1)
                    {
                        strTemp += ",";
                    }
                    i++;
                }
                strTemp += "]"; //,";
                //strTemp += "}";

                //strTemp = UtilityAssistant.CleanJSON(strTemp);

                while (strTemp.Contains("\"\""))
                {
                    strTemp = strTemp.Replace("\"\"", "\"");
                }

                while (strTemp.Contains("\\"))
                {
                    strTemp = strTemp.Replace("\\", "");
                }

                string WestEast = wldObj.WestEast.ToString();
                string Height = wldObj.Height.ToString();
                string FrontBack = wldObj.FrontBack.ToString();
                string Name = string.IsNullOrWhiteSpace(wldObj.Name) ? "null" : wldObj.Name;

                string Class = wldObj.GetType().Name;

                char[] a = { '"' };

                string wr = string.Concat("{", new string(a), "Name", new string(a), ":", new string(a), Name, new string(a),
                    ", ", new string(a), "Class", new string(a), ":", new string(a), Class, new string(a),
                    ", ", new string(a), "WestEast", new string(a), ":", WestEast,
                    ", ", new string(a), "Height", new string(a), ":", Height,
                    ", ", new string(a), "FrontBack", new string(a), ":", FrontBack,
                    ", ", strTemp,
                    "}");

                string resultJson = Regex.Replace(wr, "(\"(?:[^\"\\\\]|\\\\.)*\")|\\s+", "$1");

                writer.WriteStringValue(wr);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (WorldConverter) Write(): " + ex.Message);
            }
        }
    }
}