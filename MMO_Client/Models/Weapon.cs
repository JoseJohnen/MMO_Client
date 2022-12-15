using System;
using Stride.Engine;

namespace MMO_Client
{
    [Serializable]
    [Stride.Core.DataContract]
    public struct Weapon
    {
        private int ammo;
        private float range;
        private string nombre;
        private Entity entity;

        public int Ammo
        {
            get
            {
                if (ammo == null)
                {
                    ammo = 0;
                }
                return ammo;
            }
            set
            {
                try
                {
                    ammo = value;
                    string firstPart = FirstPart(Nombre);
                    string theRest = TheRest(Nombre);

                    nombre = firstPart + ammo + theRest;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public float Range
        {
            get
            {
                if (range == null)
                {
                    range = 0;
                }
                return range;
            }
            set
            {
                range = value;
                try
                {
                    string firstPart = FirstPart(Nombre);
                    string theRest = TheRest(Nombre).Replace(LastValue(Nombre), "");

                    nombre = firstPart + theRest + range;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
        }

        public string Nombre
        {
            get
            {
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    nombre = "WPType_AMMOMAX_RANGE";
                }
                return nombre;
            }
            set
            {
                nombre = value;
                if (Entity != null)
                {
                    entity.Name = nombre;
                }
            }
        }
        public Entity Entity { get => entity; set => entity = value; }

        public Weapon(int ammoP = 0, float rangeP = 0, string nombreP = "WPType_X_Z", Entity ent = null)
        {
            ammo = ammoP;
            range = rangeP;
            nombre = nombreP.Equals("WPType_AMMOMAX_RANGE") ? "WPType_" + ammo + "_" + range : nombreP;
            entity = ent;
            if(ent != null)
            {
                ent.Name = nombreP.Equals("WPType_AMMOMAX_RANGE") ? "WPType_" + ammo + "_" + range : nombreP;
            } 
        }

        private string FirstPart(string strName)
        {
            string strFirstPart = strName.Substring(0, strName.IndexOf("_") + 1);
            return strFirstPart;
        }

        private string TheRest(string strName)
        {
            string b = strName.Substring(strName.IndexOf("_") + 1);
            return b;
        }

        private string FirstValue(string strName)
        {
            int firstInstance = (strName.IndexOf("_") + 1);
            string c = strName.Substring(firstInstance);
            string firstValueIsolated = c.Substring(0, c.IndexOf("_"));
            return firstValueIsolated;
        }

        private string LastValue(string strName)
        {
            int firstInstance = (strName.IndexOf("_") + 1);
            string a = strName.Substring(firstInstance);
            int secondInstance = (a.IndexOf("_") + 1);
            string lastValueIsolated = strName.Substring((firstInstance + secondInstance));
            lastValueIsolated = lastValueIsolated.Substring(lastValueIsolated.IndexOf("_") + 1);
            return lastValueIsolated;
        }
    }
}
