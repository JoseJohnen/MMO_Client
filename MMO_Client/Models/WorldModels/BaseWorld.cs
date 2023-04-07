using MMO_Client.Models.PuppetModels;

namespace MMO_Client.Models.WorldModels
{
    public class BaseWorld : MMO_Client.Models.WorldModels.World
    {
        public BaseWorld()
        {
            dic_SpawnList.Add(new Imp(), 1);
            dic_SpawnList.Add(new Pinkie(), 1);
        }
    }
}