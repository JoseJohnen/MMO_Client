using Stride.Engine;
using System;
using System.Collections.Generic;
using MMO_Client.Code.Models;

namespace MMO_Client.Code.Interfaces
{
    [Serializable]
    [Stride.Core.DataContract]
    public abstract class Ship : Puppet, IPuppetWithDetector
    {
        public Ship(float hp = 10, float velocityModifier = 0.05F, float mpKillBox = 0.08F, bool isFlyer = true, float DetectionAreaP = 15f, List<Entity> l_turretsP = null) : base(hp, velocityModifier, mpKillBox, isFlyer)
        {
            L_turrets = l_turretsP;
            DetectionArea = DetectionAreaP;
        }

        public float DetectionArea { get; set; }
        public virtual List<Entity> L_turrets { get; set; }

        public bool IfDetectPlayer()
        {
            return true;
        }
    }
}
