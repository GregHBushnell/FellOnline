using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Custom/RageSlider", 34), RequireComponent(typeof(RectTransform))]
public class FRageSlider : Slider
{
	public void SetValue(float value)
	{
		Set(value, false);
	}
}