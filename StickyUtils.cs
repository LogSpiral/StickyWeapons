using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StickyWeapons.Items;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StickyWeapons;

public static class StickyUtils
{
    #region ToolTipLines

    private static Color GetRarityColorViaRare(int rare)
    {
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
            color = new Color((byte)(255f * num4), (byte)(40f * num4), (byte)(100f * num4), a);

        if (rare == 11)
            color = new Color((byte)(180f * num4), (byte)(40f * num4), (byte)(255f * num4), a);

        if (rare > 11)
            color = RarityLoader.rarities[rare - ItemRarityID.Count].RarityColor * num4;

        if (rare == -12)
            color = new Color((byte)(Main.DiscoR * num4), (byte)(Main.DiscoG * num4), (byte)(Main.DiscoB * num4), a);

        if (rare == -13)
            color = new Color((byte)(255f * num4), (byte)(Main.masterColor * 200f * num4), 0, a);

        return color;
    }

    private static Color GetRarityColorViaItem(Item item) => GetRarityColorViaRare(item switch
    {
        { master: true } => -13,
        { expert: true } => -12,
        _ => item.rare
    });

    public static List<TooltipLine> GetTooltipLines(Item item)
    {
        bool settingsEnabled_OpaqueBoxBehindTooltips = Main.SettingsEnabled_OpaqueBoxBehindTooltips;
        Color color = new(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor);
        int yoyoLogo = -1;
        int researchLine = -1;
        var rare = item.rare;
        float knockBack = item.knockBack;
        float num = 1f;
        if (item.DamageType == DamageClass.Melee && Main.LocalPlayer.kbGlove)
            num += 1f;

        if (Main.LocalPlayer.kbBuff)
            num += 0.5f;

        if (num != 1f)
            item.knockBack *= num;

        if (item.DamageType == DamageClass.Ranged && Main.LocalPlayer.shroomiteStealth)
            item.knockBack *= 1f + (1f - Main.LocalPlayer.stealth) * 0.5f;

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
        Main.MouseText_DrawItemTooltip_GetLinesInfo(item, ref yoyoLogo, ref researchLine, knockBack, ref numLines, array, array2, array3, tooltipNames, out int prefixlineIndex);
        float num3 = Main.mouseTextColor / 255f;
        float num4 = num3;
        int a = Main.mouseTextColor;
        if (Main.npcShop > 0 && item.value >= 0 && (item.type < ItemID.CopperCoin || item.type > ItemID.PlatinumCoin))
        {
            Main.LocalPlayer.GetItemExpectedPrice(item, out long calcForSelling, out long calcForBuying);
            long num5 = (item.isAShopItem || item.buyOnce) ? calcForBuying : calcForSelling;
            if (item.shopSpecialCurrency != -1)
            {
                tooltipNames[numLines] = "SpecialPrice";
                Terraria.GameContent.UI.CustomCurrencyManager.GetPriceText(item.shopSpecialCurrency, array, ref numLines, num5);
                color = new Color((byte)(255f * num4), (byte)(255f * num4), (byte)(255f * num4), a);
            }
            else if (num5 > 0)
            {
                string text = "";
                long num6 = 0;
                long num7 = 0;
                long num8 = 0;
                long num9 = 0;
                long num10 = num5 * item.stack;
                if (!item.buy)
                {
                    num10 = num5 / 5;
                    if (num10 < 1)
                        num10 = 1;

                    long num11 = num10;
                    num10 *= item.stack;
                    int amount = Main.shopSellbackHelper.GetAmount(item);
                    if (amount > 0)
                        num10 += (-num11 + calcForBuying) * Math.Min(amount, item.stack);
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

                if (!item.buy)
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
            else if (item.type != ItemID.DefenderMedal)
            {
                array[numLines] = Lang.tip[51].Value;
                tooltipNames[numLines] = "Price";
                numLines++;
                color = new Color((byte)(120f * num4), (byte)(120f * num4), (byte)(120f * num4), a);
            }
        }

        Vector2 zero = Vector2.Zero;
        List<TooltipLine> lines = ItemLoader.ModifyTooltips(item, ref numLines, tooltipNames, ref array, ref array2, ref array3, ref yoyoLogo, out Color?[] overrideColor, prefixlineIndex);
        //List<DrawableTooltipLine> drawableLines = lines.Select((TooltipLine x, int i) => new DrawableTooltipLine(x, i, 0, 0, Color.White)).ToList();

        for (int k = 0; k < lines.Count; k++)
        {
            Color lineColor = new Color(num4, num4, num4, num4);
            if (lines[k].Mod == "Terraria" && lines[k].Name == "ItemName")
            {
                lineColor = GetRarityColorViaItem(item);
            }
            else if (array2[k])
            {
                lineColor = array3[k] ? new Color((byte)(190f * num4), (byte)(120f * num4), (byte)(120f * num4), a) : new Color((byte)(120f * num4), (byte)(190f * num4), (byte)(120f * num4), a);
            }
            else if (lines[k].Mod == "Terraria" && lines[k].Name == "Price")
            {
                lineColor = color;
            }

            if (lines[k].Mod == "Terraria" && lines[k].Name == "JourneyResearch")
                lineColor = Colors.JourneyMode;

            lines[k].OverrideColor = lineColor;
        }

        var line = lines.Find(l => l.Name == "ItemName");
        line?.Text = $"[i:{item.type}] {line.Text}";

        return lines;
    }

    public static TooltipLine GetNameLine(Item item)
    {
        return new TooltipLine(StickyWeapons.Instance, "SubItemName",item.stack > 1 ? $"[i:{item.type}]{item.Name} x {item.stack}": $"[i:{item.type}]{item.Name}")
        {
            OverrideColor = GetRarityColorViaItem(item)
        };
    }

    #endregion ToolTipLines


    public static bool MeleeCheck(DamageClass damageClass)
    {
        return damageClass == DamageClass.Melee
                || damageClass.GetEffectInheritance(DamageClass.Melee)
                || !damageClass.GetModifierInheritance(DamageClass.Melee).Equals(StatInheritanceData.None);
    }

    public static Texture2D GetWeaponTextureFromItem(Texture2D texture, Item item)
    {
        var moditem = item.ModItem;
        if (moditem is StickyItem sticky && sticky.GetTexture() is Texture2D cplxTex)
            return cplxTex;
        return texture;
    }

    public static Rectangle GetWeaponFrameFromItem(Rectangle rectangle, Item item)
    {
        var moditem = item.ModItem;
        if (moditem is StickyItem sticky && sticky.GetTexture() is Texture2D cplxTex)
            return cplxTex.Frame();
        return rectangle;
    }
}