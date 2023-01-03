using Stride.Core.Mathematics;
using Stride.Engine;
using System;
using System.Text.Json.Serialization;

namespace MMO_Client.Code.Models
{
    public abstract class Proyectile
    {
        public int id = 0;
        public string NameLauncher = string.Empty;
        public Vector3 InitialPosition = Vector3.Zero;
        private Vector3 position = Vector3.Zero;
        public Vector3 MovementModifier = Vector3.Zero;
        public float Damage = 10f;

        [JsonIgnore]
        public Entity ProyectileBody;

        public DateTime LastUpdate = DateTime.Now;
        public TimeSpan Velocity = new TimeSpan(0, 0, 0, 0, 1);

        public Vector3 Position { get => position; set { position = value; if (ProyectileBody != null) { ProyectileBody.Transform.Position = position; } } }

        public Proyectile(int id, string NameLauncher, Vector3 InitialPosition, Vector3 MovementModifier, float Damage, DateTime LastUpdate = default(DateTime), TimeSpan Velocity = default(TimeSpan))
        {
            this.id = id;
            this.NameLauncher = NameLauncher;
            this.InitialPosition = InitialPosition;
            this.Position = InitialPosition;
            this.MovementModifier = MovementModifier;
            this.Damage = Damage;
            this.LastUpdate = LastUpdate;
            this.Velocity = Velocity == default(TimeSpan) ? new TimeSpan(0, 0, 0, 0, 1) : Velocity;
            this.ProyectileBody = null;
        }

        public bool ExecuteEffect()
        {
            return true;
        }
    }

    public class Bullet : Proyectile
    {
        //Write here methods or atributes of the specific object
        public Bullet(int id, string NameLauncher, Vector3 InitialPosition, Vector3 MovementModifier, float Damage = 10f, DateTime velocity = default(DateTime), TimeSpan Velocity = default(TimeSpan)) : base(id, NameLauncher, InitialPosition, MovementModifier, Damage)
        {
            this.Velocity = Velocity == default(TimeSpan) ? new TimeSpan(0, 0, 0, 0, 1) : Velocity;
        }
    }

    public class Missile : Proyectile
    {
        //Write here methods or atributes of the specific object
        public int HP = 0;

        public Missile(int id, string NameLauncher, Vector3 InitialPosition, Vector3 MovementModifier, float Damage, DateTime velocity = default(DateTime), TimeSpan Velocity = default(TimeSpan), int HP = 1) : base(id, NameLauncher, InitialPosition, MovementModifier, Damage)
        {
            this.HP = HP;
            this.Velocity = Velocity == default(TimeSpan) ? new TimeSpan(0, 0, 0, 0, 1) : Velocity;
        }
    }

}
