using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StickyWeapons.Items;

public class ReturnStickyBag : ModItem
{
    public override void SetDefaults()
    {
        Item.width = Item.height = 32;
        Item.value = 1;
        Item.rare = ItemRarityID.Cyan;
        Item.maxStack = 114514;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        if (items == null) return;
        foreach (var item in items)
            tooltips.Add(StickyUtils.GetNameLine(item));
        base.ModifyTooltips(tooltips);
    }

    public Item[] items;

    public override bool CanRightClick() => items != null;

    public override string Texture => "Terraria/Images/Item_" + ItemID.KingSlimeBossBag;

    public override void RightClick(Player player)
    {
        for (int n = 0; n < 500; n++)
        {
            var fac = n / 500f - 0.5f;
            var color = Main.hslToRgb(fac * 0.1f + 0.6f, 1f, .75f);
            fac *= MathHelper.TwoPi;
            var position = (fac * 3).ToRotationVector2() * (MathF.Sin(5 * fac) - .5f);
            Dust dust = Dust.NewDustPerfect(player.Center + position * 256, 278, new Vector2(-position.Y, position.X), 100, color, 1f);
            dust.scale = 0.4f + Main.rand.NextFloat(-1, 1) * 0.1f;
            dust.fadeIn = 0.4f + Main.rand.NextFloat() * 0.3f;
            dust.fadeIn *= .5f;
            dust.noGravity = true;
            dust.velocity *= (3f + Main.rand.NextFloat() * 4f) * 2;
        }
        foreach (var item in items)
        {
            player.QuickSpawnItem(Item.GetSource_GiftOrReward(), item, item.stack);
        }
    }
}