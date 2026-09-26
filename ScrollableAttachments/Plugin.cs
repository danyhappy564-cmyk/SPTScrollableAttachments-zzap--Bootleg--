using AttachmentScrolling.Config;
using BepInEx;
using BepInEx.Logging;
using SPT.Reflection.Patching;

namespace AttachmentScrolling;

[BepInPlugin("com.pein.attachmentscrolling", "ScrollableAttachments", "1.2.0")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private void Awake()
    {
        Logger = base.Logger;

        // 설정을 먼저 만든다 — 패치가 켜지는 순간부터 설정값을 읽기 때문
        GeneralConfig.Initialize(Config);

        var patchManager = new PatchManager(this, true);
        patchManager.EnablePatches();
    }
}
