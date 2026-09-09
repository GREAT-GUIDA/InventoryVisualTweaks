using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace GuidaSharedCode {

    public enum ParticleLayer {
        /// <summary>
        /// Will not draw.
        /// </summary>
        None,
        /// <summary>
        /// Background.
        /// </summary>
        BeforeBackground,
        /// <summary>
        /// Walls. <b>After Background.</b>
        /// </summary>
        BeforeWalls,
        /// <summary>
        /// Trees, flowers, rocks, etc. <b>After Walls.</b>
        /// </summary>
        BeforeNonSolidTiles,
        /// <summary>
        /// Worm enemies. <b>After mon-solid Tiles.</b>
        /// </summary>
        BeforeNPCsBehindTiles,
        /// <summary>
        /// Tiles. <b>After NPCs drawn behind Tiles.</b>
        /// </summary>
        BeforeSolidTiles,
        /// <summary>
        /// Player details drawn behind NPCs. <b>After Solid Tiles.</b>
        /// </summary>
        BeforePlayersBehindNPCs,
        /// <summary>
        /// NPCs. <b>After Player details drawn behind NPCs.</b>
        /// </summary>
        BeforeNPCs,
        /// <summary>
        /// Projectiles. <b>After NPCs.</b>
        /// </summary>
        BeforeProjectiles,
        /// <summary>
        /// Players. <b>After Projectiles.</b>
        /// </summary>
        BeforePlayers,
        /// <summary>
        /// Items dropped in world. <b>After Players.</b>
        /// </summary>
        BeforeItems,
        /// <summary>
        /// Rain. <b>After Items.</b>
        /// </summary>
        BeforeRain,
        /// <summary>
        /// Gore. <b>After Rain.</b>
        /// </summary>
        BeforeGore,
        /// <summary>
        /// Dust. <b>After Gore.</b>
        /// </summary>
        BeforeDust,

        AfterDust,
        /// <summary>
        /// Water <b>After Dust.</b>  Adjust draw position by new Vector2(Main.offScreenRange, Main.offScreenRange).
        /// </summary>
        BeforeWater,
        /// <summary>
        /// Before UI.
        /// </summary>
        BeforeInterface,
        /// <summary>
        /// After UI.
        /// </summary>
        AfterInterface,
        Twist
    }

    // TODO: Add sample code showcasing how to add parallax
    /// <summary>
    /// This class can be used by any mod to draw at any position in the draw order.
    /// Some events have special requirements to draw correctly such as <see cref="OnDraw_BeforeBackground"/> and <see cref="OnDraw_BeforeWater"/>.
    /// 
    /// <para>
    /// For <see cref="OnDraw_BeforeBackground"/>, initialize your SpriteBatch with this Matrix.
    /// <code>
    /// Matrix matrix = Main.BackgroundViewMatrix.TransformationMatrix;
    ///	matrix.Translation -= Main.BackgroundViewMatrix.ZoomMatrix.Translation * new Vector3(1f, Main.BackgroundViewMatrix.Effects.HasFlag(SpriteEffects.FlipVertically)? (-1f) : 1f, 1f);
    /// </code>
    /// Your drawing will not manually parallax. You will need to do this yourself for now.
    /// </para>
    /// 
    /// <para>
    /// For <see cref="OnDraw_BeforeWater"/>, ADD <see cref="Main.offScreenRange"/> to your draw position.
    /// </para>
    /// </summary>
    public class DrawHooks : ModSystem {
        public delegate void Update();
        public static event Update OnUpdateDust;
        public static event Update OnUpdateMenu;

        public delegate void Draw(ParticleLayer layer);
        /// <summary>
        /// Before background.
        /// </summary>
        public static event Draw OnDraw_BeforeBackground;
        /// <summary>
        /// After backgroumd
        /// </summary>
        public static event Draw OnDraw_BeforeWalls;
        /// <summary>
        /// After walls
        /// </summary>
        public static event Draw OnDraw_BeforeNonSolidTiles;
        /// <summary>
        /// After non-solid tiles
        /// </summary>
        public static event Draw OnDraw_BeforeNPCsBehindTiles;
        /// <summary>
        /// After NPCs behind tiles, like worms
        /// </summary>
        public static event Draw OnDraw_BeforeSolidTiles;
        /// <summary>
        /// After solid tiles
        /// </summary>
        public static event Draw OnDraw_BeforePlayersBehindNPCs;
        /// <summary>
        /// After player details drawn behind NPCs
        /// </summary>
        public static event Draw OnDraw_BeforeNPCs;
        /// <summary>
        /// After NPCs
        /// </summary>
        public static event Draw OnDraw_BeforeProjectiles;
        /// <summary>
        /// After projectiles
        /// </summary>
        public static event Draw OnDraw_BeforePlayers;
        /// <summary>
        /// After players
        /// </summary>
        public static event Draw OnDraw_BeforeItems;
        /// <summary>
        /// After items in the world
        /// </summary>
        public static event Draw OnDraw_BeforeRain;
        /// <summary>
        /// After rain
        /// </summary>
        public static event Draw OnDraw_BeforeGore;
        /// <summary>
        /// After gore
        /// </summary>
        public static event Draw OnDraw_BeforeDust;
        /// <summary>
        /// After dust
        /// </summary>
        public static event Draw OnDraw_AfterDust;
        /// <summary>
        /// After dust
        /// </summary>
        public static event Draw OnDraw_BeforeWater;
        /// <summary>
        /// After water
        /// </summary>
        public static event Draw OnDraw_BeforeInterface;
        public static event Draw OnDraw_AfterInterface;

        public override void Load() {
            if (Main.dedServ) {
                return;
            }

            On_Dust.UpdateDust += Update_BeforeDust;
            On_Main.UpdateMenu += Update_BeforeMenu;

            Main.QueueMainThreadAction(() => {
                On_Main.DrawSurfaceBG += Draw_BeforeBackground;
                On_Main.DoDraw_WallsAndBlacks += Draw_BeforeWalls;
                On_Main.DoDraw_Tiles_NonSolid += Draw_BeforeNonSolidTiles;
                On_Main.DrawPlayers_BehindNPCs += Draw_BeforePlayersBehindNPCs;
                On_Main.DrawNPCs += Draw_BeforeNPCs;
                On_Main.DoDraw_Tiles_Solid += Draw_BeforeSolidTiles;
                On_Main.DrawProjectiles += Draw_BeforeProjectiles;
                On_Main.DrawPlayers_AfterProjectiles += Draw_BeforePlayers;
                On_Main.DrawItems += Draw_BeforeItems;
                On_Main.DrawRain += Draw_BeforeRain;
                On_Main.DrawGore += Draw_BeforeGore;
                On_Main.DrawDust += Draw_BeforeAndAfterDust;
                On_Main.DrawWaters += Draw_BeforeWater;
                On_Main.DrawInterface += Draw_OnInterface;
            });
        }

        public override void Unload() {
            OnUpdateDust = null;
            OnUpdateMenu = null;
            OnDraw_BeforeBackground = null;
            OnDraw_BeforeWalls = null;
            OnDraw_BeforeNonSolidTiles = null;
            OnDraw_BeforeSolidTiles = null;
            OnDraw_BeforePlayersBehindNPCs = null;
            OnDraw_BeforeNPCsBehindTiles = null;
            OnDraw_BeforeNPCs = null;
            OnDraw_BeforeProjectiles = null;
            OnDraw_BeforePlayers = null;
            OnDraw_BeforeItems = null;
            OnDraw_BeforeRain = null;
            OnDraw_BeforeGore = null;
            OnDraw_BeforeDust = null;
            OnDraw_AfterDust = null;
            OnDraw_BeforeWater = null;
            OnDraw_BeforeInterface = null;
            OnDraw_AfterInterface = null;
        }

        public static void Hook(ParticleLayer layer, Draw method) {
            ArgumentNullException.ThrowIfNull(method);

            switch (layer) {
                case ParticleLayer.BeforeBackground:
                    OnDraw_BeforeBackground += method;
                    break;
                case ParticleLayer.BeforeWalls:
                    OnDraw_BeforeWalls += method;
                    break;
                case ParticleLayer.BeforeNonSolidTiles:
                    OnDraw_BeforeNonSolidTiles += method;
                    break;
                case ParticleLayer.BeforeNPCsBehindTiles:
                    OnDraw_BeforeNPCsBehindTiles += method;
                    break;
                case ParticleLayer.BeforeSolidTiles:
                    OnDraw_BeforeSolidTiles += method;
                    break;
                case ParticleLayer.BeforePlayersBehindNPCs:
                    OnDraw_BeforePlayersBehindNPCs += method;
                    break;
                case ParticleLayer.BeforeNPCs:
                    OnDraw_BeforeNPCs += method;
                    break;
                case ParticleLayer.BeforeProjectiles:
                    OnDraw_BeforeProjectiles += method;
                    break;
                case ParticleLayer.BeforePlayers:
                    OnDraw_BeforePlayers += method;
                    break;
                case ParticleLayer.BeforeItems:
                    OnDraw_BeforeItems += method;
                    break;
                case ParticleLayer.BeforeRain:
                    OnDraw_BeforeRain += method;
                    break;
                case ParticleLayer.BeforeGore:
                    OnDraw_BeforeGore += method;
                    break;
                case ParticleLayer.BeforeDust:
                    OnDraw_BeforeDust += method;
                    break;
                case ParticleLayer.AfterDust:
                    OnDraw_AfterDust += method;
                    break;
                case ParticleLayer.BeforeWater:
                    OnDraw_BeforeWater += method;
                    break;
                case ParticleLayer.BeforeInterface:
                    OnDraw_BeforeInterface += method;
                    break;
                case ParticleLayer.AfterInterface:
                    OnDraw_AfterInterface += method;
                    break;
            }
        }

        public static void UnHook(ParticleLayer layer, Draw method) {
            ArgumentNullException.ThrowIfNull(method);

            switch (layer) {
                case ParticleLayer.BeforeBackground:
                    OnDraw_BeforeBackground -= method;
                    break;
                case ParticleLayer.BeforeWalls:
                    OnDraw_BeforeWalls -= method;
                    break;
                case ParticleLayer.BeforeNonSolidTiles:
                    OnDraw_BeforeNonSolidTiles -= method;
                    break;
                case ParticleLayer.BeforeNPCsBehindTiles:
                    OnDraw_BeforeNPCsBehindTiles -= method;
                    break;
                case ParticleLayer.BeforeSolidTiles:
                    OnDraw_BeforeSolidTiles -= method;
                    break;
                case ParticleLayer.BeforePlayersBehindNPCs:
                    OnDraw_BeforePlayersBehindNPCs -= method;
                    break;
                case ParticleLayer.BeforeNPCs:
                    OnDraw_BeforeNPCs -= method;
                    break;
                case ParticleLayer.BeforeProjectiles:
                    OnDraw_BeforeProjectiles -= method;
                    break;
                case ParticleLayer.BeforePlayers:
                    OnDraw_BeforePlayers -= method;
                    break;
                case ParticleLayer.BeforeItems:
                    OnDraw_BeforeItems -= method;
                    break;
                case ParticleLayer.BeforeRain:
                    OnDraw_BeforeRain -= method;
                    break;
                case ParticleLayer.BeforeGore:
                    OnDraw_BeforeGore -= method;
                    break;
                case ParticleLayer.BeforeDust:
                    OnDraw_BeforeDust -= method;
                    break;
                case ParticleLayer.AfterDust:
                    OnDraw_AfterDust -= method;
                    break;
                case ParticleLayer.BeforeWater:
                    OnDraw_BeforeWater -= method;
                    break;
                case ParticleLayer.BeforeInterface:
                    OnDraw_BeforeInterface -= method;
                    break;
                case ParticleLayer.AfterInterface:
                    OnDraw_AfterInterface -= method;
                    break;
            }
        }

        public static void GetClip(out Rectangle rectangle, out RasterizerState rasterizer) {
            rectangle = Main.graphics.GraphicsDevice.ScissorRectangle;
            rasterizer = Main.graphics.GraphicsDevice.RasterizerState;
        }

        public static void SetClip(Rectangle rectangle, RasterizerState rasterizer) {
            Main.graphics.GraphicsDevice.ScissorRectangle = rectangle;
            Main.graphics.GraphicsDevice.RasterizerState = rasterizer;
        }

        private void Update_BeforeDust(On_Dust.orig_UpdateDust orig) {
            OnUpdateDust?.Invoke();

            orig();
        }

        private void Update_BeforeMenu(On_Main.orig_UpdateMenu orig) {
            OnUpdateMenu?.Invoke();

            orig();
        }

        private void Draw_BeforeBackground(On_Main.orig_DrawSurfaceBG orig, Main self) {
            Main.spriteBatch.End();

            Matrix matrix = Main.BackgroundViewMatrix.TransformationMatrix;
            matrix.Translation -= Main.BackgroundViewMatrix.ZoomMatrix.Translation * new Vector3(1f, Main.BackgroundViewMatrix.Effects.HasFlag(SpriteEffects.FlipVertically) ? -1f : 1f, 1f);

            GetClip(out Rectangle rectangle, out RasterizerState rasterizer);
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, matrix);

            OnDraw_BeforeBackground?.Invoke(ParticleLayer.BeforeBackground);

            Main.spriteBatch.End();
            SetClip(rectangle, rasterizer);

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, matrix);

            orig(self);
        }

        private void Draw_BeforeWalls(On_Main.orig_DoDraw_WallsAndBlacks orig, Main self) {
            Main.spriteBatch.End();

            GetClip(out Rectangle rectangle, out RasterizerState rasterizer);
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            OnDraw_BeforeWalls?.Invoke(ParticleLayer.BeforeWalls);

            Main.spriteBatch.End();
            SetClip(rectangle, rasterizer);

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            orig(self);
        }

        private void Draw_BeforeNonSolidTiles(On_Main.orig_DoDraw_Tiles_NonSolid orig, Main self) {
            Main.spriteBatch.End();

            GetClip(out Rectangle rectangle, out RasterizerState rasterizer);
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            OnDraw_BeforeNonSolidTiles?.Invoke(ParticleLayer.BeforeNonSolidTiles);

            Main.spriteBatch.End();
            SetClip(rectangle, rasterizer);

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            orig(self);
        }

        private void Draw_BeforeSolidTiles(On_Main.orig_DoDraw_Tiles_Solid orig, Main self) {
            GetClip(out Rectangle rectangle, out RasterizerState rasterizer);
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            OnDraw_BeforeSolidTiles?.Invoke(ParticleLayer.BeforeSolidTiles);

            Main.spriteBatch.End();
            SetClip(rectangle, rasterizer);

            orig(self);
        }

        private void Draw_BeforePlayersBehindNPCs(On_Main.orig_DrawPlayers_BehindNPCs orig, Main self) {
            GetClip(out Rectangle rectangle, out RasterizerState rasterizer);
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            OnDraw_BeforePlayersBehindNPCs?.Invoke(ParticleLayer.BeforePlayersBehindNPCs);

            Main.spriteBatch.End();
            SetClip(rectangle, rasterizer);

            orig(self);
        }

        private void Draw_BeforeNPCs(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles) {
            if (behindTiles) {
                Main.spriteBatch.End();

                GetClip(out Rectangle rectangle, out RasterizerState rasterizer);
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

                OnDraw_BeforeNPCsBehindTiles?.Invoke(ParticleLayer.BeforeNPCsBehindTiles);

                Main.spriteBatch.End();
                SetClip(rectangle, rasterizer);

                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            } else {
                Main.spriteBatch.End();

                GetClip(out Rectangle rectangle, out RasterizerState rasterizer);
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

                OnDraw_BeforeNPCs?.Invoke(ParticleLayer.BeforeNPCs);

                Main.spriteBatch.End();
                SetClip(rectangle, rasterizer);

                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            orig(self, behindTiles);
        }

        private void Draw_BeforeProjectiles(On_Main.orig_DrawProjectiles orig, Main self) {
            GetClip(out Rectangle rectangle, out RasterizerState rasterizer);
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            OnDraw_BeforeProjectiles?.Invoke(ParticleLayer.BeforeProjectiles);

            Main.spriteBatch.End();
            SetClip(rectangle, rasterizer);

            orig(self);
        }

        private void Draw_BeforePlayers(On_Main.orig_DrawPlayers_AfterProjectiles orig, Main self) {
            GetClip(out Rectangle rectangle, out RasterizerState rasterizer);
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            OnDraw_BeforePlayers?.Invoke(ParticleLayer.BeforePlayers);

            Main.spriteBatch.End();
            SetClip(rectangle, rasterizer);

            orig(self);
        }

        private void Draw_BeforeItems(On_Main.orig_DrawItems orig, Main self) {
            Main.spriteBatch.End();

            GetClip(out Rectangle rectangle, out RasterizerState rasterizer);
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            OnDraw_BeforeItems?.Invoke(ParticleLayer.BeforeItems);

            Main.spriteBatch.End();
            SetClip(rectangle, rasterizer);

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            orig(self);
        }

        private void Draw_BeforeRain(On_Main.orig_DrawRain orig, Main self) {
            Main.spriteBatch.End();

            GetClip(out Rectangle rectangle, out RasterizerState rasterizer);
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            OnDraw_BeforeRain?.Invoke(ParticleLayer.BeforeRain);

            Main.spriteBatch.End();
            SetClip(rectangle, rasterizer);

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            orig(self);
        }

        private void Draw_BeforeGore(On_Main.orig_DrawGore orig, Main self) {
            Main.spriteBatch.End();

            GetClip(out Rectangle rectangle, out RasterizerState rasterizer);
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            OnDraw_BeforeGore?.Invoke(ParticleLayer.BeforeGore);

            Main.spriteBatch.End();
            SetClip(rectangle, rasterizer);

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            orig(self);
        }

        private void Draw_BeforeAndAfterDust(On_Main.orig_DrawDust orig, Main self) {
            GetClip(out Rectangle rectangle, out RasterizerState rasterizer);
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            OnDraw_BeforeDust?.Invoke(ParticleLayer.BeforeDust);

            Main.spriteBatch.End();
            SetClip(rectangle, rasterizer);

            orig(self);

            GetClip(out rectangle, out rasterizer);
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            OnDraw_AfterDust?.Invoke(ParticleLayer.AfterDust);

            Main.spriteBatch.End();
            SetClip(rectangle, rasterizer);
        }

        private void Draw_BeforeWater(On_Main.orig_DrawWaters orig, Main self, bool isBackground) {
            Main.spriteBatch.End();

            GetClip(out Rectangle rectangle, out RasterizerState rasterizer);
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            OnDraw_BeforeWater?.Invoke(ParticleLayer.BeforeWater);

            Main.spriteBatch.End();
            SetClip(rectangle, rasterizer);

            Main.spriteBatch.Begin();

            orig(self, isBackground);
        }

        private void Draw_OnInterface(On_Main.orig_DrawInterface orig, Main self, GameTime gameTime) {
            GetClip(out Rectangle rectangle, out RasterizerState rasterizer);
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            OnDraw_BeforeInterface?.Invoke(ParticleLayer.BeforeInterface);

            Main.spriteBatch.End();
            SetClip(rectangle, rasterizer);

            orig(self, gameTime);

            GetClip(out rectangle, out rasterizer);
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            OnDraw_AfterInterface?.Invoke(ParticleLayer.AfterInterface);

            Main.spriteBatch.End();
            SetClip(rectangle, rasterizer);
        }
    }

}