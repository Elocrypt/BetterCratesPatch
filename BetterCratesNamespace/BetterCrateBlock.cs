using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace BetterCratesNamespace;

public class BetterCrateBlock : Block
{
	public BetterCrateBlockEntity betterCrateBE;

	public string NextUpgradeCodePart => ((RegistryObject)this).LastCodePart(2) switch
	{
		"wood" => "copper", 
		"copper" => "bronze", 
		"bronze" => "iron", 
		"iron" => "steel", 
		_ => "wood", 
	};

	public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
	{
		return ArrayExtensions.Append<WorldInteraction>((WorldInteraction[])(object)new WorldInteraction[6]
		{
			new WorldInteraction
			{
				ActionLangCode = "bettercrates:blockhelp-bettercrate-take",
				MouseButton = (EnumMouseButton)0,
				HotKeyCode = null,
				ShouldApply = (InteractionMatcherDelegate)delegate(WorldInteraction wi, BlockSelection bs, EntitySelection es)
				{
                    if (!(world.BlockAccessor.GetBlockEntity(bs.Position) is BetterCrateBlockEntity betterCrateBlockEntity))
                    {
						return false;
					}
					return (betterCrateBlockEntity.inventory == null || !((InventoryBase)betterCrateBlockEntity.inventory).Empty) ? true : false;
				}
			},
			new WorldInteraction
			{
				ActionLangCode = "bettercrates:blockhelp-bettercrate-takestack",
				MouseButton = (EnumMouseButton)0,
				HotKeyCode = "sneak",
				ShouldApply = (InteractionMatcherDelegate)delegate(WorldInteraction wi, BlockSelection bs, EntitySelection es)
				{
                    if (!(world.BlockAccessor.GetBlockEntity(bs.Position) is BetterCrateBlockEntity betterCrateBlockEntity2))
                    {
						return false;
					}
					return (betterCrateBlockEntity2.inventory == null || !((InventoryBase)betterCrateBlockEntity2.inventory).Empty) ? true : false;
				}
			},
			new WorldInteraction
			{
				ActionLangCode = "bettercrates:blockhelp-bettercrate-putstack",
				MouseButton = (EnumMouseButton)2,
				HotKeyCode = null,
				ShouldApply = (InteractionMatcherDelegate)delegate(WorldInteraction wi, BlockSelection bs, EntitySelection es)
				{
                    if (!(world.BlockAccessor.GetBlockEntity(bs.Position) is BetterCrateBlockEntity betterCrateBlockEntity3))
                    {
						return false;
					}
					return (betterCrateBlockEntity3.inventory != null && betterCrateBlockEntity3.lockedItemInventory != null && ((InventoryBase)betterCrateBlockEntity3.inventory).Empty && ((InventoryBase)betterCrateBlockEntity3.lockedItemInventory).Empty) ? true : false;
				}
			},
			new WorldInteraction
			{
				ActionLangCode = "bettercrates:blockhelp-bettercrate-putmatchingstack",
				MouseButton = (EnumMouseButton)2,
				HotKeyCode = null,
				ShouldApply = (InteractionMatcherDelegate)delegate(WorldInteraction wi, BlockSelection bs, EntitySelection es)
				{
                    if (!(world.BlockAccessor.GetBlockEntity(bs.Position) is BetterCrateBlockEntity betterCrateBlockEntity4))
                    {
						return false;
					}
					return (betterCrateBlockEntity4.inventory != null && betterCrateBlockEntity4.lockedItemInventory != null && (!((InventoryBase)betterCrateBlockEntity4.inventory).Empty || !((InventoryBase)betterCrateBlockEntity4.lockedItemInventory).Empty)) ? true : false;
				}
			},
			new WorldInteraction
			{
				ActionLangCode = "bettercrates:blockhelp-bettercrate-lock",
				MouseButton = (EnumMouseButton)2,
				HotKeyCode = "sprint",
				RequireFreeHand = true,
				ShouldApply = (InteractionMatcherDelegate)delegate(WorldInteraction wi, BlockSelection bs, EntitySelection es)
				{
                    if (!(world.BlockAccessor.GetBlockEntity(bs.Position) is BetterCrateBlockEntity betterCrateBlockEntity5))
                    {
						return false;
					}
					if (betterCrateBlockEntity5.inventory != null && ((InventoryBase)betterCrateBlockEntity5.inventory).Empty)
					{
						return false;
					}
					return betterCrateBlockEntity5.lockedItemInventory != null && ((InventoryBase)betterCrateBlockEntity5.lockedItemInventory).Empty;
				}
			},
			new WorldInteraction
			{
				ActionLangCode = "bettercrates:blockhelp-bettercrate-unlock",
				MouseButton = (EnumMouseButton)2,
				HotKeyCode = "sprint",
				RequireFreeHand = true,
				ShouldApply = (InteractionMatcherDelegate)delegate(WorldInteraction wi, BlockSelection bs, EntitySelection es)
				{
                    if (!(world.BlockAccessor.GetBlockEntity(bs.Position) is BetterCrateBlockEntity betterCrateBlockEntity6))
                    {
						return false;
					}
					return betterCrateBlockEntity6.lockedItemInventory != null && !((InventoryBase)betterCrateBlockEntity6.lockedItemInventory).Empty;
				}
			}
		}, ((Block)this).GetPlacedBlockInteractionHelp(world, selection, forPlayer));
	}

