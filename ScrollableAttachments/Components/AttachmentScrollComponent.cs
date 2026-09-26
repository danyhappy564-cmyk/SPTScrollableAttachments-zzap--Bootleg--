using System;
using System.Collections.Generic;
using AttachmentScrolling.Config;
using EFT.UI;
using EFT.UI.WeaponModding;
using UnityEngine;
using UnityEngine.UI;

namespace AttachmentScrolling.Components;

// 무기 개조 화면 / 프리셋 편집 화면에 하나씩 붙어서, 부착물 드롭다운 목록을 스크롤 가능한 창으로 바꾼다.
// F12 "스크롤 기능 사용"을 끄면 바꿨던 걸 원래 값으로 되돌린다.
public class AttachmentScrollComponent : MonoBehaviour
{
    private static readonly List<AttachmentScrollComponent> Instances = new();

    private ScrollRect _scrollRect;
    private RectMask2D _rectMask;
    private GridLayoutGroup _contentGrid;
    private ContentSizeFitter _fitter;
    private RectTransform _dropDownRect;
    private RectTransform _containerRect;
    private Transform _emptyItem;

    // 원래 게임 값 (스크롤 끌 때 복원용)
    private Transform _origEmptyParent;
    private int _origEmptySibling;
    private ContentSizeFitter.FitMode _origVerticalFit;
    private ContentSizeFitter.FitMode _origHorizontalFit;
    private int _origPaddingTop;
    private int _origPaddingBottom;
    private GridLayoutGroup.Constraint _origConstraint;
    private int _origConstraintCount;
    private Vector2 _origSizeDelta;
    private Vector2 _origContentPos;

    private bool _applied;
    private bool _hovering;

