using System;
using System.Collections.Generic;

namespace Hosta.Tools
{
	/// <summary>
	/// A static class that provides functionality for dealing with blobs
	/// </summary>
	public static class Blobs
	{
		/// <summary>
		/// Combines multiple blobs into a single blob.
		/// </summary>
		/// <param name="sources"></param>
		/// <returns></returns>
		public static byte[] Combine(params byte[][] sources)
		{
			List<byte> destination = new List<byte>();
			foreach (byte[] blob in sources) destination.AddRange(blob);
			return destination.ToArray();
		}

		/// <summary>
		/// Splits a source blob into multiple destination blobs.
		/// </summary>
		/// <param name="source">The source blob.</param>
		/// <param name="destinations">The destination blobs.</param>
		public static void Split(byte[] source, params byte[][] destinations)
		{
			// Check that the combined destinations are as big as the source
			int total = 0;
			foreach (byte[] destination in destinations) total += destination.Length;
			if (total != source.Length) throw new FormatException("Package cannot be split into the correct sized parts.");

			// Copy each part over
			int index = 0;
			foreach (byte[] destination in destinations)
			{
				Array.Copy(source, index, destination, 0, destination.Length);
				index += destination.Length;
			}
		}
	}
}