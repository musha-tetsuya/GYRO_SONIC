using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Musha
{
    /// <summary>
    /// ゲーム開始シーン
    /// </summary>
    public class GameStart : MonoBehaviour
    {
#if UNITY_EDITOR
        /// <summary>
        /// GameStart経由後に開くシーン名のEditorPrefsキー
        /// </summary>
        private static readonly string NEXT_SCENE_NAME_KEY = typeof(GameStart).FullName + ".nextSceneName";

        /// <summary>
        /// 再生開始シーンをセットする
        /// </summary>
        [InitializeOnLoadMethod]
        private static void SetPlayModeStartScene()
        {
            EditorSceneManager.activeSceneChangedInEditMode += (closeScene, openedScene) =>
            {
                EditorSceneManager.playModeStartScene = null;

                //開いたシーンがEditorBuildSettingsに含まれていない場合、GameStartを経由したくないのでスルーする
                if (!EditorBuildSettings.scenes.Any(x => x.path == openedScene.path))
                {
                    return;
                }

                //GameStartシーンがEditorBuildSettingsに含まれていない場合、経由出来ないのでスルーする
                var gameStartScene = EditorBuildSettings.scenes.FirstOrDefault(x => Path.GetFileNameWithoutExtension(x.path) == "GameStart");
                if (gameStartScene == null)
                {
                    return;
                }

                //開いたシーンがGameStartシーンの場合、GameStartの次のシーンはデフォルトなのでEditorPrefsをリセットする
                if (gameStartScene.path == openedScene.path)
                {
                    EditorPrefs.DeleteKey(NEXT_SCENE_NAME_KEY);
                    return;
                }

                //再生開始シーンをGameStartシーンに設定
                EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(gameStartScene.path);

                //経由後に開くシーンとして、今開いたシーンの名前を保存
                EditorPrefs.SetString(NEXT_SCENE_NAME_KEY, openedScene.name);

                Debug.LogFormat("Set playModeStartScene = {0}", openedScene.name);
            };
        }
#endif

        /// <summary>
        /// 次に開くシーン名
        /// </summary>
        [SerializeField]
        private string nextSceneName = null;

        /// <summary>
        /// Start
        /// </summary>
        private void Start()
        {
#if UNITY_EDITOR
            if (EditorPrefs.HasKey(NEXT_SCENE_NAME_KEY))
            {
                //次のシーンへ遷移
                SceneManager.LoadScene(EditorPrefs.GetString(NEXT_SCENE_NAME_KEY));
                return;
            }
#endif
            if (!string.IsNullOrEmpty(this.nextSceneName))
            {
                //次のシーンへ遷移
                SceneManager.LoadScene(this.nextSceneName);
                return;
            }

            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                //BuildSettingsの番号が最も若いシーンに遷移
                if (i != SceneManager.GetActiveScene().buildIndex)
                {
                    SceneManager.LoadScene(i);
                    return;
                }
            }
        }
    }
}