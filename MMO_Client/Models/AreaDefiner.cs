using Stride.Core.Mathematics;
using Stride.Engine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace MMO_Client.Code.Models
{
    [Serializable]
    [Stride.Core.DataContract]
    public class AreaDefiner
    {
        public string NombreArea { get => nombreArea; set => nombreArea = value; }
        private Pares<string, SerializedVector3> point = new Pares<string, SerializedVector3>(string.Empty, new SerializedVector3());
        private string nombreArea;

        public Pares<string, SerializedVector3> Point
        {
            get
            {
                if (point == null)
                {
                    point = new Pares<string, SerializedVector3>(string.Empty, new SerializedVector3());
                }
                return point;
            }
            set
            {
                if (point == null)
                {
                    point = new Pares<string, SerializedVector3>(string.Empty, new SerializedVector3());
                }
                point = value;
            }
        }

        [JsonConstructor]
        public AreaDefiner(string name = "")
        {
            NombreArea = name;
            Point = new Pares<string, SerializedVector3>(string.Empty, new SerializedVector3());
        }

        public AreaDefiner(Pares<string, SerializedVector3> point, string name = "")
        {
            NombreArea = name;
            Point = point;
        }

        public AreaDefiner()
        {

        }

        public static bool isBreakingInside(List<AreaDefiner> l_areaDefiners, Entity entity)
        {
            try
            {
                bool result = true;
                foreach (AreaDefiner arDef in l_areaDefiners)
                {
                    result = AreaDefiner.isBreakingInside(arDef, entity);
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error isBreakingInside(List<AreaDefiner> l_areaDefiners, List<Entity> l_entitys): " + ex.Message);
                return false;
            }
        }

        public static bool isBreakingInside(AreaDefiner areaDefiner, Entity entity)
        {
            try
            {
                if (entity.Transform.Position.Y > areaDefiner.Point.Item2.Y) //South Limit
                {
                    if (entity.Transform.Position.Y <= areaDefiner.Point.Item2.Y) //North Limit
                    {
                        if (entity.Transform.Position.X > areaDefiner.Point.Item2.X) //East Limit
                        {
                            if (entity.Transform.Position.X <= areaDefiner.Point.Item2.X) //West Limit
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error isBreakingInside(AreaDefiner areaDefiner, Entity entity): " + ex.Message);
                return false;
            }
        }

        public static bool isBreakingOutside(List<AreaDefiner> l_areaDefiners, Entity entity)
        {
            try
            {
                bool result = true;
                foreach (AreaDefiner arDef in l_areaDefiners)
                {
                    result = AreaDefiner.isBreakingOutside(arDef, entity);
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error isBreakingOutside(List<AreaDefiner> l_areaDefiners, List<Entity> l_entitys): " + ex.Message);
                return false;
            }
        }

        public static bool isBreakingOutside(AreaDefiner areaDefiner, Entity entity)
        {
            try
            {
                if (entity.Transform.Position.Y <= areaDefiner.Point.Item2.Y) //South Limit
                {
                    if (entity.Transform.Position.Y > areaDefiner.Point.Item2.Y) //North Limit
                    {
                        if (entity.Transform.Position.X <= areaDefiner.Point.Item2.X) //East Limit
                        {
                            if (entity.Transform.Position.X > areaDefiner.Point.Item2.X) //West Limit
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error isBreakingOutside(AreaDefiner areaDefiner, Entity entity): " + ex.Message);
                return false;
            }
        }

        #region ForEach Compatibility
        /*public IEnumerator GetEnumerator()
        {
            return (IEnumerator)this;
        }*/

        //System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }
}
