using UnityEngine;
using System.Collections.Generic;

namespace FellOnline.Client
{
	public class FLabelMaker : MonoBehaviour
	{
		private static FLabelMaker instance;
		internal static FLabelMaker Instance
		{
			get
			{
				return instance;
			}
		}
		private Queue<FCached3DLabel> pool = new Queue<FCached3DLabel>();

		public FCached3DLabel LabelPrefab;

		void Awake()
		{
			if (instance != null)
			{
				Destroy(this.gameObject);
				return;
			}
			instance = this;

			gameObject.name = typeof(FLabelMaker).Name;

			DontDestroyOnLoad(this.gameObject);
		}

		public bool Dequeue(out FCached3DLabel label)
		{
			if (LabelPrefab != null && pool != null)
			{
				if (!pool.TryDequeue(out label))
				{
					label = Instantiate(LabelPrefab);
				}
				return true;
			}
			label = null;
			return false;
		}

		public void Enqueue(FCached3DLabel label)
		{
			if (pool != null)
			{
				label.gameObject.SetActive(false);
				pool.Enqueue(label);
			}
			else
			{
				Destroy(label.gameObject);
			}
		}

		public void ClearCache()
		{
			if (pool == null ||
				pool.Count < 1)
			{
				return;
			}
			while (pool.TryDequeue(out FCached3DLabel label))
			{
				Destroy(label.gameObject);
			}
		}

		public static FCached3DLabel Display(string text, Vector3 position, Color color, float fontSize, float persistTime, bool manualCache)
		{
			if (FLabelMaker.Instance.Dequeue(out FCached3DLabel label))
			{
				label.Initialize(text, position, color, fontSize, persistTime, manualCache);
				label.gameObject.SetActive(true);
				return label;
			}
			return null;
		}

		public static void Cache(FCached3DLabel label)
		{
			if (label == null)
			{
				return;
			}

			FLabelMaker.Instance.Enqueue(label);
		}

		public static void Clear()
		{
			FLabelMaker.Instance.ClearCache();
		}
	}
}