using Stride.Engine;

namespace MMO_Client.Models.PuppetModels
{
    public class Marine : Puppet
    {
        public Marine(Entity entity, Entity realEnt = null, float range = 10, float hp = 10, float velocityModifier = 0.05F, float mpKillBox = 1.2f, bool isFlyer = false) : base(entity, realEnt, range, hp, velocityModifier, mpKillBox, isFlyer)
        {
            base.Entity = entity;
            base.RealEnt = realEnt;
            if (base.RealEnt != null)
            {
                base.RealEnt.Name = "RealEnt";
            }
            base.VelocityModifier = velocityModifier;
            base.MPKillBox = mpKillBox;
            base.IsFlyer = isFlyer;
            AnimSprite = new AnimacionSprite(new Animacion[22]
                {
                    //This must be placed by action, clockwise, starting from 9:00
                    new Animacion (26,29, "N-Walk"), //North Walk
                    new Animacion (33,36,"NWWalk"), //North-West 
                    new Animacion (3,6,"-WWalk"), //West Walk
                    new Animacion (47,50,"SWWalk"), //South-West Walk
                    new Animacion (40,43,"S-Walk"), //South Walk

                    new Animacion (23,23,"N-Pain"), //N-Pain
                    new Animacion (30,30,"NWPain"), //NWPain
                    new Animacion (0,0,"-WPain"), //-WPain
                    new Animacion (44,44,"SWPain"), //SWPain
                    new Animacion (37,37,"S-Pain"), //S-Pain

                    new Animacion (1,2,"-WShoot"), //West Shoot
                    new Animacion (31,32,"NWShoot"), //North-West Shoot
                    new Animacion (24,25,"N-Shoot"), //North Shoot
                    new Animacion (38,39,"S-Shoot"), //South Shoot
                    new Animacion (45,46,"SWShoot"), //South-West Shoot

                    new Animacion (43,44,"-WShooted"), //West Shooted
                    new Animacion (33,33,"NWShooted"), //North-West Shooted
                    new Animacion (34,34,"N-Shooted"), //North Shooted
                    new Animacion (29,30,"S-Shooted"), //South Shooted
                    new Animacion (31,31,"SWShooted"), //South-West Shooted

                    new Animacion (16,22,"DeadShoot"), //Death Shoot
                    new Animacion (7,15,"DeadExplosion"), //Death Explosion
                });
        }

        public Marine(Entity ent)
        {
            base.Entity = ent;
            base.RealEnt = new Entity("RealEnt");
            base.HP = 35;
            base.VelocityModifier = .9f;
            base.MPKillBox = 3;
            base.IsFlyer = false;
            AnimSprite = new AnimacionSprite(new Animacion[22]
                {
                    //This must be placed by action, clockwise, starting from 9:00
                    new Animacion (26,29, "N-Walk"), //North Walk
                    new Animacion (33,36,"NWWalk"), //North-West 
                    new Animacion (3,6,"-WWalk"), //West Walk
                    new Animacion (47,50,"SWWalk"), //South-West Walk
                    new Animacion (40,43,"S-Walk"), //South Walk

                    new Animacion (23,23,"N-Pain"), //N-Pain
                    new Animacion (30,30,"NWPain"), //NWPain
                    new Animacion (0,0,"-WPain"), //-WPain
                    new Animacion (44,44,"SWPain"), //SWPain
                    new Animacion (37,37,"S-Pain"), //S-Pain

                    new Animacion (1,2,"-WShoot"), //West Shoot
                    new Animacion (31,32,"NWShoot"), //North-West Shoot
                    new Animacion (24,25,"N-Shoot"), //North Shoot
                    new Animacion (38,39,"S-Shoot"), //South Shoot
                    new Animacion (45,46,"SWShoot"), //South-West Shoot

                    new Animacion (43,44,"-WShooted"), //West Shooted
                    new Animacion (33,33,"NWShooted"), //North-West Shooted
                    new Animacion (34,34,"N-Shooted"), //North Shooted
                    new Animacion (29,30,"S-Shooted"), //South Shooted
                    new Animacion (31,31,"SWShooted"), //South-West Shooted

                    new Animacion (16,22,"DeadShoot"), //Death Shoot
                    new Animacion (7,15,"DeadExplosion"), //Death Explosion
                });
        }

        public Marine()
        {
            base.Entity = new Entity();
            base.RealEnt = new Entity("RealEnt");
            base.HP = 35;
            base.VelocityModifier = .9f;
            base.MPKillBox = 3;
            base.IsFlyer = false;
            AnimSprite = new AnimacionSprite(new Animacion[22]
                {
                    //This must be placed by action, clockwise, starting from 9:00
                    new Animacion (26,29, "N-Walk"), //North Walk
                    new Animacion (33,36,"NWWalk"), //North-West 
                    new Animacion (3,6,"-WWalk"), //West Walk
                    new Animacion (47,50,"SWWalk"), //South-West Walk
                    new Animacion (40,43,"S-Walk"), //South Walk

                    new Animacion (23,23,"N-Pain"), //N-Pain
                    new Animacion (30,30,"NWPain"), //NWPain
                    new Animacion (0,0,"-WPain"), //-WPain
                    new Animacion (44,44,"SWPain"), //SWPain
                    new Animacion (37,37,"S-Pain"), //S-Pain

                    new Animacion (1,2,"-WShoot"), //West Shoot
                    new Animacion (31,32,"NWShoot"), //North-West Shoot
                    new Animacion (24,25,"N-Shoot"), //North Shoot
                    new Animacion (38,39,"S-Shoot"), //South Shoot
                    new Animacion (45,46,"SWShoot"), //South-West Shoot

                    new Animacion (43,44,"-WShooted"), //West Shooted
                    new Animacion (33,33,"NWShooted"), //North-West Shooted
                    new Animacion (34,34,"N-Shooted"), //North Shooted
                    new Animacion (29,30,"S-Shooted"), //South Shooted
                    new Animacion (31,31,"SWShooted"), //South-West Shooted

                    new Animacion (16,22,"DeadShoot"), //Death Shoot
                    new Animacion (7,15,"DeadExplosion"), //Death Explosion
                });
        }

    }
}
