#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace FellOnline.Shared
{
	[CustomEditor(typeof(FWorldSceneDetailsCache))]
	public class WorldSceneDetailsCacheEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			var script = (FWorldSceneDetailsCache)target;

			if (GUILayout.Button("Rebuild", GUILayout.Height(40)))
			{
				script.Rebuild();
				EditorUtility.SetDirty(script);
			}
		}
	}
}
#endif