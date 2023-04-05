using System;
using MMO_Client.Code.Assistants;
using System.Text.Json;
//using Newtonsoft.Json;
using Stride.Engine;

namespace MMO_Client
{
    [Serializable]
    [Stride.Core.DataContract]
    public class Tile : Interfaz.Models.Tile
    {
        private float x;
        private float z;
        private string nombre;

        //[JsonIgnore]
        private Entity entity;

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
                    string firstPart = FirstPart(Nombre);
                    string theRest = TheRest(Nombre);

                    nombre = firstPart + x + theRest;
                    if (Entity != null)
                    {
                        entity.Transform.Position.X = x;
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
                    string firstPart = FirstPart(Nombre);
                    string theRest = TheRest(Nombre).Replace(LastValue(Nombre), "");

                    nombre = firstPart + theRest + z;
                    if (Entity != null)
                    {
                        entity.Transform.Position.Z = z;
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
                    nombre = "Tile_X_Z";
                }
                return nombre;
            }
            set
            {
                nombre = value;
                if (Entity != null)
                {
                    entity.Name = nombre;
                }
            }
        }
        public Entity Entity { get => entity; set => entity = value; }

        public Tile(int xP = 0, int zP = 0, string nombreP = "Tile_X_Z", Entity ent = null)
        {
            x = xP;
            z = zP;
            nombre = nombreP.Equals("Tile_X_Z") ? "Tile_" + x + "_" + z : nombreP;
            entity = ent;
            if(ent != null)
            {
                ent.Name = nombreP.Equals("Tile_X_Z") ? "Tile_" + x + "_" + z : nombreP;
                ent.Transform.Position.X = x;
                ent.Transform.Position.Z = z;
            } 
        }

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

                string result = JsonSerializer.Serialize<Tile>(this, serializeOptions);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return string.Empty;
            }
        }

        /*private string FirstPart(string strName)
        {
            string strFirstPart = strName.Substring(0, strName.IndexOf("_") + 1);
            return strFirstPart;
        }

        private string TheRest(string strName)
        {
            string b = strName.Substring(strName.IndexOf("_") + 1);
            return b;
        }

        private string FirstValue(string strName)
        {
            int firstInstance = (strName.IndexOf("_") + 1);
            string c = strName.Substring(firstInstance);
            string firstValueIsolated = c.Substring(0, c.IndexOf("_"));
            return firstValueIsolated;
        }

        private string LastValue(string strName)
        {
            int firstInstance = (strName.IndexOf("_") + 1);
            string a = strName.Substring(firstInstance);
            int secondInstance = (a.IndexOf("_") + 1);
            string lastValueIsolated = strName.Substring((firstInstance + secondInstance));
            lastValueIsolated = lastValueIsolated.Substring(lastValueIsolated.IndexOf("_") + 1);
            return lastValueIsolated;
        }*/
    }
}
