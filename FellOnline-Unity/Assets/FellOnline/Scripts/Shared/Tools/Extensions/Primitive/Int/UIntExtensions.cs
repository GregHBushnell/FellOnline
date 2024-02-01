﻿using System;

namespace FellOnline.Shared
{
	public static class UIntExtensions
	{
		/// <summary>
		/// Returns the absolute value of the difference between two values.
		/// </summary>
		public static uint AbsoluteSubtract(this uint number, uint other)
		{
			return (number > other) ? number - other : other - number;
		}

		/// <summary>
		/// Returns the number clamped to the specified minimum and maximum value.
		/// </summary>
		public static uint Clamp(this uint number, uint minimum, uint maximum)
		{
			if (number < minimum)
			{
				return minimum;
			}
			if (number > maximum)
			{
				return maximum;
			}
			return number;
		}

		/// <summary>
		/// Returns the number of digits of the current value.
		/// </summary>
		public static int DigitCount(this uint number)
		{
			if (number != 0)
			{
				return ((int)Math.Log10(number)) + 1;
			}
			return 1;
		}

		/// <summary>
		/// Returns the specified digit of the number. Where zero is the least significant digit.
		/// </summary>
		public static uint GetDigit(this uint number, int digit)
		{
			const byte MIN_DIGITS = 0;
			const byte BASE_TEN = 10;

			digit = digit.Clamp(MIN_DIGITS, number.DigitCount());
			for (int i = MIN_DIGITS; i < digit; ++i)
			{
				number /= BASE_TEN;
			}
			return number % BASE_TEN;
		}
	}
}