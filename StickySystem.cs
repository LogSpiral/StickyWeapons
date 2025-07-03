using Microsoft.Xna.Framework.Graphics;
using StickyWeapons.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace StickyWeapons;

public class StickySystem : ModSystem
{
    #region 注册CIVE中的物品图像获取

    public override void PostSetupContent()
    {
        if (ModLoader.TryGetMod("CoolerItemVisualEffect", out var result))
            result.Call("RegisterModifyWeaponTex",
                (Func<Item, Texture2D>)(item => StickyUtils.GetWeaponTextureFromItem(null, item)), 1f);
    }

    #endregion 注册CIVE中的物品图像获取

    #region 增加合成方式的粘合与拆解

    private RecipeGroup recipeGroup;
    private const string WeaponsRecipeGroupName = "StickyWeapons:Weapons!!";

    public static int[] WeaponTypes { get; private set; }

    public override void AddRecipeGroups()
    {
        HashSet<int> types = [ModContent.ItemType<ShowItem>()];
        for (int i = 0; i < ItemLoader.ItemCount; i++)
        {
            Item item = new(i);
            if (Glue.CanChoose(item)) types.Add(i);
        }
        types.Add(ModContent.ItemType<StickyItem>());
        WeaponTypes = [.. types];
        recipeGroup = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + Language.GetTextValue("Mods.StickyWeapons.RecipeGroups.Weapons"), WeaponTypes);
        RecipeGroup.RegisterGroup(WeaponsRecipeGroupName, recipeGroup);
    }

    public override void AddRecipes()
    {
        Recipe.Create(ModContent.ItemType<StickyItem>())
        .AddIngredient<Glue>()
        .AddRecipeGroup(WeaponsRecipeGroupName)
        .AddRecipeGroup(WeaponsRecipeGroupName)
        .AddOnCraftCallback
        (
            (recipe, item, consumedItems, stack) =>
            {
                StickyItem sticky = item.ModItem as StickyItem;
                var items = from target in consumedItems where target.type != ModContent.ItemType<Glue>() select target;
                var array = items.ToArray();
                sticky.SetItemPair(array[0], array[1]);
            }
        )
        .Register();

        Recipe.Create(ModContent.ItemType<ReturnStickyBag>())
        .AddIngredient<StickyItem>()
        .AddIngredient<OrganicSolvent>()
        .AddOnCraftCallback
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
        )
        .Register();
    }

    public override void PostUpdateTime()
    {
        if (WeaponTypes != null && WeaponTypes.Length > 0 && (int)(Main.GlobalTimeWrappedHourly * 2) % 2 == 0)
            recipeGroup.IconicItemId = Main.rand.Next(WeaponTypes);
    }

    #endregion 增加合成方式的粘合与拆解

}