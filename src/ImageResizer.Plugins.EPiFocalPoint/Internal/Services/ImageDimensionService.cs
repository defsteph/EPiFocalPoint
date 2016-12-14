using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using EPiServer.Logging;
using ImageResizer.Plugins.EPiFocalPoint.Internal.Data;

namespace ImageResizer.Plugins.EPiFocalPoint.Internal.Services {
	public static class ImageDimensionService {
		private static readonly ISize Invalid = new InvalidSize();
		private static readonly ILogger Logger = LogManager.GetLogger();

		private const string ErrorMessage = "Could not recognize image format.";
		private static readonly Dictionary<byte[], Func<BinaryReader, Size>> ImageFormatDecoders = new Dictionary
			<byte[], Func<BinaryReader, Size>>()
			{
				{new byte[] {0x42, 0x4D}, DecodeBitmap},
				{new byte[] {0x47, 0x49, 0x46, 0x38, 0x37, 0x61}, DecodeGif},
				{new byte[] {0x47, 0x49, 0x46, 0x38, 0x39, 0x61}, DecodeGif},
				{new byte[] {0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A}, DecodePng},
				{new byte[] {0xff, 0xd8}, DecodeJfif},
			};
		public static ISize GetDimensions(Stream stream) {
			if(stream == null || stream.Length <= 0) {
				throw new ArgumentNullException(nameof(stream));
			}
			try {
				using(var binaryReader = new BinaryReader(stream)) {
					try {
						var size = GetDimensions(binaryReader);
						if(size == Size.Empty) {
							using(var image = Image.FromStream(stream, false, false)) {
								size = image.Size;
							}
						}
						return new SizeAdapter(size);
					} catch (Exception ex) {
						Logger.Error("Invalid image data in stream.", ex);
						return Invalid;
					}
				}
			} catch {
				return Invalid;
			}
		}
		private static Size GetDimensions(BinaryReader binaryReader) {
			var maxMagicBytesLength = ImageFormatDecoders.Keys.OrderByDescending(x => x.Length).First().Length;
			var magicBytes = new byte[maxMagicBytesLength];
			for(var i = 0; i < maxMagicBytesLength; i += 1) {
				magicBytes[i] = binaryReader.ReadByte();
				foreach(var decoder in ImageFormatDecoders) {
					if(StartsWith(magicBytes, decoder.Key)) {
						return decoder.Value(binaryReader);
					}
				}
			}
			return Size.Empty;
		}
		private static bool StartsWith(IReadOnlyList<byte> thisBytes, IReadOnlyList<byte> thatBytes) {
			for(var i = 0; i < thatBytes.Count; i += 1) {
				if(thisBytes[i] != thatBytes[i]) {
					return false;
				}
			}
			return true;
		}
		private static short ReadLittleEndianInt16(BinaryReader binaryReader) {
			var bytes = new byte[sizeof(short)];
			for(var i = 0; i < sizeof(short); i += 1) {
				bytes[sizeof(short) - 1 - i] = binaryReader.ReadByte();
			}
			return BitConverter.ToInt16(bytes, 0);
		}
		private static int ReadLittleEndianInt32(BinaryReader binaryReader) {
			var bytes = new byte[sizeof(int)];
			for(var i = 0; i < sizeof(int); i += 1) {
				bytes[sizeof(int) - 1 - i] = binaryReader.ReadByte();
			}
			return BitConverter.ToInt32(bytes, 0);
		}
		private static Size DecodeBitmap(BinaryReader binaryReader) {
			binaryReader.ReadBytes(16);
			var width = binaryReader.ReadInt32();
			var height = binaryReader.ReadInt32();
			return new Size(width, height);
		}
		private static Size DecodeGif(BinaryReader binaryReader) {
			int width = binaryReader.ReadInt16();
			int height = binaryReader.ReadInt16();
			return new Size(width, height);
		}
		private static Size DecodePng(BinaryReader binaryReader) {
			binaryReader.ReadBytes(8);
			var width = ReadLittleEndianInt32(binaryReader);
			var height = ReadLittleEndianInt32(binaryReader);
			return new Size(width, height);
		}
		private static Size DecodeJfif(BinaryReader binaryReader) {
			while(binaryReader.ReadByte() == 0xff) {
				var marker = binaryReader.ReadByte();
				var chunkLength = ReadLittleEndianInt16(binaryReader);
				if(marker == 0xc0) {
					binaryReader.ReadByte();
					int height = ReadLittleEndianInt16(binaryReader);
					int width = ReadLittleEndianInt16(binaryReader);
					return new Size(width, height);
				}
				if(chunkLength < 0) {
					var uchunkLength = (ushort)chunkLength;
					binaryReader.ReadBytes(uchunkLength - 2);
				} else {
					binaryReader.ReadBytes(chunkLength - 2);
				}
			}
			throw new ArgumentException(ErrorMessage);
		}
	}
}