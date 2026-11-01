using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Builds its own Canvas at runtime, so adding this to the player is the only setup.
[RequireComponent(typeof(Inventory))]
public class InventoryHUD : MonoBehaviour
{
    private const float SlotSize = 72f;
    private const float SlotGap = 8f;
    private const float BorderWidth = 3f;
    private const float PopScale = 1.3f;
    private const float PopTime = 0.25f;

    private static readonly Color PanelColor = Hex(0x0E1626);
    private static readonly Color FilledColor = Hex(0x16263D);
    private static readonly Color EmptyColor = Hex(0x111C2E);
    private static readonly Color LineColor = Hex(0x3A5A7A);
    private static readonly Color SelectedColor = Hex(0xF5C542);
    private static readonly Color BoostColor = Hex(0x4FA8F0);
    private static readonly Color DurabilityColor = Hex(0x7BD88F);
    private static readonly Color TextColor = Hex(0xEAF6FC);
    private static readonly Color HintColor = Hex(0xBFE3F5);

    private class SlotView
    {
        public RectTransform root;
        public Image border;
        public Image background;
        public Image icon;
        public Text count;
        public GameObject durabilityBar;
        public RectTransform durabilityFill;
        public InventorySlot shown;
    }

    private Inventory inventory;
    private JumpBoost jumpBoost;
    private Font font;
    private Transform canvas;
    private readonly SlotView[] slotViews = new SlotView[Inventory.SlotCount];
    private Text coinsText;
    private GameObject boostBar;
    private RectTransform boostFill;
    private Text boostTime;
    private RectTransform prompt;
    private GameObject promptChip;
    private Text promptKey;
    private Text promptLabel;

    private void Awake()
    {
        inventory = GetComponent<Inventory>();
        jumpBoost = GetComponent<JumpBoost>();
        font = Resources.Load<Font>("Inventory/Silkscreen");
        BuildCanvas();
    }

    private void OnEnable()
    {
        inventory.OnChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        inventory.OnChanged -= Refresh;
    }

    private void Update()
    {
        UpdateBoost();
        UpdatePrompt();
    }

    private void Refresh()
    {
        coinsText.text = inventory.coins.ToString();

        for (int i = 0; i < slotViews.Length; i++)
        {
            SlotView view = slotViews[i];
            InventorySlot slot = inventory.GetSlot(i);
            bool isNew = slot != null && view.shown != slot;
            view.shown = slot;

            view.background.color = slot != null ? FilledColor : EmptyColor;
            view.border.color = i == inventory.SelectedSlot ? SelectedColor : LineColor;
            view.icon.enabled = slot != null;
            view.icon.sprite = slot?.icon;
            view.count.text = slot != null && slot.isConsumable ? slot.count.ToString() : "";

            bool showDurability = slot != null && !slot.isConsumable;
            view.durabilityBar.SetActive(showDurability);
            if (showDurability)
            {
                view.durabilityFill.anchorMax = new Vector2((float)slot.durability / slot.maxDurability, 1f);
            }

            if (isNew)
            {
                StartCoroutine(Pop(view.root));
            }
        }
    }

    private IEnumerator Pop(RectTransform target)
    {
        for (float time = 0f; time < PopTime; time += Time.unscaledDeltaTime)
        {
            target.localScale = Vector3.one * Mathf.Lerp(PopScale, 1f, time / PopTime);
            yield return null;
        }
        target.localScale = Vector3.one;
    }

    private void UpdateBoost()
    {
        bool boosting = jumpBoost.TimeLeft > 0f;
        boostBar.SetActive(boosting);
        if (boosting)
        {
            boostFill.anchorMax = new Vector2(jumpBoost.TimeLeft / jumpBoost.duration, 1f);
            boostTime.text = $"{Mathf.CeilToInt(jumpBoost.TimeLeft)}s";
        }
    }

    private void UpdatePrompt()
    {
        Pickup pickup = inventory.FindNearbyPickup();
        Camera view = Camera.main;
        Vector3 screen = pickup != null && view != null ? view.WorldToScreenPoint(pickup.transform.position + Vector3.up) : Vector3.back;

        prompt.gameObject.SetActive(screen.z > 0f);
        if (screen.z > 0f)
        {
            prompt.position = screen;
            bool hasRoom = inventory.HasRoomFor(pickup.ToSlot());
            promptChip.SetActive(hasRoom);
            promptKey.text = inventory.pickupKey.ToString();
            promptLabel.text = hasRoom ? "Pick up" : "Inventory full";
        }
    }

    private void BuildCanvas()
    {
        GameObject canvasObject = new GameObject("InventoryHUD", typeof(Canvas), typeof(CanvasScaler));
        canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(960f, 540f);
        scaler.matchWidthOrHeight = 0.5f;
        canvas = canvasObject.transform;

        float rowWidth = Inventory.SlotCount * SlotSize + (Inventory.SlotCount - 1) * SlotGap;
        for (int i = 0; i < Inventory.SlotCount; i++)
        {
            slotViews[i] = BuildSlot(i, (SlotSize - rowWidth) / 2f + i * (SlotSize + SlotGap));
        }

        BuildCoins();
        BuildBoostBar();
        BuildPrompt();

        Text hint = MakeText("Hint", canvas, 16, HintColor, TextAnchor.LowerLeft);
        Place(hint.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(24f, 22f), new Vector2(200f, 44f));
        hint.text = $"1 to {Inventory.SlotCount} select\n{inventory.useKey} use";
    }

