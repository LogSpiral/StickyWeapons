using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StickyWeapons.Items;

public class Glue : ModItem
{
    public static bool CanChoose(Item _item)
    {
        return _item.active
                && _item.type != ItemID.None
                && (_item.damage > 0 || _item.type == ModContent.ItemType<StickyItem>())
                && _item.maxStack == 1
                && !_item.consumable
                && _item.ammo == AmmoID.None
                && _item.useAnimation > 2;
    }

    public override void Update(ref float gravity, ref float maxFallSpeed)
    {
        Item item1 = null;
        Item item2 = null;
        foreach (var _item in Main.item)
        {
            bool canChoose = CanChoose(_item);
            float length = Vector2.DistanceSquared(_item.Center, Item.Center);
            if (!canChoose || length > 4096)
                continue;

            if (item1 == null)
                item1 = _item;
            else if (item2 == null)
            {
                item2 = _item;
                break;
            }
        }
        if (item1 == null || item2 == null) return;

        var stickyItem = Main.item[Item.NewItem(Item.GetSource_Misc("Sticky!"), Item.Center, ModContent.ItemType<StickyItem>())];

        if (stickyItem.ModItem is StickyItem sticky)
        {
            for (int n = 0; n < 100; n++)
            {
                Dust.NewDustPerfect(Vector2.Lerp(item1.Center, item2.Center, n / 99f), DustID.Clentaminator_Cyan).noGravity = true;
                Dust.NewDustPerfect(Item.Center, DustID.Clentaminator_Cyan, (n / 99f * MathHelper.TwoPi).ToRotationVector2()).noGravity = true;
            }

            sticky.SetItemPair(item1, item2);

            item1.TurnToAir();
            item2.TurnToAir();
            Item.stack--;
            if (Item.stack <= 0) Item.TurnToAir();
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, Item.whoAmI, 1f);
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item1.whoAmI, 1f);
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item2.whoAmI, 1f);
            }
        }
        if (Main.netMode == NetmodeID.Server)
            NetMessage.SendData(MessageID.SyncItem, -1, -1, null, stickyItem.whoAmI, 1f);

        base.Update(ref gravity, ref maxFallSpeed);
    }

    public override void SetDefaults()
    {
        Item.maxStack = 999;
        Item.value = 5;
        Item.height = Item.width = 10;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Gel, 5)
            .Register();
    }
}