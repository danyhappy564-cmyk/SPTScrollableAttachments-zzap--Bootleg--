using System;
using AttachmentScrolling.Components;
using EFT.InventoryLogic;
using EFT.UI.WeaponModding;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace AttachmentScrolling.Patches;

// 부착물 드롭다운이 열린 직후: 목록 칸들을 검사해서 더 좋은 부착물에 테두리 표시
public class DropDownMenuShowPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(DropDownMenu), nameof(DropDownMenu.Show));
    }

    [PatchPostfix]
    private static void PatchPostfix(DropDownMenu __instance, ModdingSelectableItemContext itemContext)
    {
        try
        {
            BetterAttachmentHighlighter.OnMenuShown(__instance._itemsContainer, itemContext?.Slot?.ContainedItem);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"[Highlight] DropDownMenu.Show postfix failed: {e}");
        }
    }
}

// 드롭다운이 닫히기 직전: 칠해둔 테두리를 전부 숨긴다 (칸이 풀로 돌아가 인벤토리 등에서 재사용되기 때문)
public class DropDownMenuClosePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(DropDownMenu), nameof(DropDownMenu.Close));
    }

    [PatchPrefix]
    private static void PatchPrefix(DropDownMenu __instance)
    {
        try
        {
            BetterAttachmentHighlighter.OnMenuClosed(__instance._itemsContainer);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"[Highlight] DropDownMenu.Close prefix failed: {e}");
        }
    }
}
