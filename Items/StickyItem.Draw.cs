using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.UI;

namespace StickyWeapons.Items;

partial class StickyItem
{
    private RenderTarget2D complexTexture;

    public void DisposeTexture()
    {
        Main.RunOnMainThread(() =>
        {
            complexTexture?.Dispose();
            complexTexture = null;
        });
    }

    public RenderTarget2D GetTexture() => complexTexture;

    private void FillTexture(bool inUI)
    {
        var gd = Main.instance.GraphicsDevice;
        var sb = Main.spriteBatch;
        Vector2 size = default;
        foreach (var item in ItemSet)
        {
            if (item.type == ItemID.None) return;
            Main.instance.LoadItem(item.type);
            var curSize = TextureAssets.Item[item.type].Size();
            if (size.X < curSize.X) size.X = curSize.X;
            if (size.Y < curSize.Y) size.Y = curSize.Y;
        }
        complexTexture = new RenderTarget2D(gd, (int)size.X, (int)size.Y);
        bool flag = Terraria.Graphics.Effects.Filters.Scene._captureThisFrame;
        sb.End();
        if (flag)
        {
            sb.Begin();
            gd.SetRenderTarget(Main.screenTargetSwap);
            gd.Clear(Color.Transparent);
            sb.Draw(Main.screenTarget, Vector2.Zero, Color.White);
            sb.End();
        }
        gd.SetRenderTarget(complexTexture);
        gd.Clear(Color.Transparent);
        sb.Begin();
        List<int> types = [];
        foreach (var item in ItemSet)
        {
            if (types.Contains(item.type)) continue;
            Vector2 curSize = TextureAssets.Item[item.type].Size();
            ItemSlot.DrawItemIcon(item, 31, sb, size * Vector2.UnitY + curSize * new Vector2(1, -1) * .5f, 1, 1145, Color.White);
            types.Add(item.type);
        }

        sb.End();

        if (flag)
        {
            sb.Begin();
            gd.SetRenderTarget(Main.screenTarget);
            gd.Clear(Color.Transparent);
            sb.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);
            sb.End();
        }

        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, inUI ? Main.UIScaleMatrix : Main.GameViewMatrix.TransformationMatrix);

        if (!flag)
            gd.SetRenderTarget(null);
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        if (complexTexture == null)
        {
            if (ItemSet == null) return true;
            FillTexture(true);
        }
        if (complexTexture == null) return true;

        float scaler = 1f;
        float max = Math.Max(complexTexture.Width, complexTexture.Height);
        if (max > 30) scaler = 30 / max;
        spriteBatch.Draw(complexTexture, position, null, drawColor, 0, complexTexture.Size() * .5f, scaler, 0, 0);
        return false;
    }

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        if (complexTexture == null)
        {
            if (ItemSet == null) return true;
            FillTexture(true);
        }
        if (complexTexture == null) return true;

        spriteBatch.Draw(complexTexture, Item.Center - Main.screenPosition, null, lightColor, rotation, complexTexture.Size() * .5f, 1f, 0, 0);
        return false;
    }
}