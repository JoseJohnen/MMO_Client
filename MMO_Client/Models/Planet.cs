using Stride.Engine;
using MMO_Client.Code.Interfaces;
using Stride.Core;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Silk.NET.SDL;
using MMO_Client.Code.Assistants;
using Microsoft.VisualBasic.Logging;
using static Stride.Graphics.GeometricPrimitives.GeometricPrimitive;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Media.Media3D;
using MMO_Client.Models.FurnitureModels;
using System.Text.Json;
using MMO_Client.Assistants;

namespace MMO_Client.Code.Models
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
                JsonSerializerSettings serializeOptions = new JsonSerializerSettings
                {
                    Converters =
                    {
                        new EntityConverterJSON(),
                        //new FurnitureConverterJSON(),
                    }
                };

                string planet = JsonConvert.SerializeObject(this, Formatting.Indented, serializeOptions);
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
