using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace StickyWeapons.Items;

public class ShowItem : ModItem
{
    public override string Texture => "Terraria/Images/Item_1";

    private static bool FoundFirstWeaponInInventory;

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        if (Main.LocalPlayer == null || StickySystem.WeaponTypes == null) return true;
        bool foundFirst = false;
        foreach (var item in Main.LocalPlayer.inventory)
        {
            if (StickySystem.WeaponTypes.Contains(item.type))
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
        return true;
    }

    public override void UpdateInventory(Player player) => Item.TurnToAir();
}