using Stride.Engine;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;
using MMO_Client.Code.Models;
using MMO_Client.Code.Assistants;
using Newtonsoft.Json;

namespace MMO_Client.Models.FurnitureModels
{
    public abstract class Furniture
    {
        public int HP = 0;
        private Entity entity = null;
        public Entity Entity
        {
            get
            {
                if (entity == null)
                {
                    entity = new Entity();
                }
                return entity;
            }
            set => entity = value;
        }

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

                string strResult = JsonConvert.SerializeObject(this, serializeOptions);
                return strResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error String ToJson(): " + ex.Message);
                return string.Empty;
            }
        }

        public static List<Type> TypesOfFurniture()
        {
            List<Type> myTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => type.IsSubclassOf(typeof(Furniture)) && !type.IsAbstract).ToList();
            return myTypes;
        }

        public bool ExecuteFurnitureAction()
        {
            return true;
        }
    }

    public class Chair : Furniture
    {
        //Write here methods or atributes of the specific object
    }

    public class Tree : Furniture
    {
        //Write here methods or atributes of the specific object
    }

    public class Ground : Furniture
    {
        //Write here methods or atributes of the specific object
    }
}
