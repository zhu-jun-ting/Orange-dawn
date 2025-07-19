
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using DG.Tweening;

public class CanvasManager : MonoBehaviour, ICanvasManager {
	private static CanvasManager s_instance;
	[SerializeField] private View[] _views;

	[Serializable]
	public class KeyValuePair {
		public string key;
		public GameObject val;
	}

	[SerializeField] private List<KeyValuePair> popupList = new List<KeyValuePair>();
	public Dictionary<string, GameObject> popupAssets = new Dictionary<string, GameObject>();

	// public GameObject damagePrefab;
	public Canvas canvas;

	[Header("Message UI")]
	public Transform messageEntryParent;
	public GameObject messageEntryFullInfo;
	public GameObject messageEntryFullWarning;
	public GameObject messageEntryLocalInfo;
	public Transform popupParent; // Parent for popups
	public float fadeTime = 0.5f; // fading in out time
	public float showDuration = 1f; // how long this message is shown before fading out

	[Header("Tips UI")]
	public GameObject tipsPrefab;


	[Header("Board/Hand Panel Animation")]
	public Transform boardArea;
	public Transform handArea;
	public Transform boardAnchorOutside;
	public Transform handAnchorOutside;
	private bool panelsVisible = false;
	private Vector3 boardAreaInPos;
	private Vector3 handAreaInPos;
	


	void Awake()
	{
		s_instance = this;
	}

	// Handler for OnShowMessage event
	void Start()
	{
		if (GameEvents.instance != null)
		{
			GameEvents.instance.onShowNumberUI += DisplayDamage;
			GameEvents.instance.onShowStringUI += DisplayString;
			GameEvents.instance.OnShowMessage += HandleShowMessage;
		}

		// Store the in-view positions for animation
		if (boardArea != null)
			boardAreaInPos = boardArea.position;
		if (handArea != null)
			handAreaInPos = handArea.position;

		// Move panels to outside anchors and hide at start
		if (boardArea != null && boardAnchorOutside != null)
			boardArea.position = boardAnchorOutside.position;
		if (handArea != null && handAnchorOutside != null)
			handArea.position = handAnchorOutside.position;
		panelsVisible = false;

		// Register tab toggle event
		if (InputManager.Instance != null)
			InputManager.Instance.OnTabKeyPressed += TogglePanels;

		for (int i = 0; i < _views.Length; i++) {
			_views[i].Initialize();
		}

		foreach (var kvp in popupList) {
			popupAssets[kvp.key] = kvp.val;
		}
	}

	void OnDisable()
	{
		if (GameEvents.instance != null)
		{
			GameEvents.instance.onShowNumberUI -= DisplayDamage;
			GameEvents.instance.onShowStringUI -= DisplayString;
			GameEvents.instance.OnShowMessage -= HandleShowMessage;
		}
		if (InputManager.Instance != null)
			InputManager.Instance.OnTabKeyPressed -= TogglePanels;
	}
	


	// --- Tips System ---
	private static GameObject tipsLayoutInstance;
	private static RectTransform tipsLayoutRect;
	private static List<TipEntry> activeTips = new List<TipEntry>();
	private static Vector2 tipsOffset = new Vector2(32, -32); // Offset from lower right of cursor
	public static void ShowTip(string name, string description, float width = 60f, float spacing = 4f)
	{
		if (s_instance == null || s_instance.tipsPrefab == null || s_instance.canvas == null) return;

		// Create layout if not present
		if (tipsLayoutInstance == null)
		{
			tipsLayoutInstance = new GameObject("TipsLayout", typeof(RectTransform), typeof(UnityEngine.UI.VerticalLayoutGroup), typeof(CanvasGroup));
			tipsLayoutRect = tipsLayoutInstance.GetComponent<RectTransform>();
			tipsLayoutRect.SetParent(s_instance.canvas.transform, false);
			tipsLayoutRect.anchorMin = new Vector2(0, 1);
			tipsLayoutRect.anchorMax = new Vector2(0, 1);
			tipsLayoutRect.pivot = new Vector2(0, 1);
			tipsLayoutRect.sizeDelta = new Vector2(width, 0);
			var layout = tipsLayoutInstance.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
			layout.childForceExpandHeight = false;
			layout.childForceExpandWidth = true;
			layout.childAlignment = TextAnchor.UpperLeft;
			layout.spacing = spacing;
			// Start following mouse
			s_instance.StartCoroutine(FollowCursorRoutine());
		}
		else
		{
			// Update width and spacing if already present
			tipsLayoutRect.sizeDelta = new Vector2(width, 0);
			var layout = tipsLayoutInstance.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
			if (layout != null) layout.spacing = spacing;
		}

		// Instantiate tip
		GameObject tipGO = UnityEngine.Object.Instantiate(s_instance.tipsPrefab, tipsLayoutRect);
		var tipEntry = tipGO.GetComponent<TipEntry>();
		if (tipEntry != null)
		{
			tipEntry.SetTipName(name);
			tipEntry.SetTipDescription(description);
			activeTips.Add(tipEntry);
			// Remove from list when destroyed
			tipGO.AddComponent<TipAutoRemove>().Init(() => activeTips.Remove(tipEntry));

			// Fade in using DOTween (override any existing alpha)
			var cg = tipGO.GetComponent<CanvasGroup>();
			if (cg == null) cg = tipGO.AddComponent<CanvasGroup>();
			cg.alpha = 0f;
			float fadeDuration = tipEntry.fadeDuration > 0f ? tipEntry.fadeDuration : 0.3f;
			cg.DOFade(1f, fadeDuration);
		}
	}

