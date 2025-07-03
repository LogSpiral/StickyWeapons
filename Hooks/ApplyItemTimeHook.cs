using MonoMod.Cil;
using System;
using Terraria;
namespace StickyWeapons;

partial class StickyWeapons
{
    static void IL_Player_ApplyItemTime(ILContext il)
    {
        var cursor = new ILCursor(il);
        cursor.EmitLdarg0();
        cursor.EmitDelegate<Action<Player>>(player => player.GetModPlayer<StickyPlayer>().moreTimeShoot = true);
    }
}