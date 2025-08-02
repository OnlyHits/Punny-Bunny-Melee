
using System.Collections.Generic;

namespace GMTK
{
    public static class AttackUtils
    {
        public enum BulletType
        {
            Bullet_Normal,
            Bullet_Bubble,
            Bullet_Fireball,
            Bullet_Fireball_Big
        }

        public enum WeaponType
        {
            Minigun,
            Pistol,
            Shotgun,
            NotAnUzi,
        }

        public static Dictionary<BulletType, string> s_bulletPath = new()
        {
            { BulletType.Bullet_Normal, "Assets/Src/Prefabs/Projectile/Normal_Bullet.prefab" },
            { BulletType.Bullet_Bubble, "Assets/Src/Prefabs/Projectile/Bouncy_Bullet.prefab" },
            { BulletType.Bullet_Fireball, "Assets/Src/Prefabs/Projectile/Bullet_Fireball.prefab" },
            { BulletType.Bullet_Fireball_Big, "Assets/Src/Prefabs/Projectile/Bullet_Fireball_Big.prefab" },
        };
    }
}