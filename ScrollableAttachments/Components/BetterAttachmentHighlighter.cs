using System;
using AttachmentScrolling.Config;
using EFT.InventoryLogic;
using EFT.UI.DragAndDrop;
using UnityEngine;
using UnityEngine.UI;

namespace AttachmentScrolling.Components;

// 드롭다운에 뜬 부착물들을 "지금 슬롯에 달린 부착물"과 비교해서, 더 좋은 것에 테두리를 칠한다.
public static class BetterAttachmentHighlighter
{
    private const float Epsilon = 0.001f;

    // F12 설정을 바꿨을 때 열려 있는 목록을 바로 다시 칠하기 위해 기억해둔다
    private static RectTransform _openContainer;
    private static Item _openCurrentItem;

    private static bool _loggedFirstApply;

    public static void OnMenuShown(RectTransform container, Item currentItem)
    {
        _openContainer = container;
        _openCurrentItem = currentItem;
        Apply(container, currentItem);
    }

    public static void OnMenuClosed(RectTransform container)
    {
        ClearAll(container);
        if (_openContainer == container)
        {
            _openContainer = null;
            _openCurrentItem = null;
        }
    }

    public static void RefreshOpenMenu()
    {
        if (_openContainer == null || !_openContainer.gameObject.activeInHierarchy)
        {
            return;
        }

        Apply(_openContainer, _openCurrentItem);
    }

    private static void Apply(RectTransform container, Item currentItem)
    {
        if (container == null)
        {
            return;
        }

        bool enabled = GeneralConfig.HighlightEnabled.Value;
        int marked = 0;

        foreach (Transform child in container)
        {
            ItemView view = child.GetComponent<ItemView>();
            if (view == null)
            {
                continue;
            }

            bool better = false;
            try
            {
                better = enabled && IsBetter(view.Item, currentItem);
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"[Highlight] compare failed for {view.Item?.StringTemplateId}: {e.Message}");
            }

            BetterAttachmentBorder border = child.GetComponent<BetterAttachmentBorder>();
            if (better && border == null)
            {
                border = child.gameObject.AddComponent<BetterAttachmentBorder>();
            }

            if (border != null)
            {
                border.SetVisible(better);
            }

            if (better)
            {
                marked++;
            }
        }

        if (!_loggedFirstApply && enabled)
        {
            _loggedFirstApply = true;
            Plugin.Logger.LogInfo($"[Highlight] first apply: current={currentItem?.StringTemplateId ?? "(empty)"}, marked {marked} item(s)");
        }
    }

    private static void ClearAll(RectTransform container)
    {
        if (container == null)
        {
            return;
        }

        foreach (BetterAttachmentBorder border in container.GetComponentsInChildren<BetterAttachmentBorder>(true))
        {
            border.SetVisible(false);
        }
    }

    public static bool IsBetter(Item candidate, Item current)
    {
        if (candidate is not Mod cand)
        {
            return false;
        }

        if (current != null && candidate.TemplateId.Equals(current.TemplateId))
        {
            return false;
        }

        // 빈 슬롯이면 "아무것도 안 단 상태"(전부 0)와 비교
        Mod cur = current as Mod;
        float curErgo = cur != null ? cur.Ergonomics : 0f;
        float curRecoil = cur != null ? cur.Recoil : 0f;
        float curAccuracy = cur != null ? cur.Accuracy : 0f;
        float curWeight = current != null ? current.Weight : 0f;

        bool anyCompared = false;
        bool anyBetter = false;
        bool anyWorse = false;

        void Check(bool on, float gain)
        {
            if (!on)
            {
                return;
            }

            anyCompared = true;
            if (gain > Epsilon)
            {
                anyBetter = true;
            }
            else if (gain < -Epsilon)
            {
                anyWorse = true;
            }
        }

        // gain > 0 이면 후보가 더 좋음
        Check(GeneralConfig.CompareErgonomics.Value, cand.Ergonomics - curErgo);
        Check(GeneralConfig.CompareRecoil.Value, curRecoil - cand.Recoil);
        Check(GeneralConfig.CompareAccuracy.Value, cand.Accuracy - curAccuracy);
        Check(GeneralConfig.CompareWeight.Value, curWeight - candidate.Weight);

        if (!anyCompared)
        {
            return false;
        }

        return GeneralConfig.CompareMode.Value == BetterCompareMode.AnyBetter
            ? anyBetter
            : anyBetter && !anyWorse;
    }
}

// 아이템 칸 위에 얹는 테두리. 칸이 풀(pool)로 돌아가 비활성화되면 자동으로 숨긴다.
public class BetterAttachmentBorder : MonoBehaviour
{
    private GameObject _root;
    private Image[] _edges;

    public void SetVisible(bool visible)
    {
        if (visible)
        {
            EnsureCreated();
            ApplyStyle();
            _root.transform.SetAsLastSibling();
        }

        if (_root != null)
        {
            _root.SetActive(visible);
        }
    }

    private void OnDisable()
    {
        if (_root != null)
        {
            _root.SetActive(false);
        }
    }

    private void EnsureCreated()
    {
        if (_root != null)
        {
            return;
        }

        _root = new GameObject("SA_BetterBorder", typeof(RectTransform));
        _root.layer = gameObject.layer;
        RectTransform rootRect = (RectTransform)_root.transform;
        rootRect.SetParent(transform, false);
        Stretch(rootRect);

        // 부모에 LayoutGroup 이 있어도 배치에 끼어들지 않게
        LayoutElement layout = _root.AddComponent<LayoutElement>();
        layout.ignoreLayout = true;

        _edges = new Image[4];
        for (int i = 0; i < 4; i++)
        {
            GameObject edge = new GameObject($"Edge{i}", typeof(RectTransform));
            edge.layer = gameObject.layer;
            edge.transform.SetParent(rootRect, false);
            Image image = edge.AddComponent<Image>();
            image.raycastTarget = false;
            _edges[i] = image;
        }
    }

    private void ApplyStyle()
    {
        Color color = GeneralConfig.BorderColor.Value;
        float t = GeneralConfig.BorderThickness.Value;

        for (int i = 0; i < _edges.Length; i++)
        {
            _edges[i].color = color;
            RectTransform r = _edges[i].rectTransform;
            switch (i)
            {
                case 0: // 위
                    r.anchorMin = new Vector2(0f, 1f);
                    r.anchorMax = new Vector2(1f, 1f);
                    r.pivot = new Vector2(0.5f, 1f);
                    r.sizeDelta = new Vector2(0f, t);
                    break;
                case 1: // 아래
                    r.anchorMin = new Vector2(0f, 0f);
                    r.anchorMax = new Vector2(1f, 0f);
                    r.pivot = new Vector2(0.5f, 0f);
                    r.sizeDelta = new Vector2(0f, t);
                    break;
                case 2: // 왼쪽
                    r.anchorMin = new Vector2(0f, 0f);
                    r.anchorMax = new Vector2(0f, 1f);
                    r.pivot = new Vector2(0f, 0.5f);
                    r.sizeDelta = new Vector2(t, 0f);
                    break;
                default: // 오른쪽
                    r.anchorMin = new Vector2(1f, 0f);
                    r.anchorMax = new Vector2(1f, 1f);
                    r.pivot = new Vector2(1f, 0.5f);
                    r.sizeDelta = new Vector2(t, 0f);
                    break;
            }
            r.anchoredPosition = Vector2.zero;
        }
    }

    private static void Stretch(RectTransform r)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.pivot = new Vector2(0.5f, 0.5f);
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
    }
}
