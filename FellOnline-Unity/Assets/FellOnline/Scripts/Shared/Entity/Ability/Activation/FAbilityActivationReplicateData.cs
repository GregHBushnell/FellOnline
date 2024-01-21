﻿using FishNet.Object.Prediction;
using UnityEngine;

namespace FellOnline.Shared
{
	public struct FAbilityActivationReplicateData : IReplicateData
	{
		public bool InterruptQueued;
		public long QueuedAbilityID;
		public KeyCode HeldKey;

		public FAbilityActivationReplicateData(bool interruptQueued, long queuedAbilityID, KeyCode heldKey)
		{
			InterruptQueued = interruptQueued;
			QueuedAbilityID = queuedAbilityID;
			HeldKey = heldKey;
			_tick = 0;
		}

		private uint _tick;
		public void Dispose() { }
		public uint GetTick() => _tick;
		public void SetTick(uint value) => _tick = value;
	}
}