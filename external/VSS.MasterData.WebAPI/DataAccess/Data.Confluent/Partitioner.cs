using System.Text;

namespace VSS.MasterData.WebAPI.Data.Confluent
{
	internal static class Partitioner
	{
		private static int Murmur2(byte[] data)
		{
			int length = data.Length;

			int num1 = -1756908916 ^ length;

			int num2 = length / 4;

			for (var index = 0; index < num2; ++index)
			{
				int num3 = index * 4;

				int num4 = (data[num3 + 0] + (data[num3 + 1] << 8) + (data[num3 + 2] << 16) + (data[num3 + 3] << 24)) * 1540483477;

				int num5 = (num4 ^ (int)((uint)num4 >> 24)) * 1540483477;

				num1 = num1 * 1540483477 ^ num5;
			}

			int num6 = length;

			var num7 = 4;

			int num8 = -1;

			switch (num7 != num8 ? num6 % num7 : 0)
			{
				case 1:
					num1 = (num1 ^ data[length & -4]) * 1540483477;
					break;
				case 2:
					num1 ^= data[(length & -4) + 1] << 8;
					goto case 1;
				case 3:
					num1 ^= data[(length & -4) + 2] << 16;
					goto case 2;
			}

			int num9 = (num1 ^ (int)((uint)num1 >> 13)) * 1540483477;

			return num9 ^ (int)((uint)num9 >> 15);
		}

		public static int ComputeHash(byte[] data)
		{
			return Murmur2(data);
		}

		public static int ComputeHash(string data)
		{
			return ComputeHash(Encoding.UTF8.GetBytes(data));
		}

		public static int ToPositive(int number)
		{
			return number & int.MaxValue;
		}

		public static int GetPartitionNumber(string data, int numberOfPartitions)
		{
			return ToPositive(ComputeHash(data)) % numberOfPartitions;
		}
	}
}
