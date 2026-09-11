using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class AutoSaveOnPlay
{
    static AutoSaveOnPlay()
    {
        // 再生状態が変化した時のイベントを登録
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // Playボタンを押して、再生モードに入る直前のタイミング
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            Debug.Log("SAVE");
            // 変更のあるシーンをすべて保存
            EditorSceneManager.SaveOpenScenes();

            // 編集されたアセット（プレハブ等）も同時に保存
            AssetDatabase.SaveAssets();
        }
    }
}
