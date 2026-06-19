using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Cava.Content;

public class GrowthGlobalNPC : GlobalNPC
{
    public override void OnKill(NPC npc)
    {
        if (npc.lastInteraction >= 0 && npc.lastInteraction < Main.maxPlayers)
        {
            Player player = Main.player[npc.lastInteraction];
            GrowthPlayer gPlayer = player.GetModPlayer<GrowthPlayer>();

            // Check if your custom terrier minion is active anywhere on screen
            int minionType = ModContent.ProjectileType<Projectiles.CavaMinion>();
            if (player.ownedProjectileCounts[minionType] > 0)
            {
                gPlayer.CavaKills++; // Award a kill point directly to the player tracker!

                // Alert the player when hits landmark progression states
                if (gPlayer.CavaKills % 10 == 0)
                {
                    Main.NewText($"Your Pack of Cavas grows stronger! ({gPlayer.CavaKills} kills total)", 50, 255, 130);
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4, npc.position);
                }
            }
        }
    }
}