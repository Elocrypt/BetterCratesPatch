using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace BetterCratesNamespace
{    public class BetterCrateLabelRender : IRenderer, IDisposable
    {
        public double RenderOrder
        {
            get
            {
                return 0.5;
            }
        }

        public int RenderRange
        {
            get
            {
                return 24;
            }
        }

        public CairoFont Font
        {
            get
            {
                return this.font;
            }
            set
            {
                this.font = value;
            }
        }
        public BetterCrateLabelRender(BetterCrateBlockEntity be, BlockPos pos, ICoreClientAPI api)
        {
            this.be = be;
            this.api = api;
            this.pos = pos;
            this.font = new CairoFont((double)BetterCrateLabelRender.fontSize, GuiStyle.StandardFontName, new double[]
            {
                0.0,
                0.0,
                0.0,
                0.5
            }, null)
            {
                LineHeightMultiplier = 0.8999999761581421
            };
            api.Event.RegisterRenderer(this, EnumRenderStage.Opaque, "BetterCrateLabelRender");
            MeshData modeldata = QuadMeshUtil.GetQuad();
            modeldata.Uv = new float[]
            {
                1f,
                1f,
                0f,
                1f,
                0f,
                0f,
                1f,
                0f
            };
            modeldata.Rgba = new byte[16];
            ArrayExtensions.Fill<byte>(modeldata.Rgba, byte.MaxValue);
            this.textQuadModelRef = api.Render.UploadMesh(modeldata);
            MeshData modeldata2 = QuadMeshUtil.GetQuad();
            modeldata2.Uv = new float[]
            {
                1f,
                1f,
                0f,
                1f,
                0f,
                0f,
                1f,
                0f
            };
            modeldata2.Rgba = new byte[16];
            ArrayExtensions.Fill<byte>(modeldata2.Rgba, byte.MaxValue);
            modeldata2.Scale(new Vec3f(0.5f, 0.5f, 0.5f), 0.15f, 0.45f, 1f);
            this.lockIconQuadModelRef = api.Render.UploadMesh(modeldata2);
            AssetLocation LockIconAssetLocation = new AssetLocation("bettercrates:textures/block/crate/lockicon.png");
            this.LockIconTexture = new LoadedTexture(api);
            api.Render.GetOrLoadTexture(LockIconAssetLocation, ref this.LockIconTexture);
        }
        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            if (this.loadedTexture == null || !this.ShouldDraw)
            {
                return;
            }
            if (!this.DrawLockIcon && !this.DrawText)
            {
                return;
            }
            Vec3d camPos = this.api.World.Player.Entity.CameraPos;
            if (camPos.DistanceTo(this.pos.ToVec3d()) > (float)BetterCratesConfig.Current.LabelInfoMaxRenderDistanceInBlocks)
            {
                if (this.be != null && this.be.shouldDrawMesh)
                {
                    this.be.shouldDrawMesh = false;
                    this.be.MarkDirty(true, null);
                }
                return;
            }
            if (this.be != null && !this.be.shouldDrawMesh)
            {
                this.be.shouldDrawMesh = true;
                this.be.MarkDirty(true, null);
            }
            IRenderAPI rapi = this.api.Render;
            rapi.GlDisableCullFace();
            rapi.GlToggleBlend(true, 0);
            IStandardShaderProgram prog = rapi.PreparedStandardShader(this.pos.X, this.pos.Y, this.pos.Z, null);
            float[] tempModelMat = this.ModelMat.Identity().Translate((double)this.pos.X - camPos.X, (double)this.pos.Y - camPos.Y, (double)this.pos.Z - camPos.Z).Translate(0.5f, 0.5f, 0.5f).Rotate(this.rotation).Translate(-0.5, -0.5, -0.5).Translate(0.5f, 0.21f, -0.003f).Scale(0.45f * BetterCrateLabelRender.QuadWidth, 0.4f * BetterCrateLabelRender.QuadHeight, 0.45f * BetterCrateLabelRender.QuadWidth).Values;
            prog.ModelMatrix = tempModelMat;
            prog.ViewMatrix = rapi.CameraMatrixOriginf;
            prog.ProjectionMatrix = rapi.CurrentProjectionMatrix;
            prog.NormalShaded = 0;
            if (this.DrawText && this.loadedTexture != null)
            {
                prog.Tex2D = this.loadedTexture.TextureId;
                if (this.textQuadModelRef != null)
                {
                    rapi.RenderMesh(this.textQuadModelRef);
                }
            }
            if (this.DrawLockIcon && this.LockIconTexture != null)
            {
                prog.Tex2D = this.LockIconTexture.TextureId;
                Mat4f.Translate(tempModelMat, tempModelMat, -0.41f, -1.52f, -0.01f);
                prog.ModelMatrix = tempModelMat;
                if (this.lockIconQuadModelRef != null)
                {
                    rapi.RenderMesh(this.lockIconQuadModelRef);
                }
            }
            prog.Stop();
        }
        public void SetNewTextAndRotation(string text, int color, Vec3f rot)
        {
            this.DrawText = (text != string.Empty);
            this.font.WithColor(ColorUtil.ToRGBADoubles(color));
            if (this.loadedTexture != null)
            {
                this.loadedTexture.Dispose();
            }
            this.font.UnscaledFontsize = (double)(BetterCrateLabelRender.fontSize / RuntimeEnv.GUIScale);
            this.loadedTexture = this.api.Gui.TextTexture.GenTextTexture(text, this.font, BetterCrateLabelRender.TextWidth, BetterCrateLabelRender.TextHeight, null, EnumTextOrientation.Center, false);
            this.rotation = rot;
        }
        public void Dispose()
        {
            this.ShouldDraw = false;
            this.api.Event.UnregisterRenderer(this, EnumRenderStage.Opaque);
            if (this.loadedTexture != null)
            {
                this.loadedTexture.Dispose();
                this.loadedTexture = null;
            }
            if (this.textQuadModelRef != null)
            {
                this.textQuadModelRef.Dispose();
                this.textQuadModelRef = null;
            }
            if (this.lockIconQuadModelRef != null)
            {
                this.lockIconQuadModelRef.Dispose();
                this.lockIconQuadModelRef = null;
            }
        }
        protected static int TextWidth = 200;
        protected static int TextHeight = 50;
        protected static float QuadWidth = 0.9f;
        protected static float QuadHeight = 0.25f;
        protected CairoFont font;
        protected BlockPos pos;
        protected ICoreClientAPI api;
        protected MeshRef textQuadModelRef;
        protected LoadedTexture loadedTexture;
        protected MeshRef lockIconQuadModelRef;
        protected LoadedTexture LockIconTexture;
        public Matrixf ModelMat = new Matrixf();
        protected float rotX;
        protected float rotY;
        protected float rotZ;
        protected Vec3f rotation = Vec3f.Zero;
        protected float translateX;
        protected float translateY = 0.5625f;
        protected float translateZ;
        private static readonly float fontSize = 30f;
        private bool DrawText;
        public bool DrawLockIcon;
        public bool ShouldDraw = true;
        private BetterCrateBlockEntity be;
    }
}
