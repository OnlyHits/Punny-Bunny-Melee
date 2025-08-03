
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

        public static AttackDatas m_minigunBonusData = new()
        {
            projectile_number = 100,
            speed = 50,
            angle_range = 20f,
            randomize_distribution = true,
            fire_once = false,
            bounce_on_collision = false,
            time_between_bullet = 0.2f,
            max_distance_sqr = 10.0f,
            max_bounces = 1,
            bullet_type = BulletType.Bullet_Fireball,
            weapon_type = WeaponType.Minigun,
        };
    }
}