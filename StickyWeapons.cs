using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.Exceptions;
using Terraria.ModLoader.IO;
using Terraria.UI.Chat;
using Mono.Cecil.Cil;
using static Mono.Cecil.Cil.OpCodes;
using MonoMod.RuntimeDetour;
using Terraria.ObjectData;
using static Terraria.Player;
using Terraria.GameContent.Golf;
using Terraria.GameInput;
using Terraria.Audio;
using static StickyWeapons.StickyFunc;
using Terraria.DataStructures;
using System.IO;

namespace StickyWeapons
{
    public class StickyPlayer : ModPlayer
    {
        public Item[] items = null;
        public int index = -1;
        public int max = -1;
        public bool moreTimeShoot;
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
            On.Terraria.Player.ApplyItemTime += Player_ApplyItemTime_Sticky_On;
            //On.Terraria.Player.ItemCheck_GetMeleeHitbox += Player_ItemCheck_GetMeleeHitbox;
            On.Terraria.Player.ItemCheck_Inner += Player_ItemCheck_Inner_Sticky_On;
            //On.Terraria.Player.ge
            base.Load();
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

        private void Player_ItemCheck_Inner_Sticky_On(On.Terraria.Player.orig_ItemCheck_Inner orig, Player self, int i)
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
                orig.Invoke(self, i);
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
            if (stickyPlr.index >= stickyPlr.max) return;
            Item item = stickyPlr.items[stickyPlr.index];
            if (Main.myPlayer == i && Terraria.GameInput.PlayerInput.ShouldFastUseItem)
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

                    self.PlaceThing();
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

        private void Player_ApplyItemTime_Sticky_On(On.Terraria.Player.orig_ApplyItemTime orig, Player self, Item sItem, float multiplier, bool? callUseItem)
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
            ILCursor cursor = new ILCursor(il);
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

