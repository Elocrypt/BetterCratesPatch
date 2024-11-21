using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace BetterCratesNamespace;

public class BetterCrateBlockEntity : BlockEntityContainer, ITexPositionSource
{
	public InventoryGeneric inventory;

	public string defaultType = "wood";

	private string horizontalOrientation;

	private string verticalOrientation;

	private Vec3f labelRotation1;

	private Vec3f labelRotation2;

	public int quantitySlots = 32;

	public InventoryGeneric lockedItemInventory;

	protected Shape nowTesselatingShape;

	protected ICoreClientAPI cApi;

	public MeshData mainMeshData1;

	public MeshData mainMeshData2;

	private BetterCrateLabelRender labelRenderer1;

	private BetterCrateLabelRender labelRenderer2;

	internal bool twoSided;

	public string text = "";

	private long tickListenerHandle;

	private long lastInteractTime;

	private static readonly int packetIDClientLeftClick = 5444;

	private static readonly int packetIDLockedError = 5666;

	private static readonly int packetIDPutAll = 5777;

	private int lastInventoryCount = -1;

	public bool labelFace1OppositeIsOpaque;

	public bool labelFace2OppositeIsOpaque;

	public bool shouldDrawMesh = true;

	private int previousItemStackID = -1;

	private string curMat;

	private string curLining;

	private ITexPositionSource glassTextureSource;

	private ITexPositionSource tmpTextureSource;

	private ITexPositionSource storedItemTextureSource;

	private Dictionary<string, AssetLocation> shapeTextures;

	private bool tesselatingSpecial;

	private bool tesselatingTextureShape;

	private bool tesselatingModBlock;

	public override string InventoryClassName => "bettercrate";

	public override InventoryBase Inventory => (InventoryBase)(object)inventory;

	public Size2i AtlasSize => ((ITextureAtlasAPI)cApi.BlockTextureAtlas).Size;

	public TextureAtlasPosition this[string textureCode]
	{
		get
		{
			//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b1: Invalid comparison between Unknown and I4
			//IL_038f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0395: Invalid comparison between Unknown and I4
			//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f8: Expected O, but got Unknown
			//IL_0321: Unknown result type (might be due to invalid IL or missing references)
			//IL_0328: Expected O, but got Unknown
			//IL_023f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0246: Expected O, but got Unknown
			//IL_0368: Unknown result type (might be due to invalid IL or missing references)
			//IL_036f: Expected O, but got Unknown
			//IL_0286: Unknown result type (might be due to invalid IL or missing references)
			//IL_028d: Expected O, but got Unknown
			//IL_0413: Unknown result type (might be due to invalid IL or missing references)
			//IL_041a: Expected O, but got Unknown
			//IL_045a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0461: Expected O, but got Unknown
			if (tesselatingSpecial)
			{
				switch (textureCode)
				{
				case "material":
					return tmpTextureSource[curMat];
				case "material-deco":
					return tmpTextureSource["deco-" + curMat];
				case "lining":
					if (curLining == "plain")
					{
						return tmpTextureSource[curMat];
					}
					return tmpTextureSource[curLining];
				case "glass":
					return glassTextureSource["material"];
				default:
					return tmpTextureSource[textureCode];
				}
			}
			ItemStack storedItemStack = GetStoredItemStack();
			if (storedItemStack != null && storedItemStack.Block != null && textureCode == "painting")
			{
				return cApi.Tesselator.GetTextureSource(storedItemStack.Block, 0, false)[textureCode];
			}
			AssetLocation textureLoc = null;
			IAsset texAsset = null;
			CompositeTexture tex;
			if (tesselatingModBlock)
			{
				string originalTextureCode = textureCode;
				string type = storedItemStack.Attributes.GetString("type", (string)null);
				if (type != null)
				{
					textureCode = type + "-" + textureCode;
				}
				if (storedItemStack.Block.Textures.TryGetValue(textureCode, out tex))
				{
					textureLoc = tex.Baked.BakedName;
					TextureAtlasPosition texPos = ((ITextureAtlasAPI)cApi.BlockTextureAtlas)[textureLoc];
					if (texPos != null)
					{
						return texPos;
					}
				}
				else if (storedItemStack.Block.Textures.TryGetValue(originalTextureCode, out tex))
				{
					textureLoc = tex.Baked.BakedName;
					TextureAtlasPosition texPos = ((ITextureAtlasAPI)cApi.BlockTextureAtlas)[textureLoc];
					if (texPos != null)
					{
						return texPos;
					}
				}
			}
			int num = default(int);
			if ((int)storedItemStack.Class == 1 && storedItemStack.Item.Textures.TryGetValue(textureCode, out tex))
			{
				textureLoc = tex.Baked.BakedName;
				if (textureLoc.GetName().Equals("clearquartz"))
				{
					textureLoc = new AssetLocation("game:item/resource/ungraded/quartz");
				}
				TextureAtlasPosition texPos = ((ITextureAtlasAPI)cApi.BlockTextureAtlas)[textureLoc];
				if (texPos != null)
				{
					return texPos;
				}
				texAsset = ((ICoreAPI)cApi).Assets.TryGet(textureLoc.Clone().WithPathPrefixOnce("textures/").WithPathAppendixOnce(".png"), true);
				if (texAsset != null)
				{
					AssetLocation temp = new AssetLocation();
					if (texAsset.Location != (AssetLocation)null && texAsset.Location.FirstPathPart(0) == "textures")
					{
						temp = new AssetLocation(((object)texAsset.Location).ToString().Replace("textures/", ""));
					}
					((ITextureAtlasAPI)cApi.BlockTextureAtlas).GetOrInsertTexture(temp, ref num, ref texPos, (CreateTextureDelegate)null, 0f);
					return texPos;
				}
			}
			if (textureLoc == (AssetLocation)null && shapeTextures != null)
			{
				shapeTextures.TryGetValue(textureCode, out textureLoc);
			}
			if (textureLoc != (AssetLocation)null)
			{
				TextureAtlasPosition texPos = ((ITextureAtlasAPI)cApi.BlockTextureAtlas)[textureLoc];
				if (texPos == null)
				{
					texAsset = ((ICoreAPI)cApi).Assets.TryGet(textureLoc.Clone().WithPathPrefixOnce("textures/").WithPathAppendixOnce(".png"), true);
					if (texAsset != null)
					{
						AssetLocation temp2 = new AssetLocation();
						if (texAsset.Location != (AssetLocation)null && texAsset.Location.FirstPathPart(0) == "textures")
						{
							temp2 = new AssetLocation(((object)texAsset.Location).ToString().Replace("textures/", ""));
						}
						((ITextureAtlasAPI)cApi.BlockTextureAtlas).GetOrInsertTexture(temp2, ref num, ref texPos, (CreateTextureDelegate)null, 0f);
					}
				}
				return texPos;
			}
			if ((int)storedItemStack.Class == 1)
			{
				textureLoc = storedItemStack.Item.FirstTexture.Base;
				TextureAtlasPosition texPos = ((ITextureAtlasAPI)cApi.BlockTextureAtlas)[textureLoc];
				if (texPos != null)
				{
					return texPos;
				}
				if (tesselatingTextureShape)
				{
					textureLoc = storedItemStack.Item.FirstTexture.Base;
				}
				if (textureLoc != (AssetLocation)null)
				{
					texAsset = ((ICoreAPI)cApi).Assets.TryGet(textureLoc.Clone().WithPathPrefixOnce("textures/").WithPathAppendixOnce(".png"), true);
				}
				if (texAsset != null)
				{
					AssetLocation temp3 = new AssetLocation();
					if (texAsset.Location != (AssetLocation)null && texAsset.Location.FirstPathPart(0) == "textures")
					{
						temp3 = new AssetLocation(((object)texAsset.Location).ToString().Replace("textures/", ""));
					}
					((ITextureAtlasAPI)cApi.BlockTextureAtlas).GetOrInsertTexture(temp3, ref num, ref texPos, (CreateTextureDelegate)null, 0f);
					return texPos;
				}
			}
			return storedItemTextureSource[textureCode];
		}
	}

