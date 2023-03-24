using Stride.Engine;
using MMO_Client.Code.Controllers;
using System;
using Interfaz.Utilities;
using Newtonsoft.Json;
using Interfaz.Models;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using MMO_Client.Code.Assistants;

namespace MMO_Client.Code.Models
{
    public partial class Player : Puppet
    {
        //No usados en este proyecto, al menos de momento
        public static string WP = string.Empty;
        public static string LS = string.Empty;
        public static string RS = string.Empty;
        public static string PS = string.Empty;
        public static string RT = string.Empty;
        [JsonIgnore]
        public static string GNPS = string.Empty;
        [JsonIgnore]
        public static string GNRT = string.Empty;

        [JsonIgnore]
        public static Entity CAM = null;
        [JsonIgnore]
        public static Player PLAYER = null;

        [JsonIgnore]
        public Entity Camera { get => CAM; set { CAM = value; } }
        [JsonIgnore]
        public override Entity Weapon { get; set; }
        [JsonIgnore]
        public Entity LeftShoulder { get; set; }
        [JsonIgnore]
        public Entity RightShoulder { get; set; }
        [JsonIgnore]
        public Entity Gun { get; set; }

        public override float HP { get => hpplayer; set => hpplayer = value; }

        private float hpplayer = 15;
        public override bool IsFlyer { get => isflyer; set => isflyer = value; }

        private bool isflyer = false;
        [JsonIgnore]
        public override Entity Entity
        {
            get
            {
                /*if(entity == null)
                {
                    entity = Controller.controller.playerController.player;
                }*/
                return entity;
            }
            set => entity = value;
        }
        [JsonIgnore]
        private Entity entity = null;

        public Player()
        {
            entity = new Entity("Player"); //Controller.controller.playerController.player;
            PLAYER = this;
        }

        public Player(Entity ent)
        {
            entity = ent;
            PLAYER = this;
        }

        public Player(Entity ent, AnimacionSprite anmSpr)
        {
            entity = ent;
            base.AnimSprite = anmSpr;
            PLAYER = this;
        }

        public override string ToJson()
        {
            try
            {
                string result = System.Text.Json.JsonSerializer.Serialize(this);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Player) ToJson: " + ex.Message);
                return string.Empty;
            }
        }