    private SlotView BuildSlot(int index, float x)
    {
        SlotView view = new SlotView();

        view.border = MakeImage("Slot", canvas, LineColor);
        view.root = view.border.rectTransform;
        Place(view.root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(x, 22f), new Vector2(SlotSize, SlotSize));

        view.background = MakeImage("Background", view.root, EmptyColor);
        Fill(view.background.rectTransform, BorderWidth);

        view.icon = MakeImage("Icon", view.root, Color.white);
        Place(view.icon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 2f), new Vector2(48f, 48f));
        view.icon.enabled = false;

        Text key = MakeText("Key", view.root, 16, HintColor, TextAnchor.UpperLeft);
        Place(key.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(8f, -4f), new Vector2(20f, 18f));
        key.text = (index + 1).ToString();

        view.count = MakeText("Count", view.root, 20, TextColor, TextAnchor.LowerRight);
        Place(view.count.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-8f, 3f), new Vector2(40f, 24f));

        Image durabilityBack = MakeImage("Durability", view.root, PanelColor);
        Place(durabilityBack.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 8f), new Vector2(52f, 6f));
        view.durabilityBar = durabilityBack.gameObject;

        Image durabilityFill = MakeImage("Fill", durabilityBack.rectTransform, DurabilityColor);
        Fill(durabilityFill.rectTransform, 0f);
        view.durabilityFill = durabilityFill.rectTransform;

        return view;
    }

    private void BuildCoins()
    {
        Image border = MakeImage("Coins", canvas, LineColor);
        Place(border.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -20f), new Vector2(104f, 44f));

        Image background = MakeImage("Background", border.rectTransform, PanelColor);
        Fill(background.rectTransform, 2f);

        Image icon = MakeImage("Icon", border.rectTransform, Color.white);
        icon.sprite = Resources.Load<Sprite>("Inventory/CoinIcon");
        Place(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(12f, 0f), new Vector2(28f, 28f));

        coinsText = MakeText("Amount", border.rectTransform, 24, TextColor, TextAnchor.MiddleLeft);
        Place(coinsText.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(46f, 0f), new Vector2(54f, 30f));
    }

    private void BuildBoostBar()
    {
        GameObject root = new GameObject("Boost", typeof(RectTransform));
        root.transform.SetParent(canvas, false);
        RectTransform rootRect = (RectTransform)root.transform;
        Place(rootRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 112f), new Vector2(290f, 20f));
        boostBar = root;

        Text label = MakeText("Label", rootRect, 16, TextColor, TextAnchor.MiddleLeft);
        Place(label.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(70f, 20f));
        label.text = "Boost";

        Image border = MakeImage("Bar", rootRect, LineColor);
        Place(border.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(4f, 0f), new Vector2(140f, 14f));

        Image background = MakeImage("Background", border.rectTransform, PanelColor);
        Fill(background.rectTransform, 2f);

        Image fill = MakeImage("Fill", background.rectTransform, BoostColor);
        Fill(fill.rectTransform, 0f);
        boostFill = fill.rectTransform;

        boostTime = MakeText("Time", rootRect, 16, TextColor, TextAnchor.MiddleRight);
        Place(boostTime.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), Vector2.zero, new Vector2(30f, 20f));
    }

    private void BuildPrompt()
    {
        Image border = MakeImage("Prompt", canvas, HintColor);
        prompt = border.rectTransform;
        Place(prompt, new Vector2(0f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(176f, 32f));

        Image background = MakeImage("Background", prompt, PanelColor);
        Fill(background.rectTransform, 2f);

        Image chip = MakeImage("KeyChip", prompt, SelectedColor);
        Place(chip.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(8f, 0f), new Vector2(20f, 20f));
        promptChip = chip.gameObject;

        promptKey = MakeText("Key", chip.rectTransform, 16, PanelColor, TextAnchor.MiddleCenter);
        Fill(promptKey.rectTransform, 0f);

        promptLabel = MakeText("Label", prompt, 16, TextColor, TextAnchor.MiddleCenter);
        Place(promptLabel.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(32f, 0f), new Vector2(140f, 24f));

        prompt.gameObject.SetActive(false);
    }

    private Image MakeImage(string objectName, Transform parent, Color color)
    {
        GameObject imageObject = new GameObject(objectName, typeof(Image));
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private Text MakeText(string objectName, Transform parent, int size, Color color, TextAnchor alignment)
    {
        GameObject textObject = new GameObject(objectName, typeof(Text));
        textObject.transform.SetParent(parent, false);
        Text text = textObject.GetComponent<Text>();
        text.font = font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    private static void Place(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void Fill(RectTransform rect, float inset)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.one * inset;
        rect.offsetMax = -Vector2.one * inset;
    }

    private static Color Hex(int rgb)
    {
        return new Color(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f);
    }
}
