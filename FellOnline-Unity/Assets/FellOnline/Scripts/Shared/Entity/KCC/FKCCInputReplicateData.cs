using FishNet.Object.Prediction;
using UnityEngine;

namespace FellOnline.Shared
{
	public struct FKCCInputReplicateData : IReplicateData
	{
		public float MoveAxisForward;
		public float MoveAxisRight;
		

		public int MoveFlags;

		public FKCCInputReplicateData(float moveAxisForward, float moveAxisRight,int moveFlags)
		{
			MoveAxisForward = moveAxisForward;
			MoveAxisRight = moveAxisRight;
			MoveFlags = moveFlags;
			

			_tick = 0;
		}

		private uint _tick;
		public void Dispose() { }
		public uint GetTick() => _tick;
		public void SetTick(uint value) => _tick = value;
	}
}