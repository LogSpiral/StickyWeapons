using MonoMod.Cil;
using StickyWeapons.Items;
using System;
using System.Reflection;
using Terraria;
namespace StickyWeapons;

partial class StickyWeapons
{
    static void IL_Player_ProcessHitAgainstNPC(MonoMod.Cil.ILContext il)
    {
        var cursor = new ILCursor(il);
        var plrType = typeof(Player);
        var setMeleeHitCoolDownMethod = plrType.GetMethod("SetMeleeHitCooldown", BindingFlags.Public | BindingFlags.Instance);
        if (!cursor.TryGotoNext(i => i.MatchCall(setMeleeHitCoolDownMethod)))
            return;
        cursor.EmitLdarg1();
        cursor.EmitLdarg0();

        cursor.EmitDelegate<Func<int, Item, Player, int>>((num, item, plr) =>
        {
            if (item.ModItem is not StickyItem stickyItem)
                return num;
            var mplr = plr.GetModPlayer<StickyPlayer>();
            if (mplr.index != mplr.max - 1)
                return 0;
            return num;
        });
    }
}
