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
namespace StickyWeapons
{
    public class StickyWeapons : Mod
    {
        public override void Load()
        {
            IL.Terraria.Player.ItemCheck_Inner += Player_ItemCheck_Inner;
            base.Load();
        }
        //public delegate void RefAction<T>(ref T value);

        public delegate void RefAction<T1, T2>(ref T1 target, T2 value);

        private void Player_ItemCheck_Inner(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(i => i.MatchStloc(2)))
            {
                return;
            }
            Item[] items = null;
            int index = -1;
            int max = -1;
            //ILLabel label = null;
            cursor.EmitDelegate<Action<Item>>
            (
                item =>
                {
                    if (item.ModItem != null && item.ModItem is StickyItem sticky)
                    {
                        items = sticky.ItemSet;
                        index = 0;
                        max = items.Length;
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
            ILLabel label = cursor.DefineLabel();
            cursor.MarkLabel(label);
            cursor.Emit(Ldloca, 2);
            cursor.Emit(Ldarg_0);
            cursor.EmitDelegate<RefAction<Item, Player>>
            (
                (ref Item target, Player value) =>
                {
                    if (items != null && max > 0 && index > -1)
                    {
                        //Main.NewText(index);
                        //Main.NewText(max,Color.Red);
                        target = items[index];
                        index++;


                    }
                    else
                    {
                        target = value.HeldItem;
                    }
                    //target = value.HeldItem;

                    //Main.NewText(items != null);
                    //Main.NewText(max);
                    //Main.NewText(index);


                }
            );
            cursor.Remove();


            if (!cursor.TryGotoNext(i => i.MatchRet()))
            {
                return;
            }
            cursor.Emit(Ldc_I4, index);
            cursor.Emit(Ldc_I4, max);
            cursor.Emit(Clt);
            cursor.Emit(Brtrue_S, label);

        }
    }

    public class Glue : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("胶水");
            Tooltip.SetDefault("看起来可以把两件道具粘起来...在它把你的手和道具粘起来之前");
        }
        public bool CanChoose(Item _item) => _item.active && _item.type != 0 && (_item.damage > 0 || _item.type == ModContent.ItemType<StickyItem>()) && _item.useAnimation > 2 && Vector2.DistanceSquared(_item.Center, Item.Center) <= 4096;
        public override void Update(ref float gravity, ref float maxFallSpeed)
        {
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
                    var stickyIndex = Main.item[Item.NewItem(Item.GetSource_Misc("Sticky!"), Item.Center, ModContent.ItemType<StickyItem>())];
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
                        var list = new List<Item>();
                        sticky.GetItemSet(list);
                        sticky.ItemSet = list.ToArray();
                        int width = 0;
                        int height = 0;
                        int rare = -114514;
                        int value = 0;
                        int useTime = 0;
                        int useAnimation = 0;

                        foreach (var i in sticky.ItemSet)
                        {
                            width = i.width > width ? i.width : width;
                            height = i.height > height ? i.height : height;
                            rare = i.rare > rare ? i.rare : rare;
                            useTime = i.useTime > useTime ? i.useTime : useTime;
                            useAnimation = i.useAnimation > useAnimation ? i.useAnimation : useAnimation;

                            value += i.value + 5;
                        }
                        sticky.Item.width = width;
                        sticky.Item.height = height;
                        sticky.Item.rare = rare;
                        sticky.Item.value = value - 5;
                        sticky.Item.useTime = useTime;
                        sticky.Item.useAnimation = useAnimation;
                        sticky.Item.useStyle = 1;

                        //Item.TurnToAir();

                    }
                    else
                    {
                        stickyIndex.TurnToAir();
                    }
                    break;
                }
            }
            base.Update(ref gravity, ref maxFallSpeed);
        }
        public override void SetDefaults()
        {
            Item.maxStack = 999;
            Item.value = 1;
            Item.height = Item.width = 10;
        }
        public override string Texture => "Terraria/Images/Item_" + ItemID.Gel;
        public override void AddRecipes()
        {
            CreateRecipe().AddIngredient(ItemID.Gel, 5).Register();
        }
    }
    public class StickyItem : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.Gel;
        public Item item1
        {
            get
            {
                if (theItems.Item1 == null || theItems.Item2.type == ItemID.None) Item.TurnToAir();
                return theItems.Item1;
            }
        }
        public Item item2
        {
            get
            {
                if (theItems.Item2 == null || theItems.Item2.type == ItemID.None) Item.TurnToAir();
                return theItems.Item2;
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
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (item1 == null || item2 == null) return false;

            //return false;
            position += new Vector2(4);
            if (item1.ModItem == null || item1.ModItem.PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale))
            {
                var value = TextureAssets.Item[item1.type].Value;
                var ani = Main.itemAnimations[item1.type];
                spriteBatch.Draw(value, position, (ani == null) ? value.Frame() : ani.GetFrame(value), drawColor, 0, value.Size() * .5f / new Vector2(1, (ani == null) ? 1 : ani.FrameCount), scale, 0, 0);
            }
            if (item2.ModItem == null || item2.ModItem.PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale))
            {
                var value = TextureAssets.Item[item2.type].Value;
                var ani = Main.itemAnimations[item2.type];
                spriteBatch.Draw(value, position, (ani == null) ? value.Frame() : ani.GetFrame(value), drawColor, 0, value.Size() * .5f / new Vector2(1, (ani == null) ? 1 : ani.FrameCount), scale, 0, 0);
            }
            //spriteBatch.Draw(TextureAssets.Item[ItemID.Zenith].Value, position, null, drawColor, 0, origin, scale, 0, 0);

            return false;
        }
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            if (item1 == null || item2 == null) return false;
            for (int n = 0; n < ItemSet.Length; n++)
            {
                float _rotation = rotation;
                float _scale = scale;
                var _item = ItemSet[n];
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
                    float num3 = (float)(int)Main.mouseTextColor / 255f;
                    float num4 = num3;
                    int a = Main.mouseTextColor;
                    if (Main.npcShop > 0 && _item.value >= 0 && (_item.type < 71 || _item.type > 74))
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
                        else if (_item.type != 3817)
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
                            black = (array3[k] ? new Color((byte)(190f * num4), (byte)(120f * num4), (byte)(120f * num4), a) : new Color((byte)(120f * num4), (byte)(190f * num4), (byte)(120f * num4), a));
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
        public override void LoadData(TagCompound tag)
        {
            //LoadData(tag, 0);
            theItems.Item1 = new Item() { type = 1 };
            theItems.Item2 = new Item() { type = 1 };
            ItemIO.Load(item1, tag.Get<TagCompound>("item1"));
            ItemIO.Load(item2, tag.Get<TagCompound>("item2"));
            var list = new List<Item>();
            GetItemSet(list);
            ItemSet = list.ToArray();
            int width = 0;
            int height = 0;
            int rare = -114514;
            int value = 0;
            foreach (var i in ItemSet)
            {
                width = i.width > width ? i.width : width;
                height = i.height > height ? i.height : height;
                rare = i.rare > rare ? i.rare : rare;
                value += i.value + 5;
            }
            Item.width = width;
            Item.height = height;
            Item.rare = rare;
            Item.value = value - 5;
        }
        static ModRarity GetRarity(int type)
        {
            var rarities = (List<ModRarity>)typeof(RarityLoader).GetField("rarities", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            return type >= ItemRarityID.Count && type < RarityLoader.RarityCount ? rarities[type - ItemRarityID.Count] : null;
        }
    }
}