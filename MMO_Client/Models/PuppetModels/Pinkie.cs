using MMO_Client.Code.Models;
using System;

namespace MMO_Client.Models.PuppetModels
{
    public class Pinkie : Puppet
    {
        /*public override void RunIA()
        {
            try
            {
                if (this.IfDetectPlayer(2))
                {
                    this.ShootingToOffline(Player.PLAYER);
                }
                else if(this.IfDetectPlayer())
                {
                    this.MoveTo(Player.PLAYER.Entity.Transform.Position); //false in trasition because is runned Online
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }*/
    }
}