	public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
	{
		if (!((Block)this).CanPlaceBlock(world, byPlayer, blockSel, ref failureCode))
		{
			return false;
		}
		BlockFacing[] horVer = Block.SuggestedHVOrientation(byPlayer, blockSel);
		string vertCode = "center";
		if (horVer[1] == BlockFacing.UP)
		{
			vertCode = "up";
		}
		else if (horVer[1] == BlockFacing.DOWN)
		{
			vertCode = "down";
		}
		AssetLocation blockCode = ((RegistryObject)this).CodeWithVariants(new string[2] { "verticalorientation", "horizontalorientation" }, new string[2]
		{
			vertCode,
			horVer[0].Code
		});
		Block block = world.BlockAccessor.GetBlock(blockCode);
		if (block == null)
		{
			return false;
		}
		world.BlockAccessor.SetBlock(block.BlockId, blockSel.Position);
		return true;
	}

	public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		if (world.BlockAccessor.GetBlock(pos) != null)
		{
			string[] lastParts = new string[2] { "center", "east" };
			AssetLocation aLocation = ((RegistryObject)this).CodeWithParts(lastParts);
			return new ItemStack(world.BlockAccessor.GetBlock(aLocation), 1);
		}
		return new ItemStack(world.BlockAccessor.GetBlock(pos), 1);
	}

	public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Invalid comparison between Unknown and I4
		if (!world.Claims.TryAccess(byPlayer, blockSel.Position, (EnumBlockAccessFlags)2))
		{
			return true;
		}
		if (world.Api.ModLoader.GetModSystem<ModSystemBlockReinforcement>(true).IsLockedForInteract(blockSel.Position, byPlayer))
		{
			if ((int)world.Side == 2)
			{
				ICoreAPI api = world.Api;
				((ICoreClientAPI)((api is ICoreClientAPI) ? api : null)).TriggerIngameError((object)this, "locked", Lang.Get("ingameerror-locked", Array.Empty<object>()));
			}
			return true;
		}
		if (CheckForUpgradeItem(byPlayer, blockSel.Position))
		{
			return true;
		}
		BlockEntity be = world.BlockAccessor.GetBlockEntity(blockSel.Position);
		if (be is BetterCrateBlockEntity)
		{
			((BetterCrateBlockEntity)(object)be).OnPlayerInteract(byPlayer);
		}
		return true;
	}

	public bool CheckForUpgradeItem(IPlayer byPlayer, BlockPos pos)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Invalid comparison between Unknown and I4
		if (!((EntityAgent)byPlayer.Entity).Controls.Sneak)
		{
			return false;
		}
		ItemSlot hotbarslot = byPlayer.InventoryManager.ActiveHotbarSlot;
		if (hotbarslot == null || hotbarslot.Empty || (int)hotbarslot.Itemstack.Class != 1)
		{
			return false;
		}
		return TryToUpgrade(byPlayer, pos);
	}

	public bool TryToUpgrade(IPlayer byPlayer, BlockPos pos)
	{
        //IL_017e: Unknown result type (might be due to invalid IL or missing references)
        //IL_019a: Expected O, but got Unknown
        if (!(api.World.BlockAccessor.GetBlockEntity(pos) is BetterCrateBlockEntity bcbe))
        {
			return false;
		}
		ItemSlot hotbarslot = byPlayer.InventoryManager.ActiveHotbarSlot;
		bool doUpgrade = false;
		if (hotbarslot == null || hotbarslot.Empty || !((object)((RegistryObject)hotbarslot.Itemstack.Item).Code).ToString().Contains("bettercrates:upgrade"))
		{
			return false;
		}
		string upgradeTier = ((RegistryObject)hotbarslot.Itemstack.Item).LastCodePart(0);
		string requiredTier = ((object)((CollectibleObject)hotbarslot.Itemstack.Item).Attributes["requiredTier"][((RegistryObject)hotbarslot.Itemstack.Item).LastCodePart(0)]).ToString();
		((RegistryObject)this).LastCodePart(2);
		if (((RegistryObject)this).LastCodePart(2) == requiredTier)
		{
			doUpgrade = true;
		}
		if (doUpgrade)
		{
			Block block = api.World.GetBlock(((RegistryObject)this).CodeWithPart(upgradeTier, 1));
			if (block != null)
			{
				api.World.BlockAccessor.ExchangeBlock(block.BlockId, pos);
				api.World.BlockAccessor.MarkBlockDirty(pos, (IPlayer)null);
				bcbe.UpgradeInventory();
				byPlayer.InventoryManager.ActiveHotbarSlot.TakeOut(1);
				byPlayer.InventoryManager.ActiveHotbarSlot.MarkDirty();
				api.World.BlockAccessor.MarkBlockDirty(pos, (IPlayer)null);
				IClientPlayer player = (IClientPlayer)(object)((byPlayer is IClientPlayer) ? byPlayer : null);
				if (player != null)
				{
					player.TriggerFpAnimation((EnumHandInteract)2);
				}
				api.World.PlaySoundAt(new AssetLocation("game:sounds/tool/reinforce"), (Entity)(object)byPlayer.Entity, byPlayer, true, 16f, 1f);
				return true;
			}
		}
		return false;
	}

	public override float OnGettingBroken(IPlayer player, BlockSelection blockSel, ItemSlot itemslot, float remainingResistance, float dt, int counter)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (!itemslot.Empty && (int)itemslot.Itemstack.Class == 1 && ((CollectibleObject)itemslot.Itemstack.Item).Tool == (EnumTool?)2)
		{
			return ((Block)this).OnGettingBroken(player, blockSel, itemslot, remainingResistance, dt, counter);
		}
		BlockEntity be = ((Entity)player.Entity).World.BlockAccessor.GetBlockEntity(blockSel.Position);
		if (be != null && be is BetterCrateBlockEntity)
		{
			BetterCrateBlockEntity bcbe = (BetterCrateBlockEntity)(object)be;
			if (counter % 5 == 0)
			{
				bcbe.OnPlayerLeftClick(player);
			}
		}
		return ((Block)this).OnGettingBroken(player, blockSel, itemslot, remainingResistance, dt, counter);
	}

	public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Invalid comparison between Unknown and I4
		if ((int)byPlayer.WorldData.CurrentGameMode == 2)
		{
			if (byPlayer.InventoryManager.ActiveTool == (EnumTool?)2)
			{
				((Block)this).OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
			}
			else if ((int)world.Side == 2)
			{
				BlockEntity be = ((Entity)byPlayer.Entity).World.BlockAccessor.GetBlockEntity(pos);
				if (be != null && be is BetterCrateBlockEntity)
				{
					((BetterCrateBlockEntity)(object)be).OnPlayerLeftClick(byPlayer);
				}
			}
		}
		else
		{
			((Block)this).OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
		}
	}

	public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
	{
		if (api == null)
		{
			return string.Empty;
		}
		if (!(api.World.BlockAccessor.GetBlockEntity(pos) is BetterCrateBlockEntity bcbe))
		{
			return string.Empty;
		}
		string result = string.Empty;
		if (!((InventoryBase)bcbe.inventory).Empty || !((InventoryBase)bcbe.lockedItemInventory).Empty)
		{
			result = result + Lang.Get("Contents:", Array.Empty<object>()) + " " + Lang.Get("{0} / {1} {2}", new object[3]
			{
				bcbe.GetInventoryCount(),
				bcbe.GetStoredItemMaxStackSize() * ((IEnumerable<ItemSlot>)bcbe.inventory).Count(),
				bcbe.GetStoredItemStack().GetName() + "\r\n"
			});
			ItemStack storedItemStack = bcbe.GetStoredItemStack();
			if (storedItemStack != null && storedItemStack.Item != null && ((CollectibleObject)storedItemStack.Item).Durability > 0)
			{
				int leftDurability = storedItemStack.Attributes.GetInt("durability", ((CollectibleObject)storedItemStack.Item).Durability);
				result = result + Lang.Get("Durability: {0} / {1}", new object[2]
				{
					leftDurability,
					((CollectibleObject)storedItemStack.Item).Durability
				}) + "\r\n";
			}
		}
		result = result + Lang.Get("Requires tool tier {0} ({1}) to break", new object[2]
		{
			1,
			Lang.Get("tier_stone", Array.Empty<object>())
		}) + "\r\n";
		BlockBehavior[] blockBehaviors = base.BlockBehaviors;
		foreach (BlockBehavior bh in blockBehaviors)
		{
			result += bh.GetPlacedBlockInfo(world, pos, forPlayer);
		}
		return result;
	}

	public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		if ((int)world.Side == 2 && api.World.BlockAccessor.GetBlockEntity(pos) is BetterCrateBlockEntity bcbe)
		{
			bcbe.NeighborBlockChanged();
		}
		((Block)this).OnNeighbourBlockChange(world, pos, neibpos);
	}
}
