using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Asset
{

    public class ActionController : MonoBehaviour
    {
        public List<GameObject> characterPrefabs;
        public List<GameObject> effectPrefabs;
        public Transform characterParent;
        public Transform spawnPos;
        public Transform targetPos;
        public GameObject currentCharacterStyle;
        public GameObject currentCharacter;

        public IEnumerator ActionScript()
        {
            //Front actions
            //1.Devil, Happy
            StartCoroutine(Action(0, "Front", "happyFront", 0, "happy"));
            yield return new WaitForSeconds(2f);
            //2.Devil, Angry
            StartCoroutine(Action(0, "Front", "angryFront", 1, "angry"));
            yield return new WaitForSeconds(2f);
            //2.Devil, Sad
            StartCoroutine(Action(0, "Front", "sadFront", 2, "sad"));
            yield return new WaitForSeconds(2f);

            //3.Devil, Walk
            StartCoroutine(Action(0, "Side", "walk", -1, ""));
            Debug.Log(Vector3.Distance(currentCharacter.transform.position, targetPos.position));
            while (Vector3.Distance(currentCharacter.transform.position, targetPos.position) > 0.1f)
            {
                currentCharacter.transform.position = Vector3.MoveTowards(currentCharacter.transform.position, targetPos.position, 2f * Time.deltaTime);
                yield return null;
            }
            StartCoroutine(ActionScript());
        }

        public IEnumerator Action(int characterID, string characterStyle /* Front, Back, Side */, string characterAnimation, int effectID, string effectAnimation)
        {
            DestroyAll();
            //style
            currentCharacter = Instantiate(characterPrefabs[characterID], characterParent);
            currentCharacter.transform.position = spawnPos.position;

            //Activate Characters in Child
            currentCharacterStyle = null;
            Transform effectParent = null;

            foreach (Transform characterTransform in currentCharacter.GetComponentsInChildren<Transform>())
            {
                if (characterTransform == currentCharacter.transform) continue;
                characterTransform.gameObject.SetActive(characterTransform.name == characterStyle || characterTransform.name == "Effect");
                if (characterTransform.name == characterStyle) currentCharacterStyle = characterTransform.gameObject;
                if (characterTransform.name == "Effect") effectParent = characterTransform;
            }

            //Play Animation
            currentCharacterStyle.GetComponent<Animator>().Play(characterAnimation);

            //Spawn Effect
            if (effectID < 0) yield break;
            var effect = Instantiate(effectPrefabs[effectID], effectParent);
            effect.GetComponent<Animator>().Play(effectAnimation);
            yield return new WaitForSeconds(effect.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
            Destroy(effect);
        }

        public void DestroyAll()
        {
            foreach (Transform characterTransform in characterParent)
            {
                if (characterTransform == characterParent.transform) continue;
                Destroy(characterTransform.gameObject);
            }
        }

        private void Start()
        {
            StartCoroutine(ActionScript());
        }
    }

}
