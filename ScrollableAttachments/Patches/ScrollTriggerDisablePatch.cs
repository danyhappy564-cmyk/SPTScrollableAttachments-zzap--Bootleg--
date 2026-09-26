using AttachmentScrolling.Components;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace AttachmentScrolling.Patches;

// 스크롤 켜진 드롭다운 위에서 휠을 굴리면, 뒤의 무기 미리보기가 같이 줌되지 않게 막는다
public class ScrollTriggerDisablePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(ScrollTrigger), nameof(ScrollTrigger.OnScroll));
    }

    [PatchPrefix]
    private static bool PatchPrefix()
    {
        return !AttachmentScrollComponent.HoveringDropdown;
    }
}
