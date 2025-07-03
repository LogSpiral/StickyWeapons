using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StickyWeapons.Items;

public class OrganicSolvent : ModItem
{
    public override void Update(ref float gravity, ref float maxFallSpeed)
    {
        foreach (var i in Main.item)
        {
            if (i.active && i.ModItem is StickyItem sticky && sticky.ItemSet != null && Vector2.Distance(Item.Center, i.Center) <= 64)
            {
                sticky.DisposeTexture();

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

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Gel, 5)
            .AddIngredient(ItemID.Ale)
            .Register();
    }
}