        //En cualquier caso tira pinta a que voy a tener que eventualmente rehacer y reordenar esta clase y la del lado del cliente
        public Player FromJson(string Text)
        {
            string txt = Text;
            try
            {
                txt = Interfaz.Utilities.UtilityAssistant.CleanJSON(txt.Replace("\u002B", "+"));
                //Player nwMsg = System.Text.Json.JsonSerializer.Deserialize<Player>(txt);
                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new PlayerConverter()
                    },
                };
                Player nwMsg = System.Text.Json.JsonSerializer.Deserialize<Player>(txt, serializeOptions);
                /*Player nwMsg = new Player();
                if (plDt != null)
                {
                    nwMsg.Weapon = new SerializedVector3(plDt.WP).ConvertToVector3();
                    this.Weapon = nwMsg.Weapon;
                    nwMsg.Leftarm = new SerializedVector3(plDt.LS).ConvertToVector3();
                    this.Leftarm = nwMsg.Leftarm;
                    nwMsg.Rightarm = new SerializedVector3(plDt.RS).ConvertToVector3();
                    this.Rightarm = nwMsg.Rightarm;
                    nwMsg.Position = new SerializedVector3(plDt.PS).ConvertToVector3();
                    this.Position = nwMsg.Position;
                    nwMsg.Rotation = UtilityAssistant.StringToQuaternion(plDt.RT);
                    this.Rotation = nwMsg.Rotation;
                }*/
                return nwMsg;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Player) FromJson: " + ex.Message + " Text: " + txt);
                return new Player();
            }
        }

        public static Player CreateFromJson(string json)
        {
            try
            {
                Player msg = new Player();
                return msg.FromJson(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Player) CreateFromJson: " + ex.Message);
                return new Player();
            }
        }

        public override void RunIA()
        {
            //Es el jugador, no corre IA
            return;
        }

        //For local purposes, it's a "card", hence, is static.
        //TODO: Data can and will be modified through a method by controller when online.
    }

    public class PlayerConverter : System.Text.Json.Serialization.JsonConverter<Player>
    {
        public override Player Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string strJson = string.Empty;
            try
            {
                JsonDocument jsonDoc = JsonDocument.ParseValue(ref reader);
                strJson = jsonDoc.RootElement.GetRawText();

                Player plyr = new Player();
                strJson = strJson.Replace("\"", "").Replace(":<", ":\"<").Replace(">}", ">\"}").Replace(".�M�", ">");
                string[] a = strJson.Replace("{", "").Replace("}", "").Split(",");//UtilityAssistant.CutJson(strJson);

                if (a[0] != null)
                {
                    plyr.Entity.Name = a[0].Substring(a[0].IndexOf(":") + 1);
                }
                else
                {
                    Console.WriteLine("a[0] es null, strJason es: " + strJson);
                    plyr.Entity.Name = string.Empty;
                }

                if (a[1] != null)
                {
                    plyr.MPKillBox = Convert.ToSingle(a[1].Substring(a[1].IndexOf(":") + 1));
                }
                else
                {
                    Console.WriteLine("a[1] es null, strJason es: " + strJson);
                    plyr.MPKillBox = 0;
                }

                if (a[2] != null)
                {
                    plyr.IsFlyer = Convert.ToBoolean(a[2].Substring(a[2].IndexOf(":") + 1));
                }
                else
                {
                    Console.WriteLine("a[2] es null, strJason es: " + strJson);
                    plyr.IsFlyer = false;
                }

                if (a[3] != null)
                {
                    plyr.HP = Convert.ToSingle(a[3].Substring(a[3].IndexOf(":") + 1));
                }
                else
                {
                    Console.WriteLine("a[3] es null, strJason es: " + strJson);
                    plyr.HP = 0;
                }

                if (plyr.Entity == null)
                {
                    plyr.Entity = new Entity();
                }

                if (a[4] != null)
                {
                    string fd = a[4].Substring(a[4].IndexOf(":") + 1);
                    Player.PLAYER.Entity.Transform.Position = Assistants.UtilityAssistant.ConvertVector3NumericToStride(Vector3Converter.Converter(fd));//System.Text.Json.JsonSerializer.Deserialize<Vector3>(fd, serializeOptions);
                }
                else
                {
                    Console.WriteLine("a[4] es null, strJason es: " + strJson);
                    plyr.Entity.Transform.Position = Stride.Core.Mathematics.Vector3.Zero;
                }

                if (plyr.Weapon == null)
                {
                    plyr.Weapon = new Entity();
                }

                if (a[5] != null)
                {
                    string fd = a[5].Substring(a[5].IndexOf(":") + 1);
                    plyr.Weapon.Transform.Position = Assistants.UtilityAssistant.ConvertVector3NumericToStride(Vector3Converter.Converter(fd));//System.Text.Json.JsonSerializer.Deserialize<Vector3>(fd, serializeOptions);
                }
                else
                {
                    Console.WriteLine("a[5] es null, strJason es: " + strJson);
                    plyr.Weapon.Transform.Position = Stride.Core.Mathematics.Vector3.Zero;
                }
                
                if (plyr.RightShoulder == null)
                {
                    plyr.RightShoulder = new Entity();
                }

                if (a[6] != null)
                {
                    string fd = a[6].Substring(a[6].IndexOf(":") + 1);
                    plyr.RightShoulder.Transform.Position = Assistants.UtilityAssistant.ConvertVector3NumericToStride(Vector3Converter.Converter(fd));//System.Text.Json.JsonSerializer.Deserialize<Vector3>(fd, serializeOptions);
                }
                else
                {
                    Console.WriteLine("a[6] es null, strJason es: " + strJson);
                    plyr.RightShoulder.Transform.Position = Stride.Core.Mathematics.Vector3.Zero;
                }

                if (plyr.LeftShoulder == null)
                {
                    plyr.LeftShoulder = new Entity();
                }

                if (a[7] != null)
                {
                    string fd = a[7].Substring(a[7].IndexOf(":") + 1);
                    plyr.LeftShoulder.Transform.Position = Assistants.UtilityAssistant.ConvertVector3NumericToStride(Vector3Converter.Converter(fd));//System.Text.Json.JsonSerializer.Deserialize<Vector3>(fd, serializeOptions);
                }
                else
                {
                    Console.WriteLine("a[7] es null, strJason es: " + strJson);
                    plyr.LeftShoulder.Transform.Position = Stride.Core.Mathematics.Vector3.Zero;
                }

                Player.PLAYER = plyr;
                return plyr;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (PlayerConverter) Read(): {0} Message: {1}", strJson, ex.Message);
                return new Player();
            }
        }

        public override void Write(Utf8JsonWriter writer, Player plyr, JsonSerializerOptions options)
        {
            try
            {
                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new Vector3Converter()
                        ,new NullConverter()
                    },
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true,
                    IgnoreNullValues = true
                };

                string LeftArm = System.Text.Json.JsonSerializer.Serialize(plyr.LeftShoulder, serializeOptions);
                string Rightarm = System.Text.Json.JsonSerializer.Serialize(plyr.RightShoulder, serializeOptions);
                string Weapon = System.Text.Json.JsonSerializer.Serialize(plyr.Weapon, serializeOptions);
                string Position = System.Text.Json.JsonSerializer.Serialize(plyr.Entity.Transform.Position, serializeOptions);

                string HP = plyr.HP.ToString(); //"\"" + plyr.Id + "\"";
                string IsFlyer = plyr.IsFlyer ? "true" : "false";
                string MPKillBox = plyr.MPKillBox.ToString();
                string Name = string.IsNullOrWhiteSpace(plyr.Entity.Name) ? "null" : plyr.Entity.Name; //"\"" + plyr.Id + "\"";;

                char[] a = { '"' };

                string wr = @String.Concat("{ ", new string(a), "Name", new string(a), ":", new string(a), Name, new string(a),
                    ", ", new string(a), "MPKillBox", new string(a), ":", MPKillBox,
                    ", ", new string(a), "IsFlyer", new string(a), ":", IsFlyer,
                    ", ", new string(a), "HP", new string(a), ":", HP,
                    ", ", new string(a), "Position", new string(a), ":", new string(a), Position, new string(a),
                    ", ", new string(a), "Weapon", new string(a), ":", new string(a), Weapon, new string(a),
                    ", ", new string(a), "Rightarm", new string(a), ":", new string(a), Rightarm, new string(a),
                    ", ", new string(a), "LeftArm", new string(a), ":", new string(a), LeftArm, new string(a),
                    "}");

                string resultJson = Regex.Replace(wr, "(\"(?:[^\"\\\\]|\\\\.)*\")|\\s+", "$1");
                //string resultJson = "{Id:" + Id + ", LN:" + LauncherName + ", Type:" + Type + ", OrPos:" + LauncherPos + ", WPos:" + WeaponPos + ", Mdf:" + Moddif + "}";

                writer.WriteStringValue(resultJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (PlayerConverter) Write(): " + ex.Message);
            }
        }
    }

}
