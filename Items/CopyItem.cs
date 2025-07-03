using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace StickyWeapons.Items;

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
                var index = Item.NewItem(new EntitySource_Misc("Clone"), i.Center, 1);
                var _item = Main.item[index] = i.Clone();
                _item.whoAmI = index;
                _item.velocity = -i.velocity;
                _item.Center = Item.Center;

                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendData(
                        MessageID.SyncItem,
                        -1,
                        -1,
                        null,
                        Item.whoAmI,
                        1f);

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