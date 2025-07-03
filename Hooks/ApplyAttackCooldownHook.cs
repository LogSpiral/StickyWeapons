using StickyWeapons.Items;
using System;
using Terraria;

namespace StickyWeapons;

partial class StickyWeapons
{
    private static void On_Player_ApplyAttackCooldown(On_Player.orig_ApplyAttackCooldown orig, Player self)
    {
        if (self.HeldItem.ModItem is not StickyItem sticky)
        {
            orig.Invoke(self);
            return;
        }
        int count = 0;
        foreach (var item in sticky.ItemSet)
        {
            if (item.damage > 0 && !item.noMelee && item.useAnimation > 0)
                count++;
        }
        if (count == 0) count = 1;
        self.attackCD = Math.Max(1, (int)(self.itemAnimationMax * 0.33 / count));

    }
}
