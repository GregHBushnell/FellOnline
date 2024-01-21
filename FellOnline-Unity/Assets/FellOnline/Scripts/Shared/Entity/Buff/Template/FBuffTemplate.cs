using System.Collections.Generic;
using UnityEngine;

namespace FellOnline.Shared
{
	public abstract class FBuffTemplate : FCachedScriptableObject<FBuffTemplate>, FICachedObject
	{
		public string Description;
		public Texture2D Icon;
		public float Duration;
		public float TickRate;
		public uint UseCount;
		public uint MaxStacks;
		public bool IsPermanent;
		//do we want independant timers on FBuff stacks?
		public bool IndependantStackTimer;
		public List<FBuffAttributeTemplate> BonusAttributes;
		//public AudioEvent OnApplySounds;
		//public AudioEvent OnTickSounds;
		//public AudioEvent OnRemoveSounds;

		public string Name { get { return this.name; } }
		public abstract void OnApply(FBuff instance, Character target);
		public abstract void OnTick(FBuff instance, Character target);
		public abstract void OnRemove(FBuff instance, Character target);

		public abstract void OnApplyStack(FBuff stack, Character target);
		public abstract void OnRemoveStack(FBuff stack, Character target);
	}
}