using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Paxn.Toolkit {
    /// <summary>
    /// Component to change Scenes.
    /// </summary>
    /// 
    /// * Author: Paxn (jfriffoa@gmail.com)
    /// * Last time modified: 2020-11-17
    public class SceneChanger : MonoBehaviour {

        #region Parameters
        [Tooltip("Use this component to load multiples scenes.")]
        [SerializeField] bool _multiplesScenes;

        [Scene]
        [Tooltip("Scenes to load.")]
        [SerializeField] string[] _scenes;

        int _sceneIndex;
        #endregion

        #region Methods
        /// <summary>
        /// Load the defined Scene.
        /// </summary>
        public void LoadScene() => SceneManager.LoadScene(_scenes[0]);
        /// <summary>
        /// Load the scene index from the list.
        /// </summary>
        /// <param name="i">Index of the scene to load.</param>
        public void LoadScene(int i) => SceneManager.LoadScene(_scenes[i]);

        /// <summary>
        /// Set the scene index to load.
        /// </summary>
        /// <param name="i">Index of the scene to load.</param>
        /// See <see cref="LoadSceneIndex"/>
        public void SetSceneIndex(int i) => _sceneIndex = i;
        /// <summary>
        /// Loads the scene with the index setted previously.
        /// </summary>
        /// See <see cref="SetSceneIndex(int)"/>
        public void LoadSceneIndex() => LoadScene(_sceneIndex);

        /// <summary>
        /// Load the defined Scene Additive.
        /// </summary>
        public void LoadSceneAdditive() => SceneManager.LoadScene(_scenes[0], LoadSceneMode.Additive);
        /// <summary>
        /// Load the scene index from the list Additive.
        /// </summary>
        /// <param name="i">Index of the scene to load.</param>
        public void LoadSceneAdditive(int i) => SceneManager.LoadScene(_scenes[i], LoadSceneMode.Additive);
        #endregion
    }

    #region Custom Unity Editor
#if UNITY_EDITOR
    [CustomEditor(typeof(SceneChanger))]
    public class SceneChangerEditor : Editor {

        // Scene Changer Properties
        SerializedProperty _multiple;
        SerializedProperty _scenes;

        void OnEnable() {
            _multiple = serializedObject.FindProperty("_multiplesScenes");
            _scenes = serializedObject.FindProperty("_scenes");

            if (_scenes.arraySize < 1)
                _scenes.arraySize = 1;
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_multiple);

            if (_multiple.boolValue) {
                EditorGUILayout.PropertyField(_scenes);
            } else {
                if (_scenes.arraySize < 1)
                    _scenes.arraySize = 1;

                var child = _scenes.GetArrayElementAtIndex(0);
                //EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(child, new GUIContent("Scene", "Scene to change to."));
                //EditorGUILayout.EndHorizontal();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
    #endregion
}