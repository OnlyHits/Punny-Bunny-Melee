
using System.Collections.Generic;

namespace GMTK
{
    public static class AttackUtils
    {
        public enum BulletType
        {
            Bullet_Normal,
            Bullet_Bouncy,
        }

        public static Dictionary<BulletType, string> s_bulletPath = new()
        {
            { BulletType.Bullet_Normal, "Assets/Src/Prefabs/Projectile/Normal_Bullet.prefab" },
            { BulletType.Bullet_Bouncy, "Assets/Src/Prefabs/Projectile/Bouncy_Bullet.prefab" },
        };
    }
}