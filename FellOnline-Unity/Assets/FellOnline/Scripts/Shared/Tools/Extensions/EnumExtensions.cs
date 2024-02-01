﻿using System;

namespace FellOnline.Shared
{
	public static class EnumExtensions
	{
		public static T[] ToArray<T>() where T : Enum
		{
			return (T[])Enum.GetValues(typeof(T));
		}
	}
}