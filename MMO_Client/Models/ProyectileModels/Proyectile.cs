using Interfaz.Auxiliary;
using MMO_Client.Controllers;
using Stride.Core.Mathematics;
using Stride.Engine;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MMO_Client.Code.Models
{
    public abstract class Proyectile
    {
        public string id = string.Empty;
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

        public Proyectile(string id, string NameLauncher, Vector3 InitialPosition, Vector3 MovementModifier, float Damage, DateTime LastUpdate = default(DateTime), TimeSpan Velocity = default(TimeSpan))
        {
            this.id = string.IsNullOrWhiteSpace(id) ? GenerateId() : id;
            this.NameLauncher = NameLauncher;
            this.InitialPosition = InitialPosition;
            this.Position = InitialPosition;
            this.MovementModifier = MovementModifier;
            this.Damage = Damage;
            this.LastUpdate = LastUpdate;
            this.Velocity = Velocity == default(TimeSpan) ? new TimeSpan(0, 0, 0, 0, 1) : Velocity;
            this.ProyectileBody = null;
        }

        public string GenerateId()
        {
            try
            {
                string a = string.Empty;
                string keyAttempt = string.Empty;
                Bullet blt = null;
                do
                {
                    a = DateTime.Now.ToString() + this.Position.ToString();
                    keyAttempt = UtilityAssistant.Base64Encode(a).Substring(0, 10);
                }
                while (PlayerController.dic_bulletsOnline.TryGetValue(keyAttempt, out blt) || string.IsNullOrWhiteSpace(keyAttempt));
                id = keyAttempt;
                return id;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error string GenerateKey(): " + ex.Message);
                return string.Empty;
            }
        }

        public static string GenerateIdStatic()
        {
            try
            {
                string a = string.Empty;
                string keyAttempt = string.Empty;
                Bullet blt = null;
                do
                {
                    Random rnd = new Random(PlayerController.dic_bulletsOnline.Count);
                    a = DateTime.Now.ToString() + rnd.Next(0, 1000).ToString();
                    keyAttempt = UtilityAssistant.Base64Encode(a).Substring(0, 10);
                }
                while (PlayerController.dic_bulletsOnline.TryGetValue(keyAttempt, out blt) || string.IsNullOrWhiteSpace(keyAttempt));
                return keyAttempt;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error static string GenerateIdStatic(): " + ex.Message);
                return string.Empty;
            }
        }

        public override string ToString()
        {
            string rtrString = "NL:" + this.NameLauncher + "IP:" + InitialPosition + "PS:" + Position + "MM:" + MovementModifier + "DM:" + Damage;
            return rtrString;
        }

        public virtual string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }

        public bool ExecuteEffect()
        {
            return true;
        }
    }

    public class Bullet : Proyectile
    {
        //Write here methods or atributes of the specific object
        public Bullet(string id, string NameLauncher, Vector3 InitialPosition, Vector3 MovementModifier, float Damage = 10f, DateTime velocity = default(DateTime), TimeSpan Velocity = default(TimeSpan)) : base(id, NameLauncher, InitialPosition, MovementModifier, Damage)
        {
            this.Velocity = Velocity == default(TimeSpan) ? new TimeSpan(0, 0, 0, 0, 1) : Velocity;
        }
    }

    public class Missile : Proyectile
    {
        //Write here methods or atributes of the specific object
        public int HP = 0;

        public Missile(string id, string NameLauncher, Vector3 InitialPosition, Vector3 MovementModifier, float Damage, DateTime velocity = default(DateTime), TimeSpan Velocity = default(TimeSpan), int HP = 1) : base(id, NameLauncher, InitialPosition, MovementModifier, Damage)
        {
            this.HP = HP;
            this.Velocity = Velocity == default(TimeSpan) ? new TimeSpan(0, 0, 0, 0, 1) : Velocity;
        }
    }

}