	/// <summary>
	/// Fades out and destroys all active tip entries and the tips layout.
	/// </summary>
	public static void HideTip(float fadeOutDuration = 0.3f)
	{
		// Fade out all tips
		foreach (var tip in activeTips)
		{
			if (tip != null)
			{
				var cg = tip.GetComponent<CanvasGroup>();
				if (cg == null) cg = tip.gameObject.AddComponent<CanvasGroup>();
				cg.DOFade(0f, fadeOutDuration).OnComplete(() => {
					if (tip != null) Destroy(tip.gameObject);
				});
			}
		}
		activeTips.Clear();

		// Fade out and destroy the layout
		if (tipsLayoutInstance != null)
		{
			var cg = tipsLayoutInstance.GetComponent<CanvasGroup>();
			if (cg == null) cg = tipsLayoutInstance.AddComponent<CanvasGroup>();
			cg.DOFade(0f, fadeOutDuration).OnComplete(() => {
				if (tipsLayoutInstance != null)
				{
					Destroy(tipsLayoutInstance);
					tipsLayoutInstance = null;
					tipsLayoutRect = null;
				}
			});
		}
	}

	private static System.Collections.IEnumerator FollowCursorRoutine()
	{
		while (tipsLayoutInstance != null && tipsLayoutRect != null)
		{
			Vector2 mousePos = Input.mousePosition;
			// Place at lower right of cursor
			tipsLayoutRect.position = mousePos + tipsOffset;
			yield return null;
		}
	}

	// Helper to remove from list when destroyed
	private class TipAutoRemove : MonoBehaviour
	{
		private System.Action onDestroy;
		public void Init(System.Action onDestroy)
		{
			this.onDestroy = onDestroy;
		}
		private void OnDestroy()
		{
			onDestroy?.Invoke();
			// If no more tips, destroy layout
			if (activeTips.Count == 0 && tipsLayoutInstance != null)
			{
				Destroy(tipsLayoutInstance);
				tipsLayoutInstance = null;
				tipsLayoutRect = null;
			}
		}
	}

	private void TogglePanels()
	{
		if (panelsVisible)
		{
			// Move panels out and pause game logic (but not UI)
			DOTween.defaultTimeScaleIndependent = true;
			if (boardArea != null && boardAnchorOutside != null)
				boardArea.DOMove(boardAnchorOutside.position, 0.5f).SetEase(Ease.InOutBack).SetUpdate(true);
			if (handArea != null && handAnchorOutside != null)
				handArea.DOMove(handAnchorOutside.position, 0.5f).SetEase(Ease.InOutBack).SetUpdate(true);
			panelsVisible = false;
			DOTween.defaultTimeScaleIndependent = false;

			if (GameEvents.instance != null) GameEvents.instance.ToggleBoard(false);
		}
		else
		{
			// Move panels in and unpause
			DOTween.defaultTimeScaleIndependent = true;
			if (boardArea != null)
				boardArea.DOMove(boardAreaInPos, 0.5f).SetEase(Ease.InOutBack).SetUpdate(true);
			if (handArea != null)
				handArea.DOMove(handAreaInPos, 0.5f).SetEase(Ease.InOutBack).SetUpdate(true);
			panelsVisible = true;
			DOTween.defaultTimeScaleIndependent = false;
			
			if (GameEvents.instance != null) GameEvents.instance.ToggleBoard(true);
			
		}
	}

	// Pauses gameplay but not UI/tweens
	private void PauseGameOnly()
	{
		// Set timeScale to 0 for gameplay, but keep UI running
		Time.timeScale = 0f;
		// Optionally, pause other game systems here if needed
	}

	private void ResumeGameOnly()
	{
		Time.timeScale = 1f;
		// Optionally, resume other game systems here if needed
	}

