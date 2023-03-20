using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Rendering.Sprites;
using System;
using System.Linq;
using MMO_Client.Code.Assistants;
using MMO_Client.Code.Controllers;
using System.Collections.Generic;

namespace MMO_Client.Code.Models
{
    public class AnimacionSprite
    {
        private bool isPlayerOrientable = true;

        public Animacion[] DesdeHastaFrames { get; set; }
        public int LastFrame { get; set; }
        public DateTime LastTime { get; set; }

        public bool IsPlayerOrientable { get => isPlayerOrientable; set => isPlayerOrientable = value; }

        public AnimacionSprite(Animacion[] desdeHastaFrames = null, DateTime lastTime = default(DateTime))
        {
            DesdeHastaFrames = desdeHastaFrames != null ? desdeHastaFrames : new Animacion[1] { new Animacion(0, 0, "Idle") };
            LastTime = lastTime == default(DateTime) ? DateTime.Now : lastTime;
            LastFrame = 0;
        }

        public static void RegistrarEntidadEnAnimacionSprite(Puppet ppt, TimeSpan speed = default(TimeSpan))
        {
            try
            {
                if (ppt == null)
                {
                    return;
                }

                int startingFrame = 0;
                TimeSpan tmspSpeed = speed == default(TimeSpan) ? new TimeSpan(0, 0, 0, 1, 500) : speed;

                Controller.controller.playerController.l_AnimacionesEntitys.Add(new Trios<int, Puppet, TimeSpan>(startingFrame, ppt, tmspSpeed));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error RegistrarEntidadEnAnimacionSprite(Entity, AnimacionSprite, TimeSpan): " + ex.Message);
            }
        }

        public static void CambiarAnimacion(Entity entidadAAnimar, string nombreAnimacion)
        {
            Trios<int, Puppet, TimeSpan> cms = Controller.controller.playerController.l_AnimacionesEntitys.Where(c => c.Item2.Entity.Name.ToUpper() == entidadAAnimar.Name.ToUpper()).First();
            cms.Item1 = cms.Item2.AnimSprite.LugarAnimacionEspecificaPorNombre(nombreAnimacion);
        }

        public double CambiarSpritePorPerspectivaPlayer(Puppet ppt, Entity player, int CantidadDeDirecciones = 8)
        {
            try
            {
                if (!isPlayerOrientable)
                {
                    return 0;
                }

                if (player == null)
                {
                    return 0;
                }

                double getAngle = UtilityAssistant.AngleOfRotation(ppt, player); //From 0 to (Hero) 360

                //Animacion[] Arr_Animacion = ppt.AnimSprite.DesdeHastaFrames.Where(c => c.Nombre.Contains("Walk")).ToArray();
                Dictionary<string, Animacion> dic_Animacion = ppt.AnimSprite.DesdeHastaFrames.Where(c => c.Nombre.Contains("Walk")).Select(c => new KeyValuePair<string, Animacion>(c.Nombre, c)).ToDictionary(c => c.Key, c => c.Value);

                double sections = 360 / CantidadDeDirecciones;

                //Para averiguar cantidad de secciones
                /*List<Pares<double, double>> l_seccions = new List<Pares<double, double>>();

                for (double i = 0; i < CantidadDeDirecciones; i++)
                {
                    if (i == (CantidadDeDirecciones - 1))
                    {
                        l_seccions.Add(new Pares<double, double>((sections * i), 360));
                    }
                    else
                    {
                        l_seccions.Add(new Pares<double, double>((sections * i), (sections * (i + 1))));
                    }
                }*/

                foreach (KeyValuePair<string, Pares<double, double>> item in ppt.DirectionalsPerAngle())
                {
                    if (getAngle >= item.Value.Item1 && getAngle <= item.Value.Item2)
                    {
                        CambiarAnimacion(ppt.Entity, item.Key);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("\r {0} {1}", getAngle, item.Key);
                        Console.ResetColor();
                    }
                }


                /*

                int j = 0;
                foreach (Pares<double, double> item in l_seccions)
                {
                    if (getAngle >= item.Item1 && getAngle <= item.Item2)
                    {
                        SetAnimation(ppt, j);
                    }
                    j++;
                }*/
                /*Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("\r {0} {1}", getAngle, sections);
                Console.ResetColor();*/

                return getAngle;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error CambiarSpritePorPerspectivaPlayer(): " + ex.Message);
                return 0;
            }
        }

