
using MonoMod.Cil;
using StickyWeapons.Items;
using System;
using System.Reflection;
using Terraria;
using Terraria.ID;

namespace StickyWeapons;

partial class StickyWeapons
{
    private static void IL_Projectile_FillWhipControlPoints(MonoMod.Cil.ILContext il)
    {
        var cursor = new ILCursor(il);
        var plrType = typeof(Item);
        var useAnimationFld = plrType.GetField("useAnimation", BindingFlags.Public | BindingFlags.Instance);
        if (!cursor.TryGotoNext(i => i.MatchLdfld(useAnimationFld)))
            return;
        cursor.Index++;
        cursor.EmitLdloc(10);
        cursor.EmitDelegate<Func<int, Player, int>>((num, plr) =>
        {
            if (plr.HeldItem.ModItem is not StickyItem sticky)
                return num;
            int animation = 0;
            foreach (var item in sticky.ItemSet)
            {
                var current = ContentSamples.ItemsByType[item.type].useAnimation;
                if(current > animation)
                    animation = current;
            }
            return animation;
        });
    }
}
