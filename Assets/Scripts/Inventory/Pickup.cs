using UnityEngine;

public enum PickupKind { Coin, Consumable, Item }

public class Pickup : MonoBehaviour
{
    public PickupKind kind;
    public int amount = 1;

    [Header("Consumable or Item")]
    public string itemName;
    public Sprite icon;
    public ConsumableType consumable;
    public int durability = 3;

    [Header("Idle Animation")]
    public float spinSpeed = 90f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.15f;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
        transform.position = startPosition + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobHeight);
    }

    // Coins collect on touch, everything else waits for the pickup key in Inventory.
    private void OnTriggerEnter(Collider other)
    {
        if (kind != PickupKind.Coin || !other.CompareTag("Player"))
        {
            return;
        }

        Inventory inventory = other.GetComponent<Inventory>();
        if (inventory != null)
        {
            Collect(inventory);
        }
    }

    public InventorySlot ToSlot()
    {
        return new InventorySlot
        {
            itemName = itemName,
            icon = icon,
            isConsumable = kind == PickupKind.Consumable,
            consumable = consumable,
            count = kind == PickupKind.Consumable ? amount : 1,
            durability = durability,
            maxDurability = durability,
        };
    }

    public void Collect(Inventory inventory)
    {
        if (kind == PickupKind.Coin)
        {
            inventory.AddCoins(amount);
        }
        else if (!inventory.TryAdd(ToSlot()))
        {
            return;
        }
        Destroy(gameObject);
    }
}