        public static void SetAnimation(Puppet ppt, int specificSection)
        {
            if (specificSection > ppt.AnimSprite.DesdeHastaFrames.Length || specificSection < 0)
            {
                return;
            }

            CambiarAnimacion(ppt.Entity, ppt.AnimSprite.DesdeHastaFrames[specificSection].Nombre);
        }

        public int OrientarAPlayer(Entity ent, Entity target = null)
        {
            if (!isPlayerOrientable)
            {
                return 0;
            }

            if (Player.PLAYER.Entity == null && target == null)
            {
                return 0;
            }

            Entity plyr = target;
            if (plyr == null)
            {
                plyr = Player.PLAYER.Entity;
            }

            //Avoid the player to "rotate over itself" and generate a ton of problems because of it
            if (Player.PLAYER.Entity != null)
            {
                if (Player.PLAYER.Entity.Name == ent.Name)
                {
                    return 0;
                }
            }

            UtilityAssistant.LookAtAlt2(ent, plyr.Transform.Position);

            //Vector3 diff = UtilityAssistant.DistanceModifierByVectorComparison(ent.Transform.position, (ent.Transform.position - plyr.Transform.position));
            Vector3 diff = PositionOfPlayerRelativeToDoomGuy(ent, plyr);
            /*Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("\r{0}%   ", "differencial: " + diff);
            Console.ResetColor();*/
            return 1;
        }

        public Vector3 PositionOfPlayerRelativeToDoomGuy(Entity ent, Entity plyr)
        {

            Vector3 diff = UtilityAssistant.DistanceModifierByVectorComparison(ent.Transform.Position, (ent.Transform.Position - plyr.Transform.Position));
            return diff;
        }

        public float PositionOfPlayerRelativeToDoomGuy(Puppet ent, Entity plyr)
        {
            //Camera rotation
            float cameraRot = Controller.controller.GetActiveCamera().Entity.Transform.Rotation.X;
            float doomGuyRot = ent.Entity.Transform.Rotation.X;
            float result = doomGuyRot - cameraRot;
            //Vector3 diff = UtilityAssistant.DistanceModifierByVectorComparison(ent.Entity.Transform.position, (ent.Entity.Transform.position - plyr.Transform.position));
            return result;
        }

        public int OrientarAPlayer(Puppet ppt, Entity target = null)
        {
            try
            {
                if (!isPlayerOrientable)
                {
                    return 0;
                }

                if (Player.PLAYER.Entity == null && target == null)
                {
                    return 0;
                }

                Entity plyr = target;
                if (plyr == null)
                {
                    plyr = Player.PLAYER.Entity;
                }

                //Avoid the player to "rotate over itself" and generate a ton of problems because of it
                if (Player.PLAYER.Entity != null)
                {
                    if (Player.PLAYER.Entity.Name == ppt.Entity.Name)
                    {
                        return 0;
                    }
                }

                //ppt.Body.Get<SpriteComponent>().CurrentSprite =
                //Quaternion po1s = ppt.Body.Transform.Rotation;
                //UtilityAssistant.ManualLookAt(ppt.Body, plyr.Transform.position);


                //UtilityAssistant.RotateTo(ppt.Body, plyr.Transform.position);
                UtilityAssistant.LookAtAlt2(ppt.Entity, plyr.Transform.WorldMatrix.TranslationVector);
                //ppt.Entity.Transform.WorldMatrix.TranslationVector = plyr.Transform.position;

                //Quaternion po2s = ppt.Body.Transform.Rotation;

                //Console.WriteLine("Origen: "+po1s.ToString()+" Fin: "+po2s.ToString());

                //TODO: Hacer que cambie el sprite de posición según la ubicación del jugador
                //int result = 0;


                //double radiant = PositionOfPlayerRelativeToDoomGuy(ppt, plyr) * 10;
                //Considerar Rotación Puppet

                //Considerar Posición player

                //Definir nuevo sprite a activar

                //ppt.Entity.Transform.RotationEulerXYZ

                double angle = UtilityAssistant.GetAngle(ppt.Entity.Transform.Position, plyr.Transform.Position);

                /*Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("differencial: " + angle);
                Console.ResetColor();*/

                /*if (ppt.Entity.Transform.Scale.X < 0)
                {
                    ppt.Entity.Transform.Scale.X = 1;
                }

                if (angle >= -45 && angle <= 45)
                {
                    CambiarAnimacion(ppt.Entity, "N-Walk");
                }
                else if (angle >= 45 && angle <= 135)
                {
                    CambiarAnimacion(ppt.Entity, "-WWalk");
                }
                else if (angle >= 45 && angle <= 135)
                {
                    CambiarAnimacion(ppt.Entity, "S-Walk");
                }
                else if (angle >= 45 && angle <= 135)
                {
                    CambiarAnimacion(ppt.Entity, "-WWalk");
                    ppt.Entity.Transform.Scale.X *= -1;
                }*/

                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error OrientarAPlayer: " + ex.Message);
                return 0;
            }
        }

