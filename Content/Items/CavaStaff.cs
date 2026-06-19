using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Cava.Content.Items
{
	// This is a basic item template.
	// Please see tModLoader's ExampleMod for every other example:
	// https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod
	public class CavaStaff : ModItem
	{

		public long UniqueInstanceId = 0;
		
		public override void SetDefaults()
		{
			Item.damage = 10;
			Item.knockBack = 2f;
			Item.mana = 0;
			Item.width = 40;
			Item.height = 40;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.value = Item.buyPrice(gold: 2);
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item44;

			Item.DamageType = DamageClass.Summon;
			Item.noMelee = true;

			Item.buffType = ModContent.BuffType<Buffs.CavaBuff>();
			Item.shoot = ModContent.ProjectileType<Projectiles.CavaMinion>();
		}

		public override void OnCreated(ItemCreationContext context)
		{
			if (UniqueInstanceId != 0) return;
			var buffer = new byte[8];
			Main.rand.NextBytes(buffer);
				
			UniqueInstanceId = System.BitConverter.ToInt64(buffer, 0);
				
			// Make sure the number isn't 0 or negative
			if (UniqueInstanceId <= 0) UniqueInstanceId = Main.rand.Next(1, int.MaxValue);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			player.AddBuff(Item.buffType, 2);
			
			var spawnPosition = Main.MouseWorld;
			
			// Same safety fallback block for the shoot trigger method
			if (UniqueInstanceId == 0)
			{
				var buffer = new byte[8];
				Main.rand.NextBytes(buffer);
				UniqueInstanceId = System.BitConverter.ToInt64(buffer, 0);
				if (UniqueInstanceId <= 0) UniqueInstanceId = Main.rand.Next(1, int.MaxValue);
			}
			
			Projectile.NewProjectile(source, spawnPosition, Vector2.Zero, type, damage, knockback, player.whoAmI);
            
			return false;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.Wood, 2)
				.AddTile(TileID.WorkBenches)
				.Register();
		}

		// Peeks into the player data loop to render up-to-date stat cards
		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			var player = Main.LocalPlayer;
			var gPlayer = player.GetModPlayer<GrowthPlayer>();

			var line = new TooltipLine(Mod, "StaffKillsTooltip", $"Current Cava Kills: {gPlayer.CavaKills}")
			{
				OverrideColor = new Color(255, 215, 0) // Gold Text
			};
			tooltips.Add(line);
		}
	}
}
