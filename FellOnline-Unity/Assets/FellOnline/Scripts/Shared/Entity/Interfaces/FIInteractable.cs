using UnityEngine;

namespace FellOnline.Shared
{
	public interface FIInteractable
	{
		Transform Transform { get; }
		bool InRange(Transform transform);
		bool OnInteract(Character character);
	}
}