using Microsoft.Xna.Framework;
using StickyWeapons.Items;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Achievements;
using Terraria.GameContent.Golf;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.Player;

namespace StickyWeapons;

partial class StickyWeapons
{
    static void On_Player_ItemCheck_Inner_Sticky_OnNew(On_Player.orig_ItemCheck_Inner orig, Player self)
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

        if (flag2 && self.controlUseItem && (self.itemAnimation == 0 || stickyPlr.moreTimeShoot) && item.useStyle != ItemUseStyleID.None)
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
        Rectangle drawHitbox = StickyUtils.GetWeaponFrameFromItem(default, self.HeldItem);
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
            self.ItemCheck_ApplyUseStyle(heightOffsetHitboxCenter, item, drawHitbox);
        else
            self.ItemCheck_ApplyHoldStyle(heightOffsetHitboxCenter, item, drawHitbox);

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
                        Main.dust[num14].customData = self;
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
        goto myLabel;
    }

}