    // 마우스가 스크롤 켜진 드롭다운 위에 있으면 true → 무기 미리보기 줌(ScrollTrigger)을 막는다
    public static bool HoveringDropdown
    {
        get
        {
            foreach (AttachmentScrollComponent c in Instances)
            {
                if (c != null && c._applied && c._hovering)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public static void ForEach(Action<AttachmentScrollComponent> action)
    {
        foreach (AttachmentScrollComponent c in Instances.ToArray())
        {
            if (c == null)
            {
                continue;
            }

            try
            {
                action(c);
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"[Scroll] setting apply failed: {e}");
            }
        }
    }

    private void Awake()
    {
        Transform dropDown = FindDropDown();
        if (dropDown == null)
        {
            Plugin.Logger.LogWarning($"[Scroll] DropdownMenu not found under {name}, scroll disabled for this screen");
            Destroy(this);
            return;
        }

        // 같은 드롭다운을 다른 화면 컴포넌트가 이미 맡고 있으면 중복 처리하지 않는다
        if (dropDown.GetComponent<ScrollRect>() != null)
        {
            Destroy(this);
            return;
        }

        _emptyItem = dropDown.Find("ItemViewContainer");
        Transform container = dropDown.Find("Container");
        if (_emptyItem == null || container == null)
        {
            Plugin.Logger.LogWarning("[Scroll] DropdownMenu layout changed (ItemViewContainer/Container missing), scroll disabled");
            Destroy(this);
            return;
        }

        _containerRect = container.GetComponent<RectTransform>();
        _fitter = dropDown.GetComponent<ContentSizeFitter>();
        _contentGrid = container.GetComponent<GridLayoutGroup>();
        _dropDownRect = dropDown.GetComponent<RectTransform>();

        _origEmptyParent = _emptyItem.parent;
        _origEmptySibling = _emptyItem.GetSiblingIndex();
        _origVerticalFit = _fitter.verticalFit;
        _origHorizontalFit = _fitter.horizontalFit;
        _origPaddingTop = _contentGrid.padding.top;
        _origPaddingBottom = _contentGrid.padding.bottom;
        _origConstraint = _contentGrid.constraint;
        _origConstraintCount = _contentGrid.constraintCount;
        _origSizeDelta = _dropDownRect.sizeDelta;
        _origContentPos = _containerRect.anchoredPosition;

        // 마스크/스크롤은 한 번만 만들고, 켜고 끌 때는 enabled 만 바꾼다
        _rectMask = dropDown.gameObject.AddComponent<RectMask2D>();
        _scrollRect = dropDown.gameObject.AddComponent<ScrollRect>();
        _scrollRect.content = _containerRect;
        _scrollRect.horizontal = false;
        _scrollRect.scrollSensitivity = GeneralConfig.ScrollSpeed.Value;

        HoverTrigger trigger = dropDown.gameObject.AddComponent<HoverTrigger>();
        trigger.OnHoverStart += (_) => _hovering = true;
        trigger.OnHoverEnd += (_) => _hovering = false;

        Instances.Add(this);

        SetScrollEnabled(GeneralConfig.ScrollEnabled.Value);
    }

    private void OnDestroy()
    {
        Instances.Remove(this);
    }

    private Transform FindDropDown()
    {
        // 1순위: 이 화면 아래에 있는 DropDownMenu 컴포넌트 (이름 검색보다 안전)
        DropDownMenu fallback = null;
        foreach (DropDownMenu menu in GetComponentsInChildren<DropDownMenu>(true))
        {
            if (menu.transform.parent != null && menu.transform.parent.name == "Preview Panel")
            {
                return menu.transform;
            }
            fallback ??= menu;
        }
        if (fallback != null)
        {
            return fallback.transform;
        }

        // 2순위: 원본 모드 방식 (이름으로 전역 검색)
        GameObject previewPanel = GameObject.Find("Preview Panel");
        return previewPanel != null ? previewPanel.transform.Find("DropdownMenu") : null;
    }

    public void SetScrollEnabled(bool enabled)
    {
        if (enabled == _applied)
        {
            return;
        }

        if (enabled)
        {
            // 빈 칸("없음") 아이템을 스크롤 내용 안으로 옮긴다
            _emptyItem.SetParent(_containerRect, false);
            _emptyItem.SetSiblingIndex(0);

            _fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            _fitter.horizontalFit = ContentSizeFitter.FitMode.MinSize;

            _contentGrid.padding.top = 5;
            _contentGrid.padding.bottom = 5;
            _contentGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            _contentGrid.constraintCount = GeneralConfig.GridColumns.Value;

            _dropDownRect.sizeDelta = new Vector2(_dropDownRect.sizeDelta.x, GeneralConfig.ViewHeight.Value);

            _rectMask.enabled = true;
            _scrollRect.enabled = true;
        }
        else
        {
            _scrollRect.StopMovement();
            _scrollRect.enabled = false;
            _rectMask.enabled = false;
            _containerRect.anchoredPosition = _origContentPos;

            _emptyItem.SetParent(_origEmptyParent, false);
            _emptyItem.SetSiblingIndex(_origEmptySibling);

            _fitter.verticalFit = _origVerticalFit;
            _fitter.horizontalFit = _origHorizontalFit;

            _contentGrid.padding.top = _origPaddingTop;
            _contentGrid.padding.bottom = _origPaddingBottom;
            _contentGrid.constraint = _origConstraint;
            _contentGrid.constraintCount = _origConstraintCount;

            _dropDownRect.sizeDelta = new Vector2(_dropDownRect.sizeDelta.x, _origSizeDelta.y);
        }

        _applied = enabled;
        LayoutRebuilder.MarkLayoutForRebuild(_dropDownRect);
        Plugin.Logger.LogInfo($"[Scroll] {(enabled ? "enabled" : "disabled")} on {name}");
    }

    public void SetScrollSpeed(float speed)
    {
        _scrollRect.scrollSensitivity = speed;
    }

    public void SetGridColumns(int cols)
    {
        if (_applied)
        {
            _contentGrid.constraintCount = cols;
        }
    }

    public void SetViewHeight(float height)
    {
        if (_applied)
        {
            _dropDownRect.sizeDelta = new Vector2(_dropDownRect.sizeDelta.x, height);
        }
    }
}