	public BetterCrateBlockEntity()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		mainMeshData1 = new MeshData(true);
	}

	public override void Initialize(ICoreAPI api)
	{
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Expected O, but got Unknown
		((BlockEntity)this).Api = api;
		horizontalOrientation = ((RegistryObject)((BlockEntity)this).Block).LastCodePart(0);
		verticalOrientation = ((RegistryObject)((BlockEntity)this).Block).LastCodePart(1);
		if (inventory == null)
		{
			InitInventory(api.World);
		}
		((BlockEntityContainer)this).Initialize(api);
		((InventoryBase)lockedItemInventory).LateInitialize(((BlockEntityContainer)this).InventoryClassName + "-LockedItemInv-" + ((BlockEntity)this).Pos.X + "/" + ((BlockEntity)this).Pos.Y + "/" + ((BlockEntity)this).Pos.Z, api);
		((InventoryBase)lockedItemInventory).ResolveBlocksOrItems();
		foreach (long handlerId in ((BlockEntity)this).TickHandlers)
		{
			((BlockEntity)this).Api.Event.UnregisterGameTickListener(handlerId);
		}
		ref ICoreClientAPI reference = ref cApi;
		ICoreAPI api2 = ((BlockEntity)this).Api;
		reference = (ICoreClientAPI)(object)((api2 is ICoreClientAPI) ? api2 : null);
		if (cApi != null)
		{
			if (((RegistryObject)((BlockEntity)this).Block).FirstCodePart(0) == "bettercrate2sided")
			{
				twoSided = true;
				mainMeshData2 = new MeshData(true);
			}
			SetLabelRotation();
			UpdateMeshAndLabelRenderer();
			tickListenerHandle = ((BlockEntity)this).RegisterGameTickListener((Action<float>)Update, 4000 + ((BlockEntity)this).Api.World.Rand.Next(0, 1000), 0);
		}
	}

	private void Update(float dt)
	{
		UpdateMeshAndLabelRenderer();
		NeighborBlockChanged();
		((BlockEntity)this).MarkDirty(true, (IPlayer)null);
		((BlockEntity)this).Api.Event.UnregisterGameTickListener(tickListenerHandle);
		tickListenerHandle = 0L;
	}

	private void SetLabelRotation()
	{
		//IL_0496: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a0: Expected O, but got Unknown
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Expected O, but got Unknown
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a6: Expected O, but got Unknown
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Expected O, but got Unknown
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Expected O, but got Unknown
		//IL_0456: Unknown result type (might be due to invalid IL or missing references)
		//IL_0460: Expected O, but got Unknown
		//IL_0470: Unknown result type (might be due to invalid IL or missing references)
		//IL_047a: Expected O, but got Unknown
		//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c1: Expected O, but got Unknown
		//IL_03d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03db: Expected O, but got Unknown
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Expected O, but got Unknown
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Expected O, but got Unknown
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Expected O, but got Unknown
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Expected O, but got Unknown
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Expected O, but got Unknown
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Expected O, but got Unknown
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0357: Expected O, but got Unknown
		//IL_0367: Unknown result type (might be due to invalid IL or missing references)
		//IL_0371: Expected O, but got Unknown
		//IL_03ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Expected O, but got Unknown
		//IL_0406: Unknown result type (might be due to invalid IL or missing references)
		//IL_0410: Expected O, but got Unknown
		//IL_0421: Unknown result type (might be due to invalid IL or missing references)
		//IL_042b: Expected O, but got Unknown
		//IL_043b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0445: Expected O, but got Unknown
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Expected O, but got Unknown
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Expected O, but got Unknown
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Expected O, but got Unknown
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Expected O, but got Unknown
		switch (((RegistryObject)((BlockEntity)this).Block).LastCodePart(1) + "-" + ((RegistryObject)((BlockEntity)this).Block).LastCodePart(0))
		{
		case "center-north":
			labelRotation1 = new Vec3f(0f, (float)Math.PI, 0f);
			labelRotation2 = new Vec3f(0f, 0f, 0f);
			break;
		case "center-west":
			labelRotation1 = new Vec3f(0f, -(float)Math.PI / 2f, 0f);
			labelRotation2 = new Vec3f(0f, (float)Math.PI / 2f, 0f);
			break;
		case "center-south":
			labelRotation1 = new Vec3f(0f, 0f, 0f);
			labelRotation2 = new Vec3f(0f, (float)Math.PI, 0f);
			break;
		case "center-east":
			labelRotation1 = new Vec3f(0f, (float)Math.PI / 2f, 0f);
			labelRotation2 = new Vec3f(0f, -(float)Math.PI / 2f, 0f);
			break;
		case "up-north":
			labelRotation1 = new Vec3f(-(float)Math.PI / 2f, (float)Math.PI, 0f);
			labelRotation2 = new Vec3f((float)Math.PI / 2f, (float)Math.PI, 0f);
			break;
		case "up-west":
			labelRotation1 = new Vec3f((float)Math.PI / 2f, 0f, (float)Math.PI / 2f);
			labelRotation2 = new Vec3f(-(float)Math.PI / 2f, 0f, -(float)Math.PI / 2f);
			break;
		case "up-south":
			labelRotation1 = new Vec3f((float)Math.PI / 2f, 0f, 0f);
			labelRotation2 = new Vec3f(-(float)Math.PI / 2f, 0f, 0f);
			break;
		case "up-east":
			labelRotation1 = new Vec3f((float)Math.PI / 2f, 0f, -(float)Math.PI / 2f);
			labelRotation2 = new Vec3f(-(float)Math.PI / 2f, 0f, (float)Math.PI / 2f);
			break;
		case "down-north":
			labelRotation1 = new Vec3f((float)Math.PI / 2f, (float)Math.PI, 0f);
			labelRotation2 = new Vec3f((float)Math.PI / 2f, 0f, (float)Math.PI);
			break;
		case "down-west":
			labelRotation1 = new Vec3f(-(float)Math.PI / 2f, 0f, -(float)Math.PI / 2f);
			labelRotation2 = new Vec3f((float)Math.PI / 2f, 0f, (float)Math.PI / 2f);
			break;
		case "down-south":
			labelRotation1 = new Vec3f(-(float)Math.PI / 2f, 0f, 0f);
			labelRotation2 = new Vec3f(-(float)Math.PI / 2f, (float)Math.PI, (float)Math.PI);
			break;
		case "down-east":
			labelRotation1 = new Vec3f(-(float)Math.PI / 2f, 0f, (float)Math.PI / 2f);
			labelRotation2 = new Vec3f((float)Math.PI / 2f, 0f, -(float)Math.PI / 2f);
			break;
		default:
			labelRotation1 = Vec3f.Zero;
			labelRotation2 = new Vec3f(0f, (float)Math.PI, 0f);
			break;
		}
	}

	protected virtual void InitInventory(IWorldAccessor worldForResolving)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Expected O, but got Unknown
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		Block b = ((BlockEntity)this).Block;
		if (b == null)
		{
			b = worldForResolving.BlockAccessor.GetBlock(((BlockEntity)this).Pos);
		}
		if (b != null && ((CollectibleObject)b).Attributes != null)
		{
			string type = ((RegistryObject)b).LastCodePart(2);
			quantitySlots = ((CollectibleObject)b).Attributes["quantitySlots"][type].AsInt(quantitySlots);
		}
		inventory = new InventoryGeneric(quantitySlots, (string)null, (ICoreAPI)null, (NewSlotDelegate)null)
		{
			BaseWeight = 1f
		};
		((InventoryBase)inventory).SlotModified += OnSlotModified;
		inventory.OnGetAutoPullFromSlot = new GetAutoPullFromSlotDelegate(GetAutoPullFromSlot);
		inventory.OnGetAutoPushIntoSlot = new GetAutoPushIntoSlotDelegate(GetAutoPushIntoSlot);
		lockedItemInventory = new InventoryGeneric(1, (string)null, (ICoreAPI)null, (NewSlotDelegate)null);
		((InventoryBase)lockedItemInventory).SlotModified += OnSlotModified;
	}

	public void UpgradeInventory()
	{
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Expected O, but got Unknown
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Expected O, but got Unknown
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Expected O, but got Unknown
		string type = ((RegistryObject)((BlockEntity)this).Block).LastCodePart(2);
		quantitySlots = ((CollectibleObject)((BlockEntity)this).Block).Attributes["quantitySlots"][type].AsInt(0);
		if (quantitySlots != 0)
		{
			InventoryGeneric tempInventory = new InventoryGeneric(quantitySlots, ((BlockEntityContainer)this).InventoryClassName + "-LockedItemInv-" + ((BlockEntity)this).Pos.X + "/" + ((BlockEntity)this).Pos.Y + "/" + ((BlockEntity)this).Pos.Z, ((BlockEntity)this).Api, (NewSlotDelegate)null);
			ItemSlot[] tempItemSlotArray = ((IEnumerable<ItemSlot>)inventory).ToArray();
			for (int i = 0; i < tempItemSlotArray.Length; i++)
			{
				((InventoryBase)tempInventory)[i] = tempItemSlotArray[i];
			}
			inventory = tempInventory;
			inventory.BaseWeight = 1f;
			((InventoryBase)inventory).SlotModified += OnSlotModified;
			inventory.OnGetAutoPullFromSlot = new GetAutoPullFromSlotDelegate(GetAutoPullFromSlot);
			inventory.OnGetAutoPushIntoSlot = new GetAutoPushIntoSlotDelegate(GetAutoPushIntoSlot);
			((BlockEntity)this).MarkDirty(true, (IPlayer)null);
			((BlockEntity)this).Api.World.BlockAccessor.GetChunkAtBlockPos(((BlockEntity)this).Pos).MarkModified();
		}
	}

	private void OnSlotModified(int slot)
	{
		if (((BlockEntity)this).Api.World.BlockAccessor.GetChunkAtBlockPos(((BlockEntity)this).Pos) != null)
		{
			((BlockEntity)this).Api.World.BlockAccessor.GetChunkAtBlockPos(((BlockEntity)this).Pos).MarkModified();
		}
		((BlockEntity)this).MarkDirty(false, (IPlayer)null);
	}

	private ItemSlot GetAutoPushIntoSlot(BlockFacing atBlockFace, ItemSlot fromSlot)
	{
		if (atBlockFace != BlockFacing.DOWN)
		{
			if (!AllowedForStorage(fromSlot))
			{
				return null;
			}
			if (((InventoryBase)inventory).Empty)
			{
				return ((InventoryBase)inventory)[0];
			}
			return ((InventoryBase)inventory).GetBestSuitedSlot(fromSlot, (ItemStackMoveOperation)null, (List<ItemSlot>)null).slot;
		}
		return null;
	}

	private ItemSlot GetAutoPullFromSlot(BlockFacing atBlockFace)
	{
		if (((InventoryBase)inventory).Empty)
		{
			return null;
		}
		if (atBlockFace == BlockFacing.DOWN)
		{
			return ((IEnumerable<ItemSlot>)inventory).LastOrDefault((Func<ItemSlot, bool>)((ItemSlot slot) => !slot.Empty));
		}
		return null;
	}

	internal bool OnPlayerInteract(IPlayer byPlayer)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Invalid comparison between Unknown and I4
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Expected O, but got Unknown
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Expected O, but got Unknown
		ItemSlot playerSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
		if (playerSlot == null)
		{
			return false;
		}
		long currentTime = ((BlockEntity)this).Api.World.ElapsedMilliseconds;
		bool doubleClicked = currentTime - lastInteractTime < 500;
		lastInteractTime = currentTime;
		if (playerSlot.Empty)
		{
			if (((EntityAgent)byPlayer.Entity).Controls.Sprint)
			{
				ToggleItemLock();
				return false;
			}
			if (doubleClicked && (int)((BlockEntity)this).Api.Side == 1 && TryPutAll(byPlayer))
			{
				((ICoreServerAPI)((BlockEntity)this).Api).Network.SendBlockEntityPacket((IServerPlayer)byPlayer, ((BlockEntity)this).Pos.X, ((BlockEntity)this).Pos.Y, ((BlockEntity)this).Pos.Z, packetIDPutAll, (byte[])null);
				return true;
			}
		}
		else
		{
			if (!AllowedForStorage(playerSlot))
			{
				return false;
			}
			if (TryPut(byPlayer.InventoryManager.ActiveHotbarSlot, putBulk: true))
			{
				IClientPlayer player = (IClientPlayer)(object)((byPlayer is IClientPlayer) ? byPlayer : null);
				if (player != null)
				{
					player.TriggerFpAnimation((EnumHandInteract)2);
				}
				((BlockEntity)this).Api.World.PlaySoundAt(new AssetLocation("game:sounds/player/build"), (Entity)(object)byPlayer.Entity, byPlayer, true, 16f, 1f);
				return true;
			}
		}
		return false;
	}

	private void ToggleItemLock()
	{
		if (((InventoryBase)inventory).Empty)
		{
			if (!((InventoryBase)lockedItemInventory).Empty)
			{
				((InventoryBase)lockedItemInventory).DiscardAll();
			}
		}
		else if (lockedItemInventory != null && ((InventoryBase)lockedItemInventory).Empty)
		{
			((InventoryBase)lockedItemInventory)[0].Itemstack = GetStoredItemStack().Clone();
			((InventoryBase)lockedItemInventory)[0].Itemstack.StackSize = 1;
		}
		else if (lockedItemInventory != null)
		{
			((InventoryBase)lockedItemInventory).DiscardAll();
		}
		((BlockEntity)this).MarkDirty(false, (IPlayer)null);
		((BlockEntity)this).Api.World.BlockAccessor.GetChunkAtBlockPos(((BlockEntity)this).Pos).MarkModified();
	}

	public void OnPlayerLeftClick(IPlayer player)
	{
		if (cApi != null && !((InventoryBase)inventory).Empty)
		{
			byte[] data = new byte[1];
			if (((EntityAgent)player.Entity).Controls.Sneak)
			{
				data[0] = 1;
			}
			cApi.Network.SendBlockEntityPacket(((BlockEntity)this).Pos.X, ((BlockEntity)this).Pos.Y, ((BlockEntity)this).Pos.Z, packetIDClientLeftClick, data);
		}
	}

	public override void OnReceivedClientPacket(IPlayer fromPlayer, int packetid, byte[] data)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		((BlockEntity)this).OnReceivedClientPacket(fromPlayer, packetid, data);
		if ((int)((BlockEntity)this).Api.Side != 1 || !((BlockEntity)this).Api.World.Claims.TryAccess(fromPlayer, ((BlockEntity)this).Pos, (EnumBlockAccessFlags)2))
		{
			return;
		}
		if (((BlockEntity)this).Api.World.Api.ModLoader.GetModSystem<ModSystemBlockReinforcement>(true).IsLockedForInteract(((BlockEntity)this).Pos, fromPlayer))
		{
			((ICoreServerAPI)((BlockEntity)this).Api).Network.SendBlockEntityPacket((IServerPlayer)fromPlayer, ((BlockEntity)this).Pos.X, ((BlockEntity)this).Pos.Y, ((BlockEntity)this).Pos.Z, packetIDLockedError, (byte[])null);
		}
		else if (packetid == packetIDClientLeftClick)
		{
			bool takeBulk = false;
			if (data.Length != 0 && data[0] == 1)
			{
				takeBulk = true;
			}
			TryTake(fromPlayer, takeBulk);
		}
	}

	public override void OnReceivedServerPacket(int packetid, byte[] data)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected O, but got Unknown
		if (cApi != null)
		{
			if (packetid == packetIDLockedError)
			{
				cApi.TriggerIngameError((object)this, "locked", Lang.Get("ingameerror-locked", Array.Empty<object>()));
			}
			if (packetid == packetIDPutAll)
			{
				((BlockEntity)this).Api.World.PlaySoundAt(new AssetLocation("game:sounds/player/build"), (Entity)(object)((IPlayer)cApi.World.Player).Entity, (IPlayer)(object)cApi.World.Player, true, 16f, 1f);
			}
		}
		((BlockEntity)this).OnReceivedServerPacket(packetid, data);
	}

	public bool AllowedForStorage(ItemSlot inSlot)
	{
		if (inSlot == null || inSlot.Itemstack == null || ((BlockEntity)this).Api == null)
		{
			return false;
		}
		if (!((InventoryBase)inventory).Empty && GetInventoryFirstItemSlotNotFull(null) == null)
		{
			return false;
		}
		ItemStack currentItemStack = GetStoredItemStack();
		if (currentItemStack == null && inSlot.Itemstack.Block != null)
		{
			Block block = inSlot.Itemstack.Block;
			BlockContainer bContainer = (BlockContainer)(object)((block is BlockContainer) ? block : null);
			if (bContainer != null)
			{
				ItemStack[] containerContents = bContainer.GetNonEmptyContents(((BlockEntity)this).Api.World, inSlot.Itemstack);
				if (containerContents != null && containerContents.Length != 0)
				{
					if (cApi != null)
					{
						cApi.TriggerIngameError((object)this, "cantstore", Lang.Get("bettercrates:item-filledcontainer", Array.Empty<object>()));
					}
					return false;
				}
			}
		}
		if (currentItemStack != null && !currentItemStack.Equals(((BlockEntity)this).Api.World, inSlot.Itemstack, GlobalConstants.IgnoredStackAttributes))
		{
			bool ok = false;
			if (currentItemStack.Collectible != null && inSlot.Itemstack.Collectible != null && ((RegistryObject)currentItemStack.Collectible).Code != ((RegistryObject)inSlot.Itemstack.Collectible).Code)
			{
				return false;
			}
			if (inSlot.Itemstack.Block is BlockCrock && currentItemStack.Block is BlockCrock)
			{
				if (inSlot.Itemstack.Block != null)
				{
					Block block2 = inSlot.Itemstack.Block;
					BlockContainer bContainer2 = (BlockContainer)(object)((block2 is BlockContainer) ? block2 : null);
					if (bContainer2 != null)
					{
						ItemStack[] containerContents2 = bContainer2.GetNonEmptyContents(((BlockEntity)this).Api.World, inSlot.Itemstack);
						if (containerContents2 != null && containerContents2.Length != 0)
						{
							return false;
						}
					}
				}
				ok = true;
			}
			if (!ok)
			{
				return false;
			}
		}
		CollectibleObject inSlotColObj = inSlot.Itemstack.Collectible;
		if (inSlotColObj == null)
		{
			return false;
		}
		if (inSlotColObj.TransitionableProps != null && inSlotColObj.TransitionableProps.Length != 0)
		{
			if (cApi != null)
			{
				cApi.TriggerIngameError((object)this, "cantstore", Lang.Get("bettercrates:item-perishable", Array.Empty<object>()));
			}
			return false;
		}
		if (inSlotColObj.HasTemperature((IItemStack)(object)inSlot.Itemstack))
		{
			if (inSlotColObj.GetTemperature(((BlockEntity)this).Api.World, inSlot.Itemstack) >= 20f)
			{
				if (cApi != null)
				{
					cApi.TriggerIngameError((object)this, "cantstore", Lang.Get("bettercrates:item-toohot", Array.Empty<object>()));
				}
				return false;
			}
			inSlotColObj.SetTemperature(((BlockEntity)this).Api.World, inSlot.Itemstack, 0f, false);
		}
		return true;
	}

	public int GetInventoryCount()
	{
		if (((InventoryBase)inventory).Empty)
		{
			return 0;
		}
		if (lastInventoryCount > 0)
		{
			return lastInventoryCount;
		}
		int count = 0;
		for (int i = 0; i < ((InventoryBase)inventory).Count; i++)
		{
			ItemStack stack = ((InventoryBase)inventory)[i].Itemstack;
			if (stack == null)
			{
				break;
			}
			count += stack.StackSize;
		}
		lastInventoryCount = count;
		return count;
	}

	public void UpdateMeshAndLabelRenderer()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		if (((BlockEntity)this).Api == null || (int)((BlockEntity)this).Api.Side == 1)
		{
			return;
		}
		lastInventoryCount = -1;
		if (((InventoryBase)inventory).Empty && ((InventoryBase)lockedItemInventory).Empty)
		{
			mainMeshData1 = null;
			mainMeshData2 = null;
			previousItemStackID = -1;
			if (labelRenderer1 != null)
			{
				labelRenderer1.SetNewTextAndRotation(string.Empty, ColorUtil.BlackArgb, labelRotation1);
				labelRenderer1.DrawLockIcon = false;
			}
			if (twoSided && labelRenderer2 != null)
			{
				labelRenderer2.SetNewTextAndRotation(string.Empty, ColorUtil.BlackArgb, labelRotation2);
				labelRenderer2.DrawLockIcon = false;
			}
			return;
		}
		UpdateMesh();
		text = GetInventoryCount().ToString();
		if (labelRenderer1 != null)
		{
			labelRenderer1.SetNewTextAndRotation(text, ColorUtil.ToRgba(255, 0, 0, 0), labelRotation1);
			labelRenderer1.DrawLockIcon = !((InventoryBase)lockedItemInventory).Empty;
		}
		else
		{
			labelRenderer1 = new BetterCrateLabelRender(this, ((BlockEntity)this).Pos, cApi);
			labelRenderer1.SetNewTextAndRotation(text, ColorUtil.ToRgba(255, 0, 0, 0), labelRotation1);
			labelRenderer1.DrawLockIcon = !((InventoryBase)lockedItemInventory).Empty;
			NeighborBlockChanged();
		}
		if (twoSided)
		{
			if (labelRenderer2 != null)
			{
				labelRenderer2.SetNewTextAndRotation(text, ColorUtil.ToRgba(255, 0, 0, 0), labelRotation2);
				labelRenderer2.DrawLockIcon = !((InventoryBase)lockedItemInventory).Empty;
				return;
			}
			labelRenderer2 = new BetterCrateLabelRender(this, ((BlockEntity)this).Pos, cApi);
			labelRenderer2.SetNewTextAndRotation(text, ColorUtil.ToRgba(255, 0, 0, 0), labelRotation2);
			labelRenderer2.DrawLockIcon = !((InventoryBase)lockedItemInventory).Empty;
			NeighborBlockChanged();
		}
	}

	private bool TryPutAll(IPlayer byPlayer)
	{
		if (((InventoryBase)inventory).Empty && ((InventoryBase)lockedItemInventory).Empty)
		{
			return false;
		}
		if (GetStoredItemStack() == null)
		{
			return false;
		}
		bool result = false;
		string backpackInvClassName = byPlayer.InventoryManager.GetInventoryName("backpack");
		IInventory backpackInv = byPlayer.InventoryManager.GetInventory(backpackInvClassName);
		IInventory hotbarInv = byPlayer.InventoryManager.GetHotbarInventory();
		if (hotbarInv != null)
		{
			for (int i = 0; i < ((IReadOnlyCollection<ItemSlot>)hotbarInv).Count - 1; i++)
			{
				if (!hotbarInv[i].Empty && AllowedForStorage(hotbarInv[i]) && TryPut(hotbarInv[i], putBulk: true))
				{
					result = true;
				}
			}
		}
		if (backpackInv != null)
		{
			for (int j = 0; j < ((IReadOnlyCollection<ItemSlot>)backpackInv).Count; j++)
			{
				if (!backpackInv[j].Empty && AllowedForStorage(backpackInv[j]) && TryPut(backpackInv[j], putBulk: true))
				{
					result = true;
				}
			}
		}
		return result;
	}

	private bool TryPut(ItemSlot fromSlot, bool putBulk)
	{
		bool result = false;
		int quantity = 1;
		if (putBulk)
		{
			quantity = fromSlot.StackSize;
		}
		int count = 0;
		do
		{
			ItemSlot slot = GetInventoryFirstItemSlotNotFull(fromSlot.Itemstack);
			if (slot == null)
			{
				break;
			}
			int movedQuantity = fromSlot.TryPutInto(((BlockEntity)this).Api.World, slot, quantity);
			if (movedQuantity > 0)
			{
				result = true;
			}
			quantity -= movedQuantity;
			if (quantity <= 0)
			{
				result = true;
				break;
			}
			count++;
		}
		while (count <= ((InventoryBase)inventory).Count);
		return result;
	}

	private bool TryTake(IPlayer byPlayer, bool takeBulk)
	{
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Expected O, but got Unknown
		if (((InventoryBase)inventory).Empty)
		{
			return false;
		}
		bool result = false;
		int takeQuantity = 1;
		if (takeBulk)
		{
			takeQuantity = GetStoredItemMaxStackSize();
		}
		int quantityToTakeRemaining = takeQuantity;
		ItemStack stack = null;
		int count = 0;
		do
		{
			ItemSlot slot = GetInventoryLastItemSlotWithItem();
			if (slot == null)
			{
				break;
			}
			result = true;
			if (stack == null)
			{
				stack = slot.TakeOut(takeQuantity);
			}
			else
			{
				int take = Math.Min(quantityToTakeRemaining, slot.Itemstack.StackSize);
				ItemStack temp2 = slot.TakeOut(take);
				ItemStack obj = stack;
				obj.StackSize += temp2.StackSize;
			}
			if (stack.StackSize >= takeQuantity)
			{
				break;
			}
			quantityToTakeRemaining -= stack.StackSize;
			count++;
		}
		while (count < ((InventoryBase)inventory).Count);
		if (byPlayer.InventoryManager.TryGiveItemstack(stack, true))
		{
			if (stack.Block != null && stack.Block.Sounds != null)
			{
				((BlockEntity)this).Api.World.PlaySoundAt(stack.Block.Sounds.Place, (Entity)(object)byPlayer.Entity, byPlayer, true, 16f, 1f);
			}
			else
			{
				((BlockEntity)this).Api.World.PlaySoundAt(new AssetLocation("game:sounds/player/build"), (Entity)(object)byPlayer.Entity, byPlayer, true, 16f, 1f);
			}
		}
		if (stack.StackSize > 0)
		{
			Vec3f spawnDirection = Vec3f.Zero;
			Vec3d spawnVelocity = Vec3d.Zero;
			spawnDirection.Set(0.5f, 0.4f, 0.5f);
			switch (horizontalOrientation)
			{
			case "north":
				spawnDirection.Z = 1f;
				spawnVelocity.Z = 0.025;
				spawnVelocity.X = (((BlockEntity)this).Api.World.Rand.NextDouble() - 0.5) * 0.009999999776482582;
				break;
			case "south":
				spawnDirection.Z = 0f;
				spawnVelocity.Z = -0.025;
				spawnVelocity.X = (((BlockEntity)this).Api.World.Rand.NextDouble() - 0.5) * 0.009999999776482582;
				break;
			case "east":
				spawnDirection.X = 0f;
				spawnVelocity.X = -0.025;
				spawnVelocity.Z = (((BlockEntity)this).Api.World.Rand.NextDouble() - 0.5) * 0.009999999776482582;
				break;
			case "west":
				spawnDirection.X = 1f;
				spawnVelocity.X = 0.025;
				spawnVelocity.Z = (((BlockEntity)this).Api.World.Rand.NextDouble() - 0.5) * 0.009999999776482582;
				break;
			}
			switch (verticalOrientation)
			{
			case "up":
				spawnDirection.X = (spawnDirection.Z = 0.5f);
				spawnDirection.Y = 1f;
				spawnVelocity.Y = 0.05000000074505806;
				spawnVelocity.X = (((BlockEntity)this).Api.World.Rand.NextDouble() - 0.5) * 0.009999999776482582;
				spawnVelocity.Z = (((BlockEntity)this).Api.World.Rand.NextDouble() - 0.5) * 0.009999999776482582;
				break;
			case "down":
				spawnDirection.X = (spawnDirection.Z = 0.5f);
				spawnDirection.Y = 0f;
				spawnVelocity.Y = 0.0;
				spawnVelocity.X = (((BlockEntity)this).Api.World.Rand.NextDouble() - 0.5) * 0.009999999776482582;
				spawnVelocity.Z = (((BlockEntity)this).Api.World.Rand.NextDouble() - 0.5) * 0.009999999776482582;
				break;
			}
			((BlockEntity)this).Api.World.SpawnItemEntity(stack, ((BlockEntity)this).Pos.ToVec3d().Add(spawnDirection), spawnVelocity);
		}
		if (result)
		{
			((BlockEntity)this).MarkDirty(false, (IPlayer)null);
		}
		return result;
	}

	public override void OnBlockPlaced(ItemStack byItemStack = null)
	{
		((BlockEntityContainer)this).OnBlockPlaced((ItemStack)null);
	}

	public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
	{
	}

	public void NeighborBlockChanged()
	{
		if (((BlockEntity)this).Api != null)
		{
			labelFace1OppositeIsOpaque = false;
			labelFace2OppositeIsOpaque = false;
			switch (horizontalOrientation)
			{
			case "north":
				labelFace1OppositeIsOpaque = ((BlockEntity)this).Api.World.BlockAccessor.GetBlock(((BlockEntity)this).Pos.SouthCopy(1)).SideOpaque[BlockFacing.SOUTH.Index];
				labelFace2OppositeIsOpaque = ((BlockEntity)this).Api.World.BlockAccessor.GetBlock(((BlockEntity)this).Pos.NorthCopy(1)).SideOpaque[BlockFacing.NORTH.Index];
				break;
			case "south":
				labelFace1OppositeIsOpaque = ((BlockEntity)this).Api.World.BlockAccessor.GetBlock(((BlockEntity)this).Pos.NorthCopy(1)).SideOpaque[BlockFacing.NORTH.Index];
				labelFace2OppositeIsOpaque = ((BlockEntity)this).Api.World.BlockAccessor.GetBlock(((BlockEntity)this).Pos.SouthCopy(1)).SideOpaque[BlockFacing.SOUTH.Index];
				break;
			case "east":
				labelFace1OppositeIsOpaque = ((BlockEntity)this).Api.World.BlockAccessor.GetBlock(((BlockEntity)this).Pos.WestCopy(1)).SideOpaque[BlockFacing.WEST.Index];
				labelFace2OppositeIsOpaque = ((BlockEntity)this).Api.World.BlockAccessor.GetBlock(((BlockEntity)this).Pos.EastCopy(1)).SideOpaque[BlockFacing.EAST.Index];
				break;
			case "west":
				labelFace1OppositeIsOpaque = ((BlockEntity)this).Api.World.BlockAccessor.GetBlock(((BlockEntity)this).Pos.EastCopy(1)).SideOpaque[BlockFacing.EAST.Index];
				labelFace2OppositeIsOpaque = ((BlockEntity)this).Api.World.BlockAccessor.GetBlock(((BlockEntity)this).Pos.WestCopy(1)).SideOpaque[BlockFacing.WEST.Index];
				break;
			}
			switch (verticalOrientation)
			{
			case "up":
				labelFace1OppositeIsOpaque = ((BlockEntity)this).Api.World.BlockAccessor.GetBlock(((BlockEntity)this).Pos.UpCopy(1)).SideOpaque[BlockFacing.DOWN.Index];
				labelFace2OppositeIsOpaque = ((BlockEntity)this).Api.World.BlockAccessor.GetBlock(((BlockEntity)this).Pos.DownCopy(1)).SideOpaque[BlockFacing.UP.Index];
				break;
			case "down":
				labelFace1OppositeIsOpaque = ((BlockEntity)this).Api.World.BlockAccessor.GetBlock(((BlockEntity)this).Pos.DownCopy(1)).SideOpaque[BlockFacing.UP.Index];
				labelFace2OppositeIsOpaque = ((BlockEntity)this).Api.World.BlockAccessor.GetBlock(((BlockEntity)this).Pos.UpCopy(1)).SideOpaque[BlockFacing.DOWN.Index];
				break;
			}
			if (labelRenderer1 != null)
			{
				labelRenderer1.ShouldDraw = !labelFace1OppositeIsOpaque;
			}
			if (twoSided && labelRenderer2 != null)
			{
				labelRenderer2.ShouldDraw = !labelFace2OppositeIsOpaque;
			}
		}
	}

	public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
	{
		if (shouldDrawMesh)
		{
			if (mainMeshData1 != null && !labelFace1OppositeIsOpaque)
			{
				mesher.AddMeshData(mainMeshData1, 1);
			}
			if (mainMeshData2 != null && !labelFace2OppositeIsOpaque)
			{
				mesher.AddMeshData(mainMeshData2, 1);
			}
		}
		return false;
	}

	public int GetStoredItemMaxStackSize()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		int result = 1;
		ItemStack stack = GetStoredItemStack();
		if (stack != null)
		{
			if ((int)stack.Class == 1)
			{
				result = ((CollectibleObject)stack.Item).MaxStackSize;
			}
			else if ((int)stack.Class == 0)
			{
				result = ((CollectibleObject)stack.Block).MaxStackSize;
			}
		}
		return result;
	}

	public ItemStack GetStoredItemStack()
	{
		if (((InventoryBase)inventory).Empty)
		{
			if (((InventoryBase)lockedItemInventory).Empty)
			{
				return null;
			}
			if (((InventoryBase)lockedItemInventory)[0].Itemstack != null)
			{
				return ((InventoryBase)lockedItemInventory)[0].Itemstack;
			}
		}
		ItemSlot temp = ((IEnumerable<ItemSlot>)inventory).FirstOrDefault((Func<ItemSlot, bool>)((ItemSlot slot) => !slot.Empty));
		if (temp == null)
		{
			return null;
		}
		return temp.Itemstack;
	}

	public ItemSlot GetStoredItemSlot()
	{
		if (((InventoryBase)inventory).Empty)
		{
			if (((InventoryBase)lockedItemInventory).Empty)
			{
				return null;
			}
			if (((InventoryBase)lockedItemInventory)[0].Itemstack != null)
			{
				return ((InventoryBase)lockedItemInventory)[0];
			}
		}
		return ((IEnumerable<ItemSlot>)inventory).FirstOrDefault((Func<ItemSlot, bool>)((ItemSlot slot) => !slot.Empty));
	}

	public ItemSlot GetInventoryLastItemSlotWithItem()
	{
		if (((InventoryBase)inventory).Empty)
		{
			return null;
		}
		return ((IEnumerable<ItemSlot>)inventory).LastOrDefault((Func<ItemSlot, bool>)((ItemSlot slot) => !slot.Empty));
	}

	public ItemSlot GetInventoryFirstItemSlotNotFull(ItemStack inStack)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Invalid comparison between Unknown and I4
		ItemStack storedItemStack = ((!((InventoryBase)inventory).Empty) ? GetStoredItemStack() : inStack);
		int maxStackSize;
		if ((int)storedItemStack.Class == 1)
		{
			maxStackSize = ((CollectibleObject)storedItemStack.Item).MaxStackSize;
		}
		else
		{
			maxStackSize = ((CollectibleObject)storedItemStack.Block).MaxStackSize;
		}
		return ((IEnumerable<ItemSlot>)inventory).FirstOrDefault((Func<ItemSlot, bool>)((ItemSlot slot) => slot.Empty || slot.StackSize < maxStackSize));
	}

	public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Invalid comparison between Unknown and I4
		if (((BlockEntity)this).Pos == (BlockPos)null)
		{
			((BlockEntity)this).Pos = new BlockPos(tree.GetInt("posx", 0), tree.GetInt("posy", 0), tree.GetInt("posz", 0), tree.GetInt("posy", 0) / 32768);
		}
		if (inventory == null)
		{
			InitInventory(worldForResolving);
		}
		((BlockEntityContainer)this).FromTreeAttributes(tree, worldForResolving);
		((InventoryBase)lockedItemInventory).FromTreeAttributes(tree.GetTreeAttribute("lockedItemInv"));
		if (((BlockEntity)this).Api != null && (int)worldForResolving.Side == 2)
		{
			UpdateMeshAndLabelRenderer();
			((BlockEntity)this).MarkDirty(true, (IPlayer)null);
		}
	}

	public override void ToTreeAttributes(ITreeAttribute tree)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		((BlockEntityContainer)this).ToTreeAttributes(tree);
		if (lockedItemInventory != null)
		{
			ITreeAttribute lockedItemInvTree = (ITreeAttribute)new TreeAttribute();
			((InventoryBase)lockedItemInventory).ToTreeAttributes(lockedItemInvTree);
			tree["lockedItemInv"] = (IAttribute)(object)lockedItemInvTree;
		}
	}

	public override void OnBlockBroken(IPlayer byPlayer = null)
	{
		if (labelRenderer1 != null)
		{
			labelRenderer1.Dispose();
			labelRenderer1 = null;
		}
		if (labelRenderer2 != null)
		{
			labelRenderer2.Dispose();
			labelRenderer2 = null;
		}
		mainMeshData1 = null;
		mainMeshData2 = null;
		((BlockEntityContainer)this).OnBlockBroken(byPlayer);
	}

	public override void OnBlockRemoved()
	{
		if (labelRenderer1 != null)
		{
			labelRenderer1.Dispose();
			labelRenderer1 = null;
		}
		if (labelRenderer2 != null)
		{
			labelRenderer2.Dispose();
			labelRenderer2 = null;
		}
		mainMeshData1 = null;
		mainMeshData2 = null;
		((BlockEntity)this).OnBlockRemoved();
	}

	public override void OnBlockUnloaded()
	{
		if (labelRenderer1 != null)
		{
			labelRenderer1.Dispose();
			labelRenderer1 = null;
		}
		if (labelRenderer2 != null)
		{
			labelRenderer2.Dispose();
			labelRenderer2 = null;
		}
		mainMeshData1 = null;
		mainMeshData2 = null;
		((BlockEntity)this).OnBlockUnloaded();
	}

	private void UpdateMesh()
	{
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Expected O, but got Unknown
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Expected O, but got Unknown
		ItemStack currentItemStack = GetStoredItemStack();
		int currentItemStackID = -1;
		if (currentItemStack != null)
		{
			currentItemStackID = currentItemStack.Id;
		}
		if (currentItemStackID == previousItemStackID)
		{
			return;
		}
		previousItemStackID = currentItemStackID;
		MeshData m1 = GenMeshData(cApi.Tesselator);
		MeshData m2 = null;
		if (m1 != null)
		{
			m2 = m1.Clone();
		}
		if (m2 != null)
		{
			TranslateMesh(m2);
			UpdateXYZFaces(m2);
			mainMeshData1 = m2;
			if (twoSided)
			{
				MeshData m3 = m2.Clone();
				switch (verticalOrientation)
				{
				case "center":
					m3.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), (float)Math.PI, 0f, (float)Math.PI);
					break;
				case "up":
				case "down":
					switch (horizontalOrientation)
					{
					case "east":
					case "west":
						m3.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0f, 0f, (float)Math.PI);
						break;
					case "north":
					case "south":
						m3.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), (float)Math.PI, 0f, 0f);
						break;
					}
					break;
				}
				mainMeshData2 = m3.Clone();
			}
		}
		((BlockEntity)this).MarkDirty(true, (IPlayer)null);
	}

	private void UpdateXYZFaces(MeshData m1)
	{
		byte facing = 0;
		switch (verticalOrientation)
		{
		case "up":
			facing = (byte)(BlockFacing.UP.Index + 1);
			break;
		case "down":
			facing = (byte)(BlockFacing.DOWN.Index + 1);
			break;
		case "center":
			switch (horizontalOrientation)
			{
			case "north":
				facing = (byte)(BlockFacing.SOUTH.Index + 1);
				break;
			case "south":
				facing = (byte)(BlockFacing.NORTH.Index + 1);
				break;
			case "east":
				facing = (byte)(BlockFacing.WEST.Index + 1);
				break;
			case "west":
				facing = (byte)(BlockFacing.EAST.Index + 1);
				break;
			}
			break;
		}
		if (facing > 0)
		{
			for (int j = 0; j < m1.XyzFaces.Length; j++)
			{
				m1.XyzFaces[j] = facing;
			}
		}
	}

	public void TranslateMesh(MeshData mesh)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Invalid comparison between Unknown and I4
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Expected O, but got Unknown
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Expected O, but got Unknown
		if (mesh == null)
		{
			return;
		}
		ItemStack storedItemStack = GetStoredItemStack();
		if (storedItemStack != null)
		{
			ModelTransform modelTransform = (((int)storedItemStack.Class != 1) ? ((CollectibleObject)storedItemStack.Block).GuiTransform : ((CollectibleObject)storedItemStack.Item).GuiTransform);
			float[] modelMat = Mat4f.Create();
			Mat4f.Identity(modelMat);
			Vec3f scale = 0.25f * ((ModelTransformNoDefaults)modelTransform).ScaleXYZ;
			float rotationX = ((ModelTransformNoDefaults)modelTransform).Rotation.X * ((float)Math.PI / 180f);
			if ((int)storedItemStack.Class == 1)
			{
				rotationX += (float)Math.PI;
			}
			Mat4f.Scale(modelMat, modelMat, new float[3] { 1f, 1f, -1f });
			Mat4f.RotateX(modelMat, modelMat, rotationX);
			Mat4f.RotateY(modelMat, modelMat, (float)Math.PI / 180f * ((ModelTransformNoDefaults)modelTransform).Rotation.Y);
			Mat4f.RotateZ(modelMat, modelMat, (float)Math.PI / 180f * ((ModelTransformNoDefaults)modelTransform).Rotation.Z);
			Mat4f.Scale(modelMat, modelMat, new float[3] { scale.X, scale.Y, scale.Z });
			Mat4f.Translate(modelMat, modelMat, 0f - ((ModelTransformNoDefaults)modelTransform).Origin.X, 0f - ((ModelTransformNoDefaults)modelTransform).Origin.Y, 0f - ((ModelTransformNoDefaults)modelTransform).Origin.Z);
			mesh.MatrixTransform(modelMat);
			mesh.Scale(new Vec3f(0.5f, 0.5f, 0.5f), 1f, 1f, 0.0025f);
			mesh.Translate(0.5f, 0.565f, 0.51f);
			mesh.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), labelRotation1.X, labelRotation1.Y + (float)Math.PI, 0f - labelRotation1.Z);
		}
	}

	private MeshData GenMeshData(ITesselatorAPI tesselator)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Invalid comparison between Unknown and I4
		//IL_06ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c4: Expected O, but got Unknown
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Expected O, but got Unknown
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Expected O, but got Unknown
		//IL_0457: Unknown result type (might be due to invalid IL or missing references)
		//IL_0470: Expected O, but got Unknown
		//IL_0481: Unknown result type (might be due to invalid IL or missing references)
		//IL_048b: Expected O, but got Unknown
		//IL_04bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d4: Expected O, but got Unknown
		//IL_04e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ef: Expected O, but got Unknown
		//IL_0c17: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c21: Expected O, but got Unknown
		//IL_0566: Unknown result type (might be due to invalid IL or missing references)
		//IL_0570: Expected O, but got Unknown
		//IL_0c56: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c60: Expected O, but got Unknown
		//IL_0c71: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c8a: Expected O, but got Unknown
		//IL_07a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ae: Invalid comparison between Unknown and I4
		//IL_058f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0599: Expected O, but got Unknown
		//IL_05b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c2: Expected O, but got Unknown
		//IL_08d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f1: Expected O, but got Unknown
		//IL_0902: Unknown result type (might be due to invalid IL or missing references)
		//IL_090c: Expected O, but got Unknown
		//IL_093b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0954: Expected O, but got Unknown
		//IL_0965: Unknown result type (might be due to invalid IL or missing references)
		//IL_096f: Expected O, but got Unknown
		ItemStack storedItemStack = GetStoredItemStack();
		if (storedItemStack == null)
		{
			return null;
		}
		tesselatingSpecial = false;
		tesselatingTextureShape = false;
		tesselatingModBlock = false;
		nowTesselatingShape = null;
		tmpTextureSource = null;
		glassTextureSource = null;
		Dictionary<string, MeshData> meshes = ObjectCacheUtil.GetOrCreate<Dictionary<string, MeshData>>(((BlockEntity)this).Api, "BetterCrateContainerMeshes", (CreateCachableObjectDelegate<Dictionary<string, MeshData>>)(() => new Dictionary<string, MeshData>()));
		string key = storedItemStack.GetName();
		if ((int)storedItemStack.Class == 0)
		{
			key = key + "-" + GetStoredItemSlot().GetStackDescription(cApi.World, false);
		}
		if (meshes.TryGetValue(key, out var meshData))
		{
			return meshData;
		}
		if (storedItemStack.Block != null && storedItemStack.Block is BlockShapeFromAttributes)
		{
			Block block = storedItemStack.Block;
			BlockShapeFromAttributes clutterBlock = (BlockShapeFromAttributes)(object)((block is BlockShapeFromAttributes) ? block : null);
			if (clutterBlock != null)
			{
				IShapeTypeProps cprops = ((clutterBlock != null) ? clutterBlock.GetTypeProps(storedItemStack.Attributes.GetString("type", (string)null), storedItemStack.Clone(), (BEBehaviorShapeFromAttributes)null) : null);
				if (cprops != null)
				{
					meshData = clutterBlock.GetOrCreateMesh(cprops, (ITexPositionSource)null, (string)null).Clone().Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0f, (float)Math.PI + cprops.Rotation.Y * ((float)Math.PI / 180f), 0f)
						.Scale(new Vec3f(0.5f, 0.5f, 0.5f), -1f, 1f, 1f);
				}
				if (meshData != null)
				{
					return meshes[key] = meshData;
				}
			}
		}
		if (storedItemStack.Collectible.Attributes != null && storedItemStack.Collectible.Attributes["wearableAttachment"].AsBool(false))
		{
			MeshData armorMeshData = GenArmorMesh(cApi, storedItemStack);
			if (armorMeshData != null)
			{
				meshData = armorMeshData;
				for (int j = 0; j < meshData.RenderPassCount; j++)
				{
					if (meshData.RenderPassesAndExtraBits[j] != 3)
					{
						meshData.RenderPassesAndExtraBits[j] = 1;
					}
				}
				return meshes[key] = meshData;
			}
		}
		if ((int)storedItemStack.Class == 1)
		{
			storedItemTextureSource = cApi.Tesselator.GetTextureSource(storedItemStack.Item, false);
			if (storedItemStack.Item.Shape != null)
			{
				if (storedItemStack.Item != null)
				{
					if (((CollectibleObject)storedItemStack.Item).GetHeldItemName(storedItemStack) == "Rope")
					{
						goto IL_05d1;
					}
					try
					{
						cApi.Tesselator.TesselateItem(storedItemStack.Item, ref meshData, (ITexPositionSource)(object)this);
					}
					catch (Exception)
					{
						((BlockEntity)this).Api.World.Logger.Warning(storedItemStack.GetName() + " Item threw Exception! Shape.Base: " + ((object)storedItemStack.Item.Shape.Base).ToString());
						try
						{
							cApi.Tesselator.TesselateItem(storedItemStack.Item, ref meshData);
						}
						catch (Exception)
						{
							((BlockEntity)this).Api.World.Logger.Warning(storedItemStack.GetName() + " Item threw Exception again! Shape.Base: " + ((object)storedItemStack.Item.Shape.Base).ToString());
							Shape shape = ((ICoreAPI)cApi).Assets.TryGet("game:shapes/block/basic/cube.json", true).ToObject<Shape>((JsonSerializerSettings)null);
							tesselator.TesselateShape("bettercrate content shape", shape, ref meshData, (ITexPositionSource)(object)this, (Vec3f)null, 0, (byte)0, (byte)0, (int?)null, (string[])null);
						}
					}
				}
				if (meshData != null)
				{
					for (int i = 0; i < meshData.RenderPassCount; i++)
					{
						if (meshData.RenderPassesAndExtraBits[i] != 3)
						{
							meshData.RenderPassesAndExtraBits[i] = 1;
						}
					}
					int clearFlags = -503318784;
					for (int vertexNum = 0; vertexNum < meshData.GetVerticesCount(); vertexNum++)
					{
						meshData.Flags[vertexNum] &= clearFlags;
					}
					if (storedItemStack.Collectible != null)
					{
						if (((object)((RegistryObject)storedItemStack.Collectible).Code).ToString().EndsWith("quartz"))
						{
							if (storedItemStack.GetName().ToLower().Equals("clear quartz"))
							{
								for (int k = 0; k < meshData.RenderPassCount; k++)
								{
									meshData.RenderPassesAndExtraBits[k] = 2;
								}
							}
							else
							{
								for (int l = 0; l < meshData.RenderPassCount; l++)
								{
									meshData.RenderPassesAndExtraBits[l] = 1;
								}
							}
						}
						if (((object)((RegistryObject)storedItemStack.Collectible).Code).ToString().Contains("game:pounder-"))
						{
							meshData.Scale(new Vec3f(0.5f, 0.5f, 0.5f), 0.375f, 0.375f, 0.375f);
							meshData.Translate(new Vec3f(0f, -0.5f, 0f));
						}
						if (((object)((RegistryObject)storedItemStack.Collectible).Code).ToString().Contains("game:spear-"))
						{
							meshData.Scale(new Vec3f(0.5f, 0.5f, 0.5f), 0.4f, 0.4f, 0.4f);
							meshData.Translate(new Vec3f(0.6f, -0.5f, 0f));
							string temp = ((object)((RegistryObject)storedItemStack.Collectible).Code).ToString();
							if (temp.Contains("copper") || temp.Contains("bronze") || temp.Contains("iron") || temp.Contains("steel") || temp.Contains("silver") || temp.Contains("gold"))
							{
								meshData.Translate(new Vec3f(0.25f, 0.25f, 0f));
							}
							if (temp.Contains("scrap"))
							{
								meshData.Translate(new Vec3f(0.35f, 0.25f, 0f));
							}
							if (temp.Contains("hacking"))
							{
								meshData.Translate(new Vec3f(0.25f, 0.25f, 0f));
							}
						}
					}
					return meshes[key] = meshData;
				}
			}
			goto IL_05d1;
		}
		CompositeShape storedItemCompositeShape = storedItemStack.Block.ShapeInventory;
		storedItemTextureSource = cApi.Tesselator.GetTextureSource(storedItemStack.Block, 0, false);
		if (storedItemCompositeShape == null)
		{
			if (CheckForChiseledBlock(cApi, storedItemStack, out meshData))
			{
				return meshData;
			}
			if (CheckForSpecials(storedItemStack, out meshData))
			{
				return meshes[key] = meshData;
			}
			meshData = cApi.TesselatorManager.GetDefaultBlockMesh(storedItemStack.Block).Clone();
			if (meshData != null)
			{
				int clearFlags2 = -503318784;
				for (int m = 0; m < meshData.GetVerticesCount(); m++)
				{
					meshData.Flags[m] &= clearFlags2;
				}
				if ((int)storedItemStack.Block.BlockMaterial == 13 && (((object)((RegistryObject)storedItemStack.Block).Code).ToString().Contains("bush") || ((object)((RegistryObject)storedItemStack.Block).Code).ToString().Contains("sapling")))
				{
					for (int n = 0; n < meshData.ClimateColorMapIds.Length; n++)
					{
						if (meshData.ClimateColorMapIds[n] > 0)
						{
							meshData.ClimateColorMapIds[n] = 7;
						}
					}
					for (int num = 0; num < meshData.SeasonColorMapIds.Length; num++)
					{
						if (meshData.SeasonColorMapIds[num] > 0)
						{
							meshData.SeasonColorMapIds[num] = 10;
						}
					}
				}
				for (int num2 = 0; num2 < meshData.RenderPassCount; num2++)
				{
					if (meshData.RenderPassesAndExtraBits[num2] != 3)
					{
						meshData.RenderPassesAndExtraBits[num2] = 1;
					}
				}
				if (((object)((RegistryObject)storedItemStack.Block).Code).ToString().Contains("game:door-"))
				{
					if (((object)((RegistryObject)storedItemStack.Block).Code).ToString().Contains("1x3") || ((object)((RegistryObject)storedItemStack.Block).Code).ToString().Contains("2x2"))
					{
						meshData.Scale(new Vec3f(0.5f, 0.5f, 0.5f), 0.7f, 0.7f, 0.7f);
						meshData.Translate(new Vec3f(0f, 0.25f, 0f));
					}
					else if (((object)((RegistryObject)storedItemStack.Block).Code).ToString().Contains("2x4"))
					{
						meshData.Scale(new Vec3f(0.5f, 0.5f, 0.5f), 0.6f, 0.6f, 0.6f);
						meshData.Translate(new Vec3f(0f, 0.7f, 0f));
					}
				}
				return meshes[key] = meshData;
			}
		}
		goto IL_097e;
		IL_097e:
		List<IAsset> assets = ((!storedItemCompositeShape.Base.Path.EndsWith("*")) ? new List<IAsset> { ((BlockEntity)this).Api.Assets.TryGet(storedItemCompositeShape.Base.Clone().WithPathPrefixOnce("shapes/").WithPathAppendixOnce(".json"), true) } : ((BlockEntity)this).Api.Assets.GetMany(storedItemCompositeShape.Base.Clone().WithPathPrefixOnce("shapes/").Path.Substring(0, storedItemCompositeShape.Base.Path.Length - 1), storedItemCompositeShape.Base.Domain, true));
		if (assets != null && assets.Count > 0)
		{
			if (CheckForSpecialBlocks(storedItemStack, out meshData))
			{
				return meshes[key] = meshData;
			}
			for (int num3 = 0; num3 < 1; num3++)
			{
				Shape shape2 = assets[num3].ToObject<Shape>((JsonSerializerSettings)null);
				shapeTextures = shape2.Textures;
				try
				{
					tesselator.TesselateShape("bettercrate content shape", shape2, ref meshData, (ITexPositionSource)(object)this, (Vec3f)null, 0, (byte)0, (byte)0, (int?)null, (string[])null);
				}
				catch
				{
					try
					{
						tesselator.TesselateShape(storedItemStack.Collectible, shape2, ref meshData, (Vec3f)null, (int?)null, (string[])null);
					}
					catch
					{
						((BlockEntity)this).Api.World.Logger.Warning(storedItemStack.GetName() + " Block threw Exception! Shape.Base: " + ((object)storedItemStack.Block.Shape.Base).ToString());
						shape2 = ((ICoreAPI)cApi).Assets.TryGet("game:shapes/block/basic/cube.json", true).ToObject<Shape>((JsonSerializerSettings)null);
						tesselator.TesselateShape("bettercrate content shape", shape2, ref meshData, (ITexPositionSource)(object)this, (Vec3f)null, 0, (byte)0, (byte)0, (int?)null, (string[])null);
					}
				}
				int clearFlags3 = -503318784;
				for (int num4 = 0; num4 < meshData.GetVerticesCount(); num4++)
				{
					meshData.Flags[num4] &= clearFlags3;
				}
				for (int num5 = 0; num5 < meshData.RenderPassCount; num5++)
				{
					if (meshData.RenderPassesAndExtraBits[num5] != 3)
					{
						meshData.RenderPassesAndExtraBits[num5] = 1;
					}
				}
			}
		}
		else
		{
			((BlockEntity)this).Api.World.Logger.Error("BetterCrates: Content asset {0} not found,", new object[1] { storedItemCompositeShape.Base.WithPathPrefixOnce("shapes/").WithPathAppendixOnce(".json") });
		}
		if (storedItemStack.Collectible != null && ((object)((RegistryObject)storedItemStack.Collectible).Code).ToString().Contains("game:pulverizerframe-"))
		{
			meshData.Translate(new Vec3f(0f, -0.4f, 0f));
		}
		if (storedItemStack.Collectible != null && ((object)((RegistryObject)storedItemStack.Collectible).Code).ToString().Contains("game:wallpaper-"))
		{
			meshData.Translate(new Vec3f(0.45f, 0.45f, 0.2f));
			meshData.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), -1.55f, 0f, 0f);
		}
		return meshes[key] = meshData;
		IL_05d1:
		storedItemCompositeShape = ((!(((CollectibleObject)storedItemStack.Item).GetHeldItemName(storedItemStack) == "Rope")) ? storedItemStack.Item.Shape : null);
		if (storedItemCompositeShape == null)
		{
			tesselatingTextureShape = true;
			cApi.Tesselator.TesselateItem(storedItemStack.Item, ref meshData, (ITexPositionSource)(object)this);
			if (meshData != null)
			{
				int clearFlags4 = -503318784;
				for (int num6 = 0; num6 < meshData.GetVerticesCount(); num6++)
				{
					meshData.Flags[num6] &= clearFlags4;
				}
				for (int num7 = 0; num7 < meshData.RenderPassCount; num7++)
				{
					if (meshData.RenderPassesAndExtraBits[num7] != 3)
					{
						meshData.RenderPassesAndExtraBits[num7] = 1;
					}
				}
			}
			if (((object)storedItemStack.Item).GetType().ToString().Contains("ItemWorkItem"))
			{
				meshData.Scale(new Vec3f(0.5f, 0.5f, 0.5f), 0.3f, 0.3f, 0.3f);
				for (int num8 = 0; num8 < meshData.RenderPassCount; num8++)
				{
					meshData.RenderPassesAndExtraBits[num8] = 0;
				}
			}
			return meshes[key] = meshData;
		}
		goto IL_097e;
	}

	private bool CheckForChiseledBlock(ICoreClientAPI capi, ItemStack itemStack, out MeshData mesh)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		mesh = null;
		if ((int)itemStack.Class != 0)
		{
			return false;
		}
		if (!(itemStack.Block is BlockChisel))
		{
			return false;
		}
		ITreeAttribute tree = itemStack.Attributes;
		if (tree == null)
		{
			tree = (ITreeAttribute)new TreeAttribute();
		}
		int[] materials = BlockEntityMicroBlock.MaterialIdsFromAttributes(tree, (IWorldAccessor)(object)capi.World);
		uint[] cuboids = null;
		IAttribute obj = tree["cuboids"];
		IntArrayAttribute intArrayTemp = (IntArrayAttribute)(object)((obj is IntArrayAttribute) ? obj : null);
		if (intArrayTemp != null)
		{
			cuboids = intArrayTemp.AsUint;
		}
		if (cuboids == null)
		{
			IAttribute obj2 = tree["cuboids"];
			LongArrayAttribute longArrayTemp = (LongArrayAttribute)(object)((obj2 is LongArrayAttribute) ? obj2 : null);
			if (longArrayTemp != null)
			{
				cuboids = longArrayTemp.AsUint;
			}
		}
		List<uint> voxelCuboids = ((cuboids != null) ? new List<uint>(cuboids) : new List<uint>());
		mesh = BlockEntityMicroBlock.CreateMesh(capi, voxelCuboids, materials, (int[])null, (BlockPos)null, (uint[])null);
		if (mesh != null)
		{
			for (int vertexNum = 0; vertexNum < mesh.GetVerticesCount(); vertexNum++)
			{
				mesh.Flags[vertexNum] &= -256;
			}
			for (int j = 0; j < mesh.RenderPassCount; j++)
			{
				if (mesh.RenderPassesAndExtraBits[j] != 3)
				{
					mesh.RenderPassesAndExtraBits[j] = 1;
				}
			}
			return true;
		}
		return false;
	}

	private MeshData GenArmorMesh(ICoreClientAPI capi, ItemStack itemstack)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Expected O, but got Unknown
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Invalid comparison between Unknown and I4
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Expected O, but got Unknown
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		JsonObject attrObj = itemstack.Collectible.Attributes;
		EntityProperties entityType = ((IWorldAccessor)capi.World).GetEntityType(new AssetLocation("player"));
		Shape entityShape = entityType.Client.LoadedShape;
		AssetLocation shapePathForLogging = entityType.Client.Shape.Base;
		Shape newShape = new Shape
		{
			Elements = entityShape.CloneElements(),
			Animations = entityShape.Animations,
			AnimationsByCrc32 = entityShape.AnimationsByCrc32,
			AttachmentPointsByCode = entityShape.AttachmentPointsByCode,
			JointsById = entityShape.JointsById,
			TextureWidth = entityShape.TextureWidth,
			TextureHeight = entityShape.TextureHeight,
			Textures = null
		};
		if (attrObj != null && attrObj["attachShape"].Exists)
		{
			return null;
		}
		if ((int)itemstack.Class != 1)
		{
			return null;
		}
		CompositeShape compArmorShape = itemstack.Item.Shape;
		if (compArmorShape == null)
		{
			((IWorldAccessor)capi.World).Logger.Warning("Entity armor {0} {1} does not define a shape through either the shape property or the attachShape Attribute. Armor pieces will be invisible.", new object[2]
			{
				itemstack.Class,
				((RegistryObject)itemstack.Collectible).Code
			});
			return null;
		}
		AssetLocation shapePath = compArmorShape.Base.CopyWithPath("shapes/" + compArmorShape.Base.Path + ".json");
		IAsset asset = ((ICoreAPI)capi).Assets.TryGet(shapePath, true);
		if (asset == null)
		{
			((IWorldAccessor)capi.World).Logger.Warning("Entity armor shape {0} defined in {1} {2} not found, was supposed to be at {3}. Armor piece will be invisible.", new object[4]
			{
				compArmorShape.Base,
				itemstack.Class,
				((RegistryObject)itemstack.Collectible).Code,
				shapePath
			});
			return null;
		}
		Shape armorShape;
		try
		{
			armorShape = asset.ToObject<Shape>((JsonSerializerSettings)null);
		}
		catch (Exception ex)
		{
			((IWorldAccessor)capi.World).Logger.Warning("Exception thrown when trying to load entity armor shape {0} defined in {1} {2}. Armor piece will be invisible. Exception: {3}", new object[4]
			{
				compArmorShape.Base,
				itemstack.Class,
				((RegistryObject)itemstack.Collectible).Code,
				ex
			});
			return null;
		}
		newShape.Textures = armorShape.Textures;
		ShapeElement[] elements = armorShape.Elements;
		foreach (ShapeElement val in elements)
		{
			if (val.StepParentName != null)
			{
				ShapeElement elem = newShape.GetElementByName(val.StepParentName, StringComparison.InvariantCultureIgnoreCase);
				if (elem == null)
				{
					((IWorldAccessor)capi.World).Logger.Warning("Entity armor shape {0} defined in {1} {2} requires step parent element with name {3}, but no such element was found in shape {3}. Will not be visible.", new object[5]
					{
						compArmorShape.Base,
						itemstack.Class,
						((RegistryObject)itemstack.Collectible).Code,
						val.StepParentName,
						shapePathForLogging
					});
				}
				else if (elem.Children == null)
				{
					elem.Children = (ShapeElement[])(object)new ShapeElement[1] { val };
				}
				else
				{
					elem.Children = ArrayExtensions.Append<ShapeElement>(elem.Children, val);
				}
			}
			else
			{
				((IWorldAccessor)capi.World).Logger.Warning("Entity armor shape element {0} in shape {1} defined in {2} {3} did not define a step parent element. Will not be visible.", new object[4]
				{
					val.Name,
					compArmorShape.Base,
					itemstack.Class,
					((RegistryObject)itemstack.Collectible).Code
				});
			}
		}
		MeshData meshData = default(MeshData);
		capi.Tesselator.TesselateShapeWithJointIds("entity", newShape, ref meshData, (ITexPositionSource)(object)this, new Vec3f(), (int?)null, (string[])null);
		return meshData;
	}

	private bool CheckForSpecials(ItemStack storedItemStack, out MeshData meshData)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Expected O, but got Unknown
		meshData = new MeshData(true);
		if (storedItemStack.Attributes == null)
		{
			return false;
		}
		string material = storedItemStack.Attributes.GetString("material", (string)null);
		string lining = storedItemStack.Attributes.GetString("lining", (string)null);
		string glass = storedItemStack.Attributes.GetString("glass", (string)null);
		if (material != null && lining != null && glass != null)
		{
			tesselatingSpecial = true;
			tmpTextureSource = cApi.Tesselator.GetTextureSource(storedItemStack.Block, 0, false);
			Shape shape = ((ICoreAPI)cApi).Assets.TryGet("shapes/" + storedItemStack.Block.Shape.Base.Path + ".json", true).ToObject<Shape>((JsonSerializerSettings)null);
			curMat = material;
			curLining = lining;
			Block glassBlock = ((IWorldAccessor)cApi.World).GetBlock(new AssetLocation("glass-" + glass));
			glassTextureSource = cApi.Tesselator.GetTextureSource(glassBlock, 0, false);
			cApi.Tesselator.TesselateShape("BetterCrate-blocklantern", shape, ref meshData, (ITexPositionSource)(object)this, (Vec3f)null, 0, (byte)0, (byte)0, (int?)null, (string[])null);
			if (meshData != null)
			{
				for (int vertexNum = 0; vertexNum < meshData.GetVerticesCount(); vertexNum++)
				{
					meshData.Flags[vertexNum] &= -256;
				}
				for (int j = 0; j < meshData.RenderPassCount; j++)
				{
					if (meshData.RenderPassesAndExtraBits[j] != 3)
					{
						meshData.RenderPassesAndExtraBits[j] = 1;
					}
				}
				return true;
			}
			return false;
		}
		return false;
	}

	private bool CheckForSpecialBlocks(ItemStack storedItemStack, out MeshData meshData)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Expected O, but got Unknown
		if ((int)storedItemStack.Class != 0)
		{
			meshData = null;
			return false;
		}
		if (storedItemStack.Block.ShapeInventory == null)
		{
			meshData = null;
			return false;
		}
		if (storedItemStack.Attributes != null)
		{
			string type = storedItemStack.Attributes.GetString("type", (string)null);
			if (type != null)
			{
				tesselatingModBlock = true;
				tmpTextureSource = cApi.Tesselator.GetTextureSource(storedItemStack.Block, 0, false);
				string shapename = ((CollectibleObject)storedItemStack.Block).Attributes["shape"][type].AsString((string)null);
				if (shapename != null)
				{
					AssetLocation assetLoc = ((!shapename.StartsWith("game:")) ? new AssetLocation(((RegistryObject)storedItemStack.Block).Code.Domain, shapename).WithPathPrefixOnce("shapes/").WithPathAppendixOnce(".json") : new AssetLocation(shapename).WithPathPrefixOnce("shapes/").WithPathAppendixOnce(".json"));
					if (assetLoc != (AssetLocation)null)
					{
						IAsset asset = ((BlockEntity)this).Api.Assets.TryGet(assetLoc, true);
						if (asset != null)
						{
							Shape shape = asset.ToObject<Shape>((JsonSerializerSettings)null);
							if (shape != null)
							{
								shapeTextures = shape.Textures;
								cApi.Tesselator.TesselateShape("bettercrate content shape", shape, ref meshData, (ITexPositionSource)(object)this, (Vec3f)null, 0, (byte)0, (byte)0, (int?)null, (string[])null);
								if (meshData != null)
								{
									for (int j = 0; j < meshData.RenderPassCount; j++)
									{
										if (meshData.RenderPassesAndExtraBits[j] != 3)
										{
											meshData.RenderPassesAndExtraBits[j] = 1;
										}
									}
									int clearFlags = -503318784;
									for (int vertexNum = 0; vertexNum < meshData.GetVerticesCount(); vertexNum++)
									{
										meshData.Flags[vertexNum] &= clearFlags;
									}
									meshData.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0f, -(float)Math.PI / 2f, 0f);
									return true;
								}
							}
						}
					}
				}
			}
		}
		meshData = null;
		return false;
	}
}
