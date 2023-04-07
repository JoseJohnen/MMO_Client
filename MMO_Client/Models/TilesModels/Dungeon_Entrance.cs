using System.Numerics;

namespace MMO_Client.Models.TilesModels
{
    public class Dungeon_Entrance : MMO_Client.Models.TilesModels.Tile
    {
        public Dungeon_Entrance(string name = "", Vector3 position = default, Vector3 inworldpos = default) : base(name, position, inworldpos)
        {
        }

        public Dungeon_Entrance() 
        {
        }
    }
}
