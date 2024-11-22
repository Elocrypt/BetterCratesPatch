using System;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace BetterCratesNamespace
{
    public class BetterCrateBlock : Block
    {
        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            return ArrayExtensions.Append<WorldInteraction>(new WorldInteraction[]
            {
                new WorldInteraction
                {
                    ActionLangCode = "bettercrates:blockhelp-bettercrate-take",
                    MouseButton = EnumMouseButton.Left,
                    HotKeyCode = null,
                    ShouldApply = delegate(WorldInteraction wi, BlockSelection bs, EntitySelection es)
                    {
                        BetterCrateBlockEntity bcbe = this.api.World.BlockAccessor.GetBlockEntity(bs.Position) as BetterCrateBlockEntity;
                        return bcbe != null && (bcbe.inventory == null || !bcbe.inventory.Empty);
                    }
                },
                new WorldInteraction
                {
                    ActionLangCode = "bettercrates:blockhelp-bettercrate-takestack",
                    MouseButton = EnumMouseButton.Left,
                    HotKeyCode = "sneak",
                    ShouldApply = delegate(WorldInteraction wi, BlockSelection bs, EntitySelection es)
                    {
                        BetterCrateBlockEntity bcbe = this.api.World.BlockAccessor.GetBlockEntity(bs.Position) as BetterCrateBlockEntity;
                        return bcbe != null && (bcbe.inventory == null || !bcbe.inventory.Empty);
                    }
                },
                new WorldInteraction
                {
                    ActionLangCode = "bettercrates:blockhelp-bettercrate-putstack",
                    MouseButton = EnumMouseButton.Right,
                    HotKeyCode = null,
                    ShouldApply = delegate(WorldInteraction wi, BlockSelection bs, EntitySelection es)
                    {
                        BetterCrateBlockEntity bcbe = this.api.World.BlockAccessor.GetBlockEntity(bs.Position) as BetterCrateBlockEntity;
                        return bcbe != null && (bcbe.inventory != null && bcbe.lockedItemInventory != null && bcbe.inventory.Empty && bcbe.lockedItemInventory.Empty);
                    }
                },
                new WorldInteraction
                {
                    ActionLangCode = "bettercrates:blockhelp-bettercrate-putmatchingstack",
                    MouseButton = EnumMouseButton.Right,
                    HotKeyCode = null,
                    ShouldApply = delegate(WorldInteraction wi, BlockSelection bs, EntitySelection es)
                    {
                        BetterCrateBlockEntity bcbe = this.api.World.BlockAccessor.GetBlockEntity(bs.Position) as BetterCrateBlockEntity;
                        return bcbe != null && (bcbe.inventory != null && bcbe.lockedItemInventory != null && (!bcbe.inventory.Empty || !bcbe.lockedItemInventory.Empty));
                    }
                },
                new WorldInteraction
                {
                    ActionLangCode = "bettercrates:blockhelp-bettercrate-lock",
                    MouseButton = EnumMouseButton.Right,
                    HotKeyCode = "sprint",
                    RequireFreeHand = true,
                    ShouldApply = delegate(WorldInteraction wi, BlockSelection bs, EntitySelection es)
                    {
                        BetterCrateBlockEntity bcbe = this.api.World.BlockAccessor.GetBlockEntity(bs.Position) as BetterCrateBlockEntity;
                        return bcbe != null && (bcbe.inventory == null || !bcbe.inventory.Empty) && bcbe.lockedItemInventory != null && bcbe.lockedItemInventory.Empty;
                    }
                },
                new WorldInteraction
                {
                    ActionLangCode = "bettercrates:blockhelp-bettercrate-unlock",
                    MouseButton = EnumMouseButton.Right,
                    HotKeyCode = "sprint",
                    RequireFreeHand = true,
                    ShouldApply = delegate(WorldInteraction wi, BlockSelection bs, EntitySelection es)
                    {
                        BetterCrateBlockEntity bcbe = this.api.World.BlockAccessor.GetBlockEntity(bs.Position) as BetterCrateBlockEntity;
                        return bcbe != null && bcbe.lockedItemInventory != null && !bcbe.lockedItemInventory.Empty;
                    }
                }
            }, base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
        }

        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            if (!this.CanPlaceBlock(world, byPlayer, blockSel, ref failureCode))
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
            AssetLocation blockCode = base.CodeWithVariants(new string[]
            {
                "verticalorientation",
                "horizontalorientation"
            }, new string[]
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
            if (world.BlockAccessor.GetBlock(pos) != null)
            {
                string[] lastParts = new string[]
                {
                    "center",
                    "east"
                };
                AssetLocation aLocation = base.CodeWithParts(lastParts);
                return new ItemStack(world.BlockAccessor.GetBlock(aLocation), 1);
            }
            return new ItemStack(world.BlockAccessor.GetBlock(pos), 1);
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (!world.Claims.TryAccess(byPlayer, blockSel.Position, (EnumBlockAccessFlags)2))
            {
                return true;
            }
            if (world.Api.ModLoader.GetModSystem<ModSystemBlockReinforcement>(true).IsLockedForInteract(blockSel.Position, byPlayer))
            {
                if (world.Side == EnumAppSide.Server)
                {
                    (world.Api as ICoreClientAPI).TriggerIngameError(this, "locked", Lang.Get("ingameerror-locked", Array.Empty<object>()));
                }
                return true;
            }
            if (this.CheckForUpgradeItem(byPlayer, blockSel.Position))
            {
                return true;
            }
            BlockEntity be = world.BlockAccessor.GetBlockEntity(blockSel.Position);
            if (be is BetterCrateBlockEntity)
            {
                ((BetterCrateBlockEntity)be).OnPlayerInteract(byPlayer);
            }
            return true;
        }

        public bool CheckForUpgradeItem(IPlayer byPlayer, BlockPos pos)
        {
            if (!byPlayer.Entity.Controls.Sneak)
            {
                return false;
            }
            ItemSlot hotbarslot = byPlayer.InventoryManager.ActiveHotbarSlot;
            return hotbarslot != null && !hotbarslot.Empty && hotbarslot.Itemstack.Class == EnumItemClass.Item && this.TryToUpgrade(byPlayer, pos);
        }

        public string NextUpgradeCodePart
        {
            get
            {
                string a = base.LastCodePart(2);
                if (a == "wood")
                {
                    return "copper";
                }
                if (a == "copper")
                {
                    return "bronze";
                }
                if (a == "bronze")
                {
                    return "iron";
                }
                if (!(a == "iron"))
                {
                    return "wood";
                }
                return "steel";
            }
        }

        public bool TryToUpgrade(IPlayer byPlayer, BlockPos pos)
        {
            BetterCrateBlockEntity bcbe = this.api.World.BlockAccessor.GetBlockEntity(pos) as BetterCrateBlockEntity;
            if (bcbe == null)
            {
                return false;
            }
            ItemSlot hotbarslot = byPlayer.InventoryManager.ActiveHotbarSlot;
            bool doUpgrade = false;
            if (hotbarslot == null || hotbarslot.Empty || !hotbarslot.Itemstack.Item.Code.ToString().Contains("bettercrates:upgrade"))
            {
                return false;
            }
            string upgradeTier = hotbarslot.Itemstack.Item.LastCodePart(0);
            string requiredTier = hotbarslot.Itemstack.Item.Attributes["requiredTier"][hotbarslot.Itemstack.Item.LastCodePart(0)].ToString();
            base.LastCodePart(2);
            if (base.LastCodePart(2) == requiredTier)
            {
                doUpgrade = true;
            }
            if (doUpgrade)
            {
                Block block = this.api.World.GetBlock(base.CodeWithPart(upgradeTier, 1));
                if (block != null)
                {
                    this.api.World.BlockAccessor.ExchangeBlock(block.BlockId, pos);
                    this.api.World.BlockAccessor.MarkBlockDirty(pos, (IPlayer)null);
                    bcbe.UpgradeInventory();
                    byPlayer.InventoryManager.ActiveHotbarSlot.TakeOut(1);
                    byPlayer.InventoryManager.ActiveHotbarSlot.MarkDirty();
                    this.api.World.BlockAccessor.MarkBlockDirty(pos, (IPlayer)null);
                    IClientPlayer player = byPlayer as IClientPlayer;
                    if (player != null)
                    {
                        player.TriggerFpAnimation((EnumHandInteract)2);
                    }
                    this.api.World.PlaySoundAt(new AssetLocation("game:sounds/tool/reinforce"), byPlayer.Entity, byPlayer, true, 16f, 1f);
                    return true;
                }
            }
            return false;
        }

        public override float OnGettingBroken(IPlayer player, BlockSelection blockSel, ItemSlot itemslot, float remainingResistance, float dt, int counter)
        {
            if (!itemslot.Empty && itemslot.Itemstack.Class == EnumItemClass.Item)
            {
                EnumTool? tool = itemslot.Itemstack.Item.Tool;
                EnumTool enumTool = EnumTool.Axe;
                if (tool.GetValueOrDefault() == enumTool & tool != null)
                {
                    return base.OnGettingBroken(player, blockSel, itemslot, remainingResistance, dt, counter);
                }
            }
            BlockEntity be = player.Entity.World.BlockAccessor.GetBlockEntity(blockSel.Position);
            if (be != null && be is BetterCrateBlockEntity)
            {
                BetterCrateBlockEntity bcbe = (BetterCrateBlockEntity)be;
                if (counter % 5 == 0)
                {
                    bcbe.OnPlayerLeftClick(player);
                }
            }
            return base.OnGettingBroken(player, blockSel, itemslot, remainingResistance, dt, counter);
        }

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
        {
            if (byPlayer.WorldData.CurrentGameMode != EnumGameMode.Creative)
            {
                base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
                return;
            }
            EnumTool? activeTool = byPlayer.InventoryManager.ActiveTool;
            EnumTool enumTool = EnumTool.Axe;
            if (activeTool.GetValueOrDefault() == enumTool & activeTool != null)
            {
                base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
                return;
            }
            if (world.Side == EnumAppSide.Client)
            {
                BlockEntity be = byPlayer.Entity.World.BlockAccessor.GetBlockEntity(pos);
                if (be != null && be is BetterCrateBlockEntity)
                {
                    ((BetterCrateBlockEntity)be).OnPlayerLeftClick(byPlayer);
                }
            }
        }

        public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
        {
            if (this.api == null)
            {
                return string.Empty;
            }
            BetterCrateBlockEntity bcbe = this.api.World.BlockAccessor.GetBlockEntity(pos) as BetterCrateBlockEntity;
            if (bcbe == null)
            {
                return string.Empty;
            }
            string result = string.Empty;
            if (!bcbe.inventory.Empty || !bcbe.lockedItemInventory.Empty)
            {
                result = result + Lang.Get("Contents:", Array.Empty<object>()) + " " + Lang.Get("{0} / {1} {2}", new object[]
                {
                    bcbe.GetInventoryCount(),
                    bcbe.GetStoredItemMaxStackSize() * bcbe.inventory.Count<ItemSlot>(),
                    bcbe.GetStoredItemStack().GetName() + "\r\n"
                });
                ItemStack storedItemStack = bcbe.GetStoredItemStack();
                if (storedItemStack != null && storedItemStack.Item != null && storedItemStack.Item.Durability > 0)
                {
                    int leftDurability = storedItemStack.Attributes.GetInt("durability", storedItemStack.Item.Durability);
                    result = result + Lang.Get("Durability: {0} / {1}", new object[]
                    {
                        leftDurability,
                        storedItemStack.Item.Durability
                    }) + "\r\n";
                }
            }
            result = result + Lang.Get("Requires tool tier {0} ({1}) to break", new object[]
            {
                1,
                Lang.Get("tier_stone", Array.Empty<object>())
            }) + "\r\n";
            foreach (BlockBehavior bh in this.BlockBehaviors)
            {
                result += bh.GetPlacedBlockInfo(world, pos, forPlayer);
            }
            return result;
        }

        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
        {
            if (world.Side == EnumAppSide.Client)
            {
                BetterCrateBlockEntity bcbe = this.api.World.BlockAccessor.GetBlockEntity(pos) as BetterCrateBlockEntity;
                if (bcbe != null)
                {
                    bcbe.NeighborBlockChanged();
                }
            }
            base.OnNeighbourBlockChange(world, pos, neibpos);
        }

        public BetterCrateBlockEntity betterCrateBE;
    }
}
