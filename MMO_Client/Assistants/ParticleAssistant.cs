using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using MMO_Client.Controllers;
using MMO_Client.Code.Models;

namespace MMO_Client.Code.Assistants
{
    public class ParticleAssistant : StartupScript
    {
        public Prefab laserPoint;
        Entity[] arrayLaserPoints = new Entity[5];
        int counterPointLaser = 1;

        public override void Start()
        {
            //base.Start();
            //System.Timers.Timer aTimer = new System.Timers.Timer();
            //aTimer.Elapsed += new ElapsedEventHandler(ParticleTurn);
            //aTimer.Interval = 5000;
            //aTimer.Enabled = true;
        }

        public void ParticleTurn(object source, ElapsedEventArgs e)
        {
            if (Player.PLAYER.Entity == null)
            {
                return;
            }

            if (arrayLaserPoints[0] == null)
            {
                Entity ent = new Entity();
                ent.Transform.Position = Player.PLAYER.Entity.Transform.Position;
                arrayLaserPoints[0] = ent;

                for (int i = 1; i < arrayLaserPoints.Length; i++)
                {
                    List<Entity> instance = laserPoint.Instantiate();
                    instance.First().Transform.Position = Player.PLAYER.Entity.Transform.Position;
                    Entity.Scene.Entities.AddRange(instance);
                    arrayLaserPoints[i] = instance.First();
                }
            }
            try
            {
                foreach (Entity item in arrayLaserPoints)
                {
                    string a = item.Name.ToString();
                    string b = a;

                    if (item.GetChildren().Count() > 0)
                    {
                        foreach (Entity itm in item.GetChildren())
                        {
                            Log.Info(itm.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Info(ex.Message);
            }

            if (counterPointLaser == arrayLaserPoints.Length - 1)
            {
                counterPointLaser = 0;
            }
            else
            {
                counterPointLaser++;
            }
        }

    }
}
