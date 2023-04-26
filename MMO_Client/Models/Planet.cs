using System;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using MMO_Client.Models.FurnitureModels;
using System.Text.Json;
using MMO_Client.Assistants;

namespace MMO_Client.Models
{
    public class Planet
    {
        //private World[,,] l_worlds; //Ordered by X, Y and Z of the world
        //public World[,,] L_worlds { get => l_worlds; set => l_worlds = value; }

        public List<Furniture> L_Furnitures = new List<Furniture>();

        public string ToJson()
        {
            try
            {
                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new EntityConverter(),
                        //new FurnitureConverterJSON(),
                    }
                };

                string planet = JsonSerializer.Serialize(this, serializeOptions);//Formatting.Indented, serializeOptions);
                return planet;
            }
            catch (Exception Ex)
            {
                Console.WriteLine("Error: Planet ToJson(): " + Ex.Message);
                return string.Empty;
            }
        }
    }
}
