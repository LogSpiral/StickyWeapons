using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
namespace StickyWeapons;
public partial class StickyWeapons : Mod
{
    public static StickyWeapons Instance { get; private set; }

    public override void Load()
    {
        Instance = this;
        IL_Player.ApplyItemTime += IL_Player_ApplyItemTime;
        IL_PlayerDrawLayers.DrawPlayer_27_HeldItem += IL_PlayerDrawLayers_DrawPlayer_27_HeldItem_Sticky;
        On_Player.ItemCheck_Inner += On_Player_ItemCheck_Inner_Sticky_OnNew;
        IL_Player.ItemCheck_OwnerOnlyCode += IL_Player_ItemCheck_OwnerOnlyCode;
        IL_Player.ProcessHitAgainstNPC += IL_Player_ProcessHitAgainstNPC;
        On_Player.ApplyAttackCooldown += On_Player_ApplyAttackCooldown;
        IL_Projectile.FillWhipControlPoints += IL_Projectile_FillWhipControlPoints;
        base.Load();
    }


}