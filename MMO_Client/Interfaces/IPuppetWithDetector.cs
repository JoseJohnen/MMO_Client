using Stride.Engine;
using MMO_Client.Code.Assistants;
using MMO_Client.Code.Controllers;
using MMO_Client.Code.Models;

namespace MMO_Client.Code.Interfaces
{
    internal interface IPuppetWithDetector
    {
        public float DetectionArea { get; set; }

        public bool DetectInRange(Entity ent)
        {
            if (Player.PLAYER.Entity  == null)
            {
                return false;
            }

            float a = UtilityAssistant.DistanceComparitorByAxis(((Puppet)this).Entity.Transform.Position.X, Player.PLAYER.Entity.Transform.Position.X);
            float b = UtilityAssistant.DistanceComparitorByAxis(((Puppet)this).Entity.Transform.Position.Y, Player.PLAYER.Entity.Transform.Position.Y);
            float c = UtilityAssistant.DistanceComparitorByAxis(((Puppet)this).Entity.Transform.Position.Z, Player.PLAYER.Entity.Transform.Position.Z);

            if (
                (a < (DetectionArea / 2)) &&
                (b < (DetectionArea / 2)) &&
                (c < (DetectionArea / 2))
                )
            {
                return true;
            }
            return false;
        }

        public bool IfDetectPlayer()
        {
            if (Player.PLAYER.Entity == null)
            {
                return false;
            }

            float a = UtilityAssistant.DistanceComparitorByAxis(((Puppet)this).Entity.Transform.Position.X, Player.PLAYER.Entity.Transform.Position.X);
            float b = UtilityAssistant.DistanceComparitorByAxis(((Puppet)this).Entity.Transform.Position.Y, Player.PLAYER.Entity.Transform.Position.Y);
            float c = UtilityAssistant.DistanceComparitorByAxis(((Puppet)this).Entity.Transform.Position.Z, Player.PLAYER.Entity.Transform.Position.Z);

            if (
                (a < (DetectionArea / 2)) &&
                (b < (DetectionArea / 2)) &&
                (c < (DetectionArea / 2))
                )
            {
                return true;
            }
            return false;
        }
    }
}
