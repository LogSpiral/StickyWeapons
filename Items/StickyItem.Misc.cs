using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StickyWeapons.Items;

partial class StickyItem
{
    private void UpdateItemSet()
    {
        List<Item> items = [];
        FillItemSet(items);
        ItemSet = [.. items];
    }

    private StickyItem FillItemSet(List<Item> target)
    {
        if (SubItem1 == null || SubItem2 == null) return this;

        if (SubItem1.ModItem is StickyItem sticky1)
        {
            if (sticky1.ItemSet is { Length: > 0 })
                target.AddRange(sticky1.ItemSet);
            else
                sticky1.FillItemSet(target);
        }
        else
            target.Add(SubItem1);

        if (SubItem2.ModItem is StickyItem sticky2)
        {
            if (sticky2.ItemSet is { Length: > 0 })
                target.AddRange(sticky2.ItemSet);
            else
                sticky2.FillItemSet(target);
        }
        else
            target.Add(SubItem2);

        return this;
    }

    public override void SetDefaults()
    {
        UpdateItemSet();
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
            if (Item.DamageType == DamageClass.Default || (StickyUtils.MeleeCheck(i.DamageType) && !StickyUtils.MeleeCheck(Item.DamageType)))
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
    }

    public override string Texture => "Terraria/Images/Item_" + ItemID.Gel;

    public override bool AltFunctionUse(Player player) => true;

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        tooltips.Add(new TooltipLine(Mod, "StickyHint", this.GetLocalizedValue("StickyHint")));
        try
        {
            if (ItemSet == null || ItemSet.Length == 0) return;
            var length = ItemSet.Length;

            if (length < 6)
                foreach (var item in ItemSet)
                    tooltips.AddRange(StickyUtils.GetTooltipLines(item));
            else
                foreach (var item in ItemSet)
                    tooltips.Add(StickyUtils.GetNameLine(item));
        }
        catch { }
    }
}