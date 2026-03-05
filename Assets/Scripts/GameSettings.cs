using UnityEngine;

public class GameSettings
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }
}