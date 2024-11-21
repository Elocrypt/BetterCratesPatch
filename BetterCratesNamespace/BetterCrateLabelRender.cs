using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace BetterCratesNamespace;

public class BetterCrateLabelRender : IRenderer, IDisposable
{
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

	public double RenderOrder => 0.5;

	public int RenderRange => 24;

	public CairoFont Font
	{
		get
		{
			return font;
		}
		set
		{
			font = value;
		}
	}

	public BetterCrateLabelRender(BetterCrateBlockEntity be, BlockPos pos, ICoreClientAPI api)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Expected O, but got Unknown
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Expected O, but got Unknown
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Expected O, but got Unknown
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Expected O, but got Unknown
		this.be = be;
		this.api = api;
		this.pos = pos;
		font = new CairoFont((double)fontSize, GuiStyle.StandardFontName, new double[4] { 0.0, 0.0, 0.0, 0.5 }, (double[])null)
		{
			LineHeightMultiplier = 0.8999999761581421
		};
		api.Event.RegisterRenderer((IRenderer)(object)this, (EnumRenderStage)1, "BetterCrateLabelRender");
		MeshData modeldata = QuadMeshUtil.GetQuad();
		modeldata.Uv = new float[8] { 1f, 1f, 0f, 1f, 0f, 0f, 1f, 0f };
		modeldata.Rgba = new byte[16];
		ArrayExtensions.Fill<byte>(modeldata.Rgba, byte.MaxValue);
		textQuadModelRef = api.Render.UploadMesh(modeldata);
		MeshData modeldata2 = QuadMeshUtil.GetQuad();
		modeldata2.Uv = new float[8] { 1f, 1f, 0f, 1f, 0f, 0f, 1f, 0f };
		modeldata2.Rgba = new byte[16];
		ArrayExtensions.Fill<byte>(modeldata2.Rgba, byte.MaxValue);
		modeldata2.Scale(new Vec3f(0.5f, 0.5f, 0.5f), 0.15f, 0.45f, 1f);
		lockIconQuadModelRef = api.Render.UploadMesh(modeldata2);
		AssetLocation LockIconAssetLocation = new AssetLocation("bettercrates:textures/block/crate/lockicon.png");
		LockIconTexture = new LoadedTexture(api);
		api.Render.GetOrLoadTexture(LockIconAssetLocation, ref LockIconTexture);
	}

	public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
	{
		if (loadedTexture == null || !ShouldDraw || (!DrawLockIcon && !DrawText))
		{
			return;
		}
		Vec3d camPos = ((IPlayer)api.World.Player).Entity.CameraPos;
		if (camPos.DistanceTo(pos.ToVec3d()) > (float)BetterCratesConfig.Current.LabelInfoMaxRenderDistanceInBlocks)
		{
			if (be != null && be.shouldDrawMesh)
			{
				be.shouldDrawMesh = false;
				((BlockEntity)be).MarkDirty(true, (IPlayer)null);
			}
			return;
		}
		if (be != null && !be.shouldDrawMesh)
		{
			be.shouldDrawMesh = true;
			((BlockEntity)be).MarkDirty(true, (IPlayer)null);
		}
		IRenderAPI rapi = api.Render;
		rapi.GlDisableCullFace();
		rapi.GlToggleBlend(true, (EnumBlendMode)0);
		IStandardShaderProgram prog = rapi.PreparedStandardShader(pos.X, pos.Y, pos.Z, (Vec4f)null);
		float[] tempModelMat = (prog.ModelMatrix = ModelMat.Identity().Translate((double)pos.X - camPos.X, (double)pos.Y - camPos.Y, (double)pos.Z - camPos.Z).Translate(0.5f, 0.5f, 0.5f)
			.Rotate(rotation)
			.Translate(-0.5, -0.5, -0.5)
			.Translate(0.5f, 0.21f, -0.003f)
			.Scale(0.45f * QuadWidth, 0.4f * QuadHeight, 0.45f * QuadWidth)
			.Values);
		prog.ViewMatrix = rapi.CameraMatrixOriginf;
		prog.ProjectionMatrix = rapi.CurrentProjectionMatrix;
		prog.NormalShaded = 0;
		if (DrawText && loadedTexture != null)
		{
			prog.Tex2D = loadedTexture.TextureId;
			if (textQuadModelRef != null)
			{
				rapi.RenderMesh(textQuadModelRef);
			}
		}
		if (DrawLockIcon && LockIconTexture != null)
		{
			prog.Tex2D = LockIconTexture.TextureId;
			Mat4f.Translate(tempModelMat, tempModelMat, -0.41f, -1.52f, -0.01f);
			prog.ModelMatrix = tempModelMat;
			if (lockIconQuadModelRef != null)
			{
				rapi.RenderMesh(lockIconQuadModelRef);
			}
		}
		((IShaderProgram)prog).Stop();
	}

	public void SetNewTextAndRotation(string text, int color, Vec3f rot)
	{
		DrawText = text != string.Empty;
		font.WithColor(ColorUtil.ToRGBADoubles(color));
		if (loadedTexture != null)
		{
			loadedTexture.Dispose();
		}
		((FontConfig)font).UnscaledFontsize = fontSize / RuntimeEnv.GUIScale;
		loadedTexture = api.Gui.TextTexture.GenTextTexture(text, font, TextWidth, TextHeight, (TextBackground)null, (EnumTextOrientation)2, false);
		rotation = rot;
	}

	public void Dispose()
	{
		ShouldDraw = false;
		api.Event.UnregisterRenderer((IRenderer)(object)this, (EnumRenderStage)1);
		if (loadedTexture != null)
		{
			loadedTexture.Dispose();
			loadedTexture = null;
		}
		if (textQuadModelRef != null)
		{
			textQuadModelRef.Dispose();
			textQuadModelRef = null;
		}
		if (lockIconQuadModelRef != null)
		{
			lockIconQuadModelRef.Dispose();
			lockIconQuadModelRef = null;
		}
	}
}
