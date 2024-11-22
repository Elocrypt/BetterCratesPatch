using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace BetterCratesNamespace
{
    public class BetterCrateBlockEntity : BlockEntityContainer, ITexPositionSource
    {
        public override string InventoryClassName
        {
            get
            {
                return "bettercrate";
            }
        }

        public override InventoryBase Inventory
        {
            get
            {
                return this.inventory;
            }
        }

        public BetterCrateBlockEntity()
        {
            this.mainMeshData1 = new MeshData(true);
        }

        public override void Initialize(ICoreAPI api)
        {
            this.Api = api;
            this.horizontalOrientation = base.Block.LastCodePart(0);
            this.verticalOrientation = base.Block.LastCodePart(1);
            if (this.inventory == null)
            {
                this.InitInventory(api.World);
            }
            base.Initialize(api);
            this.lockedItemInventory.LateInitialize(string.Concat(new string[]
            {
                this.InventoryClassName,
                "-LockedItemInv-",
                this.Pos.X.ToString(),
                "/",
                this.Pos.Y.ToString(),
                "/",
                this.Pos.Z.ToString()
            }), api);
            this.lockedItemInventory.ResolveBlocksOrItems();
            foreach (long handlerId in this.TickHandlers)
            {
                this.Api.Event.UnregisterGameTickListener(handlerId);
            }
            this.cApi = (this.Api as ICoreClientAPI);
            if (this.cApi != null)
            {
                if (base.Block.FirstCodePart(0) == "bettercrate2sided")
                {
                    this.twoSided = true;
                    this.mainMeshData2 = new MeshData(true);
                }
                this.SetLabelRotation();
                this.UpdateMeshAndLabelRenderer();
                this.tickListenerHandle = this.RegisterGameTickListener(new Action<float>(this.Update), 4000 + this.Api.World.Rand.Next(0, 1000), 0);
            }
        }

        private void Update(float dt)
        {
            this.UpdateMeshAndLabelRenderer();
            this.NeighborBlockChanged();
            this.MarkDirty(true, null);
            this.Api.Event.UnregisterGameTickListener(this.tickListenerHandle);
            this.tickListenerHandle = 0L;
        }

        private void SetLabelRotation()
        {
            string blockCodeRotationParts = base.Block.LastCodePart(1) + "-" + base.Block.LastCodePart(0);
            if (blockCodeRotationParts != null)
            {
                switch (blockCodeRotationParts.Length)
                {
                    case 7:
                        {
                            char c = blockCodeRotationParts[3];
                            if (c != 'e')
                            {
                                if (c == 'w')
                                {
                                    if (blockCodeRotationParts == "up-west")
                                    {
                                        this.labelRotation1 = new Vec3f(1.5707964f, 0f, 1.5707964f);
                                        this.labelRotation2 = new Vec3f(-1.5707964f, 0f, -1.5707964f);
                                        return;
                                    }
                                }
                            }
                            else if (blockCodeRotationParts == "up-east")
                            {
                                this.labelRotation1 = new Vec3f(1.5707964f, 0f, -1.5707964f);
                                this.labelRotation2 = new Vec3f(-1.5707964f, 0f, 1.5707964f);
                                return;
                            }
                            break;
                        }
                    case 8:
                        {
                            char c = blockCodeRotationParts[3];
                            if (c != 'n')
                            {
                                if (c == 's')
                                {
                                    if (blockCodeRotationParts == "up-south")
                                    {
                                        this.labelRotation1 = new Vec3f(1.5707964f, 0f, 0f);
                                        this.labelRotation2 = new Vec3f(-1.5707964f, 0f, 0f);
                                        return;
                                    }
                                }
                            }
                            else if (blockCodeRotationParts == "up-north")
                            {
                                this.labelRotation1 = new Vec3f(-1.5707964f, 3.1415927f, 0f);
                                this.labelRotation2 = new Vec3f(1.5707964f, 3.1415927f, 0f);
                                return;
                            }
                            break;
                        }
                    case 9:
                        {
                            char c = blockCodeRotationParts[5];
                            if (c != 'e')
                            {
                                if (c == 'w')
                                {
                                    if (blockCodeRotationParts == "down-west")
                                    {
                                        this.labelRotation1 = new Vec3f(-1.5707964f, 0f, -1.5707964f);
                                        this.labelRotation2 = new Vec3f(1.5707964f, 0f, 1.5707964f);
                                        return;
                                    }
                                }
                            }
                            else if (blockCodeRotationParts == "down-east")
                            {
                                this.labelRotation1 = new Vec3f(-1.5707964f, 0f, 1.5707964f);
                                this.labelRotation2 = new Vec3f(1.5707964f, 0f, -1.5707964f);
                                return;
                            }
                            break;
                        }
                    case 10:
                        {
                            char c = blockCodeRotationParts[5];
                            if (c != 'n')
                            {
                                if (c == 's')
                                {
                                    if (blockCodeRotationParts == "down-south")
                                    {
                                        this.labelRotation1 = new Vec3f(-1.5707964f, 0f, 0f);
                                        this.labelRotation2 = new Vec3f(-1.5707964f, 3.1415927f, 3.1415927f);
                                        return;
                                    }
                                }
                            }
                            else if (blockCodeRotationParts == "down-north")
                            {
                                this.labelRotation1 = new Vec3f(1.5707964f, 3.1415927f, 0f);
                                this.labelRotation2 = new Vec3f(1.5707964f, 0f, 3.1415927f);
                                return;
                            }
                            break;
                        }
                    case 11:
                        {
                            char c = blockCodeRotationParts[7];
                            if (c != 'e')
                            {
                                if (c == 'w')
                                {
                                    if (blockCodeRotationParts == "center-west")
                                    {
                                        this.labelRotation1 = new Vec3f(0f, -1.5707964f, 0f);
                                        this.labelRotation2 = new Vec3f(0f, 1.5707964f, 0f);
                                        return;
                                    }
                                }
                            }
                            else if (blockCodeRotationParts == "center-east")
                            {
                                this.labelRotation1 = new Vec3f(0f, 1.5707964f, 0f);
                                this.labelRotation2 = new Vec3f(0f, -1.5707964f, 0f);
                                return;
                            }
                            break;
                        }
                    case 12:
                        {
                            char c = blockCodeRotationParts[7];
                            if (c != 'n')
                            {
                                if (c == 's')
                                {
                                    if (blockCodeRotationParts == "center-south")
                                    {
                                        this.labelRotation1 = new Vec3f(0f, 0f, 0f);
                                        this.labelRotation2 = new Vec3f(0f, 3.1415927f, 0f);
                                        return;
                                    }
                                }
                            }
                            else if (blockCodeRotationParts == "center-north")
                            {
                                this.labelRotation1 = new Vec3f(0f, 3.1415927f, 0f);
                                this.labelRotation2 = new Vec3f(0f, 0f, 0f);
                                return;
                            }
                            break;
                        }
                }
            }
            this.labelRotation1 = Vec3f.Zero;
            this.labelRotation2 = new Vec3f(0f, 3.1415927f, 0f);
        }

        protected virtual void InitInventory(IWorldAccessor worldForResolving)
        {
            Block b = base.Block;
            if (b == null)
            {
                b = worldForResolving.BlockAccessor.GetBlock(this.Pos);
            }
            if (b != null && b.Attributes != null)
            {
                string type = b.LastCodePart(2);
                this.quantitySlots = b.Attributes["quantitySlots"][type].AsInt(this.quantitySlots);
            }
            this.inventory = new InventoryGeneric(this.quantitySlots, null, null, null)
            {
                BaseWeight = 1f
            };
            this.inventory.SlotModified += this.OnSlotModified;
            this.inventory.OnGetAutoPullFromSlot = new GetAutoPullFromSlotDelegate(this.GetAutoPullFromSlot);
            this.inventory.OnGetAutoPushIntoSlot = new GetAutoPushIntoSlotDelegate(this.GetAutoPushIntoSlot);
            this.lockedItemInventory = new InventoryGeneric(1, null, null, null);
            this.lockedItemInventory.SlotModified += this.OnSlotModified;
        }

        public void UpgradeInventory()
        {
            string type = base.Block.LastCodePart(2);
            this.quantitySlots = base.Block.Attributes["quantitySlots"][type].AsInt(0);
            if (this.quantitySlots == 0)
            {
                return;
            }
            InventoryGeneric tempInventory = new InventoryGeneric(this.quantitySlots, string.Concat(new string[]
            {
                this.InventoryClassName,
                "-LockedItemInv-",
                this.Pos.X.ToString(),
                "/",
                this.Pos.Y.ToString(),
                "/",
                this.Pos.Z.ToString()
            }), this.Api, null);
            ItemSlot[] tempItemSlotArray = this.inventory.ToArray<ItemSlot>();
            for (int i = 0; i < tempItemSlotArray.Length; i++)
            {
                tempInventory[i] = tempItemSlotArray[i];
            }
            this.inventory = tempInventory;
            this.inventory.BaseWeight = 1f;
            this.inventory.SlotModified += this.OnSlotModified;
            this.inventory.OnGetAutoPullFromSlot = new GetAutoPullFromSlotDelegate(this.GetAutoPullFromSlot);
            this.inventory.OnGetAutoPushIntoSlot = new GetAutoPushIntoSlotDelegate(this.GetAutoPushIntoSlot);
            this.MarkDirty(true, null);
            this.Api.World.BlockAccessor.GetChunkAtBlockPos(this.Pos).MarkModified();
        }

        private void OnSlotModified(int slot)
        {
            if (this.Api.World.BlockAccessor.GetChunkAtBlockPos(this.Pos) != null)
            {
                this.Api.World.BlockAccessor.GetChunkAtBlockPos(this.Pos).MarkModified();
            }
            this.MarkDirty(false, null);
        }

        private ItemSlot GetAutoPushIntoSlot(BlockFacing atBlockFace, ItemSlot fromSlot)
        {
            if (atBlockFace == BlockFacing.DOWN)
            {
                return null;
            }
            if (!this.AllowedForStorage(fromSlot))
            {
                return null;
            }
            if (this.inventory.Empty)
            {
                return this.inventory[0];
            }
            return this.inventory.GetBestSuitedSlot(fromSlot, null, null).slot;
        }

        private ItemSlot GetAutoPullFromSlot(BlockFacing atBlockFace)
        {
            if (this.inventory.Empty)
            {
                return null;
            }
            if (atBlockFace == BlockFacing.DOWN)
            {
                return this.inventory.LastOrDefault((ItemSlot slot) => !slot.Empty);
            }
            return null;
        }

        internal bool OnPlayerInteract(IPlayer byPlayer)
        {
            ItemSlot playerSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if (playerSlot == null)
            {
                return false;
            }
            long currentTime = this.Api.World.ElapsedMilliseconds;
            bool doubleClicked = currentTime - this.lastInteractTime < 500L;
            this.lastInteractTime = currentTime;
            if (playerSlot.Empty)
            {
                if (byPlayer.Entity.Controls.Sprint)
                {
                    this.ToggleItemLock();
                    return false;
                }
                if (doubleClicked && this.Api.Side == EnumAppSide.Server && this.TryPutAll(byPlayer))
                {
                    BlockPos blockPos = this.Pos;
                    ((ICoreServerAPI)this.Api).Network.SendBlockEntityPacket((IServerPlayer)byPlayer, blockPos, BetterCrateBlockEntity.packetIDPutAll, null);
                    return true;
                }
            }
            else
            {
                if (!this.AllowedForStorage(playerSlot))
                {
                    return false;
                }
                if (this.TryPut(byPlayer.InventoryManager.ActiveHotbarSlot, true))
                {
                    IClientPlayer player = byPlayer as IClientPlayer;
                    if (player != null)
                    {
                        player.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
                    }
                    this.Api.World.PlaySoundAt(new AssetLocation("game:sounds/player/build"), byPlayer.Entity, byPlayer, true, 16f, 1f);
                    return true;
                }
            }
            return false;
        }

        private void ToggleItemLock()
        {
            if (this.inventory.Empty)
            {
                if (!this.lockedItemInventory.Empty)
                {
                    this.lockedItemInventory.DiscardAll();
                }
            }
            else if (this.lockedItemInventory != null && this.lockedItemInventory.Empty)
            {
                this.lockedItemInventory[0].Itemstack = this.GetStoredItemStack().Clone();
                this.lockedItemInventory[0].Itemstack.StackSize = 1;
            }
            else if (this.lockedItemInventory != null)
            {
                this.lockedItemInventory.DiscardAll();
            }
            this.MarkDirty(false, null);
            this.Api.World.BlockAccessor.GetChunkAtBlockPos(this.Pos).MarkModified();
        }

        public void OnPlayerLeftClick(IPlayer player)
        {
            if (this.cApi != null)
            {
                if (this.inventory.Empty)
                {
                    return;
                }
                byte[] data = new byte[1];
                if (player.Entity.Controls.Sneak)
                {
                    data[0] = 1;
                }
                this.cApi.Network.SendBlockEntityPacket(this.Pos, BetterCrateBlockEntity.packetIDClientLeftClick, data);
            }
        }

        public override void OnReceivedClientPacket(IPlayer fromPlayer, int packetid, byte[] data)
        {
            base.OnReceivedClientPacket(fromPlayer, packetid, data);
            if (this.Api.Side == EnumAppSide.Server)
            {
                if (!this.Api.World.Claims.TryAccess(fromPlayer, this.Pos, EnumBlockAccessFlags.Use))
                {
                    return;
                }
                if (this.Api.World.Api.ModLoader.GetModSystem<ModSystemBlockReinforcement>(true).IsLockedForInteract(this.Pos, fromPlayer))
                {
                    BlockPos blockPos = this.Pos;
                    ((ICoreServerAPI)this.Api).Network.SendBlockEntityPacket((IServerPlayer)fromPlayer, blockPos, BetterCrateBlockEntity.packetIDLockedError, null);
                    return;
                }
                if (packetid == BetterCrateBlockEntity.packetIDClientLeftClick)
                {
                    bool takeBulk = false;
                    if (data.Length != 0 && data[0] == 1)
                    {
                        takeBulk = true;
                    }
                    this.TryTake(fromPlayer, takeBulk);
                }
            }
        }

        public override void OnReceivedServerPacket(int packetid, byte[] data)
        {
            if (this.cApi != null)
            {
                if (packetid == BetterCrateBlockEntity.packetIDLockedError)
                {
                    this.cApi.TriggerIngameError(this, "locked", Lang.Get("ingameerror-locked", Array.Empty<object>()));
                }
                if (packetid == BetterCrateBlockEntity.packetIDPutAll)
                {
                    this.Api.World.PlaySoundAt(new AssetLocation("game:sounds/player/build"), this.cApi.World.Player.Entity, this.cApi.World.Player, true, 16f, 1f);
                }
            }
            base.OnReceivedServerPacket(packetid, data);
        }

        public bool AllowedForStorage(ItemSlot inSlot)
        {
            if (inSlot == null || inSlot.Itemstack == null || this.Api == null)
            {
                return false;
            }
            if (!this.inventory.Empty && this.GetInventoryFirstItemSlotNotFull(null) == null)
            {
                return false;
            }
            ItemStack currentItemStack = this.GetStoredItemStack();
            if (currentItemStack == null && inSlot.Itemstack.Block != null)
            {
                BlockContainer bContainer = inSlot.Itemstack.Block as BlockContainer;
                if (bContainer != null)
                {
                    ItemStack[] containerContents = bContainer.GetNonEmptyContents(this.Api.World, inSlot.Itemstack);
                    if (containerContents != null && containerContents.Length != 0)
                    {
                        if (this.cApi != null)
                        {
                            this.cApi.TriggerIngameError(this, "cantstore", Lang.Get("bettercrates:item-filledcontainer", Array.Empty<object>()));
                        }
                        return false;
                    }
                }
            }
            if (currentItemStack != null && !currentItemStack.Equals(this.Api.World, inSlot.Itemstack, GlobalConstants.IgnoredStackAttributes))
            {
                bool ok = false;
                if (currentItemStack.Collectible != null && inSlot.Itemstack.Collectible != null && currentItemStack.Collectible.Code != inSlot.Itemstack.Collectible.Code)
                {
                    return false;
                }
                if (inSlot.Itemstack.Block is BlockCrock && currentItemStack.Block is BlockCrock)
                {
                    if (inSlot.Itemstack.Block != null)
                    {
                        BlockContainer bContainer2 = inSlot.Itemstack.Block as BlockContainer;
                        if (bContainer2 != null)
                        {
                            ItemStack[] containerContents2 = bContainer2.GetNonEmptyContents(this.Api.World, inSlot.Itemstack);
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
                if (this.cApi != null)
                {
                    this.cApi.TriggerIngameError(this, "cantstore", Lang.Get("bettercrates:item-perishable", Array.Empty<object>()));
                }
                return false;
            }
            if (inSlotColObj.HasTemperature(inSlot.Itemstack))
            {
                if (inSlotColObj.GetTemperature(this.Api.World, inSlot.Itemstack) >= 20f)
                {
                    if (this.cApi != null)
                    {
                        this.cApi.TriggerIngameError(this, "cantstore", Lang.Get("bettercrates:item-toohot", Array.Empty<object>()));
                    }
                    return false;
                }
                inSlotColObj.SetTemperature(this.Api.World, inSlot.Itemstack, 0f, false);
            }
            return true;
        }

        public int GetInventoryCount()
        {
            if (this.inventory.Empty)
            {
                return 0;
            }
            if (this.lastInventoryCount > 0)
            {
                return this.lastInventoryCount;
            }
            int count = 0;
            for (int i = 0; i < this.inventory.Count; i++)
            {
                ItemStack stack = this.inventory[i].Itemstack;
                if (stack == null)
                {
                    break;
                }
                count += stack.StackSize;
            }
            this.lastInventoryCount = count;
            return count;
        }

        public void UpdateMeshAndLabelRenderer()
        {
            if (this.Api == null || this.Api.Side == EnumAppSide.Server)
            {
                return;
            }
            this.lastInventoryCount = -1;
            if (this.inventory.Empty && this.lockedItemInventory.Empty)
            {
                this.mainMeshData1 = null;
                this.mainMeshData2 = null;
                this.previousItemStackID = -1;
                if (this.labelRenderer1 != null)
                {
                    this.labelRenderer1.SetNewTextAndRotation(string.Empty, ColorUtil.BlackArgb, this.labelRotation1);
                    this.labelRenderer1.DrawLockIcon = false;
                }
                if (this.twoSided && this.labelRenderer2 != null)
                {
                    this.labelRenderer2.SetNewTextAndRotation(string.Empty, ColorUtil.BlackArgb, this.labelRotation2);
                    this.labelRenderer2.DrawLockIcon = false;
                }
                return;
            }
            this.UpdateMesh();
            this.text = this.GetInventoryCount().ToString();
            if (this.labelRenderer1 != null)
            {
                this.labelRenderer1.SetNewTextAndRotation(this.text, ColorUtil.ToRgba(255, 0, 0, 0), this.labelRotation1);
                this.labelRenderer1.DrawLockIcon = !this.lockedItemInventory.Empty;
            }
            else
            {
                this.labelRenderer1 = new BetterCrateLabelRender(this, this.Pos, this.cApi);
                this.labelRenderer1.SetNewTextAndRotation(this.text, ColorUtil.ToRgba(255, 0, 0, 0), this.labelRotation1);
                this.labelRenderer1.DrawLockIcon = !this.lockedItemInventory.Empty;
                this.NeighborBlockChanged();
            }
            if (this.twoSided)
            {
                if (this.labelRenderer2 != null)
                {
                    this.labelRenderer2.SetNewTextAndRotation(this.text, ColorUtil.ToRgba(255, 0, 0, 0), this.labelRotation2);
                    this.labelRenderer2.DrawLockIcon = !this.lockedItemInventory.Empty;
                    return;
                }
                this.labelRenderer2 = new BetterCrateLabelRender(this, this.Pos, this.cApi);
                this.labelRenderer2.SetNewTextAndRotation(this.text, ColorUtil.ToRgba(255, 0, 0, 0), this.labelRotation2);
                this.labelRenderer2.DrawLockIcon = !this.lockedItemInventory.Empty;
                this.NeighborBlockChanged();
            }
        }

        private bool TryPutAll(IPlayer byPlayer)
        {
            if (this.inventory.Empty && this.lockedItemInventory.Empty)
            {
                return false;
            }
            if (this.GetStoredItemStack() == null)
            {
                return false;
            }
            bool result = false;
            string backpackInvClassName = byPlayer.InventoryManager.GetInventoryName("backpack");
            IInventory backpackInv = byPlayer.InventoryManager.GetInventory(backpackInvClassName);
            IInventory hotbarInv = byPlayer.InventoryManager.GetHotbarInventory();
            if (hotbarInv != null)
            {
                for (int i = 0; i < hotbarInv.Count - 1; i++)
                {
                    if (!hotbarInv[i].Empty && this.AllowedForStorage(hotbarInv[i]) && this.TryPut(hotbarInv[i], true))
                    {
                        result = true;
                    }
                }
            }
            if (backpackInv != null)
            {
                for (int j = 0; j < backpackInv.Count; j++)
                {
                    if (!backpackInv[j].Empty && this.AllowedForStorage(backpackInv[j]) && this.TryPut(backpackInv[j], true))
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
            for (; ; )
            {
                ItemSlot slot = this.GetInventoryFirstItemSlotNotFull(fromSlot.Itemstack);
                if (slot == null)
                {
                    return result;
                }
                int movedQuantity = fromSlot.TryPutInto(this.Api.World, slot, quantity);
                if (movedQuantity > 0)
                {
                    result = true;
                }
                quantity -= movedQuantity;
                if (quantity <= 0)
                {
                    break;
                }
                count++;
                if (count > this.inventory.Count)
                {
                    return result;
                }
            }
            result = true;
            return result;
        }

        private bool TryTake(IPlayer byPlayer, bool takeBulk)
        {
            if (this.inventory.Empty)
            {
                return false;
            }
            bool result = false;
            int takeQuantity = 1;
            if (takeBulk)
            {
                takeQuantity = this.GetStoredItemMaxStackSize();
            }
            int quantityToTakeRemaining = takeQuantity;
            ItemStack stack = null;
            int count = 0;
            do
            {
                ItemSlot slot = this.GetInventoryLastItemSlotWithItem();
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
                    stack.StackSize += temp2.StackSize;
                }
                if (stack.StackSize >= takeQuantity)
                {
                    break;
                }
                quantityToTakeRemaining -= stack.StackSize;
                count++;
            }
            while (count < this.inventory.Count);
            if (byPlayer.InventoryManager.TryGiveItemstack(stack, true))
            {
                if (stack.Block != null && stack.Block.Sounds != null)
                {
                    this.Api.World.PlaySoundAt(stack.Block.Sounds.Place, byPlayer.Entity, byPlayer, true, 16f, 1f);
                }
                else
                {
                    this.Api.World.PlaySoundAt(new AssetLocation("game:sounds/player/build"), byPlayer.Entity, byPlayer, true, 16f, 1f);
                }
            }
            if (stack.StackSize > 0)
            {
                Vec3f spawnDirection = Vec3f.Zero;
                Vec3d spawnVelocity = Vec3d.Zero;
                spawnDirection.Set(0.5f, 0.4f, 0.5f);
                string a = this.horizontalOrientation;
                if (!(a == "north"))
                {
                    if (!(a == "south"))
                    {
                        if (!(a == "east"))
                        {
                            if (a == "west")
                            {
                                spawnDirection.X = 1f;
                                spawnVelocity.X = 0.025;
                                spawnVelocity.Z = (this.Api.World.Rand.NextDouble() - 0.5) * 0.009999999776482582;
                            }
                        }
                        else
                        {
                            spawnDirection.X = 0f;
                            spawnVelocity.X = -0.025;
                            spawnVelocity.Z = (this.Api.World.Rand.NextDouble() - 0.5) * 0.009999999776482582;
                        }
                    }
                    else
                    {
                        spawnDirection.Z = 0f;
                        spawnVelocity.Z = -0.025;
                        spawnVelocity.X = (this.Api.World.Rand.NextDouble() - 0.5) * 0.009999999776482582;
                    }
                }
                else
                {
                    spawnDirection.Z = 1f;
                    spawnVelocity.Z = 0.025;
                    spawnVelocity.X = (this.Api.World.Rand.NextDouble() - 0.5) * 0.009999999776482582;
                }
                a = this.verticalOrientation;
                if (!(a == "center"))
                {
                    if (!(a == "up"))
                    {
                        if (a == "down")
                        {
                            spawnDirection.X = (spawnDirection.Z = 0.5f);
                            spawnDirection.Y = 0f;
                            spawnVelocity.Y = 0.0;
                            spawnVelocity.X = (this.Api.World.Rand.NextDouble() - 0.5) * 0.009999999776482582;
                            spawnVelocity.Z = (this.Api.World.Rand.NextDouble() - 0.5) * 0.009999999776482582;
                        }
                    }
                    else
                    {
                        spawnDirection.X = (spawnDirection.Z = 0.5f);
                        spawnDirection.Y = 1f;
                        spawnVelocity.Y = 0.05000000074505806;
                        spawnVelocity.X = (this.Api.World.Rand.NextDouble() - 0.5) * 0.009999999776482582;
                        spawnVelocity.Z = (this.Api.World.Rand.NextDouble() - 0.5) * 0.009999999776482582;
                    }
                }
                this.Api.World.SpawnItemEntity(stack, this.Pos.ToVec3d().Add(spawnDirection), spawnVelocity);
            }
            if (result)
            {
                this.MarkDirty(false, null);
            }
            return result;
        }

        public override void OnBlockPlaced(ItemStack byItemStack = null)
        {
            base.OnBlockPlaced(null);
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
        }

        public void NeighborBlockChanged()
        {
            if (this.Api != null)
            {
                this.labelFace1OppositeIsOpaque = false;
                this.labelFace2OppositeIsOpaque = false;
                string a = this.horizontalOrientation;
                if (!(a == "north"))
                {
                    if (!(a == "south"))
                    {
                        if (!(a == "east"))
                        {
                            if (a == "west")
                            {
                                this.labelFace1OppositeIsOpaque = this.Api.World.BlockAccessor.GetBlock(this.Pos.EastCopy(1)).SideOpaque[BlockFacing.EAST.Index];
                                this.labelFace2OppositeIsOpaque = this.Api.World.BlockAccessor.GetBlock(this.Pos.WestCopy(1)).SideOpaque[BlockFacing.WEST.Index];
                            }
                        }
                        else
                        {
                            this.labelFace1OppositeIsOpaque = this.Api.World.BlockAccessor.GetBlock(this.Pos.WestCopy(1)).SideOpaque[BlockFacing.WEST.Index];
                            this.labelFace2OppositeIsOpaque = this.Api.World.BlockAccessor.GetBlock(this.Pos.EastCopy(1)).SideOpaque[BlockFacing.EAST.Index];
                        }
                    }
                    else
                    {
                        this.labelFace1OppositeIsOpaque = this.Api.World.BlockAccessor.GetBlock(this.Pos.NorthCopy(1)).SideOpaque[BlockFacing.NORTH.Index];
                        this.labelFace2OppositeIsOpaque = this.Api.World.BlockAccessor.GetBlock(this.Pos.SouthCopy(1)).SideOpaque[BlockFacing.SOUTH.Index];
                    }
                }
                else
                {
                    this.labelFace1OppositeIsOpaque = this.Api.World.BlockAccessor.GetBlock(this.Pos.SouthCopy(1)).SideOpaque[BlockFacing.SOUTH.Index];
                    this.labelFace2OppositeIsOpaque = this.Api.World.BlockAccessor.GetBlock(this.Pos.NorthCopy(1)).SideOpaque[BlockFacing.NORTH.Index];
                }
                a = this.verticalOrientation;
                if (!(a == "center"))
                {
                    if (!(a == "up"))
                    {
                        if (a == "down")
                        {
                            this.labelFace1OppositeIsOpaque = this.Api.World.BlockAccessor.GetBlock(this.Pos.DownCopy(1)).SideOpaque[BlockFacing.UP.Index];
                            this.labelFace2OppositeIsOpaque = this.Api.World.BlockAccessor.GetBlock(this.Pos.UpCopy(1)).SideOpaque[BlockFacing.DOWN.Index];
                        }
                    }
                    else
                    {
                        this.labelFace1OppositeIsOpaque = this.Api.World.BlockAccessor.GetBlock(this.Pos.UpCopy(1)).SideOpaque[BlockFacing.DOWN.Index];
                        this.labelFace2OppositeIsOpaque = this.Api.World.BlockAccessor.GetBlock(this.Pos.DownCopy(1)).SideOpaque[BlockFacing.UP.Index];
                    }
                }
                if (this.labelRenderer1 != null)
                {
                    this.labelRenderer1.ShouldDraw = !this.labelFace1OppositeIsOpaque;
                }
                if (this.twoSided && this.labelRenderer2 != null)
                {
                    this.labelRenderer2.ShouldDraw = !this.labelFace2OppositeIsOpaque;
                }
            }
        }

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            if (this.shouldDrawMesh)
            {
                if (this.mainMeshData1 != null && !this.labelFace1OppositeIsOpaque)
                {
                    mesher.AddMeshData(this.mainMeshData1, 1);
                }
                if (this.mainMeshData2 != null && !this.labelFace2OppositeIsOpaque)
                {
                    mesher.AddMeshData(this.mainMeshData2, 1);
                }
            }
            return false;
        }

        public int GetStoredItemMaxStackSize()
        {
            int result = 1;
            ItemStack stack = this.GetStoredItemStack();
            if (stack != null)
            {
                if (stack.Class == EnumItemClass.Item)
                {
                    result = stack.Item.MaxStackSize;
                }
                else if (stack.Class == EnumItemClass.Block)
                {
                    result = stack.Block.MaxStackSize;
                }
            }
            return result;
        }

        public ItemStack GetStoredItemStack()
        {
            if (this.inventory.Empty)
            {
                if (this.lockedItemInventory.Empty)
                {
                    return null;
                }
                if (this.lockedItemInventory[0].Itemstack != null)
                {
                    return this.lockedItemInventory[0].Itemstack;
                }
            }
            ItemSlot temp = this.inventory.FirstOrDefault((ItemSlot slot) => !slot.Empty);
            if (temp == null)
            {
                return null;
            }
            return temp.Itemstack;
        }

        public ItemSlot GetStoredItemSlot()
        {
            if (this.inventory.Empty)
            {
                if (this.lockedItemInventory.Empty)
                {
                    return null;
                }
                if (this.lockedItemInventory[0].Itemstack != null)
                {
                    return this.lockedItemInventory[0];
                }
            }
            return this.inventory.FirstOrDefault((ItemSlot slot) => !slot.Empty);
        }

        public ItemSlot GetInventoryLastItemSlotWithItem()
        {
            if (this.inventory.Empty)
            {
                return null;
            }
            return this.inventory.LastOrDefault((ItemSlot slot) => !slot.Empty);
        }

        public ItemSlot GetInventoryFirstItemSlotNotFull(ItemStack inStack)
        {
            ItemStack storedItemStack;
            if (this.inventory.Empty)
            {
                storedItemStack = inStack;
            }
            else
            {
                storedItemStack = this.GetStoredItemStack();
            }
            int maxStackSize;
            if (storedItemStack.Class == EnumItemClass.Item)
            {
                maxStackSize = storedItemStack.Item.MaxStackSize;
            }
            else
            {
                maxStackSize = storedItemStack.Block.MaxStackSize;
            }
            return this.inventory.FirstOrDefault((ItemSlot slot) => slot.Empty || slot.StackSize < maxStackSize);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            if (this.Pos == null)
            {
                this.Pos = new BlockPos(tree.GetInt("posx", 0), tree.GetInt("posy", 0), tree.GetInt("posz", 0), tree.GetInt("posy", 0) / 32768);
            }
            if (this.inventory == null)
            {
                this.InitInventory(worldForResolving);
            }
            base.FromTreeAttributes(tree, worldForResolving);
            this.lockedItemInventory.FromTreeAttributes(tree.GetTreeAttribute("lockedItemInv"));
            if (this.Api != null && worldForResolving.Side == EnumAppSide.Client)
            {
                this.UpdateMeshAndLabelRenderer();
                this.MarkDirty(true, null);
            }
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            if (this.lockedItemInventory != null)
            {
                ITreeAttribute lockedItemInvTree = new TreeAttribute();
                this.lockedItemInventory.ToTreeAttributes(lockedItemInvTree);
                tree["lockedItemInv"] = lockedItemInvTree;
            }
        }

        public override void OnBlockBroken(IPlayer byPlayer = null)
        {
            if (this.labelRenderer1 != null)
            {
                this.labelRenderer1.Dispose();
                this.labelRenderer1 = null;
            }
            if (this.labelRenderer2 != null)
            {
                this.labelRenderer2.Dispose();
                this.labelRenderer2 = null;
            }
            this.mainMeshData1 = null;
            this.mainMeshData2 = null;
            base.OnBlockBroken(byPlayer);
        }

        public override void OnBlockRemoved()
        {
            if (this.labelRenderer1 != null)
            {
                this.labelRenderer1.Dispose();
                this.labelRenderer1 = null;
            }
            if (this.labelRenderer2 != null)
            {
                this.labelRenderer2.Dispose();
                this.labelRenderer2 = null;
            }
            this.mainMeshData1 = null;
            this.mainMeshData2 = null;
            base.OnBlockRemoved();
        }

        public override void OnBlockUnloaded()
        {
            if (this.labelRenderer1 != null)
            {
                this.labelRenderer1.Dispose();
                this.labelRenderer1 = null;
            }
            if (this.labelRenderer2 != null)
            {
                this.labelRenderer2.Dispose();
                this.labelRenderer2 = null;
            }
            this.mainMeshData1 = null;
            this.mainMeshData2 = null;
            base.OnBlockUnloaded();
        }

        private void UpdateMesh()
        {
            ItemStack currentItemStack = this.GetStoredItemStack();
            int currentItemStackID = -1;
            if (currentItemStack != null)
            {
                currentItemStackID = currentItemStack.Id;
            }
            if (currentItemStackID == this.previousItemStackID)
            {
                return;
            }
            this.previousItemStackID = currentItemStackID;
            MeshData m = this.GenMeshData(this.cApi.Tesselator);
            MeshData m2 = null;
            if (m != null)
            {
                m2 = m.Clone();
            }
            if (m2 != null)
            {
                this.TranslateMesh(m2);
                this.UpdateXYZFaces(m2);
                this.mainMeshData1 = m2;
                if (this.twoSided)
                {
                    MeshData m3 = m2.Clone();
                    string a = this.verticalOrientation;
                    if (!(a == "center"))
                    {
                        if (a == "up" || a == "down")
                        {
                            string a2 = this.horizontalOrientation;
                            if (!(a2 == "east") && !(a2 == "west"))
                            {
                                if (a2 == "north" || a2 == "south")
                                {
                                    m3.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 3.1415927f, 0f, 0f);
                                }
                            }
                            else
                            {
                                m3.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0f, 0f, 3.1415927f);
                            }
                        }
                    }
                    else
                    {
                        m3.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 3.1415927f, 0f, 3.1415927f);
                    }
                    this.mainMeshData2 = m3.Clone();
                }
            }
            this.MarkDirty(true, null);
        }

        private void UpdateXYZFaces(MeshData m1)
        {
            byte facing = 0;
            string a = this.verticalOrientation;
            if (!(a == "up"))
            {
                if (!(a == "down"))
                {
                    if (a == "center")
                    {
                        string a2 = this.horizontalOrientation;
                        if (!(a2 == "north"))
                        {
                            if (!(a2 == "south"))
                            {
                                if (!(a2 == "east"))
                                {
                                    if (a2 == "west")
                                    {
                                        facing = (byte)(BlockFacing.EAST.Index + 1);
                                    }
                                }
                                else
                                {
                                    facing = (byte)(BlockFacing.WEST.Index + 1);
                                }
                            }
                            else
                            {
                                facing = (byte)(BlockFacing.NORTH.Index + 1);
                            }
                        }
                        else
                        {
                            facing = (byte)(BlockFacing.SOUTH.Index + 1);
                        }
                    }
                }
                else
                {
                    facing = (byte)(BlockFacing.DOWN.Index + 1);
                }
            }
            else
            {
                facing = (byte)(BlockFacing.UP.Index + 1);
            }
            if (facing > 0)
            {
                for (int i = 0; i < m1.XyzFaces.Length; i++)
                {
                    m1.XyzFaces[i] = facing;
                }
            }
        }

        public void TranslateMesh(MeshData mesh)
        {
            if (mesh == null)
            {
                return;
            }
            ItemStack storedItemStack = this.GetStoredItemStack();
            if (storedItemStack == null)
            {
                return;
            }
            ModelTransform modelTransform;
            if (storedItemStack.Class == EnumItemClass.Item)
            {
                modelTransform = storedItemStack.Item.GuiTransform;
            }
            else
            {
                modelTransform = storedItemStack.Block.GuiTransform;
            }
            float[] modelMat = Mat4f.Create();
            Mat4f.Identity(modelMat);
            Vec3f scale = 0.25f * modelTransform.ScaleXYZ;
            float rotationX = modelTransform.Rotation.X * 0.017453292f;
            if (storedItemStack.Class == EnumItemClass.Item) //Block?
            {
                rotationX += 3.1415927f;
            }
            Mat4f.Scale(modelMat, modelMat, new float[]
            {
                1f,
                1f,
                -1f
            });
            Mat4f.RotateX(modelMat, modelMat, rotationX);
            Mat4f.RotateY(modelMat, modelMat, 0.017453292f * modelTransform.Rotation.Y);
            Mat4f.RotateZ(modelMat, modelMat, 0.017453292f * modelTransform.Rotation.Z);
            Mat4f.Scale(modelMat, modelMat, new float[]
            {
                scale.X,
                scale.Y,
                scale.Z
            });
            Mat4f.Translate(modelMat, modelMat, -modelTransform.Origin.X, -modelTransform.Origin.Y, -modelTransform.Origin.Z);
            mesh.MatrixTransform(modelMat);
            mesh.Scale(new Vec3f(0.5f, 0.5f, 0.5f), 1f, 1f, 0.0025f);
            mesh.Translate(0.5f, 0.565f, 0.51f);
            mesh.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), this.labelRotation1.X, this.labelRotation1.Y + 3.1415927f, -this.labelRotation1.Z);
        }

        public Size2i AtlasSize
        {
            get
            {
                return this.cApi.BlockTextureAtlas.Size;
            }
        }

        public TextureAtlasPosition this[string textureCode]
        {
            get
            {
                if (this.tesselatingSpecial)
                {
                    if (textureCode == "material")
                    {
                        return this.tmpTextureSource[this.curMat];
                    }
                    if (textureCode == "material-deco")
                    {
                        return this.tmpTextureSource["deco-" + this.curMat];
                    }
                    if (textureCode == "lining")
                    {
                        if (this.curLining == "plain")
                        {
                            return this.tmpTextureSource[this.curMat];
                        }
                        return this.tmpTextureSource[this.curLining];
                    }
                    else
                    {
                        if (textureCode == "glass")
                        {
                            return this.glassTextureSource["material"];
                        }
                        return this.tmpTextureSource[textureCode];
                    }
                }
                else
                {
                    ItemStack storedItemStack = this.GetStoredItemStack();
                    if (storedItemStack != null && storedItemStack.Block != null && textureCode == "painting")
                    {
                        return this.cApi.Tesselator.GetTextureSource(storedItemStack.Block, 0, false)[textureCode];
                    }
                    AssetLocation textureLoc = null;
                    IAsset texAsset = null;
                    CompositeTexture tex;
                    if (this.tesselatingModBlock)
                    {
                        string originalTextureCode = textureCode;
                        string type = storedItemStack.Attributes.GetString("type", null);
                        if (type != null)
                        {
                            textureCode = type + "-" + textureCode;
                        }
                        if (storedItemStack.Block.Textures.TryGetValue(textureCode, out tex))
                        {
                            textureLoc = tex.Baked.BakedName;
                            TextureAtlasPosition texPos = this.cApi.BlockTextureAtlas[textureLoc];
                            if (texPos != null)
                            {
                                return texPos;
                            }
                        }
                        else if (storedItemStack.Block.Textures.TryGetValue(originalTextureCode, out tex))
                        {
                            textureLoc = tex.Baked.BakedName;
                            TextureAtlasPosition texPos = this.cApi.BlockTextureAtlas[textureLoc];
                            if (texPos != null)
                            {
                                return texPos;
                            }
                        }
                    }
                    int num = 0;
                    TextureAtlasPosition mainTexPos = null;
                    if (storedItemStack.Class == EnumItemClass.Item && storedItemStack.Item.Textures.TryGetValue(textureCode, out tex))
                    {
                        textureLoc = tex.Baked.BakedName;
                        if (textureLoc.GetName().Equals("clearquartz"))
                        {
                            textureLoc = new AssetLocation("game:item/resource/ungraded/quartz");
                        }
                        mainTexPos = this.cApi.BlockTextureAtlas[textureLoc];
                        if (mainTexPos != null)
                        {
                            return mainTexPos;
                        }
                        texAsset = this.cApi.Assets.TryGet(textureLoc.Clone().WithPathPrefixOnce("textures/").WithPathAppendixOnce(".png"), true);
                        if (texAsset != null)
                        {
                            AssetLocation temp = new AssetLocation();
                            if (texAsset.Location != null && texAsset.Location.FirstPathPart(0) == "textures")
                            {
                                temp = new AssetLocation(texAsset.Location.ToString().Replace("textures/", ""));
                            }
                            this.cApi.BlockTextureAtlas.GetOrInsertTexture(temp, out num, out mainTexPos, null, 0f);
                            return mainTexPos;
                        }
                    }
                    if (textureLoc == null && this.shapeTextures != null)
                    {
                        this.shapeTextures.TryGetValue(textureCode, out textureLoc);
                    }
                    if (textureLoc != null)
                    {
                        TextureAtlasPosition shapeTexPos = this.cApi.BlockTextureAtlas[textureLoc];
                        if (shapeTexPos == null)
                        {
                            texAsset = this.cApi.Assets.TryGet(textureLoc.Clone().WithPathPrefixOnce("textures/").WithPathAppendixOnce(".png"), true);
                            if (texAsset != null)
                            {
                                AssetLocation temp2 = new AssetLocation();
                                if (texAsset.Location != null && texAsset.Location.FirstPathPart(0) == "textures")
                                {
                                    temp2 = new AssetLocation(texAsset.Location.ToString().Replace("textures/", ""));
                                }
                                this.cApi.BlockTextureAtlas.GetOrInsertTexture(temp2, out num, out shapeTexPos, null, 0f);
                            }
                        }
                        return shapeTexPos;
                    }
                    if (storedItemStack.Class == EnumItemClass.Item)
                    {
                        textureLoc = storedItemStack.Item.FirstTexture.Base;
                        TextureAtlasPosition tessaTexPos = this.cApi.BlockTextureAtlas[textureLoc];
                        if (tessaTexPos != null)
                        {
                            return tessaTexPos;
                        }
                        if (this.tesselatingTextureShape)
                        {
                            textureLoc = storedItemStack.Item.FirstTexture.Base;
                        }
                        if (textureLoc != null)
                        {
                            texAsset = this.cApi.Assets.TryGet(textureLoc.Clone().WithPathPrefixOnce("textures/").WithPathAppendixOnce(".png"), true);
                        }
                        if (texAsset != null)
                        {
                            AssetLocation temp3 = new AssetLocation();
                            if (texAsset.Location != null && texAsset.Location.FirstPathPart(0) == "textures")
                            {
                                temp3 = new AssetLocation(texAsset.Location.ToString().Replace("textures/", ""));
                            }
                            this.cApi.BlockTextureAtlas.GetOrInsertTexture(temp3, out num, out tessaTexPos, null, 0f);
                            return tessaTexPos;
                        }
                    }
                    return this.storedItemTextureSource[textureCode];
                }
            }
        }

        private MeshData GenMeshData(ITesselatorAPI tesselator)
        {
            ItemStack storedItemStack = this.GetStoredItemStack();
            if (storedItemStack == null)
            {
                return null;
            }
            this.tesselatingSpecial = false;
            this.tesselatingTextureShape = false;
            this.tesselatingModBlock = false;
            this.nowTesselatingShape = null;
            this.tmpTextureSource = null;
            this.glassTextureSource = null;
            Dictionary<string, MeshData> meshes = ObjectCacheUtil.GetOrCreate<Dictionary<string, MeshData>>(this.Api, "BetterCrateContainerMeshes", () => new Dictionary<string, MeshData>());
            string key = storedItemStack.GetName();
            if (storedItemStack.Class == EnumItemClass.Block)
            {
                key = key + "-" + this.GetStoredItemSlot().GetStackDescription(this.cApi.World, false);
            }
            MeshData meshData;
            if (meshes.TryGetValue(key, out meshData))
            {
                return meshData;
            }
            if (storedItemStack.Block != null && storedItemStack.Block is BlockShapeFromAttributes)
            {
                BlockShapeFromAttributes clutterBlock = storedItemStack.Block as BlockShapeFromAttributes;
                if (clutterBlock != null)
                {
                    IShapeTypeProps cprops = (clutterBlock != null) ? clutterBlock.GetTypeProps(storedItemStack.Attributes.GetString("type", null), storedItemStack.Clone(), null) : null;
                    if (cprops != null)
                    {
                        meshData = clutterBlock.GetOrCreateMesh(cprops, null, null).Clone().Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0f, 3.1415927f + cprops.Rotation.Y * 0.017453292f, 0f).Scale(new Vec3f(0.5f, 0.5f, 0.5f), -1f, 1f, 1f);
                    }
                    if (meshData != null)
                    {
                        return meshes[key] = meshData;
                    }
                }
            }
            if (storedItemStack.Collectible.Attributes != null && storedItemStack.Collectible.Attributes["wearableAttachment"].AsBool(false))
            {
                MeshData armorMeshData = this.GenArmorMesh(this.cApi, storedItemStack);
                if (armorMeshData != null)
                {
                    meshData = armorMeshData;
                    for (int i = 0; i < meshData.RenderPassCount; i++)
                    {
                        if (meshData.RenderPassesAndExtraBits[i] != 3)
                        {
                            meshData.RenderPassesAndExtraBits[i] = 1;
                        }
                    }
                    return meshes[key] = meshData;
                }
            }
            CompositeShape storedItemCompositeShape;
            if (storedItemStack.Class == EnumItemClass.Item)
            {
                this.storedItemTextureSource = this.cApi.Tesselator.GetTextureSource(storedItemStack.Item, false);
                if (storedItemStack.Item.Shape != null)
                {
                    if (storedItemStack.Item != null)
                    {
                        if (storedItemStack.Item.GetHeldItemName(storedItemStack) == "Rope")
                        {
                            goto IL_5D1;
                        }
                        try
                        {
                            this.cApi.Tesselator.TesselateItem(storedItemStack.Item, out meshData, this);
                        }
                        catch (Exception)
                        {
                            this.Api.World.Logger.Warning(storedItemStack.GetName() + " Item threw Exception! Shape.Base: " + storedItemStack.Item.Shape.Base.ToString());
                            try
                            {
                                this.cApi.Tesselator.TesselateItem(storedItemStack.Item, out meshData);
                            }
                            catch (Exception)
                            {
                                this.Api.World.Logger.Warning(storedItemStack.GetName() + " Item threw Exception again! Shape.Base: " + storedItemStack.Item.Shape.Base.ToString());
                                Shape shape = this.cApi.Assets.TryGet("game:shapes/block/basic/cube.json", true).ToObject<Shape>(null);
                                tesselator.TesselateShape("bettercrate content shape", shape, out meshData, this, null, 0, 0, 0, null, null);
                            }
                        }
                    }
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
                        if (storedItemStack.Collectible != null)
                        {
                            if (storedItemStack.Collectible.Code.ToString().EndsWith("quartz"))
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
                            if (storedItemStack.Collectible.Code.ToString().Contains("game:pounder-"))
                            {
                                meshData.Scale(new Vec3f(0.5f, 0.5f, 0.5f), 0.375f, 0.375f, 0.375f);
                                meshData.Translate(new Vec3f(0f, -0.5f, 0f));
                            }
                            if (storedItemStack.Collectible.Code.ToString().Contains("game:spear-"))
                            {
                                meshData.Scale(new Vec3f(0.5f, 0.5f, 0.5f), 0.4f, 0.4f, 0.4f);
                                meshData.Translate(new Vec3f(0.6f, -0.5f, 0f));
                                string temp = storedItemStack.Collectible.Code.ToString();
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
            IL_5D1:
                if (storedItemStack.Item.GetHeldItemName(storedItemStack) == "Rope")
                {
                    storedItemCompositeShape = null;
                }
                else
                {
                    storedItemCompositeShape = storedItemStack.Item.Shape;
                }
                if (storedItemCompositeShape == null)
                {
                    this.tesselatingTextureShape = true;
                    this.cApi.Tesselator.TesselateItem(storedItemStack.Item, out meshData, this);
                    if (meshData != null)
                    {
                        int clearFlags2 = -503318784;
                        for (int vertexNum2 = 0; vertexNum2 < meshData.GetVerticesCount(); vertexNum2++)
                        {
                            meshData.Flags[vertexNum2] &= clearFlags2;
                        }
                        for (int m = 0; m < meshData.RenderPassCount; m++)
                        {
                            if (meshData.RenderPassesAndExtraBits[m] != 3)
                            {
                                meshData.RenderPassesAndExtraBits[m] = 1;
                            }
                        }
                    }
                    if (storedItemStack.Item.GetType().ToString().Contains("ItemWorkItem"))
                    {
                        meshData.Scale(new Vec3f(0.5f, 0.5f, 0.5f), 0.3f, 0.3f, 0.3f);
                        for (int n = 0; n < meshData.RenderPassCount; n++)
                        {
                            meshData.RenderPassesAndExtraBits[n] = 0;
                        }
                    }
                    return meshes[key] = meshData;
                }
            }
            else
            {
                storedItemCompositeShape = storedItemStack.Block.ShapeInventory;
                this.storedItemTextureSource = this.cApi.Tesselator.GetTextureSource(storedItemStack.Block, 0, false);
                if (storedItemCompositeShape == null)
                {
                    if (this.CheckForChiseledBlock(this.cApi, storedItemStack, out meshData))
                    {
                        return meshData;
                    }
                    if (this.CheckForSpecials(storedItemStack, out meshData))
                    {
                        return meshes[key] = meshData;
                    }
                    meshData = this.cApi.TesselatorManager.GetDefaultBlockMesh(storedItemStack.Block).Clone();
                    if (meshData != null)
                    {
                        int clearFlags3 = -503318784;
                        for (int vertexNum3 = 0; vertexNum3 < meshData.GetVerticesCount(); vertexNum3++)
                        {
                            meshData.Flags[vertexNum3] &= clearFlags3;
                        }
                        if (storedItemStack.Block.BlockMaterial == EnumBlockMaterial.Plant && (storedItemStack.Block.Code.ToString().Contains("bush") || storedItemStack.Block.Code.ToString().Contains("sapling")))
                        {
                            for (int i2 = 0; i2 < meshData.ClimateColorMapIds.Length; i2++)
                            {
                                if (meshData.ClimateColorMapIds[i2] > 0)
                                {
                                    meshData.ClimateColorMapIds[i2] = 7;
                                }
                            }
                            for (int i3 = 0; i3 < meshData.SeasonColorMapIds.Length; i3++)
                            {
                                if (meshData.SeasonColorMapIds[i3] > 0)
                                {
                                    meshData.SeasonColorMapIds[i3] = 10;
                                }
                            }
                        }
                        for (int i4 = 0; i4 < meshData.RenderPassCount; i4++)
                        {
                            if (meshData.RenderPassesAndExtraBits[i4] != 3)
                            {
                                meshData.RenderPassesAndExtraBits[i4] = 1;
                            }
                        }
                        if (storedItemStack.Block.Code.ToString().Contains("game:door-"))
                        {
                            if (storedItemStack.Block.Code.ToString().Contains("1x3") || storedItemStack.Block.Code.ToString().Contains("2x2"))
                            {
                                meshData.Scale(new Vec3f(0.5f, 0.5f, 0.5f), 0.7f, 0.7f, 0.7f);
                                meshData.Translate(new Vec3f(0f, 0.25f, 0f));
                            }
                            else if (storedItemStack.Block.Code.ToString().Contains("2x4"))
                            {
                                meshData.Scale(new Vec3f(0.5f, 0.5f, 0.5f), 0.6f, 0.6f, 0.6f);
                                meshData.Translate(new Vec3f(0f, 0.7f, 0f));
                            }
                        }
                        return meshes[key] = meshData;
                    }
                }
            }
            List<IAsset> assets;
            if (storedItemCompositeShape.Base.Path.EndsWith("*"))
            {
                assets = this.Api.Assets.GetMany(storedItemCompositeShape.Base.Clone().WithPathPrefixOnce("shapes/").Path.Substring(0, storedItemCompositeShape.Base.Path.Length - 1), storedItemCompositeShape.Base.Domain, true);
            }
            else
            {
                assets = new List<IAsset>
                {
                    this.Api.Assets.TryGet(storedItemCompositeShape.Base.Clone().WithPathPrefixOnce("shapes/").WithPathAppendixOnce(".json"), true)
                };
            }
            if (assets != null && assets.Count > 0)
            {
                if (this.CheckForSpecialBlocks(storedItemStack, out meshData))
                {
                    return meshes[key] = meshData;
                }
                for (int i5 = 0; i5 < 1; i5++)
                {
                    Shape shape2 = assets[i5].ToObject<Shape>(null);
                    this.shapeTextures = shape2.Textures;
                    try
                    {
                        tesselator.TesselateShape("bettercrate content shape", shape2, out meshData, this, null, 0, 0, 0, null, null);
                    }
                    catch
                    {
                        try
                        {
                            tesselator.TesselateShape(storedItemStack.Collectible, shape2, out meshData, null, null, null);
                        }
                        catch
                        {
                            this.Api.World.Logger.Warning(storedItemStack.GetName() + " Block threw Exception! Shape.Base: " + storedItemStack.Block.Shape.Base.ToString());
                            shape2 = this.cApi.Assets.TryGet("game:shapes/block/basic/cube.json", true).ToObject<Shape>(null);
                            tesselator.TesselateShape("bettercrate content shape", shape2, out meshData, this, null, 0, 0, 0, null, null);
                        }
                    }
                    int clearFlags4 = -503318784;
                    for (int vertexNum4 = 0; vertexNum4 < meshData.GetVerticesCount(); vertexNum4++)
                    {
                        meshData.Flags[vertexNum4] &= clearFlags4;
                    }
                    for (int j2 = 0; j2 < meshData.RenderPassCount; j2++)
                    {
                        if (meshData.RenderPassesAndExtraBits[j2] != 3)
                        {
                            meshData.RenderPassesAndExtraBits[j2] = 1;
                        }
                    }
                }
            }
            else
            {
                this.Api.World.Logger.Error("BetterCrates: Content asset {0} not found,", new object[]
                {
                    storedItemCompositeShape.Base.WithPathPrefixOnce("shapes/").WithPathAppendixOnce(".json")
                });
            }
            if (storedItemStack.Collectible != null && storedItemStack.Collectible.Code.ToString().Contains("game:pulverizerframe-"))
            {
                meshData.Translate(new Vec3f(0f, -0.4f, 0f));
            }
            if (storedItemStack.Collectible != null && storedItemStack.Collectible.Code.ToString().Contains("game:wallpaper-"))
            {
                meshData.Translate(new Vec3f(0.45f, 0.45f, 0.2f));
                meshData.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), -1.55f, 0f, 0f);
            }
            return meshes[key] = meshData;
        }

        private bool CheckForChiseledBlock(ICoreClientAPI capi, ItemStack itemStack, out MeshData mesh)
        {
            mesh = null;
            if (itemStack.Class != EnumItemClass.Block)
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
                tree = new TreeAttribute();
            }
            int[] materials = BlockEntityMicroBlock.MaterialIdsFromAttributes(tree, capi.World);
            uint[] cuboids = null;
            IntArrayAttribute intArrayTemp = tree["cuboids"] as IntArrayAttribute;
            if (intArrayTemp != null)
            {
                cuboids = intArrayTemp.AsUint;
            }
            if (cuboids == null)
            {
                LongArrayAttribute longArrayTemp = tree["cuboids"] as LongArrayAttribute;
                if (longArrayTemp != null)
                {
                    cuboids = longArrayTemp.AsUint;
                }
            }
            List<uint> voxelCuboids;
            if (cuboids == null)
            {
                voxelCuboids = new List<uint>();
            }
            else
            {
                voxelCuboids = new List<uint>(cuboids);
            }
            int[] blockIds = null;
            BlockPos blockPos = null;
            mesh = BlockEntityMicroBlock.CreateMesh(capi, voxelCuboids, materials, blockIds, blockPos, null);
            if (mesh != null)
            {
                for (int vertexNum = 0; vertexNum < mesh.GetVerticesCount(); vertexNum++)
                {
                    mesh.Flags[vertexNum] &= -256;
                }
                for (int i = 0; i < mesh.RenderPassCount; i++)
                {
                    if (mesh.RenderPassesAndExtraBits[i] != 3)
                    {
                        mesh.RenderPassesAndExtraBits[i] = 1;
                    }
                }
                return true;
            }
            return false;
        }

        private MeshData GenArmorMesh(ICoreClientAPI capi, ItemStack itemstack)
        {
            JsonObject attrObj = itemstack.Collectible.Attributes;
            EntityProperties entityType = capi.World.GetEntityType(new AssetLocation("player"));
            Shape entityShape = entityType.Client.LoadedShape;
            AssetLocation shapePathForLogging = entityType.Client.Shape.Base;
            Shape newShape = new Shape
            {
                Elements = entityShape.CloneElements(),
                Animations = entityShape.Animations,
                AnimationsByCrc32 = entityShape.AnimationsByCrc32,
                //AttachmentPointsByCode = entityShape.AttachmentPointsByCode,
                JointsById = entityShape.JointsById,
                TextureWidth = entityShape.TextureWidth,
                TextureHeight = entityShape.TextureHeight,
                Textures = null
            };
            if (attrObj != null && attrObj["attachShape"].Exists)
            {
                return null;
            }
            if (itemstack.Class != EnumItemClass.Item)
            {
                return null;
            }
            CompositeShape compArmorShape = itemstack.Item.Shape;
            if (compArmorShape == null)
            {
                capi.World.Logger.Warning("Entity armor {0} {1} does not define a shape through either the shape property or the attachShape Attribute. Armor pieces will be invisible.", new object[]
                {
                    itemstack.Class,
                    itemstack.Collectible.Code
                });
                return null;
            }
            AssetLocation shapePath = compArmorShape.Base.CopyWithPath("shapes/" + compArmorShape.Base.Path + ".json");
            IAsset asset = capi.Assets.TryGet(shapePath, true);
            if (asset == null)
            {
                capi.World.Logger.Warning("Entity armor shape {0} defined in {1} {2} not found, was supposed to be at {3}. Armor piece will be invisible.", new object[]
                {
                    compArmorShape.Base,
                    itemstack.Class,
                    itemstack.Collectible.Code,
                    shapePath
                });
                return null;
            }
            Shape armorShape;
            try
            {
                armorShape = asset.ToObject<Shape>(null);
            }
            catch (Exception e)
            {
                capi.World.Logger.Warning("Exception thrown when trying to load entity armor shape {0} defined in {1} {2}. Armor piece will be invisible. Exception: {3}", new object[]
                {
                    compArmorShape.Base,
                    itemstack.Class,
                    itemstack.Collectible.Code,
                    e
                });
                return null;
            }
            newShape.Textures = armorShape.Textures;
            foreach (ShapeElement val in armorShape.Elements)
            {
                if (val.StepParentName != null)
                {
                    ShapeElement elem = newShape.GetElementByName(val.StepParentName, StringComparison.InvariantCultureIgnoreCase);
                    if (elem == null)
                    {
                        capi.World.Logger.Warning("Entity armor shape {0} defined in {1} {2} requires step parent element with name {3}, but no such element was found in shape {3}. Will not be visible.", new object[]
                        {
                            compArmorShape.Base,
                            itemstack.Class,
                            itemstack.Collectible.Code,
                            val.StepParentName,
                            shapePathForLogging
                        });
                    }
                    else if (elem.Children == null)
                    {
                        elem.Children = new ShapeElement[]
                        {
                            val
                        };
                    }
                    else
                    {
                        elem.Children = ArrayExtensions.Append<ShapeElement>(elem.Children, val);
                    }
                }
                else
                {
                    capi.World.Logger.Warning("Entity armor shape element {0} in shape {1} defined in {2} {3} did not define a step parent element. Will not be visible.", new object[]
                    {
                        val.Name,
                        compArmorShape.Base,
                        itemstack.Class,
                        itemstack.Collectible.Code
                    });
                }
            }
            MeshData meshData;
            capi.Tesselator.TesselateShapeWithJointIds("entity", newShape, out meshData, this, new Vec3f(), null, null);
            return meshData;
        }

        private bool CheckForSpecials(ItemStack storedItemStack, out MeshData meshData)
        {
            meshData = new MeshData(true);
            if (storedItemStack.Attributes == null)
            {
                return false;
            }
            string material = storedItemStack.Attributes.GetString("material", null);
            string lining = storedItemStack.Attributes.GetString("lining", null);
            string glass = storedItemStack.Attributes.GetString("glass", null);
            if (material == null || lining == null || glass == null)
            {
                return false;
            }
            this.tesselatingSpecial = true;
            this.tmpTextureSource = this.cApi.Tesselator.GetTextureSource(storedItemStack.Block, 0, false);
            Shape shape = this.cApi.Assets.TryGet("shapes/" + storedItemStack.Block.Shape.Base.Path + ".json", true).ToObject<Shape>(null);
            this.curMat = material;
            this.curLining = lining;
            Block glassBlock = this.cApi.World.GetBlock(new AssetLocation("glass-" + glass));
            this.glassTextureSource = this.cApi.Tesselator.GetTextureSource(glassBlock, 0, false);
            this.cApi.Tesselator.TesselateShape("BetterCrate-blocklantern", shape, out meshData, this, null, 0, 0, 0, null, null);
            if (meshData != null)
            {
                for (int vertexNum = 0; vertexNum < meshData.GetVerticesCount(); vertexNum++)
                {
                    meshData.Flags[vertexNum] &= -256;
                }
                for (int i = 0; i < meshData.RenderPassCount; i++)
                {
                    if (meshData.RenderPassesAndExtraBits[i] != 3)
                    {
                        meshData.RenderPassesAndExtraBits[i] = 1;
                    }
                }
                return true;
            }
            return false;
        }

        private bool CheckForSpecialBlocks(ItemStack storedItemStack, out MeshData meshData)
        {
            if (storedItemStack.Class != EnumItemClass.Block)
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
                string type = storedItemStack.Attributes.GetString("type", null);
                if (type != null)
                {
                    this.tesselatingModBlock = true;
                    this.tmpTextureSource = this.cApi.Tesselator.GetTextureSource(storedItemStack.Block, 0, false);
                    string shapename = storedItemStack.Block.Attributes["shape"][type].AsString(null);
                    if (shapename != null)
                    {
                        AssetLocation assetLoc;
                        if (shapename.StartsWith("game:"))
                        {
                            assetLoc = new AssetLocation(shapename).WithPathPrefixOnce("shapes/").WithPathAppendixOnce(".json");
                        }
                        else
                        {
                            assetLoc = new AssetLocation(storedItemStack.Block.Code.Domain, shapename).WithPathPrefixOnce("shapes/").WithPathAppendixOnce(".json");
                        }
                        if (assetLoc != null)
                        {
                            IAsset asset = this.Api.Assets.TryGet(assetLoc, true);
                            if (asset != null)
                            {
                                Shape shape = asset.ToObject<Shape>(null);
                                if (shape != null)
                                {
                                    this.shapeTextures = shape.Textures;
                                    this.cApi.Tesselator.TesselateShape("bettercrate content shape", shape, out meshData, this, null, 0, 0, 0, null, null);
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
                                        meshData.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0f, -1.5707964f, 0f);
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
    }
}
