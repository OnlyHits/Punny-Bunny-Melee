
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

        public static Dictionary<WeaponType, string> s_weaponPath = new()
        {
            { WeaponType.Minigun, "Assets/Src/Prefabs/Weapon/Minigun_standardShader.prefab" },
            { WeaponType.Pistol, "Assets/Src/Prefabs/Weapon/BlusterPistol01_standardShader.prefab" },
            { WeaponType.Shotgun, "Assets/Src/Prefabs/Weapon/Shotgun03_standardShader.prefab" },
            { WeaponType.NotAnUzi, "Assets/Src/Prefabs/Weapon/Shotgun04_standardShader.prefab" },
        };

        public static Dictionary<BulletType, string> s_bulletPath = new()
        {
            { BulletType.Bullet_Normal, "Assets/Src/Prefabs/Projectile/Normal_Bullet.prefab" },
            { BulletType.Bullet_Bubble, "Assets/Src/Prefabs/Projectile/Bouncy_Bullet.prefab" },
            { BulletType.Bullet_Fireball, "Assets/Src/Prefabs/Projectile/Bullet_Fireball.prefab" },
            { BulletType.Bullet_Fireball_Big, "Assets/Src/Prefabs/Projectile/Bullet_Fireball_Big.prefab" },
        };
    }
}