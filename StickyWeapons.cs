using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Achievements;
using Terraria.GameContent.Golf;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using Terraria.UI;
using static Mono.Cecil.Cil.OpCodes;
using static Terraria.Player;

namespace StickyWeapons
{
    public class StickyPlayer : ModPlayer
    {
        public Item[] items = null;
        public int index = -1;
        public int max = -1;
        public bool moreTimeShoot;
        public override void ResetEffects()
        {
            base.ResetEffects();
        }
    }
    public static class StickyFunc
    {
        internal static bool IsNotTheSameAs(this Item item, Item compareItem)
        {
            if (item.netID == compareItem.netID && item.stack == compareItem.stack)
                return item.prefix != compareItem.prefix;

            return true;
        }
        public static void StickyDrawInInventory(this Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin)
        {
            var tex = TextureAssets.Item[item.type];
            var _scale = MathHelper.Max(tex.Width(), tex.Height());
            _scale = _scale > 30 ? 30f / _scale : 1;
            if (item.ModItem == null || item.ModItem.PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, _scale))
            {
                var value = TextureAssets.Item[item.type].Value;
                var ani = Main.itemAnimations[item.type];

                spriteBatch.Draw(value, position + new Vector2(4), (ani == null) ? value.Frame() : ani.GetFrame(value), drawColor, 0, value.Size() * .5f / new Vector2(1, (ani == null) ? 1 : ani.FrameCount), _scale, 0, 0);
            }
        }
    }
    public delegate void ItemCheck_GetMeleeHitboxDelegate(Player player, Item item, Rectangle drawHitbox, out bool dontAttack, out Rectangle itemRectangle);
    //public enum NetDataID
    //{
    //    StickyPlayer
    //}
    public class StickyWeapons : Mod
    {
        //ILHook hook1;
        //ILHook hook2;
        //public override void HandlePacket(BinaryReader reader, int whoAmI)
        //{
        //    var dataType = (NetDataID)reader.ReadByte();
        //    if (Main.netMode == NetmodeID.Server)
        //    {
        //        switch (dataType)
        //        {
        //            case NetDataID.StickyPlayer:
        //                {
        //                    float direct = reader.ReadSingle();
        //                    var HitboxPosition = reader.ReadPackedVector2();

        //                    CoolerItemVisualEffectPlayer modPlayer = Main.player[whoAmI].GetModPlayer<CoolerItemVisualEffectPlayer>();
        //                    modPlayer.direct = direct;
        //                    modPlayer.HitboxPosition = HitboxPosition;


        //                    ModPacket packet = CoolerItemVisualEffect.Instance.GetPacket();
        //                    packet.Write((byte)MessageType.rotationDirect);
        //                    packet.Write(direct);
        //                    packet.WritePackedVector2(HitboxPosition);

        //                    packet.Write((byte)whoAmI);
        //                    packet.Send(-1, whoAmI);
        //                    return;
        //                }
        //        }
        //    }
        //    else 
        //    {
        //        switch (dataType) 
        //        {

        //        }
        //    }
        //}
        public override void Load()
        {
            //IL.Terraria.Player.ApplyItemTime += Player_ApplyItemTime_Sticky;
            //IL.Terraria.Player.ItemCheck_Inner += Player_ItemCheck_InnerSticky;//_ShootLoop

            On_Player.ApplyItemTime += Player_ApplyItemTime_Sticky_On;
            //On.Terraria.Player.ItemCheck_GetMeleeHitbox += Player_ItemCheck_GetMeleeHitbox;
            //On_Player.ItemCheck_Inner += Player_ItemCheck_Inner_Sticky_On;
            On_Player.ItemCheck_Inner += On_Player_ItemCheck_Inner_Sticky_OnNew;
            On_Player.ItemCheck_OwnerOnlyCode += On_Player_ItemCheck_OwnerOnlyCode;
            //On_PlayerDrawLayers.DrawPlayer_27_HeldItem += On_PlayerDrawLayers_DrawPlayer_27_HeldItem;
            IL_PlayerDrawLayers.DrawPlayer_27_HeldItem += IL_PlayerDrawLayers_DrawPlayer_27_HeldItem_Sticky;
            //On.Terraria.Player.ge

            base.Load();
        }
        public static string testText;
        private void IL_PlayerDrawLayers_DrawPlayer_27_HeldItem_Sticky(ILContext il)
        {
            var c = new ILCursor(il);
            if (!c.TryGotoNext(i => i.MatchStloc(3)))
                return;
            c.Emit(Ldloc_0);
            c.EmitDelegate(GetWeaponTextureFromItem);
            if (!c.TryGotoNext(i => i.MatchStloc(5)))
                return;
            c.Emit(Ldloc_0);
            c.EmitDelegate(GetWeaponFrameFromItem);
        }
        public static Texture2D GetWeaponTextureFromItem(Texture2D texture, Item item)
        {
            var moditem = item.ModItem;
            if (moditem is StickyItem sticky && sticky.complexTexture != null)
            {
                return sticky.complexTexture;
            }
            return texture;
        }
        public static Rectangle GetWeaponFrameFromItem(Rectangle rectangle, Item item)
        {
            var moditem = item.ModItem;
            if (moditem is StickyItem sticky && sticky.complexTexture != null)
            {
                return sticky.complexTexture.Frame();
            }
            return rectangle;
        }
        private void On_Player_ItemCheck_OwnerOnlyCode(On_Player.orig_ItemCheck_OwnerOnlyCode orig, Player self, ref ItemCheckContext context, Item sItem, int weaponDamage, Rectangle heldItemFrame)
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

        private void On_Player_ItemCheck_Inner_Sticky_OnNew(On_Player.orig_ItemCheck_Inner orig, Player self)
        {
            var stickyPlr = self.GetModPlayer<StickyPlayer>();
            if (self.HeldItem.ModItem is StickyItem sticky)
            {
                stickyPlr.items = sticky.ItemSet;
                stickyPlr.index = -1;
                stickyPlr.max = stickyPlr.items.Length;
            }
            else
            {
                stickyPlr.items = null;
                stickyPlr.index = -1;
                stickyPlr.max = -1;
                orig.Invoke(self);
                return;
            }
            if (self.CCed)
            {
                self.channel = false;
                self.itemAnimation = (self.itemAnimationMax = 0);
                return;
            }

            //float heightOffsetHitboxCenter = self.HeightOffsetHitboxCenter;
            //Item item = self.inventory[self.selectedItem];
            bool flag = false;
            stickyPlr.moreTimeShoot = false;
            float heightOffsetHitboxCenter = self.HeightOffsetHitboxCenter;
        myLabel:
            stickyPlr.index++;
            if (stickyPlr.index >= stickyPlr.max)
            {
                if (Main.myPlayer == self.whoAmI)
                {
                    if (self.selectedItem == 58)
                        Main.mouseItem = self.HeldItem;
                    else
                        Main.mouseItem.SetDefaults();
                }
                return;
            }
            Item item = stickyPlr.items[stickyPlr.index];
            ItemCheckContext context = default(ItemCheckContext);
            if (Main.myPlayer == self.whoAmI)
            {
                if (PlayerInput.ShouldFastUseItem)
                {
                    self.controlUseItem = true;
                    flag = true;
                }

                if (!self.cursorItemIconEnabled && item.stack > 0 && item.fishingPole > 0)
                {
                    self.Fishing_GetBait(out var bait);
                    if (bait != null)
                    {
                        self.cursorItemIconEnabled = true;
                        self.cursorItemIconID = bait.type;
                        self.cursorItemIconPush = 6;
                    }
                }

                if (!self.cursorItemIconEnabled && item.stack > 0 && (item.type == 779 || item.type == 5134))
                {
                    for (int i = 54; i < 58; i++)
                    {
                        if (self.inventory[i].ammo == item.useAmmo && self.inventory[i].stack > 0)
                        {
                            self.cursorItemIconEnabled = true;
                            self.cursorItemIconID = self.inventory[i].type;
                            self.cursorItemIconPush = 10;
                            break;
                        }
                    }

                    if (!self.cursorItemIconEnabled)
                    {
                        for (int j = 0; j < 54; j++)
                        {
                            if (self.inventory[j].ammo == item.useAmmo && self.inventory[j].stack > 0)
                            {
                                self.cursorItemIconEnabled = true;
                                self.cursorItemIconID = self.inventory[j].type;
                                self.cursorItemIconPush = 10;
                                break;
                            }
                        }
                    }
                }
            }

            // #2351
            // TML is motivated to bring the itemAnimation and itemTime counters to parity, fixing desync bugs with autoReuse items and providing clearer behavior.
            // The flow of this method has changed as follows...
            //
            // VANILLA:
            // 1. Reuse delay is applied
            // 2. Item animation is applied if button is pressed
            // 3. Item animation is reduced
            // 4. 'releaseUseItem' is set
            // 5. Item time is reduced
            // 6. Hold / Use styles are invoked
            // 7. Item logic applies item time if (itemAnimation > 0 && itemTime == 0)
            // 8. More item logic/and effects (consumables, teleportations) if (itemAnimation > 0)
            //
            // TML:
            // 1. Item animation is reduced
            // 2. Item time is reduced
            // 3. Reuse delay is applied
            // 4. Item animation is applied if button is pressed
            // 5. 'releaseUseItem' is set
            // 6. Hold / Use styles are invoked
            // 7. Item logic applies item time if (itemAnimation > 0 && itemTime == 0)
            // 8. More item logic/and effects (consumables, teleportations) if (itemAnimation > 0)
            //
            // At the end of ItemCheck:
            //   VANILLA: itemAnimation goes from itemAnimationMax-1 to 0, before it can restart
            //   TML:     itemAnimation goes from itemAnimationMax to 1, before it can restart
            //
            //   VANILLA: If the item has autoReuse, then it can restart at itemAnimation = 1, making the actual animation one frame shorter than expected.
            //            If a reusable item has equal itemAnimation and itemTime, the animation will be faster, and they will start to fire at different times
            //
            //   TML:     ItemCheck_HandleMPItemAnimation unnecessary. Animation times for modded items as written. See Item.ApplyItemAnimationCompensations
            //
            //   VANILLA: All items which don't have autoReuse get 1 frame where itemAnimation == 0, this is often used to despawn projectiles
            //   TML:     There will be no frame with itemAnimation == 0 if an item is 'reused immediately', via autoReuse, knockbackGlove, or perfect click timing.
            //            bound projectiles should despawn in the frame of itemAnimation <= 1, after doing damage.
            //            Player.ItemAnimationEndingOrEnded has been made for this purpose but its use is not recommended due to potential for multiplayer desync.
            //            Better to have ai counters for projectile lifetime set to itemAnimationMax on spawn
            //
            //   VANILLA: hitbox calulation and duration is based on an itemAnimation value between itemAnimationMax-1 and 1, resulting in itemAnimationMax-1 frames of hitbox
            //   TML:     hitbox lasts the same length as itemAnimation, slightly more backswing (rotation) in the first frame
            //
            //   VANILLA: itemTime goes from itemTimeMax to 1, before it can restart, 0 means not using item
            //   TML:     no change

            goto DecrementItemAnimation;

        ReuseDelayAndAnimationStart:
            self.ItemCheck_HandleMount();
            int weaponDamage = self.GetWeaponDamage(item);
            self.ItemCheck_HandleMPItemAnimation(item);
            self.ItemCheck_HackHoldStyles(item);
            if (self.itemAnimation < 0)
                self.itemAnimation = 0;

            if (self.itemTime < 0)
                self.itemTime = 0;

            if (self.itemAnimation == 0 && self.reuseDelay > 0)
                self.ApplyReuseDelay();

            self.UpdatePlacementPreview(item);
            if (self.itemAnimation == 0 && self.altFunctionUse == 2)
                self.altFunctionUse = 0;

            bool flag2 = true;
            if (self.gravDir == -1f && GolfHelper.IsPlayerHoldingClub(self))
                flag2 = false;

            if (flag2 && self.controlUseItem && self.releaseUseItem && self.itemAnimation == 0 && item.useStyle != 0)
            {
                if (self.altFunctionUse == 1)
                    self.altFunctionUse = 2;

                if (item.shoot == 0)
                    self.itemRotation = 0f;

                bool flag3 = self.ItemCheck_CheckCanUse(item);
                if (!stickyPlr.moreTimeShoot)
                {
                    if (item.potion && flag3)
                        self.ApplyPotionDelay(item);

                    if (item.mana > 0 && flag3 && self.whoAmI == Main.myPlayer && item.buffType != 0 && item.buffTime != 0)
                        self.AddBuff(item.buffType, item.buffTime);

                    if (item.shoot <= 0 || !ProjectileID.Sets.MinionTargettingFeature[item.shoot] || self.altFunctionUse != 2)
                        self.ItemCheck_ApplyPetBuffs(item);

                    if (self.whoAmI == Main.myPlayer && self.gravDir == 1f && item.mountType != -1 && self.mount.CanMount(item.mountType, self))
                        self.mount.SetMount(item.mountType, self);

                    if ((item.shoot <= 0 || !ProjectileID.Sets.MinionTargettingFeature[item.shoot] || self.altFunctionUse != 2) && flag3 && self.whoAmI == Main.myPlayer && item.shoot >= 0 && (ProjectileID.Sets.LightPet[item.shoot] || Main.projPet[item.shoot]))
                        self.FreeUpPetsAndMinions(item);

                    if (flag3)
                    {
                        stickyPlr.moreTimeShoot = true;
                        self.ItemCheck_StartActualUse(item);

                    }
                }

            }

            bool flag4 = self.controlUseItem;
            if (self.mount.Active && self.mount.Type == 8)
                flag4 = self.controlUseItem || self.controlUseTile;

            if (!flag4)
                self.channel = false;

            goto ReleaseUseItem;

        DecrementItemAnimation:
            Item item2 = ((self.itemAnimation > 0) ? self.lastVisualizedSelectedItem : item);
            Rectangle drawHitbox = Item.GetDrawHitbox(item2.type, self);
            self.compositeFrontArm.enabled = false;
            self.compositeBackArm.enabled = false;
            if (self.itemAnimation > 0)
            {
                if (item.mana > 0)
                    self.ItemCheck_ApplyManaRegenDelay(item);

                if (Main.dedServ)
                {
                    self.itemHeight = item.height;
                    self.itemWidth = item.width;
                }
                else
                {
                    self.itemHeight = drawHitbox.Height;
                    self.itemWidth = drawHitbox.Width;
                }
                if (stickyPlr.index == 0)
                    self.itemAnimation--;
                if (self.itemAnimation == 0 && self.whoAmI == Main.myPlayer)
                    PlayerInput.TryEndingFastUse();
            }

            goto DecrementItemTime;

        ReleaseUseItem:
            self.releaseUseItem = !self.controlUseItem;
            goto HoldAndUseStyle;

        DecrementItemTime:
            if (self.itemTime > 0)
            {
                if (stickyPlr.index == 0)
                    self.itemTime--;
                if (self.ItemTimeIsZero && self.whoAmI == Main.myPlayer && !self.JustDroppedAnItem)
                {
                    int type = item.type;
                    if (type == 65 || type == 724 || type == 989 || type == 1226)
                        self.EmitMaxManaEffect();
                }
            }

            goto ReuseDelayAndAnimationStart;

        HoldAndUseStyle:
            ItemLoader.HoldItem(item, self);

            if (self.itemAnimation > 0)
                self.ItemCheck_ApplyUseStyle(heightOffsetHitboxCenter, item2, drawHitbox);
            else
                self.ItemCheck_ApplyHoldStyle(heightOffsetHitboxCenter, item2, drawHitbox);

            if (!self.JustDroppedAnItem)
            {
                self.ItemCheck_EmitHeldItemLight(item);
                self.ItemCheck_EmitFoodParticles(item);
                self.ItemCheck_EmitDrinkParticles(item);

                // TML attempts to make ApplyItemTime calls run on remote players, so this check is removed. #ItemTimeOnAllClients
                if (self.whoAmI == Main.myPlayer || true)
                    self.ItemCheck_OwnerOnlyCode(ref context, item, weaponDamage, drawHitbox);

                if (self.ItemTimeIsZero && self.itemAnimation > 0)
                {
                    if (ItemLoader.UseItem(item, self) == true)
                        self.ApplyItemTime(item, callUseItem: false);

                    if (item.hairDye >= 0)
                    {
                        self.ApplyItemTime(item);
                        if (self.whoAmI == Main.myPlayer)
                        {
                            self.hairDye = item.hairDye;
                            NetMessage.SendData(4, -1, -1, null, self.whoAmI);
                        }
                    }

                    if (item.healLife > 0 || item.healMana > 0)
                    {
                        self.ApplyLifeAndOrMana(item);
                        self.ApplyItemTime(item);
                        if (Main.myPlayer == self.whoAmI && item.type == 126 && self.breath == 0)
                            AchievementsHelper.HandleSpecialEvent(self, 25);
                    }

                    if (item.buffType > 0)
                    {
                        if (self.whoAmI == Main.myPlayer && item.buffType != 90 && item.buffType != 27)
                            self.AddBuff(item.buffType, item.buffTime);

                        self.ApplyItemTime(item);
                    }

                    if (item.type == 678)
                    {
                        if (Main.getGoodWorld)
                        {
                            self.ApplyItemTime(item);
                            if (self.whoAmI == Main.myPlayer)
                            {
                                for (int k = 0; k < 3; k++)
                                {
                                    int type2 = 0;
                                    int timeToAdd = 108000;
                                    switch (Main.rand.Next(18))
                                    {
                                        case 0:
                                            type2 = 16;
                                            break;
                                        case 1:
                                            type2 = 111;
                                            break;
                                        case 2:
                                            type2 = 114;
                                            break;
                                        case 3:
                                            type2 = 8;
                                            break;
                                        case 4:
                                            type2 = 105;
                                            break;
                                        case 5:
                                            type2 = 17;
                                            break;
                                        case 6:
                                            type2 = 116;
                                            break;
                                        case 7:
                                            type2 = 5;
                                            break;
                                        case 8:
                                            type2 = 113;
                                            break;
                                        case 9:
                                            type2 = 7;
                                            break;
                                        case 10:
                                            type2 = 6;
                                            break;
                                        case 11:
                                            type2 = 104;
                                            break;
                                        case 12:
                                            type2 = 115;
                                            break;
                                        case 13:
                                            type2 = 2;
                                            break;
                                        case 14:
                                            type2 = 9;
                                            break;
                                        case 15:
                                            type2 = 3;
                                            break;
                                        case 16:
                                            type2 = 117;
                                            break;
                                        case 17:
                                            type2 = 1;
                                            break;
                                    }

                                    self.AddBuff(type2, timeToAdd);
                                }
                            }
                        }
                        else
                        {
                            self.ApplyItemTime(item);
                            if (self.whoAmI == Main.myPlayer)
                            {
                                self.AddBuff(20, 216000);
                                self.AddBuff(22, 216000);
                                self.AddBuff(23, 216000);
                                self.AddBuff(24, 216000);
                                self.AddBuff(30, 216000);
                                self.AddBuff(31, 216000);
                                self.AddBuff(32, 216000);
                                self.AddBuff(33, 216000);
                                self.AddBuff(35, 216000);
                                self.AddBuff(36, 216000);
                                self.AddBuff(68, 216000);
                            }
                        }
                    }
                }

                if ((item.type == 50 || item.type == 3124 || item.type == 3199 || item.type == 5358) && self.itemAnimation > 0)
                {
                    if (Main.rand.NextBool(2))
                        Dust.NewDust(self.position, self.width, self.height, 15, 0f, 0f, 150, default(Color), 1.1f);

                    if (self.ItemTimeIsZero)
                    {
                        self.ApplyItemTime(item);
                    }
                    else if (self.itemTime == self.itemTimeMax / 2)
                    { // item.useTime -> itemTimeMax
                        for (int l = 0; l < 70; l++)
                        {
                            Dust.NewDust(self.position, self.width, self.height, 15, self.velocity.X * 0.5f, self.velocity.Y * 0.5f, 150, default(Color), 1.5f);
                        }

                        self.RemoveAllGrapplingHooks();
                        self.Spawn(PlayerSpawnContext.RecallFromItem);
                        for (int m = 0; m < 70; m++)
                        {
                            Dust.NewDust(self.position, self.width, self.height, 15, 0f, 0f, 150, default(Color), 1.5f);
                        }
                    }
                }

                if ((item.type == 4263 || item.type == 5360) && self.itemAnimation > 0)
                {
                    Vector2 vector = Vector2.UnitY.RotatedBy((float)self.itemAnimation * ((float)Math.PI * 2f) / 30f) * new Vector2(15f, 0f);
                    for (int n = 0; n < 2; n++)
                    {
                        if (Main.rand.NextBool(3))
                        {
                            Dust dust = Dust.NewDustPerfect(self.Bottom + vector, Dust.dustWater());
                            dust.velocity.Y *= 0f;
                            dust.velocity.Y -= 4.5f;
                            dust.velocity.X *= 1.5f;
                            dust.scale = 0.8f;
                            dust.alpha = 130;
                            dust.noGravity = true;
                            dust.fadeIn = 1.1f;
                        }
                    }

                    if (self.ItemTimeIsZero)
                    {
                        self.ApplyItemTime(item);
                    }
                    else if (self.itemTime == item.useTime / 2)
                    {
                        if (Main.netMode == 0)
                            self.MagicConch();
                        else if (Main.netMode == 1 && self.whoAmI == Main.myPlayer)
                            NetMessage.SendData(73, -1, -1, null, 1);
                    }
                }

                if ((item.type == 4819 || item.type == 5361) && self.itemAnimation > 0)
                {
                    Vector2 vector2 = Vector2.UnitY.RotatedBy((float)self.itemAnimation * ((float)Math.PI * 2f) / 30f) * new Vector2(15f, 0f);
                    for (int num = 0; num < 2; num++)
                    {
                        if (Main.rand.NextBool(3))
                        {
                            Dust dust2 = Dust.NewDustPerfect(self.Bottom + vector2, 35);
                            dust2.velocity.Y *= 0f;
                            dust2.velocity.Y -= 4.5f;
                            dust2.velocity.X *= 1.5f;
                            dust2.scale = 0.8f;
                            dust2.alpha = 130;
                            dust2.noGravity = true;
                            dust2.fadeIn = 1.1f;
                        }
                    }

                    if (self.ItemTimeIsZero)
                    {
                        self.ApplyItemTime(item);
                    }
                    else if (self.itemTime == item.useTime / 2)
                    {
                        if (Main.netMode == 0)
                            self.DemonConch();
                        else if (Main.netMode == 1 && self.whoAmI == Main.myPlayer)
                            NetMessage.SendData(73, -1, -1, null, 2);
                    }
                }

                if (item.type == 5359 && self.itemAnimation > 0)
                {
                    if (Main.rand.NextBool(2))
                    {
                        int num2 = Main.rand.Next(4);
                        Color color = Color.Green;
                        switch (num2)
                        {
                            case 0:
                            case 1:
                                color = new Color(100, 255, 100);
                                break;
                            case 2:
                                color = Color.Yellow;
                                break;
                            case 3:
                                color = Color.White;
                                break;
                        }

                        Dust dust3 = Dust.NewDustPerfect(Main.rand.NextVector2FromRectangle(self.Hitbox), 267);
                        dust3.noGravity = true;
                        dust3.color = color;
                        dust3.velocity *= 2f;
                        dust3.scale = 0.8f + Main.rand.NextFloat() * 0.6f;
                    }

                    if (self.ItemTimeIsZero)
                    {
                        self.ApplyItemTime(item);
                    }
                    else if (self.itemTime == item.useTime / 2)
                    {
                        if (Main.netMode == 0)
                            self.Shellphone_Spawn();
                        else if (Main.netMode == 1 && self.whoAmI == Main.myPlayer)
                            NetMessage.SendData(73, -1, -1, null, 3);
                    }
                }

                if (item.type == 2350 && self.itemAnimation > 0)
                {
                    if (self.ItemTimeIsZero)
                    {
                        self.ApplyItemTime(item);
                        SoundEngine.PlaySound(SoundID.Item3, self.position);
                        for (int num3 = 0; num3 < 10; num3++)
                        {
                            Main.dust[Dust.NewDust(self.position, self.width, self.height, 15, self.velocity.X * 0.2f, self.velocity.Y * 0.2f, 150, Color.Cyan, 1.2f)].velocity *= 0.5f;
                        }
                    }
                    else if (self.itemTime == 20)
                    {
                        SoundEngine.PlaySound(self.HeldItem.UseSound, self.position);
                        for (int num4 = 0; num4 < 70; num4++)
                        {
                            Main.dust[Dust.NewDust(self.position, self.width, self.height, 15, self.velocity.X * 0.2f, self.velocity.Y * 0.2f, 150, Color.Cyan, 1.2f)].velocity *= 0.5f;
                        }

                        self.RemoveAllGrapplingHooks();
                        bool flag5 = self.immune;
                        int num5 = self.immuneTime;
                        self.Spawn(PlayerSpawnContext.RecallFromItem);
                        self.immune = flag5;
                        self.immuneTime = num5;
                        for (int num6 = 0; num6 < 70; num6++)
                        {
                            Main.dust[Dust.NewDust(self.position, self.width, self.height, 15, 0f, 0f, 150, Color.Cyan, 1.2f)].velocity *= 0.5f;
                        }

                        if (ItemLoader.ConsumeItem(item, self) && item.stack > 0)
                            item.stack--;
                    }
                }

                if (item.type == 4870 && self.itemAnimation > 0)
                {
                    if (self.ItemTimeIsZero)
                    {
                        self.ApplyItemTime(item);
                        SoundEngine.PlaySound(SoundID.Item3, self.position);
                        for (int num7 = 0; num7 < 10; num7++)
                        {
                            Main.dust[Dust.NewDust(self.position, self.width, self.height, 15, self.velocity.X * 0.2f, self.velocity.Y * 0.2f, 150, Color.Cyan, 1.2f)].velocity *= 0.5f;
                        }
                    }
                    else if (self.itemTime == 20)
                    {
                        SoundEngine.PlaySound(self.HeldItem.UseSound, self.position);
                        for (int num8 = 0; num8 < 70; num8++)
                        {
                            Main.dust[Dust.NewDust(self.position, self.width, self.height, 15, self.velocity.X * 0.2f, self.velocity.Y * 0.2f, 150, Color.Cyan, 1.2f)].velocity *= 0.5f;
                        }

                        if (self.whoAmI == Main.myPlayer)
                            self.DoPotionOfReturnTeleportationAndSetTheComebackPoint();

                        for (int num9 = 0; num9 < 70; num9++)
                        {
                            Main.dust[Dust.NewDust(self.position, self.width, self.height, 15, 0f, 0f, 150, Color.Cyan, 1.2f)].velocity *= 0.5f;
                        }

                        if (ItemLoader.ConsumeItem(item, self) && item.stack > 0)
                            item.stack--;
                    }
                }

                if (item.type == 2351 && self.itemAnimation > 0)
                {
                    if (self.ItemTimeIsZero)
                    {
                        self.ApplyItemTime(item);
                    }
                    else if (self.itemTime == 2)
                    {
                        if (Main.netMode == 0)
                            self.TeleportationPotion();
                        else if (Main.netMode == 1 && self.whoAmI == Main.myPlayer)
                            NetMessage.SendData(73);

                        if (ItemLoader.ConsumeItem(item, self) && item.stack > 0)
                            item.stack--;
                    }
                }

                if (item.type == 2756 && self.itemAnimation > 0)
                {
                    if (self.ItemTimeIsZero)
                    {
                        self.ApplyItemTime(item);
                    }
                    else if (self.itemTime == 2)
                    {
                        if (self.whoAmI == Main.myPlayer)
                        {
                            self.Male = !self.Male;
                            if (Main.netMode == 1)
                                NetMessage.SendData(4, -1, -1, null, self.whoAmI);
                        }

                        if (ItemLoader.ConsumeItem(item, self) && item.stack > 0)
                            item.stack--;
                    }
                    else
                    {
                        float num10 = self.itemTimeMax;
                        num10 = (num10 - (float)self.itemTime) / num10;
                        float num11 = 44f;
                        float num12 = (float)Math.PI * 3f;
                        Vector2 vector3 = new Vector2(15f, 0f).RotatedBy(num12 * num10);
                        vector3.X *= self.direction;
                        for (int num13 = 0; num13 < 2; num13++)
                        {
                            int type3 = 221;
                            if (num13 == 1)
                            {
                                vector3.X *= -1f;
                                type3 = 219;
                            }

                            Vector2 vector4 = new(vector3.X, num11 * (1f - num10) - num11 + (float)(self.height / 2));
                            vector4 += self.Center;
                            int num14 = Dust.NewDust(vector4, 0, 0, type3, 0f, 0f, 100);
                            Main.dust[num14].position = vector4;
                            Main.dust[num14].noGravity = true;
                            Main.dust[num14].velocity = Vector2.Zero;
                            Main.dust[num14].scale = 1.3f;
                            Main.dust[num14].customData = this;
                        }
                    }
                }

                if (self.whoAmI == Main.myPlayer)
                {
                    if (((self.itemTimeMax != 0 && self.itemTime == self.itemTimeMax) | (!item.IsAir && item.IsNotTheSameAs(self.lastVisualizedSelectedItem))) && stickyPlr.index == 0)
                        self.lastVisualizedSelectedItem = self.HeldItem.Clone();
                }
                else
                {
                    self.lastVisualizedSelectedItem = item.Clone();
                }

                if (self.whoAmI == Main.myPlayer)
                {
                    /*
                    if (!dontConsumeWand && itemTime == (int)((float)item.useTime * tileSpeed) && item.tileWand > 0) {
                    */
                    if (!self.dontConsumeWand && self.itemTimeMax != 0 && self.itemTime == self.itemTimeMax && item.tileWand > 0)
                    {
                        int tileWand = item.tileWand;
                        for (int num15 = 0; num15 < 58; num15++)
                        {
                            if (tileWand == self.inventory[num15].type && self.inventory[num15].stack > 0)
                            {
                                if (ItemLoader.ConsumeItem(self.inventory[num15], self))
                                    self.inventory[num15].stack--;

                                if (self.inventory[num15].stack <= 0)
                                    self.inventory[num15] = new Item();

                                break;
                            }
                        }
                    }

                    if (self.itemTimeMax != 0 && stickyPlr.moreTimeShoot && item.consumable && !context.SkipItemConsumption)//self.itemTime == self.itemTimeMax
                    {
                        bool flag6 = true;
                        if (item.CountsAsClass(DamageClass.Ranged))
                        {
                            if (self.huntressAmmoCost90 && Main.rand.NextBool(10))
                                flag6 = false;

                            if (self.chloroAmmoCost80 && Main.rand.NextBool(5))
                                flag6 = false;

                            if (self.ammoCost80 && Main.rand.NextBool(5))
                                flag6 = false;

                            if (self.ammoCost75 && Main.rand.NextBool(4))
                                flag6 = false;
                        }

                        // Copied as-is from 1.3
                        if (item.CountsAsClass(DamageClass.Throwing))
                        {
                            if (self.ThrownCost50 && Main.rand.Next(100) < 50)
                                flag6 = false;

                            if (self.ThrownCost33 && Main.rand.Next(100) < 33)
                                flag6 = false;
                        }

                        if (item.IsACoin)
                            flag6 = true;

                        bool? flag7 = ItemID.Sets.ForceConsumption[item.type];
                        if (flag7.HasValue)
                            flag6 = flag7.Value;

                        if (flag6 && ItemLoader.ConsumeItem(item, self))
                        {
                            if (item.stack > 0)
                                item.stack--;

                            if (item.stack <= 0)
                            {
                                self.itemTime = self.itemAnimation;
                                Main.blockMouse = true;
                            }
                        }
                    }

                    if (item.stack <= 0 && self.itemAnimation == 0)
                        self.inventory[self.selectedItem] = new Item();

                    if (self.selectedItem == 58 && self.itemAnimation != 0)
                        Main.mouseItem = item.Clone();
                }
            }

            if (self.itemAnimation == 0)
                self.JustDroppedAnItem = false;

            if (self.whoAmI == Main.myPlayer && flag)
                PlayerInput.TryEndingFastUse();
            if (stickyPlr.index < stickyPlr.max) goto myLabel;
        }

        //private void Player_ItemCheck_GetMeleeHitbox(On.Terraria.Player.orig_ItemCheck_GetMeleeHitbox orig, Player self, Item sItem, Rectangle heldItemFrame, out bool dontAttack, out Rectangle itemRectangle)
        //{
        //    dontAttack = false;
        //    itemRectangle = new Rectangle((int)self.itemLocation.X, (int)self.itemLocation.Y, 32, 32);
        //    if (!Main.dedServ)
        //    {
        //        int num = heldItemFrame.Width;
        //        int num2 = heldItemFrame.Height;
        //        switch (sItem.type)
        //        {
        //            case 5094:
        //                num -= 10;
        //                num2 -= 10;
        //                break;
        //            case 5095:
        //                num -= 10;
        //                num2 -= 10;
        //                break;
        //            case 5096:
        //                num -= 12;
        //                num2 -= 12;
        //                break;
        //            case 5097:
        //                num -= 8;
        //                num2 -= 8;
        //                break;
        //        }

        //        itemRectangle = new Rectangle((int)self.itemLocation.X, (int)self.itemLocation.Y, num, num2);
        //        Main.NewText((itemRectangle, "index0"), Color.Red);
        //    }

        //    float adjustedItemScale = self.GetAdjustedItemScale(sItem);
        //    itemRectangle.Width = (int)(itemRectangle.Width * adjustedItemScale);
        //    itemRectangle.Height = (int)(itemRectangle.Height * adjustedItemScale);
        //    Main.NewText((itemRectangle, "index1"), Color.Red);

        //    if (self.direction == -1)
        //        itemRectangle.X -= itemRectangle.Width;

        //    if (self.gravDir == 1f)
        //        itemRectangle.Y -= itemRectangle.Height;

        //    if (sItem.useStyle == ItemUseStyleID.Swing)
        //    {
        //        if (self.itemAnimation < self.itemAnimationMax * 0.333)
        //        {
        //            if (self.direction == -1)
        //                itemRectangle.X -= (int)(itemRectangle.Width * 1.4 - itemRectangle.Width);

        //            itemRectangle.Width = (int)(itemRectangle.Width * 1.4);
        //            itemRectangle.Y += (int)(itemRectangle.Height * 0.5 * self.gravDir);
        //            itemRectangle.Height = (int)(itemRectangle.Height * 1.1);
        //        }
        //        else if (!(self.itemAnimation < self.itemAnimationMax * 0.666))
        //        {
        //            if (self.direction == 1)
        //                itemRectangle.X -= (int)(itemRectangle.Width * 1.2);

        //            itemRectangle.Width *= 2;
        //            itemRectangle.Y -= (int)((itemRectangle.Height * 1.4 - itemRectangle.Height) * self.gravDir);
        //            itemRectangle.Height = (int)(itemRectangle.Height * 1.4);
        //        }
        //        Main.NewText((itemRectangle, "index2"), Color.Red);

        //    }
        //    else if (sItem.useStyle == ItemUseStyleID.Thrust)
        //    {
        //        if (self.itemAnimation > self.itemAnimationMax * 0.666)
        //        {
        //            dontAttack = true;
        //        }
        //        else
        //        {
        //            if (self.direction == -1)
        //                itemRectangle.X -= (int)(itemRectangle.Width * 1.4 - itemRectangle.Width);

        //            itemRectangle.Width = (int)(itemRectangle.Width * 1.4);
        //            itemRectangle.Y += (int)(itemRectangle.Height * 0.6);
        //            itemRectangle.Height = (int)(itemRectangle.Height * 0.6);
        //            if (sItem.type == ItemID.Umbrella || sItem.type == ItemID.TragicUmbrella)
        //            {
        //                itemRectangle.Height += 14;
        //                itemRectangle.Width -= 10;
        //                if (self.direction == -1)
        //                    itemRectangle.X += 10;
        //            }
        //        }
        //    }
        //    Main.NewText((itemRectangle, "index3"), Color.Red);
        //    ItemLoader.UseItemHitbox(sItem, self, ref itemRectangle, ref dontAttack);
        //    Main.NewText((itemRectangle, "index4"), Color.Red);
        //    if (sItem.type == ItemID.BubbleWand && Main.rand.NextBool(3))
        //    {
        //        int num3 = -1;
        //        float x = itemRectangle.X + Main.rand.Next(itemRectangle.Width);
        //        float y = itemRectangle.Y + Main.rand.Next(itemRectangle.Height);
        //        if (Main.rand.NextBool(500))
        //            num3 = Gore.NewGore(default, new Vector2(x, y), default, 415, Main.rand.Next(51, 101) * 0.01f);
        //        else if (Main.rand.NextBool(250))
        //            num3 = Gore.NewGore(default, new Vector2(x, y), default, 414, Main.rand.Next(51, 101) * 0.01f);
        //        else if (Main.rand.NextBool(80))
        //            num3 = Gore.NewGore(default, new Vector2(x, y), default, 413, Main.rand.Next(51, 101) * 0.01f);
        //        else if (Main.rand.NextBool(10))
        //            num3 = Gore.NewGore(default, new Vector2(x, y), default, 412, Main.rand.Next(51, 101) * 0.01f);
        //        else if (Main.rand.NextBool(3))
        //            num3 = Gore.NewGore(default, new Vector2(x, y), default, 411, Main.rand.Next(51, 101) * 0.01f);

        //        if (num3 >= 0)
        //        {
        //            Main.gore[num3].velocity.X += self.direction * 2;
        //            Main.gore[num3].velocity.Y *= 0.3f;
        //        }
        //    }

        //    if (sItem.type == ItemID.NebulaBlaze)
        //        dontAttack = true;

        //    if (sItem.type == ItemID.SpiritFlame)
        //    {
        //        dontAttack = true;
        //        Vector2 vector = self.itemLocation + new Vector2(self.direction * 30, -8f);
        //        Vector2 value = vector - self.position;
        //        for (float num4 = 0f; num4 < 1f; num4 += 0.2f)
        //        {
        //            Vector2 position = Vector2.Lerp(self.oldPosition + value + new Vector2(0f, self.gfxOffY), vector, num4);
        //            Dust obj = Main.dust[Dust.NewDust(vector - Vector2.One * 8f, 16, 16, DustID.Shadowflame, 0f, -2f)];
        //            obj.noGravity = true;
        //            obj.position = position;
        //            obj.velocity = new Vector2(0f, (0f - self.gravDir) * 2f);
        //            obj.scale = 1.2f;
        //            obj.alpha = 200;
        //        }
        //    }
        //}

        private void Player_ItemCheck_Inner_Sticky_On(Terraria.On_Player.orig_ItemCheck_Inner orig, Player self, int i)
        {
            var stickyPlr = self.GetModPlayer<StickyPlayer>();
            if (self.HeldItem.ModItem is StickyItem sticky)
            {
                stickyPlr.items = sticky.ItemSet;
                stickyPlr.index = -1;
                stickyPlr.max = stickyPlr.items.Length;
            }
            else
            {
                stickyPlr.items = null;
                stickyPlr.index = -1;
                stickyPlr.max = -1;
                orig.Invoke(self);
                return;
            }
            if (self.CCed)
            {
                self.channel = false;
                self.itemAnimation = self.itemAnimationMax = 0;
                return;
            }
            var statType = typeof(Player);

            bool flag = false;
            stickyPlr.moreTimeShoot = false;
            float heightOffsetHitboxCenter = self.HeightOffsetHitboxCenter;
        myLabel:
            stickyPlr.index++;
            if (stickyPlr.index >= stickyPlr.max)
            {
                if (Main.myPlayer == self.whoAmI)
                {
                    if (self.selectedItem == 58)
                        Main.mouseItem = self.HeldItem;
                    else
                        Main.mouseItem.SetDefaults();
                }
                return;
            }// self.lastVisualizedSelectedItem = self.HeldItem; 
            //Main.NewText(self.lastVisualizedSelectedItem.Name);
            Item item = stickyPlr.items[stickyPlr.index];
            ItemCheckContext context = default(ItemCheckContext);

            if (Main.myPlayer == i && PlayerInput.ShouldFastUseItem)
                self.controlUseItem = true;
            goto DecrementItemAnimation;
        ReuseDelayAndAnimationStart:
            //ItemCheck_HandleMount();
            statType.GetMethod("ItemCheck_HandleMount", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, null);
            int weaponDamage = self.GetWeaponDamage(item);
            //ItemCheck_HandleMPItemAnimation(item);
            statType.GetMethod("ItemCheck_HandleMPItemAnimation", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item });
            //ItemCheck_HackHoldStyles(item);
            statType.GetMethod("ItemCheck_HackHoldStyles", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item });
            if (self.itemAnimation < 0)
                self.itemAnimation = 0;

            if (self.itemTime < 0)
                self.itemTime = 0;

            if (self.itemAnimation == 0 && self.reuseDelay > 0)
                //self.ApplyReuseDelay();
                statType.GetMethod("ApplyReuseDelay", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, null);

            if (Main.myPlayer == i && self.itemAnimation == 0 && TileObjectData.CustomPlace(item.createTile, item.placeStyle))
            {
                int hackCreateTile = item.createTile;
                int hackPlaceStyle = item.placeStyle;
                if (hackCreateTile == TileID.Saplings)
                {
                    Tile soil = Main.tile[tileTargetX, tileTargetY + 1];
                    if (soil.HasTile)
                        TileLoader.SaplingGrowthType(soil.TileType, ref hackCreateTile, ref hackPlaceStyle);
                }

                TileObject.CanPlace(tileTargetX, tileTargetY, hackCreateTile, hackPlaceStyle, self.direction, out _, true);
            }

            if (self.itemAnimation == 0 && self.altFunctionUse == 2)
                self.altFunctionUse = 0;

            bool flag2 = true;
            if (self.gravDir == -1f && GolfHelper.IsPlayerHoldingClub(self))
                flag2 = false;
            //Main.NewText("YWEEEEEE", Color.Red);
            if (flag2 && self.controlUseItem && self.releaseUseItem && (self.itemAnimation == 0 || stickyPlr.moreTimeShoot) && item.useStyle != ItemUseStyleID.None)
            {
                if (self.altFunctionUse == 1)
                    self.altFunctionUse = 2;

                if (item.shoot == ProjectileID.None)
                    self.itemRotation = 0f;
                bool flag3 = (bool)statType.GetMethod("ItemCheck_CheckCanUse", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item });

                //bool flag3 = self.ItemCheck_CheckCanUse(item);
                if (!stickyPlr.moreTimeShoot)
                {
                    if (item.potion && flag3)
                        //self.ApplyPotionDelay(item);
                        statType.GetMethod("ApplyPotionDelay", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item });

                    if (item.mana > 0 && flag3 && self.whoAmI == Main.myPlayer && item.buffType != 0 && item.buffTime != 0)
                        self.AddBuff(item.buffType, item.buffTime);

                    if (item.shoot <= ProjectileID.None || !ProjectileID.Sets.MinionTargettingFeature[item.shoot] || self.altFunctionUse != 2)
                        //self.ItemCheck_ApplyPetBuffs(item);
                        statType.GetMethod("ItemCheck_HackHoldStyles", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item });


                    if (self.whoAmI == Main.myPlayer && self.gravDir == 1f && item.mountType != -1 && self.mount.CanMount(item.mountType, self))
                        self.mount.SetMount(item.mountType, self);

                    if ((item.shoot <= ProjectileID.None || !ProjectileID.Sets.MinionTargettingFeature[item.shoot] || self.altFunctionUse != 2) && flag3 && self.whoAmI == Main.myPlayer && item.shoot >= ProjectileID.None && (ProjectileID.Sets.LightPet[item.shoot] || Main.projPet[item.shoot]))
                        //FreeUpPetsAndMinions(item);
                        statType.GetMethod("FreeUpPetsAndMinions", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item });


                    if (flag3)
                    {
                        stickyPlr.moreTimeShoot = true;
                        statType.GetMethod("ItemCheck_StartActualUse", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item });
                        //Main.NewText("YEEE");
                    }
                }

                //ItemCheck_StartActualUse(item);

            }

            if (!self.controlUseItem)
                self.channel = false;

            goto HoldAndUseStyle;

        DecrementItemAnimation:
            Item item2 = (self.itemAnimation > 0) ? self.lastVisualizedSelectedItem : item;
            Rectangle drawHitbox = Item.GetDrawHitbox(item2.type, self);
            self.compositeFrontArm.enabled = false;
            self.compositeBackArm.enabled = false;
            if (self.itemAnimation > 0)
            {
                if (item.mana > 0)
                    //ItemCheck_ApplyManaRegenDelay(item);
                    statType.GetMethod("ItemCheck_ApplyManaRegenDelay", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item });


                if (Main.dedServ)
                {
                    self.itemHeight = item.height;
                    self.itemWidth = item.width;
                }
                else
                {
                    self.itemHeight = drawHitbox.Height;
                    self.itemWidth = drawHitbox.Width;
                }
                if (stickyPlr.index == 0)//stickyPlr.max
                    self.itemAnimation--;
            }

            goto DecrementItemTime;

        HoldAndUseStyle:

            ItemLoader.HoldItem(item, self);

            if (self.itemAnimation > 0)
                self.ItemCheck_ApplyUseStyle(heightOffsetHitboxCenter, item2, drawHitbox);
            //statType.GetMethod("ItemCheck_ApplyUseStyle", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { heightOffsetHitboxCenter, item2, drawHitbox });

            else
                //ItemCheck_ApplyHoldStyle(heightOffsetHitboxCenter, item2, drawHitbox);
                statType.GetMethod("ItemCheck_ApplyHoldStyle", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { heightOffsetHitboxCenter, item2, drawHitbox });


            self.releaseUseItem = !self.controlUseItem;
            goto ItemTimeUseItem;

        DecrementItemTime:

            if (self.itemTime > 0)
            {
                if (stickyPlr.index == 0)//stickyPlr.max
                    self.itemTime--;
                if (self.ItemTimeIsZero && self.whoAmI == Main.myPlayer)
                {
                    if (!self.JustDroppedAnItem)
                    {
                        int type = item.type;
                        if (type == 65 || type == 676 || type == 723 || type == 724 || type == 989 || type == 1226 || type == 1227)
                            //self.EmitMaxManaEffect();
                            statType.GetMethod("EmitMaxManaEffect", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, null);

                    }

                    PlayerInput.TryEndingFastUse();
                }
            }

            goto ReuseDelayAndAnimationStart;

        ItemTimeUseItem:

            if (!self.JustDroppedAnItem)
            {
                //ItemCheck_EmitHeldItemLight(item);
                statType.GetMethod("ItemCheck_EmitHeldItemLight", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item });

                //ItemCheck_EmitFoodParticles(item);
                statType.GetMethod("ItemCheck_EmitFoodParticles", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item });

                //ItemCheck_EmitDrinkParticles(item);
                statType.GetMethod("ItemCheck_EmitDrinkParticles", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item });


                // TML attempts to make ApplyItemTime calls run on remote players, so self check is removed. #ItemTimeOnAllClients
                // if (whoAmI == Main.myPlayer) {
                //bool? useItemCheckFlag = ItemLoader.UseItem(item, self);
                //if (useItemCheckFlag == null) Main.NewText("null");
                //else Main.NewText(useItemCheckFlag == null);
                if (true)
                {
                    bool flag4 = true;
                    int type2 = item.type;
                    if ((type2 == 65 || type2 == 676 || type2 == 723 || type2 == 724 || type2 == 757 || type2 == 674 || type2 == 675 || type2 == 989 || type2 == 1226 || type2 == 1227) && !self.ItemAnimationJustStarted)
                        flag4 = false;

                    if (type2 == 5097 && self.ItemAnimationJustStarted)
                        statType.GetField("_batbatCanHeal", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(self, true);
                    //self._batbatCanHeal = true;

                    if (type2 == 5094 && self.ItemAnimationJustStarted)
                        statType.GetField("_spawnTentacleSpikes", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(self, true);

                    //self._spawnTentacleSpikes = true;

                    if (type2 == 3852)
                    {
                        /* handled by Item.useLimitPerAnimation
						if (itemAnimation < itemAnimationMax - 12)
							flag4 = false;
						*/

                        if (self.altFunctionUse == 2 && !self.ItemAnimationJustStarted)
                            flag4 = false;
                    }

                    /* Eventide and nightglow handled by Item.useLimitPerAnimation. Zenith use limit didn't do anything anyway
					if (type2 == 4956 && itemAnimation < itemAnimationMax - 3 * item.useTime)
						flag4 = false;

					if (type2 == 4952 && itemAnimation < itemAnimationMax - 8)
						flag4 = false;

					if (type2 == 4953 && itemAnimation < itemAnimationMax - 10)
						flag4 = false;
					*/

                    if (item.useLimitPerAnimation != null && self.ItemUsesThisAnimation >= item.useLimitPerAnimation.Value)
                        flag4 = false;

                    //ItemCheck_TurretAltFeatureUse(item, flag4);
                    statType.GetMethod("ItemCheck_TurretAltFeatureUse", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item, flag4 });

                    //ItemCheck_MinionAltFeatureUse(item, flag4);
                    statType.GetMethod("ItemCheck_MinionAltFeatureUse", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item, flag4 });
                    //if (self.ItemTimeIsZero && self.itemAnimation > 0)
                    //{
                    //    if (useItemCheckFlag == true)
                    //    {
                    //        moreTimeShoot = true;
                    //        Main.NewText("Yee");
                    //    }
                    //}
                    if (item.shoot > ProjectileID.None && self.itemAnimation > 0 && (stickyPlr.moreTimeShoot || self.ItemTimeIsZero) && flag4)//self.ItemTimeIsZero
                    {
                        //Main.NewText(weaponDamage);
                        statType.GetMethod("ItemCheck_Shoot", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { i, item, weaponDamage });
                        //Main.NewText((self.itemTime, self.HeldItem.type), Color.Cyan);
                    }
                    //ItemCheck_Shoot(i, item, weaponDamage);


                    // Added by TML. #ItemTimeOnAllClients - TODO: item time application with these item types
                    if (self.whoAmI != Main.myPlayer)
                        goto endItemChecks;

                    //ItemCheck_UseWiringTools(item);
                    statType.GetMethod("ItemCheck_UseWiringTools", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item });

                    //ItemCheck_UseLawnMower(item);
                    statType.GetMethod("ItemCheck_UseLawnMower", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item });

                    //ItemCheck_PlayInstruments(item);
                    statType.GetMethod("ItemCheck_PlayInstruments", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item });

                    //ItemCheck_UseBuckets(item);
                    statType.GetMethod("ItemCheck_UseBuckets", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item });

                    if (!self.channel)
                    {
                        if (stickyPlr.index == stickyPlr.max - 1)
                            self.toolTime = self.itemTime;
                    }
                    else
                    {
                        if (stickyPlr.index == 0)//stickyPlr.max
                            self.toolTime--;
                        if (self.toolTime < 0)
                            self.toolTime = item.useTime;
                    }

                    //ItemCheck_UseMiningTools(item);
                    statType.GetMethod("ItemCheck_UseMiningTools", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item });

                    //ItemCheck_UseRodOfDiscord(item);
                    statType.GetMethod("ItemCheck_UseRodOfDiscord", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item });

                    //ItemCheck_UseLifeCrystal(item);
                    statType.GetMethod("ItemCheck_UseLifeCrystal", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item });

                    //ItemCheck_UseLifeFruit(item);
                    statType.GetMethod("ItemCheck_UseLifeFruit", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item });

                    //ItemCheck_UseManaCrystal(item);
                    statType.GetMethod("ItemCheck_UseManaCrystal", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item });

                    //ItemCheck_UseDemonHeart(item);
                    statType.GetMethod("ItemCheck_UseDemonHeart", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item });

                    //ItemCheck_UseTorchGodsFavor(item);
                    statType.GetMethod("ItemCheck_UseTorchGodsFavor", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item });

                    //ItemCheck_UseEventItems(item);
                    statType.GetMethod("ItemCheck_UseEventItems", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item });

                    //ItemCheck_UseBossSpawners(whoAmI, item);
                    statType.GetMethod("ItemCheck_UseBossSpawners", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { self.whoAmI, item });

                    //ItemCheck_UseCombatBook(item);
                    statType.GetMethod("ItemCheck_UseCombatBook", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item });

                    //ItemCheck_UsePetLicenses(item);
                    statType.GetMethod("ItemCheck_UsePetLicenses", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item });

                    //if (item.type == ItemID.GolfWhistle && self.itemAnimation == 2)
                    //    Main.LocalGolfState.ResetGolfBall();

                    self.PlaceThing(ref context);
                    if (item.makeNPC > 0)
                    {
                        if (!Main.GamepadDisableCursorItemIcon && self.position.X / 16f - tileRangeX - item.tileBoost <= tileTargetX && (self.position.X + self.width) / 16f + tileRangeX + item.tileBoost - 1f >= tileTargetX && self.position.Y / 16f - tileRangeY - item.tileBoost <= tileTargetY && (self.position.Y + self.height) / 16f + tileRangeY + item.tileBoost - 2f >= tileTargetY)
                        {
                            self.cursorItemIconEnabled = true;
                            Main.ItemIconCacheUpdate(item.type);
                        }

                        if (self.ItemTimeIsZero && self.itemAnimation > 0 && self.controlUseItem)
                            //flag = ItemCheck_ReleaseCritter(flag, item);
                            flag = (bool)statType.GetMethod("ItemCheck_ReleaseCritter", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { flag, item });

                    }

                    if (self.boneGloveItem != null && !self.boneGloveItem.IsAir && self.boneGloveTimer == 0 && self.itemAnimation > 0 && item.damage > 0)
                    {
                        self.boneGloveTimer = 60;
                        Vector2 center = self.Center;
                        Vector2 vector = self.DirectionTo(self.ApplyRangeCompensation(0.2f, center, Main.MouseWorld)) * 10f;
                        statType.GetMethod("ItemCheck_HackHoldStyles", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item });
                        //self.GetProjectileSource_Accessory(self.boneGloveItem),
                        Projectile.NewProjectile((IEntitySource)statType.GetMethod("GetProjectileSource_Accessory", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(self, new object[] { self.boneGloveItem }), center.X, center.Y, vector.X, vector.Y, ProjectileID.BoneGloveProj, 25, 5f, self.whoAmI);
                    }
                endItemChecks: { }
                }

                if (((item.damage >= 0 && item.type > ItemID.None && !item.noMelee) || item.type == ItemID.BubbleWand || ItemID.Sets.CatchingTool[item.type] || item.type == ItemID.NebulaBlaze || item.type == ItemID.SpiritFlame) && self.itemAnimation > 0)
                {
                    //ItemCheck_GetMeleeHitbox(item, drawHitbox, out bool dontAttack, out Rectangle itemRectangle);
                    //object daobj = false;
                    //object irobj = new Rectangle(0, 0, 0, 0);
                    statType.GetMethod("ItemCheck_GetMeleeHitbox", BindingFlags.NonPublic | BindingFlags.Instance).CreateDelegate<ItemCheck_GetMeleeHitboxDelegate>().Invoke(self, item, drawHitbox, out bool dontAttack, out Rectangle itemRectangle);
                    //Main.NewText((drawHitbox, itemRectangle, self.GetAdjustedItemScale(item)));
                    //.Invoke(self, new object[] { item, drawHitbox, daobj, irobj  });
                    if (!dontAttack)
                    {
                        statType.GetMethod("ItemCheck_HackHoldStyles", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item });

                        //itemRectangle = ItemCheck_EmitUseVisuals(item, itemRectangle);
                        itemRectangle = (Rectangle)statType.GetMethod("ItemCheck_EmitUseVisuals", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item, itemRectangle });
                        if (Main.myPlayer == self.whoAmI && ItemID.Sets.CatchingTool[item.type])
                            //itemRectangle = ItemCheck_CatchCritters(item, itemRectangle);
                            itemRectangle = (Rectangle)statType.GetMethod("ItemCheck_CatchCritters", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item, itemRectangle });
                        statType.GetMethod("ItemCheck_HackHoldStyles", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item });


                        //if (item.type == ItemID.GoldenBugNet || item.type == ItemID.FireproofBugNet)
                        //{
                        //    //List<ushort> ignoreList = ItemCheck_GetTileCutIgnoreList(item);
                        //    List<ushort> ignoreList = (List<ushort>)statType.GetMethod("ItemCheck_GetTileCutIgnoreList", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item });

                        //    //ItemCheck_CutTiles(item, itemRectangle, ignoreList);
                        //    statType.GetMethod("ItemCheck_CutTiles", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item, itemRectangle, ignoreList });
                        //}

                        if (Main.myPlayer == i && item.damage > 0)
                        {
                            int num = weaponDamage;
                            float knockBack = self.GetWeaponKnockback(item, item.knockBack);
                            /*
							float num2 = 1f;
							if (kbGlove)
								num2 += 1f;

							if (kbBuff)
								num2 += 0.5f;

							knockBack *= num2;
							if (inventory[selectedItem].type == 3106)
								knockBack += knockBack * (1f - stealth);
							*/
                            //List<ushort> ignoreList2 = ItemCheck_GetTileCutIgnoreList(item);
                            List<ushort> ignoreList2 = (List<ushort>)statType.GetMethod("ItemCheck_GetTileCutIgnoreList", BindingFlags.NonPublic | BindingFlags.Static).Invoke(self, new object[] { item });
                            //ItemCheck_CutTiles(item, itemRectangle, ignoreList2);
                            statType.GetMethod("ItemCheck_CutTiles", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item, itemRectangle, ignoreList2 });

                            //ItemCheck_MeleeHitNPCs(item, itemRectangle, num, knockBack);
                            statType.GetMethod("ItemCheck_MeleeHitNPCs", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item, itemRectangle, num, knockBack });

                            //ItemCheck_MeleeHitPVP(item, itemRectangle, num, knockBack);
                            statType.GetMethod("ItemCheck_MeleeHitPVP", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { item, itemRectangle, num, knockBack });

                            //ItemCheck_EmitHammushProjectiles(i, item, itemRectangle, num);
                            statType.GetMethod("ItemCheck_EmitHammushProjectiles", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { i, item, itemRectangle, num });

                        }
                    }
                }

                if (self.ItemTimeIsZero && self.itemAnimation > 0)
                {
                    if (ItemLoader.UseItem(item, self) == true)
                    {
                        self.ApplyItemTime(item, callUseItem: false);
                    }
                    if (item.hairDye >= 0)
                    {
                        self.ApplyItemTime(item);
                        if (self.whoAmI == Main.myPlayer)
                        {
                            self.hairDye = item.hairDye;
                            NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, self.whoAmI);
                        }
                    }

                    if (item.healLife > 0)
                    {
                        int healLife = self.GetHealLife(item);
                        self.statLife += healLife;
                        self.ApplyItemTime(item);
                        if (healLife > 0 && Main.myPlayer == self.whoAmI)
                            self.HealEffect(healLife, true);
                    }

                    if (item.healMana > 0)
                    {
                        int healMana = self.GetHealMana(item);
                        self.statMana += healMana;
                        self.ApplyItemTime(item);
                        if (healMana > 0 && Main.myPlayer == self.whoAmI)
                        {
                            self.AddBuff(94, manaSickTime);
                            self.ManaEffect(healMana);
                        }
                    }

                    if (item.buffType > 0)
                    {
                        if (self.whoAmI == Main.myPlayer && item.buffType != 90 && item.buffType != 27)
                            self.AddBuff(item.buffType, item.buffTime);

                        self.ApplyItemTime(item);
                    }
                }
                if (i == Main.myPlayer)
                {
                    if ((self.itemTimeMax != 0 && self.itemTime == self.itemTimeMax) | (!item.IsAir && item.IsNotTheSameAs(self.lastVisualizedSelectedItem)))
                        self.lastVisualizedSelectedItem = item.Clone();
                }
                else
                {
                    self.lastVisualizedSelectedItem = item.Clone();
                }

                if (i == Main.myPlayer)
                {
                    if (self.itemTimeMax != 0 && item.tileWand > 0 && !(bool)statType.GetField("dontConsumeWand", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(self) && self.itemTime == self.itemTimeMax)
                    {
                        int tileWand = item.tileWand;
                        for (int num15 = 0; num15 < 58; num15++)
                        {
                            if (tileWand == self.inventory[num15].type && self.inventory[num15].stack > 0)
                            {
                                if (ItemLoader.ConsumeItem(self.inventory[num15], self))
                                    self.inventory[num15].stack--;
                                if (self.inventory[num15].stack <= 0)
                                    self.inventory[num15] = new Item();

                                break;
                            }
                        }
                    }

                    if (self.itemTimeMax != 0 && stickyPlr.moreTimeShoot && item.consumable && !flag)//self.itemTime == self.itemTimeMax
                    {
                        bool flag6 = true;
                        if (item.DamageType == DamageClass.Ranged)
                        {
                            if (self.huntressAmmoCost90 && Main.rand.NextBool(10))
                                flag6 = false;

                            if (self.chloroAmmoCost80 && Main.rand.NextBool(5))
                                flag6 = false;

                            if (self.ammoCost80 && Main.rand.NextBool(5))
                                flag6 = false;

                            if (self.ammoCost75 && Main.rand.NextBool(4))
                                flag6 = false;
                        }

                        if (item.IsACoin)
                            flag6 = true;

                        bool? flag7 = ItemID.Sets.ForceConsumption[item.type];
                        if (flag7.HasValue)
                            flag6 = flag7.Value;

                        if (flag6 && ItemLoader.ConsumeItem(item, self))
                        {
                            if (item.stack > 0)
                                item.stack--;

                            if (item.stack <= 0)
                            {
                                self.itemTime = self.itemAnimation;
                                Main.blockMouse = true;
                            }
                        }
                    }

                    if (item.stack <= 0 && self.itemAnimation == 0)
                        self.inventory[self.selectedItem] = new Item();

                    if (self.selectedItem == 58 && self.itemAnimation != 0)
                        Main.mouseItem = item.Clone();
                }
            }
            //Main.spriteBatch.Draw(texture,new Rectangle(0,0,Main.screenWidth,Main.screenHeight),Color.White);
            if (self.itemAnimation == 0)
                self.JustDroppedAnItem = false;

            if (stickyPlr.index < stickyPlr.max) goto myLabel;
        }

        private void Player_ApplyItemTime_Sticky_On(Terraria.On_Player.orig_ApplyItemTime orig, Player self, Item sItem, float multiplier, bool? callUseItem)
        {
            //var stickyPlr = self.GetModPlayer<StickyPlayer>();
            //if (stickyPlr.items != null && stickyPlr.index != stickyPlr.max) return;
            self.GetModPlayer<StickyPlayer>().moreTimeShoot = true;
            orig.Invoke(self, sItem, multiplier, callUseItem);
            //if ((callUseItem ?? self.ItemTimeIsZero) && ItemLoader.UseItem(sItem, self) == false)
            //    return;

            //self.SetItemTime(CombinedHooks.TotalUseTime(sItem.useTime * multiplier, self, sItem));
            ////self.ItemUsesThisAnimation++;
            //typeof(Player).GetMethod("set_ItemUsesThisAnimation").Invoke(self,new object[] { self.ItemUsesThisAnimation + 1 });
            ////throw new NotImplementedException();
        }

        public override void Unload()
        {
            //IL.Terraria.Player.ApplyItemTime -= Player_ApplyItemTime_Sticky;
            //IL.Terraria.Player.ItemCheck_Inner -= Player_ItemCheck_InnerSticky;
            base.Unload();
        }
        private void Player_ApplyItemTime_Sticky(ILContext il)
        {
            return;
            ILCursor cursor = new(il);
            if (!cursor.TryGotoNext(i => i.MatchRet())) return;
            ILLabel label = cursor.DefineLabel();
            cursor.MarkLabel(label);
            if (!cursor.TryGotoPrev(i => i.MatchLdarg(3))) return;
            cursor.Emit(Ldarg_0);
            cursor.Emit(Ldarg_1);
            cursor.EmitDelegate<Func<Player, Item, bool>>((player, item) => { var stickyPlr = player.GetModPlayer<StickyPlayer>(); if (stickyPlr.items == null || stickyPlr.index == stickyPlr.max) { return false; } return true; });//!(stickyPlr.items == null || stickyPlr.index == stickyPlr.max)//Main.NewText(item.Name, Color.Chocolate);
            cursor.Emit(Brtrue_S, label);
            //if (!cursor.TryGotoNext(i => i.MatchLdfld<Item>("useTime"))) return;
            //cursor.Index++;
            //cursor.Emit(Ldarg_0);
            //cursor.EmitDelegate<Func<int, Player, int>>((num, player) => player.GetModPlayer<StickyPlayer>().items == null ? num : player.HeldItem.useTime);
        }

        //public delegate void RefAction<T>(ref T value);

        public delegate void RefAction<T1, T2>(ref T1 target, T2 value);

        private void Player_ItemCheck_InnerSticky(ILContext il)
        {
            //return;

            ILCursor cursor = new(il);
            //cursor.Emit(Ldarg_0);
            //cursor.EmitDelegate<Action<Player>>(player => Main.NewText(player.itemAnimation));
            if (!cursor.TryGotoNext(i => i.MatchStloc(2)))
            {
                return;
            }
            //Item[] items = null;
            //int index = -1;
            //int max = -1;
            //bool canShoot = false;
            ////ILLabel label = null;

            cursor.Emit(Ldarg_0);
            cursor.EmitDelegate<Action<Item, Player>>
            (
                (item, player) =>
                {
                    var stickyPlr = player.GetModPlayer<StickyPlayer>();
                    if (item.ModItem != null && item.ModItem is StickyItem sticky)
                    {
                        stickyPlr.items = sticky.ItemSet;
                        stickyPlr.index = 0;
                        stickyPlr.max = stickyPlr.items.Length;
                        //canShoot =player.itemAnimation == 
                    }
                    else
                    {
                        stickyPlr.items = null;
                        stickyPlr.index = -1;
                        stickyPlr.max = -1;
                    }
                }
            );
            //if (items != null && max > 0 && index > 0)// && index < max
            //{
            //    cursor.MarkLabel(label);
            //    cursor.Emit(Ldloc_2);
            //    cursor.EmitDelegate<RefAction<Item>>((ref Item item) => { item = items[index]; index++; });

            //    if (!cursor.TryGotoNext(i => i.MatchRet()))
            //    {
            //        return;
            //    }
            //    cursor.Emit(Ldc_I4, index);
            //    cursor.Emit(Ldc_I4, max);
            //    cursor.Emit(Clt);
            //    cursor.Emit(Brtrue_S, label);
            //}
            //else
            //{
            //    cursor.Emit(Ldloc_0);
            //    cursor.Emit(Ldfld, "inventory");
            //    cursor.Emit(Ldloc_0);
            //    cursor.Emit(Ldfld, "selectedItem");
            //    cursor.Emit(Stloc, 2);
            //}
            #region 原本打算直接换掉HeldItem多循环几次，但是跑不了(x  （现在能跑，但是弹幕发射等还是有问题


            ILLabel label = cursor.DefineLabel();
            cursor.MarkLabel(label);
            cursor.Emit(Ldloca, 2);
            cursor.Emit(Ldarg_0);
            cursor.EmitDelegate<RefAction<Item, Player>>
            (
                (ref Item target, Player value) =>
                {
                    var stickyPlr = value.GetModPlayer<StickyPlayer>();

                    if (stickyPlr.items != null && stickyPlr.max > 0 && stickyPlr.index > -1)
                    {
                        //Main.NewText(index);
                        //Main.NewText(max,Color.Red);
                        target = stickyPlr.items[stickyPlr.index];
                        stickyPlr.index++;
                        //Main.NewText(index);

                    }
                    else
                    {
                        target = value.HeldItem;
                    }
                    //target = value.HeldItem;

                    ////Main.NewText(items != null);
                    //Main.NewText(items != null, Color.Red);
                    //Main.NewText(Main.GameUpdateCount, Color.LightGreen);
                    ////Main.NewText(max, Color.Green);
                    ////Main.NewText(index, Color.Blue);
                    //Main.NewText(target.Name, Color.Cyan);


                }
            );
            cursor.Remove();

            #region animation正确减少
            for (int n = 0; n < 2; n++)
                if (!cursor.TryGotoNext(i => i.MatchStfld<Player>("itemAnimation"))) {/* value++;*/ return; }
            cursor.Index--;
            cursor.Emit(Ldarg_0);
            cursor.EmitDelegate<Func<int, Player, int>>((num, player) => { var stickyPlr = player.GetModPlayer<StickyPlayer>(); return stickyPlr.items == null || stickyPlr.index == stickyPlr.max ? 1 : 0; });
            #endregion

            #region 在正确的时候减少
            //for (int n = 0; n < 2; n++)
            if (!cursor.TryGotoNext(i => i.MatchStfld<Player>("itemTime"))) {/* value++;*/ return; }
            cursor.Index--;
            cursor.Emit(Ldarg_0);
            cursor.EmitDelegate<Func<int, Player, int>>((num, player) => { var stickyPlr = player.GetModPlayer<StickyPlayer>(); return stickyPlr.items == null || stickyPlr.index == stickyPlr.max ? 1 : 0; });
            #endregion

            #region shoot检测
            ////if (!cursor.TryGotoNext(i => i.MatchLdloc(14))) return;
            ////if (!cursor.TryGotoNext(i => i.MatchLdloc(14))) return;
            ////if (!cursor.TryGotoNext(i => i.MatchLdloc(14))) return;
            ////cursor.Index++;
            ////cursor.EmitDelegate((bool value) => Main.NewText(value, Main.DiscoColor));
            ////cursor.Emit(Ldloc, 14);
            //if (!cursor.TryGotoNext(i => i.MatchCall<Player>("get_ItemTimeIsZero"))) return;
            //if (!cursor.TryGotoNext(i => i.MatchCall<Player>("get_ItemTimeIsZero"))) return;
            //cursor.Index++;
            //cursor.EmitDelegate((bool flag) => Main.NewText(flag));
            //cursor.Emit(Ldarg_0);
            //cursor.Emit<Player>(OpCodes.Call, "get_ItemTimeIsZero");
            ////cursor.Emit(Mono.Cecil.Cil.OpCodes.Call,)

            //if (!cursor.TryGotoNext(i => i.MatchCall<Player>("ItemCheck_Shoot"))) return;
            //cursor.Index -= 19;
            //cursor.Emit(Ldloc_2);
            //cursor.Emit(Ldarg_0);
            //cursor.EmitDelegate<Action<Item, Player>>((item, player) => Main.NewText((item.shoot, item.Name, player.itemAnimation, player.itemTime, item.shoot > 0 && player.itemAnimation > 0 && player.ItemTimeIsZero), Color.Cyan));

            //if (!cursor.TryGotoNext(i => i.MatchCall<Player>("ItemCheck_Shoot"))) return;
            //cursor.Index--;
            //cursor.EmitDelegate<Action<Item>>(item => Main.NewText(item.Name));
            //cursor.Emit(Ldloc_2);

            //if (!cursor.TryGotoNext(i => i.MatchCall<Player>("ApplyItemTime"))) { value++; return; }
            //cursor.EmitDelegate<Action<Player, Item, float, bool?>>((player, item, num, flag) => { if (items == null || index == max) { player.ApplyItemTime(item, num, flag); Main.NewText(item.Name,Color.Red); } });
            //cursor.Remove();

            //cursor.EmitDelegate<Action<Item>>(Item => Main.NewText(Item.Name));
            //cursor.Emit(Ldloc_2);
            #endregion

            #region 防止itemTime被写入奇怪的值

            ////if (!cursor.TryGotoNext(i => i.MatchStsfld<Player>("itemTime"))) return;
            ////if (!cursor.TryGotoNext(i => i.MatchStsfld<Player>("itemTime"))) return;



            //if (!cursor.TryGotoNext(i => i.MatchStfld<Player>("itemTime"))) { value++; return; }
            //cursor.Emit(Ldarg_0);
            //cursor.EmitDelegate<Func<int, Player, int>>((value,player) => (items == null || index == max) ? player.itemAnimation : 0);//value//HeldItem.useAnimation
            #endregion
            #region 循环
            if (!cursor.TryGotoNext(i => i.MatchRet()))
            {
                return;
            }
            //cursor.Next = Instruction.Create(Nop);
            //cursor.EmitDelegate<Action<int>>(num => { });
            //cursor.Emit(Ldc_I4, index);
            //cursor.Index -= 4;
            cursor.Index -= 5;
            //cursor.Emit(Ldarg_0);
            cursor.Emit(Ldloc_2);
            cursor.EmitDelegate<Func<Player, Item, bool>>((player, item) => { if (player.itemAnimation == 0) player.JustDroppedAnItem = false; var stickyPlr = player.GetModPlayer<StickyPlayer>(); return stickyPlr.items != null && stickyPlr.index < stickyPlr.max; });// Main.NewText((items == null, index, max), Main.DiscoColor); //Main.NewText((player.itemTime, item.Name), Main.DiscoColor);
            cursor.Emit(Brtrue_S, label);
            cursor.Emit(Ldarg_0);
            //cursor.Emit(Ret);
            #endregion

            #endregion
            //GetOne();
            #region 尝试多次shoot
            //if (!cursor.TryGotoNext(i => i.MatchCall<Player>("ItemCheck_Shoot"))) return;//"Terraria.Player",
            //cursor.Index--;
            //ILLabel label = cursor.DefineLabel();
            //cursor.MarkLabel(label);
            //cursor.EmitDelegate<Func<Item, Item>>
            //(
            //    item =>
            //    {
            //        if (items != null && max > 0 && index > -1)
            //        {
            //            index++;
            //            return items[index - 1];
            //        }
            //        return item;
            //    }
            //);
            //cursor.Index += 2;
            //cursor.EmitDelegate(() => index < max);
            //cursor.Emit(Brtrue_S, label);
            #endregion
            //MyFunc(1, 2, 3, 4, out _, 5);
        }
        //public static int value;
        private void Player_ItemCheck_Inner_ShootLoop(ILContext il)
        {
            ILCursor cursor = new(il);
            //if (!cursor.TryGotoNext(i => i.MatchStloc(2)))
            //{
            //    return;
            //}
            //Item[] items = null;
            //int index = -1;
            //int max = -1;
            ////bool canShoot = false;
            //////ILLabel label = null;

            //cursor.Emit(Ldarg_0);
            //cursor.EmitDelegate<Action<Item, Player>>
            //(
            //    (item, player) =>
            //    {
            //        if (item.ModItem != null && item.ModItem is StickyItem sticky)
            //        {
            //            items = sticky.ItemSet;
            //            index = 0;
            //            max = items.Length;
            //            //canShoot =player.itemAnimation == 
            //        }
            //        else
            //        {
            //            items = null;
            //            index = -1;
            //            max = -1;
            //        }
            //    }
            //);
            //cursor.Emit(Ldarg_0);
            //cursor.EmitDelegate<Func<Player, Item>>(player => player.HeldItem);
            #region 尝试多次shoot
            //if (!cursor.TryGotoNext(i => i.MatchCall<Player>("ItemCheck_Shoot"))) return;//"Terraria.Player",
            //cursor.Index -= 4;
            //ILLabel label = cursor.DefineLabel();
            //cursor.MarkLabel(label);
            //if (!cursor.TryGotoNext(i => i.MatchCall<Player>("ItemCheck_Shoot"))) return;
            //cursor.Index--;
            //cursor.Emit(Ldarg_0);
            //cursor.EmitDelegate<Func<Item, Item>>//, Player
            //(
            //    (item) =>
            //    {
            //        if (items != null && max > 0 && index > -1)
            //        {
            //            index++;
            //            return items[index - 1];
            //        }
            //        return item;
            //        //return player.inventory[0];
            //    }
            //);//, player
            //if (!cursor.TryGotoNext(i => i.MatchCall<Player>("ItemCheck_Shoot"))) return;
            //cursor.Index++;
            //cursor.EmitDelegate(() => index < max);
            //cursor.Emit(Brtrue_S, label);
            int index = -1;
            if (!cursor.TryGotoNext(i => i.MatchCall<Player>("ItemCheck_Shoot"))) return;//"Terraria.Player",
            cursor.Index -= 4;
            cursor.EmitDelegate(() => { index++; });
            ILLabel label = cursor.DefineLabel();
            cursor.MarkLabel(label);
            if (!cursor.TryGotoNext(i => i.MatchCall<Player>("ItemCheck_Shoot"))) return;
            cursor.Index--;
            cursor.Emit(Ldarg_0);
            cursor.EmitDelegate<Func<Item, Item>>//, Player
            (
                (item) =>
                {
                    index++;
                    return item;
                    //return player.inventory[0];
                }
            );//, player
            if (!cursor.TryGotoNext(i => i.MatchCall<Player>("ItemCheck_Shoot"))) return;
            cursor.Index++;
            cursor.EmitDelegate(() => index < 5);
            cursor.Emit(Brtrue_S, label);
            #endregion
        }
        //public static void MyFunc(int v0, int v2, int v3, int v4, out int vout, int v5) { vout = 0; }
        //static int GetOne() => 1;
    }
    public class Glue : ModItem
    {
        //public override void SetStaticDefaults()
        //{
        //    DisplayName.SetDefault("胶水");
        //    Tooltip.SetDefault("看起来可以把两件道具粘起来...在它把你的手和道具粘起来之前");
        //}
        public static bool CanChoose(Item _item) => _item.active && _item.type != ItemID.None && (_item.damage > 0 || _item.type == ModContent.ItemType<StickyItem>()) && _item.maxStack == 1 && !_item.consumable && _item.useAnimation > 2;
        public override void Update(ref float gravity, ref float maxFallSpeed)
        {
            //Main.NewText(StickyWeapons.value);
            Item item1 = null;
            foreach (var _item in Main.item)
            {
                if (CanChoose(_item) && Vector2.DistanceSquared(_item.Center, Item.Center) <= 4096)
                {
                    item1 = _item;
                    break;
                }
            }
            if (item1 == null) return;
            foreach (var _item in Main.item)
            {
                if (CanChoose(_item) && Vector2.DistanceSquared(_item.Center, Item.Center) <= 4096 && _item.GetHashCode() != item1.GetHashCode())
                {

                    var stickyIndex = Main.item[Item.NewItem(Item.GetSource_Misc("Sticky!"), Item.Center, ModContent.ItemType<StickyItem>())];// Item.NewItem(Item.GetSource_Misc("Sticky!"), Item.Center, ModContent.ItemType<StickyItem>())
                    if (stickyIndex.ModItem != null && stickyIndex.ModItem is StickyItem sticky)
                    {
                        sticky.theItems.Item1 = item1.Clone();
                        sticky.theItems.Item2 = _item.Clone();
                        for (int n = 0; n < 100; n++)
                        {
                            Dust.NewDustPerfect(Vector2.Lerp(item1.Center, _item.Center, n / 99f), DustID.Clentaminator_Cyan).noGravity = true;
                            Dust.NewDustPerfect(Item.Center, DustID.Clentaminator_Cyan, (n / 99f * MathHelper.TwoPi).ToRotationVector2()).noGravity = true;
                        }
                        //Main.NewText(item1.Name, Color.Red);
                        //Main.NewText(_item.Name, Color.Cyan);
                        //Main.NewText(Item.useAnimation, Color.Green);
                        //Main.NewText("草");
                        if (item1.ModItem != null && item1.ModItem is StickyItem _sticky1)
                        {
                            Main.RunOnMainThread(() =>
                            {
                                _sticky1.complexTexture?.Dispose();
                                _sticky1.complexTexture = null;
                            });
                        }
                        if (_item.ModItem != null && _item.ModItem is StickyItem _sticky2)
                        {
                            Main.RunOnMainThread(() =>
                            {
                                _sticky2.complexTexture?.Dispose();
                                _sticky2.complexTexture = null;
                            });
                        }
                        item1.TurnToAir();
                        _item.TurnToAir();
                        Item.stack--;
                        if (Item.stack <= 0) Item.TurnToAir();
                        if (Main.netMode == NetmodeID.MultiplayerClient)
                        {
                            NetMessage.SendData(MessageID.SyncItem, -1, -1, null, Item.whoAmI, 1f);
                            NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item1.whoAmI, 1f);
                            NetMessage.SendData(MessageID.SyncItem, -1, -1, null, _item.whoAmI, 1f);

                        }

                        var list = new List<Item>();
                        sticky.GetItemSet(list);
                        sticky.ItemSet = [.. list];
                        sticky.SetDefaults();
                        //Item.TurnToAir();

                    }
                    //else
                    //{
                    //    stickyIndex.TurnToAir();
                    //}
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                        NetMessage.SendData(MessageID.SyncItem, -1, -1, null, stickyIndex.whoAmI, 1f);
                    break;
                }
            }
            base.Update(ref gravity, ref maxFallSpeed);
        }
        public override void SetDefaults()
        {
            Item.maxStack = 999;
            Item.value = 5;
            Item.height = Item.width = 10;
            //var func = CanChoose;
        }
        //public override string Texture => "Terraria/Images/Item_" + ItemID.Gel;
        public override void AddRecipes()
        {
            CreateRecipe().AddIngredient(ItemID.Gel, 5).Register();
        }
    }
    public class OrganicSolvent : ModItem
    {
        public override void Update(ref float gravity, ref float maxFallSpeed)
        {
            foreach (var i in Main.item)
            {
                if (i.active && i.type == ModContent.ItemType<StickyItem>() && i.ModItem != null && Vector2.Distance(Item.Center, i.Center) <= 64 && i.ModItem is StickyItem sticky && sticky.ItemSet != null)
                {
                    Main.RunOnMainThread(() =>
                    {
                        sticky.complexTexture?.Dispose();
                        sticky.complexTexture = null;
                    });

                    Item.stack--;
                    if (Item.stack <= 0) Item.TurnToAir();
                    foreach (var _item in sticky.ItemSet)
                    {
                        var index = Item.NewItem(Item.GetSource_Misc(""), Item.Center, 1);
                        Main.item[index] = _item.Clone();
                        var currentItem = Main.item[index];
                        currentItem.whoAmI = index;
                        currentItem.velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 4);
                        currentItem.Center = Item.Center;
                        //Main.NewText((_item.Name, currentItem.Name, currentItem.active, currentItem.stack, _item.whoAmI, index, currentItem.whoAmI));
                        if (Main.netMode == NetmodeID.MultiplayerClient)
                            NetMessage.SendData(MessageID.SyncItem, -1, -1, null, index, 1f);
                    }
                    for (int n = 0; n < 100; n++)
                    {
                        Dust.NewDustPerfect(Item.Center, DustID.Clentaminator_Cyan, (n / 99f * MathHelper.TwoPi).ToRotationVector2()).noGravity = true;
                    }
                    var index1 = Item.NewItem(Item.GetSource_Misc(""), i.Center, ItemID.Gel, 5 * sticky.ItemSet.Length);
                    var index2 = Item.NewItem(Item.GetSource_Misc(""), i.Center, ItemID.Ale);
                    i.TurnToAir();

                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        NetMessage.SendData(MessageID.SyncItem, -1, -1, null, index1, 1f);
                        NetMessage.SendData(MessageID.SyncItem, -1, -1, null, index2, 1f);
                        NetMessage.SendData(MessageID.SyncItem, -1, -1, null, Item.whoAmI, 1f);
                        NetMessage.SendData(MessageID.SyncItem, -1, -1, null, i.whoAmI, 1f);
                    }

                    break;
                }

            }
        }
        public override void SetDefaults()
        {
            Item.maxStack = 999;
            Item.value = 25;
            Item.height = Item.width = 10;
        }
        //public override string Texture => "Terraria/Images/Item_" + ItemID.Gel;
        public override void AddRecipes()
        {
            CreateRecipe().AddIngredient(ItemID.Gel, 5).AddIngredient(ItemID.Ale).Register();
        }
    }
    public class StickyItem : ModItem
    {
        public override void NetReceive(BinaryReader reader)
        {
            ItemIO.Receive(theItems.Item1 ??= new Item(), reader, true, true);
            ItemIO.Receive(theItems.Item2 ??= new Item(), reader, true, true);
            var list = new List<Item>();
            GetItemSet(list);
            ItemSet = [.. list];
            SetDefaults();
            base.NetReceive(reader);
        }
        public override void NetSend(BinaryWriter writer)
        {
            ItemIO.Send(item1, writer, true, true);
            ItemIO.Send(item2, writer, true, true);
        }
        public static bool MeleeCheck(DamageClass damageClass) => damageClass == DamageClass.Melee
|| damageClass.GetEffectInheritance(DamageClass.Melee) || !damageClass.GetModifierInheritance(DamageClass.Melee).Equals(StatInheritanceData.None);
        public override void SetDefaults()
        {
            if (ItemSet == null) return;
            int width = -1;
            int height = -1;
            int rare = -114514;
            int value = 0;
            int useTime = 0;
            int useAnimation = 0;
            float shootSpeed = 0;
            bool channel = false;
            float knockBack = 0;
            int damage = 0;
            int useStyle = ItemUseStyleID.Shoot;
            foreach (var i in ItemSet)
            {
                width = i.width > width ? i.width : width;
                height = i.height > height ? i.height : height;
                rare = i.rare > rare ? i.rare : rare;
                useTime = i.useTime > useTime ? i.useTime : useTime;
                useAnimation = i.useAnimation > useAnimation ? i.useAnimation : useAnimation;
                shootSpeed = i.shootSpeed > shootSpeed ? i.shootSpeed : shootSpeed;
                damage = i.damage > damage ? i.damage : damage;
                knockBack = i.knockBack > knockBack ? i.knockBack : knockBack;
                value += i.value + 5;
                channel |= i.channel;
                if (i.useAmmo != 0)
                    Item.useAmmo = i.useAmmo;
                if (i.useStyle == ItemUseStyleID.Swing) useStyle = ItemUseStyleID.Swing;
                if (Item.DamageType == DamageClass.Default || (MeleeCheck(i.DamageType) && !MeleeCheck(Item.DamageType)))
                {
                    Item.DamageType = i.DamageType;
                }
            }
            Item.width = width == -1 ? 32 : width;
            Item.height = height == -1 ? 32 : height;
            Item.rare = rare;
            Item.value = value - 5;
            Item.useTime = useTime;
            Item.useAnimation = useAnimation;
            Item.shootSpeed = shootSpeed;
            Item.damage = damage;
            Item.knockBack = knockBack;
            Item.useStyle = useStyle;
            Item.channel = channel;
            if (Main.gameMenu) return;
            var gd = Main.instance.GraphicsDevice;
            var sp = Main.spriteBatch;
            Main.RunOnMainThread(
                () =>
                {
                    Vector2 size = default;
                    foreach (var item in ItemSet)
                    {
                        Main.instance.LoadItem(item.type);
                        var curSize = TextureAssets.Item[item.type].Size();
                        if (size.X < curSize.X) size.X = curSize.X;
                        if (size.Y < curSize.Y) size.Y = curSize.Y;

                    }
                    complexTexture?.Dispose();
                    complexTexture = new RenderTarget2D(gd, (int)size.X, (int)size.Y);
                    gd.SetRenderTarget(complexTexture);
                    gd.Clear(Color.Transparent);
                    sp.Begin();
                    List<int> types = [];
                    foreach (var item in ItemSet)
                    {
                        if (types.Contains(item.type)) continue;
                        Vector2 curSize = TextureAssets.Item[item.type].Size();
                        ItemSlot.DrawItemIcon(item, 31, sp, size * Vector2.UnitY + curSize * new Vector2(1, -1) * .5f, 1, 1145, Color.White);
                        types.Add(item.type);
                    }
                    sp.End();
                    gd.SetRenderTarget(Main.screenTarget);
                }
                );
        }
        public override string Texture => "Terraria/Images/Item_" + ItemID.Gel;
        public Item item1
        {
            get
            {
                //if ((theItems.Item1 == null || theItems.Item1.type == ItemID.None) && synced) { Item.TurnToAir(); Console.WriteLine("item1为空"); }
                return theItems.Item1 ??= new Item();
            }
        }
        public Item item2
        {
            get
            {
                //if ((theItems.Item2 == null || theItems.Item2.type == ItemID.None) && synced) { Item.TurnToAir(); Console.WriteLine("item2为空"); }
                return theItems.Item2 ??= new Item();
            }
        }
        public (Item, Item) theItems;
        public Item[] ItemSet;
        public RenderTarget2D complexTexture;
        public StickyItem GetItemSet(List<Item> target)
        {
            if (item1 == null || item2 == null) return this;
            if (item1.ModItem != null && item1.ModItem is StickyItem sticky1)
            {
                sticky1.GetItemSet(target);
            }
            else
            {
                target.Add(item1);
            }
            if (item2.ModItem != null && item2.ModItem is StickyItem sticky2)
            {
                sticky2.GetItemSet(target);
            }
            else
            {
                target.Add(item2);
            }
            return this;
        }
        public override bool AltFunctionUse(Player player) => true;
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            //if (item1 == null || item2 == null) return false;
            //Main.NewText(complexTexture.GetHashCode());

            if (complexTexture != null)
            {
                //ItemSlot.DrawItemIcon()
                float scaler = 1f;
                float max = Math.Max(complexTexture.Width, complexTexture.Height);
                if (max > 30) scaler = 30 / max;
                spriteBatch.Draw(complexTexture, position, null, drawColor, 0, complexTexture.Size() * .5f, scaler, 0, 0);
                //spriteBatch.Draw(TextureAssets.MagicPixel.Value, position, new Rectangle(0, 0, 1, 1), Color.Red, 0, new Vector2(.5f), 4f, 0, 0);
                return false;
            }
            else
            {
                //Main.NewText("render，没有你我怎么活啊");
                if (ItemSet == null) return true;

                var gd = Main.instance.GraphicsDevice;
                var sp = Main.spriteBatch;
                Vector2 size = default;
                foreach (var item in ItemSet)
                {
                    Main.instance.LoadItem(item.type);
                    var curSize = TextureAssets.Item[item.type].Size();
                    if (size.X < curSize.X) size.X = curSize.X;
                    if (size.Y < curSize.Y) size.Y = curSize.Y;
                }
                complexTexture = new RenderTarget2D(gd, (int)size.X, (int)size.Y);
                sp.End();
                gd.SetRenderTarget(complexTexture);
                gd.Clear(Color.Transparent);
                sp.Begin();
                List<int> types = [];
                foreach (var item in ItemSet)
                {
                    if (types.Contains(item.type)) continue;
                    Vector2 curSize = TextureAssets.Item[item.type].Size();
                    ItemSlot.DrawItemIcon(item, 31, sp, size * Vector2.UnitY + curSize * new Vector2(1, -1) * .5f, 1, 1145, Color.White);
                    types.Add(item.type);
                }
                sp.End();
                sp.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.UIScaleMatrix);

                gd.SetRenderTarget(Main.screenTarget);
            }
            if (item1 != null)
                Main.instance.LoadItem(item1.type);
            if (item2 != null)
                Main.instance.LoadItem(item2.type);
            item1?.StickyDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin);
            item2?.StickyDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin);
            //Main.NewText()
            return false;
        }
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            if (complexTexture != null)
            {
                spriteBatch.Draw(complexTexture, Item.Center - Main.screenPosition, null, lightColor, rotation, complexTexture.Size() * .5f, 1f, 0, 0);
                return false;
            }
            if (item1 == null || item2 == null) return false;
            if (ItemSet == null) return false;
            for (int n = 0; n < ItemSet.Length; n++)
            {
                float _rotation = rotation;
                float _scale = scale;
                var _item = ItemSet[n];
                if (_item == null) continue;
                Main.instance.LoadItem(_item.type);
                if (_item.ModItem == null || _item.ModItem.PreDrawInWorld(spriteBatch, lightColor, alphaColor, ref _rotation, ref _scale, whoAmI))
                {
                    var value = TextureAssets.Item[_item.type].Value;
                    var ani = Main.itemAnimations[_item.type];

                    spriteBatch.Draw(value, Item.Center - Main.screenPosition, (ani == null) ? value.Frame() : ani.GetFrame(value), alphaColor, rotation, value.Size() * .5f / new Vector2(1, (ani == null) ? 1 : ani.FrameCount), scale, 0, 0);
                }
            }
            return false;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "WTFText", "目前这货包含的物品有："));
            try
            {
                //if (item1 == null || item2 == null) return;
                //if (item1.ModItem != null && item1.ModItem is StickyItem sticky1)
                //{

                //}
                //else
                //{
                //    tooltips.Add(new TooltipLine(Mod, item1.Name, item1.Name));
                //}
                //if (item2.ModItem != null && item2.ModItem is StickyItem sticky2)
                //{

                //}
                //else
                //{
                //    tooltips.Add(new TooltipLine(Mod, item2.Name, item2.Name));
                //}
                if (ItemSet == null || ItemSet.Length == 0) return;
                for (int n = 0; n < ItemSet.Length; n++)
                {
                    var _item = ItemSet[n];
                    //tooltips.Add(new TooltipLine(Mod, _item.Name, _item.Name) { OverrideColor = Color.Aqua });

                    #region 原版TooltipLine

                    bool settingsEnabled_OpaqueBoxBehindTooltips = Main.SettingsEnabled_OpaqueBoxBehindTooltips;
                    Color color = new(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor);
                    int yoyoLogo = -1;
                    int researchLine = -1;
                    var rare = _item.rare;
                    float knockBack = _item.knockBack;
                    float num = 1f;
                    if (_item.DamageType == DamageClass.Melee && Main.LocalPlayer.kbGlove)
                        num += 1f;

                    if (Main.LocalPlayer.kbBuff)
                        num += 0.5f;

                    if (num != 1f)
                        _item.knockBack *= num;

                    if (_item.DamageType == DamageClass.Ranged && Main.LocalPlayer.shroomiteStealth)
                        _item.knockBack *= 1f + (1f - Main.LocalPlayer.stealth) * 0.5f;

                    int num2 = 30;
                    int numLines = 1;
                    string[] array = new string[num2];
                    bool[] array2 = new bool[num2];
                    bool[] array3 = new bool[num2];
                    for (int i = 0; i < num2; i++)
                    {
                        array2[i] = false;
                        array3[i] = false;
                    }
                    string[] tooltipNames = new string[num2];
                    Main.MouseText_DrawItemTooltip_GetLinesInfo(_item, ref yoyoLogo, ref researchLine, knockBack, ref numLines, array, array2, array3, tooltipNames, out int prefixlineIndex);
                    float num3 = Main.mouseTextColor / 255f;
                    float num4 = num3;
                    int a = Main.mouseTextColor;
                    if (Main.npcShop > 0 && _item.value >= 0 && (_item.type < ItemID.CopperCoin || _item.type > ItemID.PlatinumCoin))
                    {
                        Main.LocalPlayer.GetItemExpectedPrice(_item, out long calcForSelling, out long calcForBuying);
                        long num5 = (_item.isAShopItem || _item.buyOnce) ? calcForBuying : calcForSelling;
                        if (_item.shopSpecialCurrency != -1)
                        {
                            tooltipNames[numLines] = "SpecialPrice";
                            Terraria.GameContent.UI.CustomCurrencyManager.GetPriceText(_item.shopSpecialCurrency, array, ref numLines, num5);
                            color = new Color((byte)(255f * num4), (byte)(255f * num4), (byte)(255f * num4), a);
                        }
                        else if (num5 > 0)
                        {
                            string text = "";
                            long num6 = 0;
                            long num7 = 0;
                            long num8 = 0;
                            long num9 = 0;
                            long num10 = num5 * _item.stack;
                            if (!_item.buy)
                            {
                                num10 = num5 / 5;
                                if (num10 < 1)
                                    num10 = 1;

                                long num11 = num10;
                                num10 *= _item.stack;
                                int amount = Main.shopSellbackHelper.GetAmount(_item);
                                if (amount > 0)
                                    num10 += (-num11 + calcForBuying) * Math.Min(amount, _item.stack);
                            }

                            if (num10 < 1)
                                num10 = 1;

                            if (num10 >= 1000000)
                            {
                                num6 = num10 / 1000000;
                                num10 -= num6 * 1000000;
                            }

                            if (num10 >= 10000)
                            {
                                num7 = num10 / 10000;
                                num10 -= num7 * 10000;
                            }

                            if (num10 >= 100)
                            {
                                num8 = num10 / 100;
                                num10 -= num8 * 100;
                            }

                            if (num10 >= 1)
                                num9 = num10;

                            if (num6 > 0)
                                text = text + num6 + " " + Lang.inter[15].Value + " ";

                            if (num7 > 0)
                                text = text + num7 + " " + Lang.inter[16].Value + " ";

                            if (num8 > 0)
                                text = text + num8 + " " + Lang.inter[17].Value + " ";

                            if (num9 > 0)
                                text = text + num9 + " " + Lang.inter[18].Value + " ";

                            if (!_item.buy)
                                array[numLines] = Lang.tip[49].Value + " " + text;
                            else
                                array[numLines] = Lang.tip[50].Value + " " + text;

                            tooltipNames[numLines] = "Price";
                            numLines++;
                            if (num6 > 0)
                                color = new Color((byte)(220f * num4), (byte)(220f * num4), (byte)(198f * num4), a);
                            else if (num7 > 0)
                                color = new Color((byte)(224f * num4), (byte)(201f * num4), (byte)(92f * num4), a);
                            else if (num8 > 0)
                                color = new Color((byte)(181f * num4), (byte)(192f * num4), (byte)(193f * num4), a);
                            else if (num9 > 0)
                                color = new Color((byte)(246f * num4), (byte)(138f * num4), (byte)(96f * num4), a);
                        }
                        else if (_item.type != ItemID.DefenderMedal)
                        {
                            array[numLines] = Lang.tip[51].Value;
                            tooltipNames[numLines] = "Price";
                            numLines++;
                            color = new Color((byte)(120f * num4), (byte)(120f * num4), (byte)(120f * num4), a);
                        }
                    }

                    Vector2 zero = Vector2.Zero;
                    List<TooltipLine> lines = ItemLoader.ModifyTooltips(_item, ref numLines, tooltipNames, ref array, ref array2, ref array3, ref yoyoLogo, out Color?[] overrideColor, prefixlineIndex);
                    //List<DrawableTooltipLine> drawableLines = lines.Select((TooltipLine x, int i) => new DrawableTooltipLine(x, i, 0, 0, Color.White)).ToList();

                    for (int k = 0; k < lines.Count; k++)
                    {
                        Color black = Color.Black;
                        black = new Color(num4, num4, num4, num4);
                        if (lines[k].Mod == "Terraria" && lines[k].Name == "ItemName")
                        {
                            if (rare == -11)
                                black = new Color((byte)(255f * num4), (byte)(175f * num4), (byte)(0f * num4), a);

                            if (rare == -1)
                                black = new Color((byte)(130f * num4), (byte)(130f * num4), (byte)(130f * num4), a);

                            if (rare == 1)
                                black = new Color((byte)(150f * num4), (byte)(150f * num4), (byte)(255f * num4), a);

                            if (rare == 2)
                                black = new Color((byte)(150f * num4), (byte)(255f * num4), (byte)(150f * num4), a);

                            if (rare == 3)
                                black = new Color((byte)(255f * num4), (byte)(200f * num4), (byte)(150f * num4), a);

                            if (rare == 4)
                                black = new Color((byte)(255f * num4), (byte)(150f * num4), (byte)(150f * num4), a);

                            if (rare == 5)
                                black = new Color((byte)(255f * num4), (byte)(150f * num4), (byte)(255f * num4), a);

                            if (rare == 6)
                                black = new Color((byte)(210f * num4), (byte)(160f * num4), (byte)(255f * num4), a);

                            if (rare == 7)
                                black = new Color((byte)(150f * num4), (byte)(255f * num4), (byte)(10f * num4), a);

                            if (rare == 8)
                                black = new Color((byte)(255f * num4), (byte)(255f * num4), (byte)(10f * num4), a);

                            if (rare == 9)
                                black = new Color((byte)(5f * num4), (byte)(200f * num4), (byte)(255f * num4), a);

                            if (rare == 10)
                            {
                                black = new Color((byte)(255f * num4), (byte)(40f * num4), (byte)(100f * num4), a);
                            }

                            if (rare == 11)
                                black = new Color((byte)(180f * num4), (byte)(40f * num4), (byte)(255f * num4), a);

                            if (rare > 11)
                                black = GetRarity(rare).RarityColor * num4;

                            if (_item.expert || rare == -12)
                                black = new Color((byte)(Main.DiscoR * num4), (byte)(Main.DiscoG * num4), (byte)(Main.DiscoB * num4), a);

                            if (_item.master || rare == -13)
                                black = new Color((byte)(255f * num4), (byte)(Main.masterColor * 200f * num4), 0, a);
                        }
                        else if (array2[k])
                        {
                            black = array3[k] ? new Color((byte)(190f * num4), (byte)(120f * num4), (byte)(120f * num4), a) : new Color((byte)(120f * num4), (byte)(190f * num4), (byte)(120f * num4), a);
                        }
                        else if (lines[k].Mod == "Terraria" && lines[k].Name == "Price")
                        {
                            black = color;
                        }

                        if (lines[k].Mod == "Terraria" && lines[k].Name == "JourneyResearch")
                            black = Colors.JourneyMode;

                        //drawableLines[k].Color = black;
                        //Color realLineColor = black;

                        //if (overrideColor[k].HasValue)
                        //{
                        //    realLineColor = overrideColor[k].Value * num4;
                        //    lines[k].OverrideColor = realLineColor;
                        //}
                        lines[k].OverrideColor = black;
                    }

                    #region Tooltip绘制
                    //List<DrawableTooltipLine> drawableLines = lines.Select((TooltipLine x, int i) => new DrawableTooltipLine(x, i, 0, 0, Color.White)).ToList();
                    //int num12 = 0;
                    //for (int j = 0; j < numLines; j++)
                    //{
                    //    Vector2 stringSize = ChatManager.GetStringSize(FontAssets.MouseText.Value, array[j], Vector2.One);
                    //    if (stringSize.X > zero.X)
                    //        zero.X = stringSize.X;

                    //    zero.Y += stringSize.Y + (float)num12;
                    //}

                    //if (yoyoLogo != -1)
                    //    zero.Y += 24f;

                    //var X = 6;
                    //var Y = 6;
                    //int num13 = 4;
                    //if (settingsEnabled_OpaqueBoxBehindTooltips)
                    //{
                    //    X += 8;
                    //    Y += 2;
                    //    num13 = 18;
                    //}

                    //int num14 = Main.screenWidth;
                    //int num15 = Main.screenHeight;
                    //if ((float)X + zero.X + (float)num13 > (float)num14)
                    //    X = (int)((float)num14 - zero.X - (float)num13);

                    //if ((float)Y + zero.Y + (float)num13 > (float)num15)
                    //    Y = (int)((float)num15 - zero.Y - (float)num13);

                    //int num16 = 0;
                    //num3 = (float)(int)Main.mouseTextColor / 255f;
                    //if (settingsEnabled_OpaqueBoxBehindTooltips)
                    //{
                    //    num3 = MathHelper.Lerp(num3, 1f, 1f);
                    //    int num17 = 14;
                    //    int num18 = 9;
                    //    Utils.DrawInvBG(Main.spriteBatch, new Microsoft.Xna.Framework.Rectangle(X - num17, Y - num18, (int)zero.X + num17 * 2, (int)zero.Y + num18 + num18 / 2), new Microsoft.Xna.Framework.Color(23, 25, 81, 255) * 0.925f);
                    //}

                    //bool globalCanDraw = ItemLoader.PreDrawTooltip(_item, lines.AsReadOnly(), ref X, ref Y);
                    //for (int k = 0; k < numLines; k++)
                    //{
                    //    drawableLines[k].OriginalX = X;
                    //    drawableLines[k].OriginalY = Y + num16;
                    //    if (drawableLines[k].Mod == "Terraria" && drawableLines[k].Name == "OneDropLogo")
                    //    {
                    //        int num20 = (int)((float)(int)mouseTextColor * 1f);
                    //        Color color2 = Color.Black;
                    //        drawableLines[k].Color = new Color(num20, num20, num20, num20);
                    //        if (!ItemLoader.PreDrawTooltipLine(HoverItem, drawableLines[k], ref num12) || !globalCanDraw)
                    //            goto PostDraw;

                    //        for (int l = 0; l < 5; l++)
                    //        {
                    //            int num21 = drawableLines[k].X;
                    //            int num22 = drawableLines[k].Y;
                    //            if (l == 4)
                    //                color2 = new Microsoft.Xna.Framework.Color(num20, num20, num20, num20);

                    //            switch (l)
                    //            {
                    //                case 0:
                    //                    num21--;
                    //                    break;
                    //                case 1:
                    //                    num21++;
                    //                    break;
                    //                case 2:
                    //                    num22--;
                    //                    break;
                    //                case 3:
                    //                    num22++;
                    //                    break;
                    //            }
                    //            Color drawColor2 = drawableLines[k].OverrideColor ?? drawableLines[k].Color;
                    //            Main.spriteBatch.Draw(TextureAssets.OneDropLogo.Value, new Vector2(num21, num22), null, (l != 4) ? color2 : drawColor2, drawableLines[k].Rotation, drawableLines[k].Origin, (drawableLines[k].BaseScale.X + drawableLines[k].BaseScale.Y) / 2f, SpriteEffects.None, 0f);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        Microsoft.Xna.Framework.Color black = Microsoft.Xna.Framework.Color.Black;
                    //        black = new Microsoft.Xna.Framework.Color(num4, num4, num4, num4);
                    //        if (drawableLines[k].Mod == "Terraria" && drawableLines[k].Name == "ItemName")
                    //        {
                    //            if (rare == -11)
                    //                black = new Microsoft.Xna.Framework.Color((byte)(255f * num4), (byte)(175f * num4), (byte)(0f * num4), a);

                    //            if (rare == -1)
                    //                black = new Microsoft.Xna.Framework.Color((byte)(130f * num4), (byte)(130f * num4), (byte)(130f * num4), a);

                    //            if (rare == 1)
                    //                black = new Microsoft.Xna.Framework.Color((byte)(150f * num4), (byte)(150f * num4), (byte)(255f * num4), a);

                    //            if (rare == 2)
                    //                black = new Microsoft.Xna.Framework.Color((byte)(150f * num4), (byte)(255f * num4), (byte)(150f * num4), a);

                    //            if (rare == 3)
                    //                black = new Microsoft.Xna.Framework.Color((byte)(255f * num4), (byte)(200f * num4), (byte)(150f * num4), a);

                    //            if (rare == 4)
                    //                black = new Microsoft.Xna.Framework.Color((byte)(255f * num4), (byte)(150f * num4), (byte)(150f * num4), a);

                    //            if (rare == 5)
                    //                black = new Microsoft.Xna.Framework.Color((byte)(255f * num4), (byte)(150f * num4), (byte)(255f * num4), a);

                    //            if (rare == 6)
                    //                black = new Microsoft.Xna.Framework.Color((byte)(210f * num4), (byte)(160f * num4), (byte)(255f * num4), a);

                    //            if (rare == 7)
                    //                black = new Microsoft.Xna.Framework.Color((byte)(150f * num4), (byte)(255f * num4), (byte)(10f * num4), a);

                    //            if (rare == 8)
                    //                black = new Microsoft.Xna.Framework.Color((byte)(255f * num4), (byte)(255f * num4), (byte)(10f * num4), a);

                    //            if (rare == 9)
                    //                black = new Microsoft.Xna.Framework.Color((byte)(5f * num4), (byte)(200f * num4), (byte)(255f * num4), a);

                    //            if (rare == 10)
                    //                black = new Microsoft.Xna.Framework.Color((byte)(255f * num4), (byte)(40f * num4), (byte)(100f * num4), a);

                    //            if (rare == 11)
                    //                black = new Microsoft.Xna.Framework.Color((byte)(180f * num4), (byte)(40f * num4), (byte)(255f * num4), a);

                    //            if (rare > 11)
                    //                black = GetRarity(rare).RarityColor * num4;


                    //            if (_item.expert || rare == -12)
                    //                black = new Microsoft.Xna.Framework.Color((byte)((float)Main.DiscoR * num4), (byte)((float)Main.DiscoG * num4), (byte)((float)Main.DiscoB * num4), a);

                    //            if (_item.master || rare == -13)
                    //                black = new Microsoft.Xna.Framework.Color((byte)(255f * num4), (byte)(Main.masterColor * 200f * num4), 0, a);
                    //        }
                    //        else if (array2[k])
                    //        {
                    //            black = (array3[k] ? new Microsoft.Xna.Framework.Color((byte)(190f * num4), (byte)(120f * num4), (byte)(120f * num4), a) : new Microsoft.Xna.Framework.Color((byte)(120f * num4), (byte)(190f * num4), (byte)(120f * num4), a));
                    //        }
                    //        else if (drawableLines[k].Mod == "Terraria" && drawableLines[k].Name == "Price")
                    //        {
                    //            black = color;
                    //        }

                    //        if (drawableLines[k].Mod == "Terraria" && drawableLines[k].Name == "JourneyResearch")
                    //            black = Colors.JourneyMode;

                    //        drawableLines[k].Color = black;
                    //        Color realLineColor = black;

                    //        if (overrideColor[k].HasValue)
                    //        {
                    //            realLineColor = overrideColor[k].Value * num4;
                    //            drawableLines[k].OverrideColor = realLineColor;
                    //        }

                    //        if (!ItemLoader.PreDrawTooltipLine(_item, drawableLines[k], ref num12) || !globalCanDraw)
                    //            goto PostDraw;

                    //        ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, drawableLines[k].Font, drawableLines[k].Text, new Vector2(drawableLines[k].X, drawableLines[k].Y), realLineColor, drawableLines[k].Rotation, drawableLines[k].Origin, drawableLines[k].BaseScale, drawableLines[k].MaxWidth, drawableLines[k].Spread);
                    //    }

                    //PostDraw:
                    //    ItemLoader.PostDrawTooltipLine(_item, drawableLines[k]);

                    //    num16 += (int)(FontAssets.MouseText.Value.MeasureString(drawableLines[k].Text).Y + (float)num12);
                    //}

                    //ItemLoader.PostDrawTooltip(_item, drawableLines.AsReadOnly());
                    #endregion


                    tooltips.AddRange(lines);


                    #endregion
                }

            }
            catch { }
        }
        public override void SaveData(TagCompound tag)
        {
            //SaveData(tag, 0);
            tag.Add("item1", ItemIO.Save(item1));
            tag.Add("item2", ItemIO.Save(item2));
            //tag.Add("YEEValue", omgValue);

        }
        public override void LoadData(TagCompound tag)
        {
            //omgValue = tag.GetInt("YEEValue");

            //LoadData(tag, 0);
            theItems.Item1 = new Item() { type = ItemID.IronPickaxe };
            theItems.Item2 = new Item() { type = ItemID.IronPickaxe };
            ItemIO.Load(item1, tag.Get<TagCompound>("item1"));
            ItemIO.Load(item2, tag.Get<TagCompound>("item2"));
            var list = new List<Item>();
            GetItemSet(list);
            ItemSet = [.. list];
            SetDefaults();
        }
        public static ModRarity GetRarity(int type)
        {
            var rarities = RarityLoader.rarities;
            return type >= ItemRarityID.Count && type < RarityLoader.RarityCount ? rarities[type - ItemRarityID.Count] : null;
        }
    }
    public class CopyItem : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.SlimeHook;
        public override void Update(ref float gravity, ref float maxFallSpeed)
        {
            foreach (var i in Main.item)
            {
                if (i.active && i.type != ItemID.None && i.type != Type && Vector2.Distance(Item.Center, i.Center) <= 64)
                {
                    Item.stack--;
                    if (Item.stack <= 0) Item.TurnToAir();
                    var index = Item.NewItem(Item.GetSource_Misc(""), i.Center, 1);
                    var _item = Main.item[index] = i.Clone();
                    _item.whoAmI = index;
                    _item.velocity = -i.velocity;
                    _item.Center = Item.Center;

                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        NetMessage.SendData(MessageID.SyncItem, -1, -1, null, index, 1f);
                        NetMessage.SendData(MessageID.SyncItem, -1, -1, null, Item.whoAmI, 1f);
                    }
                    break;
                }
            }
        }
        public override void SetDefaults()
        {
            Item.maxStack = 999;
            Item.height = Item.width = 10;
        }
    }
    public class ShowItem : ModItem
    {
        public override string Texture => "Terraria/Images/Item_1";
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (Main.LocalPlayer == null || StickySystem.weaponTypes == null) return true;

            //if ((int)(Main.GlobalTimeWrappedHourly * 60) % 60 == 0)
            //    TextureAssets.Item[Type] = TextureAssets.Item[StickySystem.weaponTypes == null ? 1 : Main.rand.Next(StickySystem.weaponTypes)];
            bool foundFirst = false;
            foreach (var item in Main.LocalPlayer.inventory)
            {
                if (StickySystem.weaponTypes.Contains(item.type))
                {
                    if (!foundFirst)
                    {
                        foundFirst = true;
                        if (!FoundFirstWeaponInInventory)
                        {
                            FoundFirstWeaponInInventory = true;
                            TextureAssets.Item[Type] = TextureAssets.Item[item.type];
                            break;
                        }
                    }
                    else
                    {
                        FoundFirstWeaponInInventory = false;
                        TextureAssets.Item[Type] = TextureAssets.Item[item.type];
                        break;
                    }

                }
            }
            //spriteBatch.DrawString(FontAssets.MouseText.Value, FoundFirstWeaponInInventory.ToString(), position, Color.White);
            return base.PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
        }
        public static bool FoundFirstWeaponInInventory;
        public override void UpdateInventory(Player player)
        {
            Item.TurnToAir();
            base.UpdateInventory(player);
        }
    }
    public class ReturnStickyBag : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = Item.height = 32;
            Item.value = 1;
            Item.rare = ItemRarityID.Cyan;
            Item.maxStack = 114514;
            base.SetDefaults();
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (items == null) return;
            foreach (var item in items)
            {
                var tip = new TooltipLine(Mod, "Items", $"[i:{item.type}]{item.Name} x {item.stack}");
                var rare = item.rare;
                float num3 = Main.mouseTextColor / 255f;
                float num4 = num3;
                int a = Main.mouseTextColor;
                var color = new Color(num4, num4, num4, num4);
                if (rare == -11)
                    color = new Color((byte)(255f * num4), (byte)(175f * num4), (byte)(0f * num4), a);

                if (rare == -1)
                    color = new Color((byte)(130f * num4), (byte)(130f * num4), (byte)(130f * num4), a);

                if (rare == 1)
                    color = new Color((byte)(150f * num4), (byte)(150f * num4), (byte)(255f * num4), a);

                if (rare == 2)
                    color = new Color((byte)(150f * num4), (byte)(255f * num4), (byte)(150f * num4), a);

                if (rare == 3)
                    color = new Color((byte)(255f * num4), (byte)(200f * num4), (byte)(150f * num4), a);

                if (rare == 4)
                    color = new Color((byte)(255f * num4), (byte)(150f * num4), (byte)(150f * num4), a);

                if (rare == 5)
                    color = new Color((byte)(255f * num4), (byte)(150f * num4), (byte)(255f * num4), a);

                if (rare == 6)
                    color = new Color((byte)(210f * num4), (byte)(160f * num4), (byte)(255f * num4), a);

                if (rare == 7)
                    color = new Color((byte)(150f * num4), (byte)(255f * num4), (byte)(10f * num4), a);

                if (rare == 8)
                    color = new Color((byte)(255f * num4), (byte)(255f * num4), (byte)(10f * num4), a);

                if (rare == 9)
                    color = new Color((byte)(5f * num4), (byte)(200f * num4), (byte)(255f * num4), a);

                if (rare == 10)
                {
                    color = new Color((byte)(255f * num4), (byte)(40f * num4), (byte)(100f * num4), a);
                }

                if (rare == 11)
                    color = new Color((byte)(180f * num4), (byte)(40f * num4), (byte)(255f * num4), a);

                if (rare > 11)
                    color = StickyItem.GetRarity(rare).RarityColor * num4;

                if (item.expert || rare == -12)
                    color = new Color((byte)(Main.DiscoR * num4), (byte)(Main.DiscoG * num4), (byte)(Main.DiscoB * num4), a);

                if (item.master || rare == -13)
                    color = new Color((byte)(255f * num4), (byte)(Main.masterColor * 200f * num4), 0, a);
                tip.OverrideColor = color;
                tooltips.Add(tip);
            }
            base.ModifyTooltips(tooltips);
        }
        public Item[] items;
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("返还袋");
            // Tooltip.SetDefault("包含以下物品");
            base.SetStaticDefaults();
        }
        public override bool CanRightClick()
        {
            return true;
        }
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
    public class StickySystem : ModSystem
    {
        public static int[] weaponTypes;
        public RecipeGroup recipeGroup;
        public override void AddRecipeGroups()
        {
            List<int> types =
            [
                ModContent.ItemType<ShowItem>()
            ];
            for (int i = 0; i < ItemLoader.ItemCount; i++)
            {
                Item item = new(i);
                if (Glue.CanChoose(item)) types.Add(i);
            }
            weaponTypes = [.. types];
            recipeGroup = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " 武器Weapons(包里前两个The first two in Inventory)", weaponTypes);
            RecipeGroup.RegisterGroup("SitckyWeapons:Weapons!!", recipeGroup);
        }
        public override void PostUpdateTime()
        {
            if (weaponTypes != null && weaponTypes.Length > 0 && (int)(Main.GlobalTimeWrappedHourly * 2) % 2 == 0)
                recipeGroup.IconicItemId = Main.rand.Next(weaponTypes);
        }
        public override void PostSetupContent()
        {
            if (ModLoader.TryGetMod("CoolerItemVisualEffect", out var result))
            {
                result.Call("RegisterModifyWeaponTex", (Func<Item, Texture2D>)(item => StickyWeapons.GetWeaponTextureFromItem(null, item)), 1f);
            }
            base.PostSetupContent();
        }
        public override void AddRecipes()
        {
            Recipe recipe = Recipe.Create(ModContent.ItemType<StickyItem>());
            recipe.AddIngredient<Glue>(1);
            recipe.AddRecipeGroup("SitckyWeapons:Weapons!!");
            recipe.AddRecipeGroup("SitckyWeapons:Weapons!!");
            recipe.AddOnCraftCallback
            (
                (recipe, item, consumedItems, stack) =>
                {
                    StickyItem sticky = item.ModItem as StickyItem;
                    var items = from target in consumedItems where target.type != ModContent.ItemType<Glue>() select target;
                    var array = items.ToArray();
                    var item1 = sticky.theItems.Item1 = array[0].Clone();
                    var item2 = sticky.theItems.Item2 = array[1].Clone();
                    item1.stack = 1;
                    item2.stack = 1;
                    if (item1.ModItem != null && item1.ModItem is StickyItem _sticky1)
                    {
                        Main.RunOnMainThread(() =>
                        {
                            _sticky1.complexTexture?.Dispose();
                            _sticky1.complexTexture = null;
                        });
                    }
                    if (item2.ModItem != null && item2.ModItem is StickyItem _sticky2)
                    {
                        Main.RunOnMainThread(() =>
                        {
                            _sticky2.complexTexture?.Dispose();
                            _sticky2.complexTexture = null;
                        });
                    }
                    var list = new List<Item>();
                    sticky.GetItemSet(list);
                    sticky.ItemSet = [.. list];
                    sticky.SetDefaults();

                    //foreach (var i in consumedItems)
                    //{
                    //    Main.NewText((i.Clone().Name, i.type, i.stack));
                    //}
                }
            );
            recipe.Register();

            recipe = Recipe.Create(ModContent.ItemType<ReturnStickyBag>());
            recipe.AddIngredient<StickyItem>(1);
            recipe.AddIngredient<OrganicSolvent>(1);
            recipe.AddOnCraftCallback
            (
                (recipe, item, consumedItems, stack) =>
                {
                    ReturnStickyBag returnStickyBag = item.ModItem as ReturnStickyBag;
                    List<Item> items = [];
                    var set = (consumedItems[0].ModItem as StickyItem).ItemSet;
                    foreach (var _item in set)
                    {
                        items.Add(_item.Clone());
                    }
                    items.Add(new Item(ItemID.Ale));
                    items.Add(new Item(ItemID.Gel, set.Length * 5));
                    returnStickyBag.items = [.. items];
                }
            );
            recipe.Register();
        }
    }
    //public class TestClass : Projectile
    //{
    //    private void AI_075()
    //    {
    //        Player player = Main.player[owner];
    //        float num = (float)Math.PI / 2f;
    //        Vector2 vector = player.RotatedRelativePoint(player.MountedCenter);
    //        int num2 = 2;
    //        float num3 = 0f;
    //        if (type == 633)
    //        {
    //            float num35 = 30f;
    //            if (ai[0] > 90f)
    //                num35 = 15f;

    //            if (ai[0] > 120f)
    //                num35 = 5f;

    //            damage = (int)player.GetTotalDamage(DamageClass.Magic).ApplyTo(player.inventory[player.selectedItem].damage);
    //            ai[0] += 1f;
    //            ai[1] += 1f;
    //            bool flag8 = false;
    //            if (ai[0] % num35 == 0f)
    //                flag8 = true;

    //            int num36 = 10;
    //            bool flag9 = false;
    //            if (ai[0] % num35 == 0f)
    //                flag9 = true;

    //            if (ai[1] >= 1f)
    //            {
    //                ai[1] = 0f;
    //                flag9 = true;
    //                if (Main.myPlayer == owner)
    //                {
    //                    float num37 = player.inventory[player.selectedItem].shootSpeed * scale;
    //                    Vector2 value14 = vector;
    //                    Vector2 value15 = Main.screenPosition + new Vector2(Main.mouseX, Main.mouseY) - value14;
    //                    if (player.gravDir == -1f)
    //                        value15.Y = (float)(Main.screenHeight - Main.mouseY) + Main.screenPosition.Y - value14.Y;

    //                    Vector2 value16 = Vector2.Normalize(value15);
    //                    if (float.IsNaN(value16.X) || float.IsNaN(value16.Y))
    //                        value16 = -Vector2.UnitY;

    //                    value16 = Vector2.Normalize(Vector2.Lerp(value16, Vector2.Normalize(base.velocity), 0.92f));
    //                    value16 *= num37;
    //                    if (value16.X != base.velocity.X || value16.Y != base.velocity.Y)
    //                        netUpdate = true;

    //                    base.velocity = value16;
    //                }
    //            }

    //            frameCounter++;
    //            int num38 = (!(ai[0] < 120f)) ? 1 : 4;
    //            if (frameCounter >= num38)
    //            {
    //                frameCounter = 0;
    //                if (++frame >= 5)
    //                    frame = 0;
    //            }

    //            if (soundDelay <= 0)
    //            {
    //                soundDelay = num36;
    //                soundDelay *= 2;
    //                if (ai[0] != 1f)
    //                    SoundEngine.PlaySound(SoundID.Item15, base.position);
    //            }

    //            if (flag9 && Main.myPlayer == owner)
    //            {
    //                bool flag10 = false;
    //                flag10 = (!flag8 || player.CheckMana(player.inventory[player.selectedItem], pay: true));
    //                if (player.channel && flag10 && !player.noItems && !player.CCed)
    //                {
    //                    if (ai[0] == 1f)
    //                    {
    //                        Vector2 center2 = base.Center;
    //                        Vector2 vector8 = Vector2.Normalize(base.velocity);
    //                        if (float.IsNaN(vector8.X) || float.IsNaN(vector8.Y))
    //                            vector8 = -Vector2.UnitY;

    //                        int num39 = damage;
    //                        for (int m = 0; m < 6; m++)
    //                        {
    //                            NewProjectile(GetProjectileSource_FromThis(), center2.X, center2.Y, vector8.X, vector8.Y, 632, num39, knockBack, owner, m, whoAmI);
    //                        }

    //                        netUpdate = true;
    //                    }
    //                }
    //                else
    //                {
    //                    Kill();
    //                }
    //            }
    //        }
    //        base.position = player.RotatedRelativePoint(player.MountedCenter, reverseRotation: false, addGfxOffY: false) - base.Size / 2f;
    //        rotation = base.velocity.ToRotation() + num;
    //        spriteDirection = direction;
    //        timeLeft = 2;
    //        player.ChangeDir(direction);
    //        player.heldProj = whoAmI;
    //        player.SetDummyItemTime(num2);
    //        player.itemRotation = MathHelper.WrapAngle((float)Math.Atan2(base.velocity.Y * (float)direction, base.velocity.X * (float)direction) + num3);
    //    }
    //}
}