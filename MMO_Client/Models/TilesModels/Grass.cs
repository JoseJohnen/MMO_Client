using Stride.Engine;
using System.Numerics;

namespace MMO_Client.Models.TilesModels
{
    public class Grass : MMO_Client.Models.TilesModels.Tile
    {
        public override string Name 
        { 
            get 
            { 
                return base.Name; 
            } 
            set 
            { 
                base.Name = value; 
                if(Entity != null)
                {
                    Entity.Name = value;
                }
            } 
        }

        public Grass(string name = "", Vector3 position = default, Vector3 inworldpos = default) : base(name, position, inworldpos)
        {
            Entity = new Entity(name);
        }

        public Grass() 
        {
            Entity = new Entity();
        }
    }
}
