using MMO_Client.Code.Assistants;
using MMO_Client.Code.Models;
using Stride.Core.Mathematics;
using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MMO_Client
{
    [Serializable]
    [Stride.Core.DataContract]
    public struct World
    {
        private List<Tile> l_tiles;

        //For position load/save in the planet object
        private float x;
        private float y;
        private float z;
        private string nombre;
        private Entity entity;

        public World(List<Tile> listP = null,float xP = 0, float yP = 0, float zP = 0, string nombreP = "World_X_Y_Z", Entity ent = null)
        {
            l_tiles = listP != null ? listP : new List<Tile>();
            x = xP;
            y = yP;
            z = zP;
            nombre = nombreP.Equals("World_X_Y_Z") ? "World_" + x + "_" + y + "_" + z : nombreP;
            entity = ent;

            if (ent != null)
            {
                ent.Name = nombreP.Equals("World_X_Y_Z") ? "World_" + x + "_" + y + "_" + z : nombreP;
                ent.Transform.Position.X = x;
                ent.Transform.Position.Z = z;
            }
        }

        public List<Tile> L_tiles
        {
            get
            {
                if (l_tiles == null)
                {
                    l_tiles = new List<Tile>();
                }
                return l_tiles;
            }
            set => l_tiles = value;
        }

        public float X
        {
            get
            {
                if (x == null)
                {
                    x = 0;
                }
                return x;
            }
            set
            {
                try
                {
                    x = value;
                    #region Parte trabajo de X original (Comentada)
                    /*string Nombre = "Tile_X_Y_Z";
                    int firstInstance = (Nombre.IndexOf("_") + 1);
                    string a = Nombre.Substring(firstInstance);
                    int secondInstance = (a.IndexOf("_") + 1);

                    string firstPart = Nombre.Substring(0, Nombre.IndexOf("_") + 1);

                    string c = Nombre.Substring(firstInstance);
                    string lastValueIsolated = c.Substring(((firstInstance + secondInstance) - 1));*/
                    #endregion

                    string firstPart = FirstPart(Nombre);
                    string theRest = TheRest(Nombre).Replace(FirstValue(Nombre)+"_", "_");
                    nombre = firstPart + x + theRest;
                    if (Entity != null)
                    {
                        entity.Transform.Position.X = x;
                        entity.Name = firstPart + x + theRest;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        //Make changes in the position from a vector3 Without notifing the name to change.
        internal void SetPosition(Vector3 position)
        {
            this.x = position.X;
            this.y = position.Y;
            this.z = position.Z;
            this.entity.Transform.Position = position;
        }

        public float Y
        {
            get
            {
                if (y == null)
                {
                    y = 0;
                }
                return y;
            }
            set
            {
                try
                {
                    y = value;
                    string firstPart = FirstValue(Nombre);
                    string theRest = TheRest(Nombre).Replace(FirstValue(Nombre), "");
                    nombre = TheFirstHalfFromMiddle(Nombre.ToString()) + y + TheLastHalfFromMiddle(Nombre.ToString());
                    if (Entity != null)
                    {
                        entity.Transform.Position.Y = y;
                        entity.Name = TheFirstHalfFromMiddle(Nombre.ToString()) + y + TheLastHalfFromMiddle(Nombre.ToString());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public float Z
        {
            get
            {
                if (z == null)
                {
                    z = 0;
                }
                return z;
            }
            set
            {
                z = value;
                try
                {
                    #region Parte trabajo de Z original (Comentada)
                    /*int firstInstance = (Nombre.IndexOf("_") + 1);

                    string firstPart = Nombre.Substring(0, Nombre.IndexOf("_") + 1);

                    string c = Nombre.Substring(firstInstance);
                    string firstValueIsolated = c.Substring(0, c.IndexOf("_"));*/
                    #endregion

                    string firstPart = FirstPart(Nombre);
                    string theRest = TheRest(Nombre).Replace(LastValue(Nombre), "");
                    nombre = firstPart + theRest + z;
                    if (Entity != null)
                    {
                        entity.Transform.Position.Z = z;
                        entity.Name = firstPart + theRest + z;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public string Nombre
        {
            get
            {
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    nombre = String.Empty;
                }
                return nombre;
            }
            set
            {
                nombre = value;
            }
        }

        public Entity Entity { get => entity; set => entity = value; }

        public string ToJson()
        {
            try
            {
                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Converters =
                    {
                        new EntityConverter()
                    }
                };

                string result = JsonSerializer.Serialize<World>(this, serializeOptions);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return string.Empty;
            }
        }

        private string FirstPart(string strName)
        {
            string strFirstPart = strName.Substring(0, strName.IndexOf("_") + 1);
            return strFirstPart;
        }

        private string TheRest(string strName)
        {
            string b = strName.Substring(strName.IndexOf("_") + 1);
            return b;
        }

        private string TheFirstHalfFromMiddle(string strName)
        {
            string a = MiddleValue(strName);
            string theFirstHalf = FirstPart(strName) + TheRest(strName).Substring(0, TheRest(strName).IndexOf(a));
            return theFirstHalf;
        }

        private string TheLastHalfFromMiddle(string strName)
        {
            string a = TheRest(TheRest(strName)).Replace(MiddleValue(strName), "");
            return a;

        }

        private string FirstValue(string strName)
        {
            int firstInstance = (strName.IndexOf("_") + 1);
            string c = strName.Substring(firstInstance);
            string firstValueIsolated = c.Substring(0, c.IndexOf("_"));
            return firstValueIsolated;
        }

        private string MiddleValue(string strName)
        {
            int firstInstance = (strName.IndexOf("_") + 1);
            string a = strName.Substring(firstInstance);
            int secondInstance = (a.IndexOf("_") + 1);
            string MiddleValueIsolated = strName.Substring((firstInstance + secondInstance));
            MiddleValueIsolated = MiddleValueIsolated.Substring(0, MiddleValueIsolated.IndexOf("_"));
            return MiddleValueIsolated;
        }

        private string LastValue(string strName)
        {
            int firstInstance = (strName.IndexOf("_") + 1);
            string a = strName.Substring(firstInstance);
            int secondInstance = (a.IndexOf("_") + 1);
            string lastValueIsolated = strName.Substring((firstInstance + secondInstance));
            lastValueIsolated = lastValueIsolated.Substring(lastValueIsolated.IndexOf("_") + 1);
            return lastValueIsolated;
        }
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

    /*public class WorldConverter : JsonConverter<World>
    {
        //For position load/save in the planet object

        public override World Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                string strEntity = reader.GetString();

                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Converters =
                    {
                        new EntityConverter()
                    }
                };

                World nwWorld = JsonSerializer.Deserialize<World>(strEntity, serializeOptions);
                return nwWorld;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (WorldConvert) Read(): " + ex.Message);
                return new World();
            }
        }

        public override void Write(Utf8JsonWriter writer, World world, JsonSerializerOptions options)
        {
            try
            {
                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    ReferenceHandler = ReferenceHandler.Preserve,
                    Converters =
                    {
                        new EntityConverter()
                    }
                };

                string resultJson = world.ToJson();
                writer.WriteStringValue(resultJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (WorldConvert) Write(): " + ex.Message);
            }
        }
    }*/
}
