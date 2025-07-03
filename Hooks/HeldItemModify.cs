using MonoMod.Cil;
namespace StickyWeapons;

partial class StickyWeapons
{
    static void IL_PlayerDrawLayers_DrawPlayer_27_HeldItem_Sticky(ILContext il)
    {
        var c = new ILCursor(il);
        if (!c.TryGotoNext(i => i.MatchStloc(3)))
            return;
        c.EmitLdloc0();
        c.EmitDelegate(StickyUtils.GetWeaponTextureFromItem);
        if (!c.TryGotoNext(i => i.MatchStloc(5)))
            return;
        c.EmitLdloc0();
        c.EmitDelegate(StickyUtils.GetWeaponFrameFromItem);
    }
}
