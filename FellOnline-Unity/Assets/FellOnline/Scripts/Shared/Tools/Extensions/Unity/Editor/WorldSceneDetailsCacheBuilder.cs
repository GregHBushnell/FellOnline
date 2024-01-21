using UnityEngine;
using UnityEditor;

namespace FellOnline.Shared
{
	public class WorldSceneDetailsCacheBuilder
	{
		[MenuItem("FellOnline/Build/World Scene Details", priority = -9)]
		public static void Rebuild()
		{
			// rebuild world details cache, this includes teleporters, teleporter destinations, spawn points, and other constant scene data
			FWorldSceneDetailsCache worldDetailsCache = AssetDatabase.LoadAssetAtPath<FWorldSceneDetailsCache>(FWorldSceneDetailsCache.CACHE_FULL_PATH);
			if (worldDetailsCache != null)
			{
				worldDetailsCache.Rebuild();
				EditorUtility.SetDirty(worldDetailsCache);
			}
			else
			{
				worldDetailsCache = ScriptableObject.CreateInstance<FWorldSceneDetailsCache>();
				worldDetailsCache.Rebuild();
				EditorUtility.SetDirty(worldDetailsCache);
				AssetDatabase.CreateAsset(worldDetailsCache, FWorldSceneDetailsCache.CACHE_FULL_PATH);
			}
			AssetDatabase.SaveAssets();
		}
	}
} 