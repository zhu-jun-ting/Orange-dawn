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

	public static T GetView<T>() where T : View {
		for (int i = 0; i < s_instance._views.Length; i++) {

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

	void Awake() => s_instance = this;

	void Start() {
        for (int i = 0; i < _views.Length; i++) {
			_views[i].Initialize();
		}

		// register all events handlers
        GameEvents.instance.onShowNumberUI += DisplayDamage;

		foreach (var kvp in popupList) {
    		popupAssets[kvp.key] = kvp.val;
			// Debug.Log( kvp.key );
			// Debug.Log( kvp.val );
  		}
	}

	void Update() { 
		for (int i = 0; i < _views.Length; i++) {
			_views[i].Update(); 
		} 
	}

	// Receive damage number and location
	// TODO: implement HEAL and CRIT UI
	public void DisplayDamage( int damage_, PawnMaster reciever_, GameEvents.DamageType damage_type_, Vector2 location_ ) {
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
				default:
					break;
			}	
		}

		if ( popupPrefab != null ) {
			GameObject damageDisplay = Instantiate( popupPrefab, location_, Quaternion.identity );
			damageDisplay.GetComponent<TextMeshPro>().text = damage_.ToString();

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
