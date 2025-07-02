
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
			GameEvents.instance.OnShowMessage -= HandleShowMessage;
		if (InputManager.Instance != null)
			InputManager.Instance.OnTabKeyPressed -= TogglePanels;
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
			// Pause only gameplay, not UI
			// ResumeGameOnly();
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
			// PauseGameOnly();
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

	// Receive damage number and location
	// TODO: implement HEAL and CRIT UI
	public void DisplayDamage( int damage_, PawnMaster reciever_, GameEvents.DamageType damage_type_, Vector2 location_, string prefix = "" ) {
		GameObject popupPrefab = null;

		// Check if receiver is player
		if ( true )  {
			switch( damage_type_ ) {
				case GameEvents.DamageType.Normal:
					popupPrefab = popupAssets[ "Damage" ];
					break;
				case GameEvents.DamageType.Crit:
					popupPrefab = popupAssets[ "Crit" ];
					break;
				case GameEvents.DamageType.Heal:
					popupPrefab = popupAssets[ "Heal" ];
					break;
				case GameEvents.DamageType.Aoe:
					popupPrefab = popupAssets[ "Damage" ];
					break;
				default:
					break;
			}	
		}

		if ( popupPrefab != null ) {
			GameObject damageDisplay = Instantiate( popupPrefab, location_, Quaternion.identity );
			damageDisplay.GetComponent<TextMeshPro>().text = damage_.ToString();
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

	public void UpdateKillCount(int kill_count_) {
		
	}


	void OnDestroy()
	{
		// deregister all events
		GameEvents.instance.onShowNumberUI -= DisplayDamage;
	}
}
