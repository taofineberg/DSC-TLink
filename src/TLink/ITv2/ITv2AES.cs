﻿// DSC TLink - a communications library for DSC Powerseries NEO alarm panels
// Copyright (C) 2024 Brian Humlicek
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Security.Cryptography;

namespace DSC.TLink.ITv2
{
	internal class ITv2AES
	{
		public static byte[] GetRandomKey() => RandomNumberGenerator.GetBytes(16);
		//The Type2 initializer protocol is not 'symetric' like the Type1.  Instead, the same transform
		// used to 'encode' the local initializer is the same transform used to 'decode' the remote initializer.
		public static byte[] Type2InitializerTransform(string integrationAccessCode, byte[] initializer)
		{
			if (integrationAccessCode.Length != 32) throw new ArgumentException($"integration access code is {integrationAccessCode.Length} digits long; it needs to be 32.", nameof(integrationAccessCode));
			if (initializer.Length != 16) throw new ArgumentException(nameof(initializer));

			using (Aes aes = Aes.Create())
			{
				aes.Key = Convert.FromHexString(integrationAccessCode);
				return aes.EncryptEcb(initializer, PaddingMode.Zeros);
			}
		}
		public static (byte[] initializer, byte[]encodedKey) GenerateKeyAndType1Initializer(string integrationAccessCode)
		{
			if (integrationAccessCode.Length < 8) throw new ArgumentException($"integration access code is {integrationAccessCode.Length} digits long; it needs to be at least 8.");

			byte[] randomBytes = RandomNumberGenerator.GetBytes(32);
			var checkBytes = evenIndexes(randomBytes);
			byte[] encodedKey = oddIndexes(randomBytes).ToArray();

			byte[] cipherText;
			using (Aes aes = Aes.Create())
			{
				aes.Key = transformKeyString(integrationAccessCode);
				cipherText = aes.EncryptEcb(randomBytes, PaddingMode.Zeros);
			}
			return (checkBytes.Concat(cipherText).ToArray(), encodedKey);
		}
		/// <summary>
		/// Calculate the remote AES key for Type 1 encryption
		/// </summary>
		/// <param name="integrationIdentificationNumber">12 digit Integration Identification Number [851][422]</param>
		/// <param name="remoteInitializer">The 48byte command payload sent by the panel with command 0x060E Connection_Request_Access</param>
		/// <returns>The AES key used to decrypt messages from the panel</returns>
		/// <exception cref="Exception"></exception>
		public static byte[] ParseType1Initializer(string integrationIdentificationNumber, byte[] remoteInitializer)
		{
			//integrationIdentificationNumber is 12 digits long, but we only need the first 8 so that is all we check for.
			if (integrationIdentificationNumber.Length < 8) throw new ArgumentException($"integration access code is {integrationIdentificationNumber.Length} digits long; it needs to be at least 8, and should be 12.");
			if (remoteInitializer.Length != 48) throw new ArgumentException(nameof(remoteInitializer));

			var checkBytes = remoteInitializer.Take(16);
			var cipherText = remoteInitializer.Skip(16).Take(32).ToArray();

			byte[] plainText;
			using (var aes = Aes.Create())
			{
				aes.Key = transformKeyString(integrationIdentificationNumber);
				plainText = aes.DecryptEcb(cipherText, PaddingMode.Zeros);
			}

			if (!checkBytes.SequenceEqual(evenIndexes(plainText))) throw new Exception("");

			return oddIndexes(plainText).ToArray();
		}
		static byte[] transformKeyString(string keyString)
		{
			string first8 = keyString.Substring(0, 8);
			//This makes a 32 digit base 10 string, and the reads it as base 16 string which it makes a 16 byte array.
			return Convert.FromHexString($"{first8}{first8}{first8}{first8}");
		}
		static IEnumerable<byte> evenIndexes(IEnumerable<byte> bytes) => bytes.Where((element, index) => index % 2 == 0);
		static IEnumerable<byte> oddIndexes(IEnumerable<byte> bytes) => bytes.Where((element, index) => index % 2 == 1);
	}
}