            ILCursor cursor = new ILCursor(il);
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
            ILCursor cursor = new ILCursor(il);
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
        public bool CanChoose(Item _item) => _item.active && _item.type != ItemID.None && (_item.damage > 0 || _item.type == ModContent.ItemType<StickyItem>()) && _item.maxStack == 1 && !_item.consumable && _item.useAnimation > 2 && Vector2.DistanceSquared(_item.Center, Item.Center) <= 4096;
        public override void Update(ref float gravity, ref float maxFallSpeed)
        {
            //Main.NewText(StickyWeapons.value);
            Item item1 = null;
            foreach (var _item in Main.item)
            {
                if (CanChoose(_item))
                {
                    item1 = _item;
                    break;
                }
            }
            if (item1 == null) return;
            foreach (var _item in Main.item)
            {
                if (CanChoose(_item) && _item.GetHashCode() != item1.GetHashCode())
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
                        item1.TurnToAir();
                        _item.TurnToAir();
                        Item.stack--;
                        if (Item.stack <= 0) Item.TurnToAir();
                        var list = new List<Item>();
                        sticky.GetItemSet(list);
                        sticky.ItemSet = list.ToArray();
                        sticky.SetDefaults();
                        //Item.TurnToAir();

                    }
                    else
                    {
                        stickyIndex.TurnToAir();
                    }
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
                    Item.stack--;
                    if (Item.stack <= 0) Item.TurnToAir();
                    foreach (var _item in sticky.ItemSet)
                    {
                        var index = Item.NewItem(Item.GetSource_Misc(""), Item.Center, 1);
                        Main.item[index] = _item.Clone();
                        var currentItem = Main.item[index];
                        //currentItem.whoAmI = index;
                        currentItem.velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 4);
                        Main.NewText((_item.Name, currentItem.Name, currentItem.active, currentItem.stack, _item.whoAmI, index, currentItem.whoAmI));
                        if (Main.netMode == NetmodeID.MultiplayerClient)
                            NetMessage.SendData(MessageID.SyncItem, -1, -1, null, index, 1f);
                    }
                    for (int n = 0; n < 100; n++)
                    {
                        Dust.NewDustPerfect(Item.Center, DustID.Clentaminator_Cyan, (n / 99f * MathHelper.TwoPi).ToRotationVector2()).noGravity = true;
                    }
                    var index1 = Item.NewItem(Item.GetSource_Misc(""), i.Center, ItemID.Gel, 5 * sticky.ItemSet.Length);
                    var index2 = Item.NewItem(Item.GetSource_Misc(""), i.Center, ItemID.Ale);
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        NetMessage.SendData(MessageID.SyncItem, -1, -1, null, index1, 1f);
                        NetMessage.SendData(MessageID.SyncItem, -1, -1, null, index2, 1f);
                    }
                    i.TurnToAir();
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
            ItemSet = list.ToArray();
            SetDefaults();
            base.NetReceive(reader);
        }
        public override void NetSend(BinaryWriter writer)
        {
            ItemIO.Send(item1, writer, true, true);
            ItemIO.Send(item2, writer, true, true);
        }
        public override void SetDefaults()
        {
            if (ItemSet == null) return;
            int width = 0;
            int height = 0;
            int rare = -114514;
            int value = 0;
            int useTime = 0;
            int useAnimation = 0;
            float shootSpeed = 0;
            bool channel = false;
            float knockBack = 0;
            int damage = 0;
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
            }
            Item.width = width;
            Item.height = height;
            Item.rare = rare;
            Item.value = value - 5;
            Item.useTime = useTime;
            Item.useAnimation = useAnimation;
            Item.shootSpeed = shootSpeed;
            Item.damage = damage;
            Item.knockBack = knockBack;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.channel = channel;
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
            item1?.StickyDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin);
            item2?.StickyDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin);
            //Main.NewText()
            return false;
        }
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            if (item1 == null || item2 == null) return false;
            if (ItemSet == null) return false;
            for (int n = 0; n < ItemSet.Length; n++)
            {
                float _rotation = rotation;
                float _scale = scale;
                var _item = ItemSet[n];
                if (_item == null) continue;
                if (_item.ModItem == null || _item.ModItem.PreDrawInWorld(spriteBatch, lightColor, alphaColor, ref _rotation, ref _scale, whoAmI))
                {
                    var value = TextureAssets.Item[_item.type].Value;
                    var ani = Main.itemAnimations[_item.type];

                    spriteBatch.Draw(value, Item.Center - Main.screenPosition, (ani == null) ? value.Frame() : ani.GetFrame(value), alphaColor, rotation, value.Size() * .5f / new Vector2(1, (ani == null) ? 1 : ani.FrameCount), scale, 0, 0);
                }
            }
            //float _rotation = rotation;
            //float _scale = scale;
            //if (item1.ModItem == null || item1.ModItem.PreDrawInWorld(spriteBatch, lightColor, alphaColor, ref _rotation, ref _scale, whoAmI))
            //{
            //    var value = TextureAssets.Item[item1.type].Value;
            //    var ani = Main.itemAnimations[item1.type];

            //    spriteBatch.Draw(value, Item.Center - Main.screenPosition, (ani == null) ? value.Frame() : ani.GetFrame(value), alphaColor, rotation, value.Size() * .5f / new Vector2(1, (ani == null) ? 1 : ani.FrameCount), scale, 0, 0);
            //}
            //_rotation = rotation;
            //_scale = scale;
            //if (item2.ModItem == null || item2.ModItem.PreDrawInWorld(spriteBatch, lightColor, alphaColor, ref _rotation, ref _scale, whoAmI))
            //{
            //    var value = TextureAssets.Item[item2.type].Value;
            //    var ani = Main.itemAnimations[item2.type];
            //    spriteBatch.Draw(value, Item.Center - Main.screenPosition, (ani == null) ? value.Frame() : ani.GetFrame(value), alphaColor, rotation, value.Size() * .5f / new Vector2(1, (ani == null) ? 1 : ani.FrameCount), scale, 0, 0);
            //}
            //spriteBatch.Draw(TextureAssets.MagicPixel.Value, Item.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), Color.Red, 0, new Vector2(.5f), 16f, 0, 0);
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
                    Color color = new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor);
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
                    Main.MouseText_DrawItemTooltip_GetLinesInfo(_item, ref yoyoLogo, ref researchLine, knockBack, ref numLines, array, array2, array3, tooltipNames);
                    float num3 = Main.mouseTextColor / 255f;
                    float num4 = num3;
                    int a = Main.mouseTextColor;
                    if (Main.npcShop > 0 && _item.value >= 0 && (_item.type < ItemID.CopperCoin || _item.type > ItemID.PlatinumCoin))
                    {
                        Main.LocalPlayer.GetItemExpectedPrice(_item, out int calcForSelling, out int calcForBuying);
                        int num5 = (_item.isAShopItem || _item.buyOnce) ? calcForBuying : calcForSelling;
                        if (_item.shopSpecialCurrency != -1)
                        {
                            tooltipNames[numLines] = "SpecialPrice";
                            Terraria.GameContent.UI.CustomCurrencyManager.GetPriceText(_item.shopSpecialCurrency, array, ref numLines, num5);
                            color = new Color((byte)(255f * num4), (byte)(255f * num4), (byte)(255f * num4), a);
                        }
                        else if (num5 > 0)
                        {
                            string text = "";
                            int num6 = 0;
                            int num7 = 0;
                            int num8 = 0;
                            int num9 = 0;
                            int num10 = num5 * _item.stack;
                            if (!_item.buy)
                            {
                                num10 = num5 / 5;
                                if (num10 < 1)
                                    num10 = 1;

                                int num11 = num10;
                                num10 *= _item.stack;
                                int amount = Main.shopSellbackHelper.GetAmount(_item);
                                if (amount > 0)
                                    num10 += (-num11 + calcForBuying) * System.Math.Min(amount, _item.stack);
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
                    List<TooltipLine> lines = ItemLoader.ModifyTooltips(_item, ref numLines, tooltipNames, ref array, ref array2, ref array3, ref yoyoLogo, out Color?[] overrideColor);
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
        //public void SaveData(TagCompound tag, int depth)
        //{
        //    tag.Add("item1" + depth, Save(item1));
        //    if (item1.ModItem is StickyItem sticky1)
        //    {
        //        sticky1.SaveData(tag, depth + 1);
        //    }
        //    tag.Add("item2" + depth, Save(item2));
        //    if (item2.ModItem is StickyItem sticky2)
        //    {
        //        sticky2.SaveData(tag, depth + 1);
        //    }
        //}
        public override void SaveData(TagCompound tag)
        {
            //SaveData(tag, 0);
            tag.Add("item1", ItemIO.Save(item1));
            tag.Add("item2", ItemIO.Save(item2));
            //tag.Add("YEEValue", omgValue);

        }
        //internal static List<TagCompound> SaveGlobals(Item item)
        //{
        //    if (item.ModItem is UnloadedItem)
        //        return null; // UnloadedItems cannot have global data

        //    var list = new List<TagCompound>();

        //    var saveData = new TagCompound();

        //    foreach (var globalItem in (List<GlobalItem>)typeof(ItemLoader).GetField("globalItems", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null))
        //    {
        //        var globalItemInstance = globalItem.Instance(item);

        //        globalItemInstance?.SaveData(item, saveData);

        //        if (saveData.Count == 0)
        //            continue;

        //        list.Add(new TagCompound
        //        {
        //            ["mod"] = globalItemInstance.Mod.Name,
        //            ["name"] = globalItemInstance.Name,
        //            ["data"] = saveData
        //        });
        //        saveData = new TagCompound();
        //    }

        //    return list.Count > 0 ? list : null;
        //}
        //public static TagCompound Save(Item item)
        //{
        //    var tag = new TagCompound();

        //    if (item.type <= 0)
        //        return tag;

        //    if (item.ModItem == null)
        //    {
        //        tag.Set("mod", "Terraria");
        //        tag.Set("id", item.netID);
        //    }
        //    else
        //    {
        //        tag.Set("mod", item.ModItem.Mod.Name);
        //        tag.Set("name", item.ModItem.Name);

        //        var saveData = new TagCompound();

        //        if (item.ModItem is not StickyItem)
        //            item.ModItem.SaveData(saveData);

        //        if (saveData.Count > 0)
        //        {
        //            tag.Set("data", saveData);
        //        }
        //    }

        //    if (item.prefix != 0 && item.prefix < PrefixID.Count)
        //        tag.Set("prefix", (byte)item.prefix);

        //    if (item.prefix >= PrefixID.Count)
        //    {
        //        ModPrefix modPrefix = PrefixLoader.GetPrefix(item.prefix);

        //        if (modPrefix != null)
        //        {
        //            tag.Set("modPrefixMod", modPrefix.Mod.Name);
        //            tag.Set("modPrefixName", modPrefix.Name);
        //        }
        //    }

        //    if (item.stack > 1)
        //        tag.Set("stack", item.stack);

        //    if (item.favorited)
        //        tag.Set("fav", true);

        //    tag.Set("globalData", SaveGlobals(item));

        //    return tag;
        //}
        //internal static void LoadGlobals(Item item, IList<TagCompound> list)
        //{
        //    foreach (var tag in list)
        //    {
        //        if (ModContent.TryFind(tag.GetString("mod"), tag.GetString("name"), out GlobalItem globalItemBase) && item.TryGetGlobalItem(globalItemBase, out var globalItem))
        //        {
        //            try
        //            {
        //                globalItem.LoadData(item, tag.GetCompound("data"));
        //            }
        //            catch (Exception e)
        //            {
        //                throw new CustomModDataException(globalItem.Mod, $"Error in reading custom player data for {globalItem.FullName}", e);
        //            }
        //        }
        //        else
        //        {
        //            //Unloaded GlobalItems and GlobalItems that are no longer valid on an item (e.g. through AppliesToEntity)
        //            var data = (IList<TagCompound>)typeof(UnloadedGlobalItem).GetField("data", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(item.GetGlobalItem<UnloadedGlobalItem>());
        //            data.Add(tag);
        //        }
        //    }
        //}
        //public static void Load(Item item, TagCompound tag)
        //{
        //    string modName = tag.GetString("mod");
        //    if (modName == "")
        //    {
        //        item.netDefaults(0);
        //        return;
        //    }

        //    if (modName == "Terraria")
        //    {
        //        item.netDefaults(tag.GetInt("id"));
        //    }
        //    else
        //    {
        //        if (ModContent.TryFind(modName, tag.GetString("name"), out ModItem modItem))
        //        {
        //            item.SetDefaults(modItem.Type);
        //            if (item.ModItem is not StickyItem)
        //                item.ModItem.LoadData(tag.GetCompound("data"));
        //        }
        //        else
        //        {
        //            item.SetDefaults(ModContent.ItemType<UnloadedItem>());
        //            var unloadedItem = (UnloadedItem)item.ModItem;
        //            typeof(UnloadedItem).GetMethod("Setup", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(unloadedItem, new object[] { tag });
        //            //mi.CreateDelegate<Action<TagCompound>>().Invoke(tag);
        //            //var mi = 
        //        }
        //    }

        //    if (tag.ContainsKey("modPrefixMod") && tag.ContainsKey("modPrefixName"))
        //    {
        //        item.Prefix(ModContent.TryFind(tag.GetString("modPrefixMod"), tag.GetString("modPrefixName"), out ModPrefix prefix) ? prefix.Type : 0);
        //    }
        //    else if (tag.ContainsKey("prefix"))
        //    {
        //        item.Prefix(tag.GetByte("prefix"));
        //    }

        //    item.stack = tag.Get<int?>("stack") ?? 1;
        //    item.favorited = tag.GetBool("fav");

        //    if (!(item.ModItem is UnloadedItem))
        //        LoadGlobals(item, tag.GetList<TagCompound>("globalData"));
        //}
        //public void LoadData(TagCompound tag, int depth)
        //{
        //    theItems.Item1 = new Item() { type = 1 };
        //    theItems.Item2 = new Item() { type = 1 };
        //    Load(item1, tag.Get<TagCompound>("item1" + depth));
        //    if (item1.ModItem is StickyItem sticky1)
        //    {
        //        sticky1.LoadData(tag, depth + 1);
        //    }
        //    Load(item2, tag.Get<TagCompound>("item2" + depth));
        //    if (item2.ModItem is StickyItem sticky2)
        //    {
        //        sticky2.LoadData(tag, depth + 1);
        //    }
        //}
        //int omgValue;
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
            ItemSet = list.ToArray();
            SetDefaults();
        }
        static ModRarity GetRarity(int type)
        {
            var rarities = (List<ModRarity>)typeof(RarityLoader).GetField("rarities", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            return type >= ItemRarityID.Count && type < RarityLoader.RarityCount ? rarities[type - ItemRarityID.Count] : null;
        }
        //public override void SetStaticDefaults()
        //{
        //    DisplayName.SetDefault("黏在一起的武器！");
        //}
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
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                        NetMessage.SendData(MessageID.SyncItem, -1, -1, null, index, 1f);
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