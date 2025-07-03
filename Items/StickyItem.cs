using Terraria;
using Terraria.ModLoader;

namespace StickyWeapons.Items;

public partial class StickyItem : ModItem
{
    public Item SubItem1
    {
        get => field ??= new();
        private set;
    }

    public Item SubItem2
    {
        get => field ??= new();
        private set;
    }

    public Item[] ItemSet { get; private set; }

    public void SetItemPair(Item item1, Item item2)
    {
        SubItem1 = item1.Clone();
        SubItem2 = item2.Clone();
        if (item1.ModItem is StickyItem _sticky1)
            _sticky1.DisposeTexture();

        if (item2.ModItem is StickyItem _sticky2)
            _sticky2.DisposeTexture();
        SetDefaults();
    }
}