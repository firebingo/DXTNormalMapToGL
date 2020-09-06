using ImageMagick;
using System;
using System.IO;
using System.Linq;

namespace DXTNormalMapToGl
{
	class Program
	{
		static void Main(string[] args)
		{
			if(args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
				Console.WriteLine("Invalid Path");

			var path = args[0];
			if(!File.Exists(path))
				Console.WriteLine($"File at \"{path}\" not found");

			try
			{
				using var image = new MagickImage(args[0]);

				using var memStream = new MemoryStream();
				using var binaryWriter = new BinaryWriter(memStream);

				CovertDXTToGL(image, binaryWriter);

				memStream.Seek(0, SeekOrigin.Begin);

				var settings = new MagickReadSettings
				{
					Width = image.Width,
					Height = image.Height,
					Depth = 16,
					Format = MagickFormat.Rgb,
				};

				using var newimage = new MagickImage(memStream, settings)
				{
					Format = MagickFormat.Png
				};

				newimage.Write(Path.Combine(Path.GetDirectoryName(path), $"{Path.GetFileNameWithoutExtension(path)}_normal.png"));
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex);
			}
		}

		public static void CovertDXTToGL(MagickImage source, BinaryWriter pixelDestStream)
		{
			var sourcePixels = source.GetPixels();
			for (var y = 0; y < source.Height; ++y)
			{
				for(var x = 0; x < source.Width; ++x)
				{
					var sourcePixel = sourcePixels[x, y];
					//Im assuming there is a g and a channel
					//magick.net 16 reads these always as 16bit even if the source image isint
					float sourcey = sourcePixel.GetChannel(1) / 65536;
					float sourcex = sourcePixel.GetChannel(3) / 65536;

					float destx = sourcex;
					float desty = 1.0f - sourcey;
					float destz = (float)Math.Sqrt(1.0f - (sourcex * sourcex + sourcey * sourcey));

					pixelDestStream.Write((short)(destx * 65536));
					pixelDestStream.Write((short)(desty * 65536));
					pixelDestStream.Write((short)(destz * 65536));
				}
			}
		}
	}
}
