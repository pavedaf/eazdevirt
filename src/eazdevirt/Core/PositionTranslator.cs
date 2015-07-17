﻿using System;
using System.IO;

namespace eazdevirt
{
	/// <summary>
	/// Translates strings into positions.
	/// </summary>
	public class PositionTranslator : IPositionTranslator
	{
		public static readonly PositionTranslator DefaultInstance = new PositionTranslator();

		/// <summary>
		/// Although these values appear "random," they are consistent across
		/// all samples I've observed.
		/// </summary>
		private static UInt32[] DefaultPseudoRandomInts = new uint[]
		{
			52200625u,
			614125u,
			7225u,
			85u,
			1u
		};

		/// <summary>
		/// Random integers used in translation.
		/// </summary>
		private UInt32[] _randomInts;

		/// <summary>
		/// Construct a PositionTranslator with the default random integers.
		/// </summary>
		public PositionTranslator()
			: this(DefaultPseudoRandomInts)
		{
		}

		/// <summary>
		/// Construct a PositionTranslator.
		/// </summary>
		/// <param name="randomInts">Random integers used in translation</param>
		public PositionTranslator(UInt32[] randomInts)
		{
			_randomInts = randomInts;
		}

		/// <summary>
		/// Given the crypto key, convert a position string into a position.
		/// </summary>
		/// <param name="s">Position string</param>
		/// <param name="cryptoKey">Crypto key</param>
		/// <returns>Position</returns>
		public Int64 ToPosition(String s, Int32 cryptoKey)
		{
			byte[] array = Convert(s);
			MemoryStream memoryStream = new MemoryStream(array);
			CryptoStream stream = new CryptoStream(memoryStream, cryptoKey);
			BinaryReader binaryReader = new BinaryReader(stream);
			long result = binaryReader.ReadInt64();
			memoryStream.Dispose();
			return result;
		}

		/// <summary>
		/// Convert a position string to the corresponding byte array.
		/// </summary>
		/// <param name="str">Position string</param>
		/// <remarks>Most of this is copied from decompilation</remarks>
		/// <returns>Byte array</returns>
		private Byte[] Convert(String str)
		{
			if (str == null)
				throw new ArgumentNullException();

			if (str.Length != 10)
				throw new FormatException("Position string must be 10 characters in length");

			MemoryStream memoryStream = new MemoryStream(str.Length * 4 / 5);
			byte[] result;

			try
			{
				int num = 0;
				uint num2 = 0u;

				for (int i = 0; i < str.Length; i++)
				{
					char c = str[i];
					if (c == 'z' && num == 0)
					{
						WriteValue(memoryStream, num2, 0);
					}
					else
					{
						if (c < '!' || c > 'u')
						{
							throw new FormatException("Illegal character");
						}
						checked
						{
							num2 += (uint)(unchecked((ulong)_randomInts[num]) * (ulong)unchecked((long)checked(c - '!')));
						}
						num++;
						if (num == 5)
						{
							WriteValue(memoryStream, num2, 0);
							num = 0;
							num2 = 0u;
						}
					}
				}

				if (num == 1)
				{
					throw new Exception();
				}

				if (num > 1)
				{
					for (int j = num; j < 5; j++)
					{
						checked
						{
							num2 += 84u * _randomInts[j];
						}
					}
					WriteValue(memoryStream, num2, 5 - num);
				}

				result = memoryStream.ToArray();
			}
			finally
			{
				((IDisposable)memoryStream).Dispose();
			}

			return result;
		}

		private static void WriteValue(Stream stream, uint val, int int_0)
		{
			stream.WriteByte((byte)(val >> 24));
			if (int_0 == 3)
			{
				return;
			}
			stream.WriteByte((byte)(val >> 16 & 255u));
			if (int_0 == 2)
			{
				return;
			}
			stream.WriteByte((byte)(val >> 8 & 255u));
			if (int_0 == 1)
			{
				return;
			}
			stream.WriteByte((byte)(val & 255u));
		}
	}
}