	private void HandleShowMessage(string message, GameEvents.MessageType type, Vector2 position)
	{
		if (messageEntryParent == null) return;

		if (type == GameEvents.MessageType.FullInfo || type == GameEvents.MessageType.FullWarning)
		{
			// Choose prefab based on type
			GameObject prefab = null;
			switch (type)
			{
				case GameEvents.MessageType.FullInfo:
					prefab = messageEntryFullInfo;
					break;
				case GameEvents.MessageType.FullWarning:
					prefab = messageEntryFullWarning;
					break;
				default:
					prefab = messageEntryFullInfo;
					break;
			}
			if (prefab == null) return;

			// Activate parent if not active
			if (!messageEntryParent.gameObject.activeSelf)
				messageEntryParent.gameObject.SetActive(true);

			// Ensure parent is fully opaque
			var parentCanvasGroup = messageEntryParent.GetComponent<CanvasGroup>();
			if (parentCanvasGroup != null && parentCanvasGroup.alpha < 1f)
				parentCanvasGroup.alpha = 1f;

			// Instantiate message entry
			GameObject entry = Instantiate(prefab, messageEntryParent);
			var uiMsg = entry.GetComponent<UIMessageFull>();
			if (uiMsg != null)
			{
				uiMsg.SetText(message);
				uiMsg.SetDurationAndFade(showDuration, fadeTime);
			}
		}
		else if (type == GameEvents.MessageType.LocalInfo)
		{
			if (messageEntryLocalInfo == null || popupParent == null) return;
			GameObject entry = Instantiate(messageEntryLocalInfo, popupParent);
			var uiMsg = entry.GetComponent<UIMessageLocal>();
			if (uiMsg != null)
			{
				uiMsg.SetText(message);
			}
			// Set position in screen space
			RectTransform entryRect = entry.transform as RectTransform;
			if (entryRect != null)
			{
				// Convert screen position (Vector2) to local position in popupParent's RectTransform
				Vector2 localPos;
				RectTransform parentRect = popupParent as RectTransform;
				if (parentRect != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, position, null, out localPos))
				{
					entryRect.anchoredPosition = localPos;
				}
				else
				{
					entryRect.anchoredPosition = position;
				}
			}
		}
	}





	public static T GetView<T>() where T : View
	{
		for (int i = 0; i < s_instance._views.Length; i++)
		{

			if (s_instance._views[i] is T tView) { return tView; }
		}

		return null;
	}

	public static void Show<T>() where T : View {
		for (int i = 0; i < s_instance._views.Length; i++) {
			if (s_instance._views[i] is T) {

				s_instance._views[i].Show();
			}
		}
	}

	public static void Hide<T>() where T : View {
		for (int i = 0; i < s_instance._views.Length; i++) {
			if (s_instance._views[i] is T) {

				s_instance._views[i].Hide();
			}
		}
	}

	void Update() { 
		for (int i = 0; i < _views.Length; i++) {
			_views[i].Update(); 
		} 
	}

	public void DisplayString(string damage_, PawnMaster reciever_, GameEvents.DamageType damage_type_, Vector2 location_, string prefix = "")
	{
		GameObject popupPrefab = null;

		// Check if receiver is player
		if (true)
		{
			switch (damage_type_)
			{
				case GameEvents.DamageType.Normal:
					popupPrefab = popupAssets["Damage"];
					break;
				case GameEvents.DamageType.Crit:
					popupPrefab = popupAssets["Crit"];
					break;
				case GameEvents.DamageType.Heal:
					popupPrefab = popupAssets["Heal"];
					break;
				case GameEvents.DamageType.Aoe:
					popupPrefab = popupAssets["Damage"];
					break;
				default:
					break;
			}
		}

		if (popupPrefab != null)
		{
			GameObject damageDisplay = Instantiate(popupPrefab, location_, Quaternion.identity);
			damageDisplay.GetComponent<TextMeshPro>().text = damage_;
			if (damageDisplay.transform.childCount > 0)
			{
				var childTMP = damageDisplay.transform.GetChild(0).GetComponent<TextMeshPro>();
				if (childTMP != null)
					childTMP.text = prefix;
			}

			var seq = DOTween.Sequence();
			seq.Append(damageDisplay.transform.DOJump(location_ + new Vector2(UnityEngine.Random.Range(-0.5f, 0.5f), 0), 0.3f, 3, 1.5f));

			// Set lifetime of damage popup
			Destroy(damageDisplay, 1.5f);
		}
	}

	// Receive damage number and location
	// TODO: implement HEAL and CRIT UI
	public void DisplayDamage(int damage_, PawnMaster reciever_, GameEvents.DamageType damage_type_, Vector2 location_, string prefix = "")
	{
		DisplayString(damage_.ToString(), reciever_, damage_type_, location_, prefix);
	}

	public void UpdateKillCount(int kill_count_) {
		
	}


	void OnDestroy()
	{
		// deregister all events
		GameEvents.instance.onShowNumberUI -= DisplayDamage;
	}
}
