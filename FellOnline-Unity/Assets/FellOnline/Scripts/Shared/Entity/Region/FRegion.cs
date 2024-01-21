using System.Collections.Generic;
using UnityEngine;

namespace FellOnline.Shared
{
	public class FRegion : MonoBehaviour
	{
		public string RegionName;
		public Collider Collider;
		[Tooltip("Add a terrain if you would like the region to span the entire map. (Requires BoxCollider)")]
		public Terrain Terrain;

		public List<FRegionAction> OnUpdate = new List<FRegionAction>();
		public List<FRegionAction> OnRegionEnter = new List<FRegionAction>();
		public List<FRegionAction> OnRegionStay = new List<FRegionAction>();
		public List<FRegionAction> OnRegionExit = new List<FRegionAction>();

		void Awake()
		{
			Collider = gameObject.GetComponent<Collider>();
			if (Collider == null)
			{
				Debug.Log(RegionName + " collider is null and will not function properly.");
				return;
			}
			// set the collider to trigger just incase we forgot to set it in the inspector
			Collider.isTrigger = true;

			// terrain bounds override the collider
			if (Terrain != null)
			{
				BoxCollider box = Collider as BoxCollider;
				if (box != null)
				{
					box.size = Terrain.terrainData.size;
				}
			}
		}

#if UNITY_EDITOR
		public Color RegionColor = Color.red;

		void OnDrawGizmosSelected()
		{
			Gizmos.color = RegionColor;

			BoxCollider box = Collider as BoxCollider;
			if (box != null)
			{
				Gizmos.DrawWireCube(transform.position, box.size);
				return;
			}
			else
			{
				SphereCollider sphere = Collider as SphereCollider;
				if (sphere != null)
				{
					Gizmos.DrawWireSphere(transform.position, sphere.radius);
					return;
				}
			}
		}
#endif

		private void OnTriggerEnter(Collider other)
		{
			Character character = other.GetComponent<Character>();
			if (character != null)
			{
				foreach (FRegionAction action in OnRegionEnter)
				{
					action.Invoke(character, this);
				}
			}
		}

		private void OnTriggerStay(Collider other)
		{
			Character character = other.GetComponent<Character>();
			if (character != null)
			{
				foreach (FRegionAction action in OnRegionStay)
				{
					action.Invoke(character, this);
				}
			}
		}

		private void OnTriggerExit(Collider other)
		{
			Character character = other.GetComponent<Character>();
			if (character != null)
			{
				foreach (FRegionAction action in OnRegionExit)
				{
					action.Invoke(character, this);
				}
			}
		}
	}
}