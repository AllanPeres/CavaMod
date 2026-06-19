using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Cava.Content;

public class GrowthPlayer : ModPlayer
{
    public int CavaKills = 0;

    public override void SaveData(TagCompound tag)
    {
        tag["CavaKills"] = CavaKills;
    }

    public override void LoadData(TagCompound tag)
    {
        if (tag.ContainsKey("CavaKills"))
        {
            CavaKills = tag.GetInt("CavaKills");
        }
    }
    
    public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
    {
        if (item.type == ModContent.ItemType<Items.CavaStaff>())
        {
            damage += (CavaKills * 2) / (float) item.damage;
        }
    }
}