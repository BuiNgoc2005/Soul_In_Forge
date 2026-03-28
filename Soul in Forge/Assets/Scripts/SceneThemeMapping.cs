using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/SceneThemeMapping")]
public class SceneThemeMapping : ScriptableObject
{
    [System.Serializable]
    public class SceneTheme
    {
        public string sceneName;   //  scene name
        public AudioClip clip;     //  .wav file
    }

    public List<SceneTheme> themes = new List<SceneTheme>();

    public AudioClip GetClipFor(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return null;
        foreach (var st in themes)
        {
            if (st != null && st.sceneName == sceneName)
                return st.clip;
        }
        return null;
    }
}
