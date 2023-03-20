using MMO_Client.Code.Models;
using System;

namespace MMO_Client.Models.PuppetModels
{
    public class Imp : Puppet
    {
        public override void RunIA()
        {
            try
            {

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
