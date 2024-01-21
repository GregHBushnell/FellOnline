﻿using TMPro;
using UnityEngine;

namespace FellOnline.Client
{
	public class FUITooltip : FUIControl
	{
		public TMP_Text text;
		public RectTransform background;

		public override void OnStarting()
		{
		}

		public override void OnDestroying()
		{
		}

		void Update()
		{
			if (text != null && background != null)
			{
				Vector3 offset = new Vector3(0.0f, background.rect.height, 0.0f);
				transform.position = Input.mousePosition + offset;
			}
		}

		public void Open(string text)
		{
			if (this.text != null)
			{
				this.text.text = text;
				Vector3 offset = new Vector3(0.0f, background.rect.height, 0.0f);
				transform.position = Input.mousePosition + offset;
				Show();
			}
		}
	}
}