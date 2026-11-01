using System;
using UnityEngine;

public enum ConsumableType { JumpBoost }

public class InventorySlot
{
    public string itemName;
    public Sprite icon;
    public bool isConsumable;
    public ConsumableType consumable;
    public int count = 1;
    public int durability;
    public int maxDurability;
}

// Lives on the player, so everything here is kept through death and respawn.
[RequireComponent(typeof(JumpBoost))]
public class Inventory : MonoBehaviour
{
    public const int SlotCount = 5;

    public int coins;
    public float coinLossOnDeath = 0.25f;
    public KeyCode useKey = KeyCode.Q;
    public KeyCode pickupKey = KeyCode.E;
    public float pickupRange = 1.5f;

    public event Action OnChanged;

    public int SelectedSlot { get; private set; }

    private readonly InventorySlot[] slots = new InventorySlot[SlotCount];
    private PenguinMenuController menu;

    private void Start()
    {
        menu = FindFirstObjectByType<PenguinMenuController>();
    }

    private void OnEnable()
    {
        GetComponent<PlayerController>().OnPlayerDeath += LoseCoins;
    }

    private void OnDisable()
    {
        GetComponent<PlayerController>().OnPlayerDeath -= LoseCoins;
    }

    private void Update()
    {
        if (menu != null && !menu.IsGameplayActive)
        {
            return;
        }

        for (int i = 0; i < SlotCount; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i);
            }
        }

        if (Input.GetKeyDown(useKey))
        {
            UseSelectedSlot();
        }

        if (Input.GetKeyDown(pickupKey))
        {
            CollectNearbyPickup();
        }
    }

    public InventorySlot GetSlot(int index)
    {
        return slots[index];
    }

    public void SelectSlot(int index)
    {
        SelectedSlot = index;
        OnChanged?.Invoke();
    }

    // Checked live because respawn teleports skip OnTriggerExit, tracked lists go stale.
    public Pickup FindNearbyPickup()
    {
        foreach (Collider hit in Physics.OverlapSphere(transform.position, pickupRange, ~0, QueryTriggerInteraction.Collide))
        {
            Pickup pickup = hit.GetComponent<Pickup>();
            if (pickup != null && pickup.kind != PickupKind.Coin)
            {
                return pickup;
            }
        }
        return null;
    }

    public void CollectNearbyPickup()
    {
        Pickup pickup = FindNearbyPickup();
        if (pickup != null)
        {
            pickup.Collect(this);
        }
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        OnChanged?.Invoke();
    }

    public void LoseCoins()
    {
        coins -= Mathf.FloorToInt(coins * coinLossOnDeath);
        OnChanged?.Invoke();
    }

    public bool HasRoomFor(InventorySlot incoming)
    {
        return FindSlotFor(incoming) >= 0;
    }

    // Returns false when the slots are full, so the pickup stays in the world.
    public bool TryAdd(InventorySlot incoming)
    {
        int index = FindSlotFor(incoming);
        if (index < 0)
        {
            return false;
        }

        if (slots[index] == null)
        {
            slots[index] = incoming;
        }
        else
        {
            slots[index].count += incoming.count;
        }
        OnChanged?.Invoke();
        return true;
    }

    // Consumables of the same type stack, items always take their own slot.
    private int FindSlotFor(InventorySlot incoming)
    {
        int firstEmpty = -1;
        for (int i = 0; i < SlotCount; i++)
        {
            InventorySlot slot = slots[i];
            if (slot == null)
            {
                if (firstEmpty < 0) firstEmpty = i;
            }
            else if (incoming.isConsumable && slot.isConsumable && slot.consumable == incoming.consumable)
            {
                return i;
            }
        }
        return firstEmpty;
    }

    public bool UseSelectedSlot()
    {
        InventorySlot slot = slots[SelectedSlot];
        if (slot == null)
        {
            return false;
        }

        if (slot.isConsumable)
        {
            ApplyConsumable(slot.consumable);
            slot.count--;
        }
        else
        {
            // shortcut: items only lose durability for now, add effects here once items are designed
            slot.durability--;
        }

        if (slot.count <= 0 || (!slot.isConsumable && slot.durability <= 0))
        {
            slots[SelectedSlot] = null;
        }
        OnChanged?.Invoke();
        return true;
    }

    private void ApplyConsumable(ConsumableType type)
    {
        switch (type)
        {
            case ConsumableType.JumpBoost:
                GetComponent<JumpBoost>().Activate();
                break;
        }
    }
}
