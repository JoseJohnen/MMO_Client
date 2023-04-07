using System.Numerics;

namespace MMO_Client.Models.TilesModels
{
    public class Grass : MMO_Client.Models.TilesModels.Tile
    {
        public Grass(string name = "", Vector3 position = default, Vector3 inworldpos = default) : base(name, position, inworldpos)
        {
        }

        public Grass() 
        {
        }
    }
}
