using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Cava.Content.Projectiles;

public class CavaMinion: ModProjectile
{

    public long LinkedWeaponInstanceId = -1;
    
    public override void SetStaticDefaults()
    {
        Main.projPet[Type] = true; 
        ProjectileID.Sets.MinionTargettingFeature[Type] = true; 
        ProjectileID.Sets.MinionSacrificable[Type] = true;

        // 6 frames: 0=idle, 1-4=running, 5=jumping
        Main.projFrames[Type] = 1; 
    }

    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 28;
        Projectile.friendly = true;
        Projectile.minion = true;
        Projectile.DamageType = DamageClass.Summon;
        Projectile.minionSlots = 1f;
        Projectile.penetrate = -1;

        // Ground movement setup
        Projectile.tileCollide = true; 
        Projectile.aiStyle = ProjAIStyleID.CommonFollow; 
        AIType = ProjectileID.BabySpider; 

        // Initialize damage cadence rule tracking
        Projectile.usesLocalNPCImmunity = true;     
        Projectile.localNPCHitCooldown = 12; // Controls hit rate (5 times a second)
    }

    public override bool? CanDamage()
    {
        // Keeps our contact frame checking logic active manually
        return true; 
    }

    public override void OnSpawn(IEntitySource source)
    {
        if (source is EntitySource_ItemUse itemSource)
        {
            if (itemSource.Item.ModItem is Items.CavaStaff staff)
            {
                LinkedWeaponInstanceId = staff.UniqueInstanceId;
            }
        }
    }

    public override bool PreAI()
    {
        var player = Main.player[Projectile.owner];

        // De-spawn tracking checks
        if (player.dead || !player.HasBuff(ModContent.BuffType<Buffs.CavaBuff>()))
        {
            Projectile.Kill();
            return false; 
        }

        Projectile.timeLeft = 2;
        
        // --- ADD THIS TO MANUALLY PUSH THE MINION FARTHER AWAY ---
        // If the minion is not chasing an enemy and is close to the player
        var distanceToPlayer = Vector2.Distance(Projectile.Center, player.Center);
        if (!(distanceToPlayer < 200f) || Projectile.velocity.Y != 0f) return true;
        // Calculate which side of the player the minion is on
        var sideSign = (Projectile.Center.X < player.Center.X) ? -1 : 1;
        
        // If it gets too close to your feet (closer than 100 pixels), gently push it outward
        if (distanceToPlayer < 100f)
        {
            Projectile.velocity.X = MathHelper.Lerp(Projectile.velocity.X, 4f * sideSign, 0.05f);
        }

        return true; 
    }

    public override void PostAI()
    {
        var player = Main.player[Projectile.owner];

        var originalWeaponInstance = FindLinkedWeaponInstance(player);

        int realTimeDamage;

        if (originalWeaponInstance != null)
        {
            var weaponItem = originalWeaponInstance.Item;
            realTimeDamage = player.GetWeaponDamage(weaponItem);
        }
        else
        {
            realTimeDamage = (int) player.GetDamage(DamageClass.Summon).ApplyTo(Projectile.damage);
        }

        // --- 1.4.4 CONTACT DAMAGE LOOP ---
        for (var i = 0; i < Main.maxNPCs; i++)
        {
            var npc = Main.npc[i];
            if (!npc.active || npc.friendly || !npc.CanBeChasedBy() ||
                !Projectile.Hitbox.Intersects(npc.Hitbox)) continue;
            if (Projectile.localNPCImmunity[npc.whoAmI] != 0) continue;
            var hit = npc.CalculateHitInfo(realTimeDamage, Projectile.direction, false, Projectile.knockBack, Projectile.DamageType, true);
            npc.StrikeNPC(hit);

            if (Main.netMode != NetmodeID.SinglePlayer)
            {
                NetMessage.SendData(MessageID.DamageNPC, -1, -1, null, npc.whoAmI, realTimeDamage, Projectile.knockBack, Projectile.direction);
            }
                
            Projectile.localNPCImmunity[npc.whoAmI] = Projectile.localNPCHitCooldown;
        }

        // USE THIS WHEN HAVING MORE SPRITES
        // // --- ANIMATION INTERPOLATION TRACKING ---
        // if (Projectile.velocity.Y != 0f)
        // {
        //     Projectile.frame = 5; // Jumping Frame
        // }
        // else if (Projectile.velocity.X != 0f)
        // {
        //     Projectile.frameCounter++;
        //     if (Projectile.frameCounter >= 6) 
        //     {
        //         Projectile.frameCounter = 0;
        //         Projectile.frame++;
        //         if (Projectile.frame > 4) 
        //         {
        //             Projectile.frame = 1; 
        //         }
        //     }
        // }
        // else
        // {
        //     Projectile.frame = 0; // Sitting Idle Frame
        // }

        // Make sure the sprite mirrors to match its path vector direction
        if (Projectile.velocity.X != 0f)
        {
            Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
        }
    }

    private ModItem FindLinkedWeaponInstance(Player player)
    {
        for (var i = 0; i < 50; i++)
        {
            var item = player.inventory[i];
            if (item == null || item.IsAir) continue;
            if (item.ModItem is Items.CavaStaff staff && staff.UniqueInstanceId == LinkedWeaponInstanceId)
            {
                return staff;
            }
        }

        if (Main.mouseItem == null || Main.mouseItem.IsAir ||
            Main.mouseItem.ModItem is not Items.CavaStaff mouseStaff) return null;
        return mouseStaff.UniqueInstanceId == LinkedWeaponInstanceId ? mouseStaff : null;
    }
}