using System.Net.Mime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuffIconController : MonoBehaviour
{
    public void SetIcon(Sprite icon_) {
        GetComponent<Image>().sprite = icon_;
        Vector2 cell_size = transform.parent.GetComponent<GridLayoutGroup>().cellSize;
        RectTransform rt = transform.GetChild(0).GetComponent<RectTransform>();
        rt.sizeDelta = cell_size;
    }

    public void SetProgress(float progress_) {
        transform.GetChild(0).GetComponent<Image>().fillAmount = progress_;
    }

    public bool RemoveIcon() {
        try {
            Destroy(gameObject);
            return true;
        } catch (System.Exception e) {
            return false;
        }
    }
}