        public int LugarAnimacionEspecificaPorNombre(string nombreAnimacionACorrer)
        {
            try
            {
                int i = 0;
                for (i = 0; i < DesdeHastaFrames.Length; i++)
                {
                    if (DesdeHastaFrames[i].Nombre.ToUpper() == nombreAnimacionACorrer.ToUpper())
                    {
                        return i;
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Animar(Entity, string) : " + ex.Message);
                return 0;
            }
        }

        public int Animar(Entity ent, string nombreAnimacionACorrer)
        {
            try
            {
                if (ent.Get<SpriteComponent>() == null)
                {
                    return 0;
                }

                int i = 0;
                for (i = 0; i < DesdeHastaFrames.Length - 1; i++)
                {
                    if (DesdeHastaFrames[i].Nombre.ToUpper() == nombreAnimacionACorrer.ToUpper())
                    {
                        return Animar(ent, i);
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Animar(Entity, string) : " + ex.Message);
                return 0;
            }
        }

        public int Animar(Entity ent, int desdeHastaAUsar = 0)
        {
            try
            {
                if (ent.Get<SpriteComponent>() == null)
                {
                    return desdeHastaAUsar;
                }

                int procDHUsar = desdeHastaAUsar;
                SpriteFromSheet spr = ent.Get<SpriteComponent>().SpriteProvider as SpriteFromSheet;
                if (this.DesdeHastaFrames[procDHUsar].DesdeFrame == this.DesdeHastaFrames[procDHUsar].HastaFrame)
                {
                    spr.CurrentFrame = this.DesdeHastaFrames[procDHUsar].DesdeFrame;
                    return procDHUsar;
                }

                if ((spr.CurrentFrame < this.DesdeHastaFrames[procDHUsar].DesdeFrame) || (spr.CurrentFrame > this.DesdeHastaFrames[procDHUsar].HastaFrame))
                {
                    for (int i = 0; i < DesdeHastaFrames.Length - 1; i++)
                    {
                        if ((spr.CurrentFrame >= DesdeHastaFrames[i].DesdeFrame) && (spr.CurrentFrame <= DesdeHastaFrames[i].HastaFrame))
                        {
                            spr.CurrentFrame = this.DesdeHastaFrames[procDHUsar].DesdeFrame;
                        }
                    }
                }

                if (spr.CurrentFrame >= this.DesdeHastaFrames[procDHUsar].DesdeFrame)
                {
                    if (spr.CurrentFrame <= this.DesdeHastaFrames[procDHUsar].HastaFrame)
                    {
                        if (spr.CurrentFrame == this.DesdeHastaFrames[procDHUsar].HastaFrame)
                        {
                            spr.CurrentFrame = this.DesdeHastaFrames[procDHUsar].DesdeFrame;
                        }
                        else
                        {
                            spr.CurrentFrame++;
                        }

                        //Dar vuelta si es reversible
                        if (!this.DesdeHastaFrames[procDHUsar].isReversible && ent.Transform.Scale.X < 0)
                        {
                            ent.Transform.Scale.X = ent.Transform.Scale.X * -1;
                        }
                        else if (this.DesdeHastaFrames[procDHUsar].isReversible && ent.Transform.Scale.X > 0)
                        {
                            ent.Transform.Scale.X = ent.Transform.Scale.X * -1;
                        }
                    }
                }

                return procDHUsar;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Animar(Entity, int) : " + ex.Message);
                return 0;
            }
        }
    }

    public struct Animacion
    {
        public Animacion(int desdeFrame = 0, int hastaFrame = 0, string nombre = "")
        {
            Nombre = nombre;
            DesdeFrame = desdeFrame;
            HastaFrame = hastaFrame;
        }
        public string Nombre { get; }
        public int DesdeFrame { get; }
        public int HastaFrame { get; }

        public bool isReversible { get; set; } = false;
    }
}
