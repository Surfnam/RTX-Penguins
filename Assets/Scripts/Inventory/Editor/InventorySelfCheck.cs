using UnityEditor;
using UnityEngine;

// Run from Tools > Inventory Self Check, or in batch mode with -executeMethod InventorySelfCheck.Run
public static class InventorySelfCheck
{
    [MenuItem("Tools/Inventory Self Check")]
    public static void Run()
    {
        GameObject playerObject = new GameObject("InventoryCheckPlayer");
        try
        {
            PlayerController player = playerObject.AddComponent<PlayerController>();
            Inventory inventory = playerObject.AddComponent<Inventory>();
            JumpBoost boost = playerObject.GetComponent<JumpBoost>();
            float normalJump = player.baseJumpHeight;

            Check(!inventory.UseSelectedSlot(), "empty slot does nothing");
            Check(player.baseJumpHeight == normalJump, "empty slot leaves jump alone");

            Check(inventory.TryAdd(Potion(2)), "potion fits");
            Check(inventory.TryAdd(Potion(1)), "second potion fits");
            Check(inventory.GetSlot(0).count == 3 && inventory.GetSlot(1) == null, "potions stack in one slot");

            Check(inventory.UseSelectedSlot(), "drinks a potion");
            Check(inventory.GetSlot(0).count == 2, "potion count drops");
            Check(Mathf.Approximately(player.baseJumpHeight, normalJump * boost.jumpMultiplier), "jump is boosted");

            boost.Tick(boost.duration - 1f);
            inventory.UseSelectedSlot();
            Check(Mathf.Approximately(player.baseJumpHeight, normalJump * boost.jumpMultiplier), "second potion does not stack");
            Check(boost.TimeLeft == boost.duration, "second potion restarts the timer");

            boost.Tick(boost.duration + 1f);
            Check(player.baseJumpHeight == normalJump, "jump returns to normal");
            Check(boost.TimeLeft == 0f, "timer ends at zero");

            inventory.UseSelectedSlot();
            Check(inventory.GetSlot(0) == null, "last potion empties its slot");
            boost.Cancel();
            Check(player.baseJumpHeight == normalJump, "death cancels the boost");

            Check(inventory.TryAdd(Item(2)), "item fits");
            Check(inventory.GetSlot(0) != null && !inventory.GetSlot(0).isConsumable, "item takes first empty slot");
            inventory.UseSelectedSlot();
            Check(inventory.GetSlot(0).durability == 1, "using an item costs durability");
            inventory.UseSelectedSlot();
            Check(inventory.GetSlot(0) == null, "broken item empties its slot");

            for (int i = 0; i < Inventory.SlotCount; i++)
            {
                Check(inventory.TryAdd(Item(3)), "items fill every slot");
            }
            Check(!inventory.TryAdd(Item(3)), "full inventory refuses an item");
            Check(!inventory.TryAdd(Potion(1)), "full inventory refuses a new potion");

            inventory.SelectSlot(4);
            Check(inventory.SelectedSlot == 4, "selects a slot");

            inventory.AddCoins(10);
            inventory.LoseCoins();
            Check(inventory.coins == 8, "death takes a quarter of the coins, rounded down");
            Check(inventory.GetSlot(0) != null, "death keeps items");

            Debug.Log("Inventory self check passed");
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"Inventory self check FAILED: {exception.Message}");
            if (Application.isBatchMode) EditorApplication.Exit(1);
        }
        finally
        {
            Object.DestroyImmediate(playerObject);
        }
    }

    private static InventorySlot Potion(int count)
    {
        return new InventorySlot { itemName = "Jump Potion", isConsumable = true, consumable = ConsumableType.JumpBoost, count = count };
    }

    private static InventorySlot Item(int durability)
    {
        return new InventorySlot { itemName = "Test Item", durability = durability, maxDurability = durability };
    }

    private static void Check(bool condition, string description)
    {
        if (!condition) throw new System.Exception(description);
    }
}
