using Microsoft.Xna.Framework;
using MonoMod.Cil;
using StickyWeapons.Items;
using System;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.Player;

namespace StickyWeapons;

partial class StickyWeapons
{   
    static void IL_Player_ItemCheck_OwnerOnlyCode(ILContext il)
    {
        var cursor = new ILCursor(il);

        if (!cursor.TryGotoNext(i => i.MatchBr(out _)))
            return;
        cursor.EmitLdarg0();
        cursor.EmitDelegate<Func<bool, Player, bool>>((flag, player) =>
        {
            if (player.HeldItem.ModItem is not StickyItem sticky) return flag;
            return flag || player.GetModPlayer<StickyPlayer>().moreTimeShoot;
        });

        var plrType = typeof(Player);

        var useBucketMethod = plrType.GetMethod("ItemCheck_UseBuckets", BindingFlags.NonPublic | BindingFlags.Instance);
        var tryDestroyingDronesMethod = plrType.GetMethod("ItemCheck_TryDestroyingDrones", BindingFlags.NonPublic | BindingFlags.Instance);
        if (useBucketMethod == null || tryDestroyingDronesMethod == null)
            return;
        if (!cursor.TryGotoNext(i => i.MatchCall(useBucketMethod)))
            return;
        cursor.Index++;
        var index = cursor.Index;
        if (!cursor.TryGotoNext(i => i.MatchCall(tryDestroyingDronesMethod)))
            return;
        cursor.Index -= 2;

        var index2 = cursor.Index;

        cursor.Index = index;

        cursor.RemoveRange(index2 - index);

        cursor.EmitLdarg0();
        cursor.EmitLdarg2();

        cursor.EmitDelegate<Action<Player, Item>>((player, item) =>
        {
            var stickyPlr = player.GetModPlayer<StickyPlayer>();
            if (!player.channel)
            {
                if (stickyPlr.index == 0) // stickyPlr.max - 1
                    player.toolTime = player.itemTime;
            }
            else
            {
                if (stickyPlr.index == 0)
                    player.toolTime--;
                if (player.toolTime < 0)
                    player.toolTime = CombinedHooks.TotalUseTime(item.useTime, player, item);
            }
        });
        return;
    }
    [Obsolete]
    private static void On_Player_ItemCheck_OwnerOnlyCode(On_Player.orig_ItemCheck_OwnerOnlyCode orig, Player self, ref ItemCheckContext context, Item sItem, int weaponDamage, Rectangle heldItemFrame)
    {
        var stickyPlr = self.GetModPlayer<StickyPlayer>();
        if (self.HeldItem.ModItem is StickyItem sticky)
        {
            //stickyPlr.items = sticky.ItemSet;
            //stickyPlr.index = -1;
            //stickyPlr.max = stickyPlr.items.Length;
        }
        else
        {
            //stickyPlr.items = null;
            //stickyPlr.index = -1;
            //stickyPlr.max = -1;
            orig.Invoke(self, ref context, sItem, weaponDamage, heldItemFrame);
            return;
        }
        bool flag = true;
        int type = sItem.type;
        if ((type == 65 || type == 676 || type == 723 || type == 724 || type == 757 || type == 674 || type == 675 || type == 989 || type == 1226 || type == 1227) && !self.ItemAnimationJustStarted)
            flag = false;

        if (type == 5097 && self.ItemAnimationJustStarted)
            self._batbatCanHeal = true;

        if (type == 5094 && self.ItemAnimationJustStarted)
            self._spawnTentacleSpikes = true;

        if (type == 795 && self.ItemAnimationJustStarted)
            self._spawnBloodButcherer = true;

        if (type == 121 && self.ItemAnimationJustStarted)
            self._spawnVolcanoExplosion = true;

        if (type == 155 && self.ItemAnimationJustStarted)
            self._spawnMuramasaCut = true;

        if (type == 3852)
        {
            //TML: This is handled by Item.useLimitPerAnimation
            /*
            if (itemAnimation < itemAnimationMax - 12)
                flag = false;
            */

            if (self.altFunctionUse == 2 && !self.ItemAnimationJustStarted)
                flag = false;
        }

        //TML: Eventide and nightglow handled by Item.useLimitPerAnimation. Zenith use limit didn't do anything anyway
        /*
        if (type == 4956 && itemAnimation < itemAnimationMax - 3 * sItem.useTime)
            flag = false;

        if (type == 4952 && itemAnimation < itemAnimationMax - 8)
            flag = false;

        if (type == 4953 && itemAnimation < itemAnimationMax - 10)
            flag = false;
        */

        if (type == 5451 && self.ownedProjectileCounts[1020] > 0)
            flag = false;

        // Added by TML
        if (sItem.useLimitPerAnimation != null && self.ItemUsesThisAnimation >= sItem.useLimitPerAnimation.Value)
            flag = false;

        self.ItemCheck_TurretAltFeatureUse(sItem, flag);
        self.ItemCheck_MinionAltFeatureUse(sItem, flag);
        bool flag2 = self.itemAnimation > 0 && (self.ItemTimeIsZero || stickyPlr.moreTimeShoot) && flag;
        if (sItem.shootsEveryUse)
            flag2 = self.ItemAnimationJustStarted;

        if (sItem.shoot > 0 && flag2)
            self.ItemCheck_Shoot(self.whoAmI, sItem, weaponDamage);

        // Added by TML. #ItemTimeOnAllClients - TODO: item time application with these item types
        if (self.whoAmI != Main.myPlayer)
            return;

        self.ItemCheck_UseWiringTools(sItem);
        self.ItemCheck_UseLawnMower(sItem);
        self.ItemCheck_PlayInstruments(sItem);
        self.ItemCheck_UseBuckets(sItem);
        if (!self.channel)
        {
            if (stickyPlr.index == stickyPlr.max - 1)
                self.toolTime = self.itemTime;
        }
        else
        {
            if (stickyPlr.index == 0)
                self.toolTime--;
            if (self.toolTime < 0)
                self.toolTime = sItem.useTime;
        }

        self.ItemCheck_TryDestroyingDrones(sItem);
        self.ItemCheck_UseMiningTools(sItem);
        self.ItemCheck_UseTeleportRod(sItem);
        self.ItemCheck_UseLifeCrystal(sItem);
        self.ItemCheck_UseLifeFruit(sItem);
        self.ItemCheck_UseManaCrystal(sItem);
        self.ItemCheck_UseDemonHeart(sItem);
        self.ItemCheck_UseMinecartPowerUp(sItem);
        self.ItemCheck_UseTorchGodsFavor(sItem);
        self.ItemCheck_UseArtisanLoaf(sItem);
        self.ItemCheck_UseEventItems(sItem);
        self.ItemCheck_UseBossSpawners(self.whoAmI, sItem);
        self.ItemCheck_UseCombatBook(sItem);
        self.ItemCheck_UsePeddlersSatchel(sItem);
        self.ItemCheck_UsePetLicenses(sItem);
        self.ItemCheck_UseShimmerPermanentItems(sItem);
        if (sItem.type == 4095 && self.itemAnimation == 2)
            Main.LocalGolfState.ResetGolfBall();

        self.PlaceThing(ref context);
        if (sItem.makeNPC > 0)
        {
            if (!Main.GamepadDisableCursorItemIcon && self.position.X / 16f - (float)tileRangeX - (float)sItem.tileBoost <= (float)tileTargetX && (self.position.X + (float)self.width) / 16f + (float)tileRangeX + (float)sItem.tileBoost - 1f >= (float)tileTargetX && self.position.Y / 16f - (float)tileRangeY - (float)sItem.tileBoost <= (float)tileTargetY && (self.position.Y + (float)self.height) / 16f + (float)tileRangeY + (float)sItem.tileBoost - 2f >= (float)tileTargetY)
            {
                self.cursorItemIconEnabled = true;
                Main.ItemIconCacheUpdate(sItem.type);
            }

            if (self.ItemTimeIsZero && self.itemAnimation > 0 && self.controlUseItem)
                self.ItemCheck_ReleaseCritter(sItem);
        }

        if (self.boneGloveItem != null && !self.boneGloveItem.IsAir && self.boneGloveTimer == 0 && self.itemAnimation > 0 && sItem.damage > 0)
        {
            self.boneGloveTimer = 60;
            Vector2 center = self.Center;
            Vector2 vector = self.DirectionTo(self.ApplyRangeCompensation(0.2f, center, Main.MouseWorld)) * 10f;
            Projectile.NewProjectile(self.GetProjectileSource_Accessory(self.boneGloveItem), center.X, center.Y, vector.X, vector.Y, 532, 25, 5f, self.whoAmI);
        }

        /*
        if (((sItem.damage < 0 || sItem.type <= 0 || sItem.noMelee) && sItem.type != 1450 && sItem.type != 1991 && sItem.type != 3183 && sItem.type != 4821 && sItem.type != 3542 && sItem.type != 3779) || itemAnimation <= 0)
        */
        if (((sItem.damage < 0 || sItem.type <= 0 || sItem.noMelee) && sItem.type != 1450 && !ItemID.Sets.CatchingTool[sItem.type] && sItem.type != 3542 && sItem.type != 3779) || self.itemAnimation <= 0)
            return;

        self.ItemCheck_GetMeleeHitbox(sItem, heldItemFrame, out var dontAttack, out var itemRectangle);
        if (dontAttack)
            return;

        itemRectangle = self.ItemCheck_EmitUseVisuals(sItem, itemRectangle);

        /*
        if (Main.myPlayer == whoAmI && (sItem.type == 1991 || sItem.type == 3183 || sItem.type == 4821))
        */
        if (Main.myPlayer == self.whoAmI && ItemID.Sets.CatchingTool[sItem.type])
            itemRectangle = self.ItemCheck_CatchCritters(sItem, itemRectangle);

        if (sItem.type == 3183 || sItem.type == 4821)
        {
            bool[] shouldIgnore = self.ItemCheck_GetTileCutIgnoreList(sItem);
            self.ItemCheck_CutTiles(sItem, itemRectangle, shouldIgnore);
        }

        if (sItem.damage > 0)
        {
            self.UpdateMeleeHitCooldowns();
            float knockBack = self.GetWeaponKnockback(sItem, sItem.knockBack);
            // Knockback glove, buff and psycho knife moved to UpdateEquips, Update
            /*
            float num = 1f;
            if (kbGlove)
                num += 1f;

            if (kbBuff)
                num += 0.5f;

            knockBack *= num;
            if (inventory[selectedItem].type == 3106)
                knockBack += knockBack * (1f - stealth);
            */

            bool[] shouldIgnore2 = self.ItemCheck_GetTileCutIgnoreList(sItem);
            self.ItemCheck_CutTiles(sItem, itemRectangle, shouldIgnore2);
            self.ItemCheck_MeleeHitNPCs(sItem, itemRectangle, weaponDamage, knockBack);
            self.ItemCheck_MeleeHitPVP(sItem, itemRectangle, weaponDamage, knockBack);
            self.ItemCheck_EmitHammushProjectiles(self.whoAmI, sItem, itemRectangle, weaponDamage);
        }
    }
}
