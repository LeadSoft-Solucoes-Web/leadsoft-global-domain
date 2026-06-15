using LeadSoft.Common.GlobalDomain.Entities.CloudFiles;

using SkiaSharp;

using Xunit.Sdk;

namespace LeadSoft.Common.GlobalDomain.Tests.Entities.CloudFiles
{
    public class CloudImageTests(ITestOutputHelper output)
    {
        // ── helpers ─────────────────────────────────────────────────────────────

        /// <summary>Creates an in-memory PNG stream filled with a solid color.</summary>
        private static MemoryStream CreatePng(int width, int height)
        {
            using var bitmap = new SKBitmap(width, height);
            bitmap.Erase(SKColors.CornflowerBlue);
            using var image = SKImage.FromBitmap(bitmap);
            using var encoded = image.Encode(SKEncodedImageFormat.Png, 100);
            var ms = new MemoryStream();
            encoded.SaveTo(ms);
            ms.Position = 0;
            return ms;
        }

        /// <summary>Decodes pixel dimensions from a PNG stream (rewinds before reading).</summary>
        private static (int Width, int Height) DecodeDimensions(Stream stream)
        {
            stream.Position = 0;
            using var bitmap = SKBitmap.Decode(stream);
            return (bitmap.Width, bitmap.Height);
        }

        // ── CloudImage.Resize ────────────────────────────────────────────────────

        [Fact]
        public void Resize_ReturnsNonEmptyStreamPositionedAtZero()
        {
            using var input = CreatePng(800, 600);
            using var result = CloudImage.Resize(input);

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
            Assert.Equal(0, result.Position);
        }

        [Fact]
        public void Resize_OutputIsDecodableAsPng()
        {
            using var input = CreatePng(800, 600);
            using var result = CloudImage.Resize(input);

            var (w, h) = DecodeDimensions(result);
            Assert.True(w > 0);
            Assert.True(h > 0);
        }

        /// <summary>
        /// Verifies that Resize respects ResizeMode.Max semantics: the result fits
        /// within the requested bounds while preserving the original aspect ratio.
        /// </summary>
        [Theory]
        [InlineData(2000, 1000, 1024, 1024, 1024,  512)]   // landscape  → capped pela largura
        [InlineData( 500, 2000, 1024, 1024,  256, 1024)]   // portrait   → capped pela altura
        [InlineData(1500, 1500, 1024, 1024, 1024, 1024)]   // quadrado   → ambos os eixos
        [InlineData(2048,  768, 1024, 1024, 1024,  384)]   // ultra-wide → largura domina
        public void Resize_ScalesDown_PreservingAspectRatio(
            int srcW, int srcH, int maxW, int maxH, int expectedW, int expectedH)
        {
            using var input = CreatePng(srcW, srcH);
            using var result = CloudImage.Resize(input, maxW, maxH);

            var (w, h) = DecodeDimensions(result);
            output.WriteLine($"{srcW}x{srcH} → Resize({maxW},{maxH}) → {w}x{h}  (expected {expectedW}x{expectedH})");

            Assert.Equal(expectedW, w);
            Assert.Equal(expectedH, h);
        }

        /// <summary>
        /// Images smaller than or equal to the target must NOT be upscaled.
        /// </summary>
        [Theory]
        [InlineData( 100,  100)]
        [InlineData( 200,  150)]
        [InlineData(1024, 1024)]
        [InlineData(  50,  800)]
        public void Resize_DoesNotUpscale_WhenImageFitsTarget(int srcW, int srcH)
        {
            using var input = CreatePng(srcW, srcH);
            using var result = CloudImage.Resize(input, 1024, 1024);

            var (w, h) = DecodeDimensions(result);
            output.WriteLine($"{srcW}x{srcH} → Resize(1024,1024) → {w}x{h}  (sem upscale esperado)");

            Assert.Equal(srcW, w);
            Assert.Equal(srcH, h);
        }

        [Fact]
        public void Resize_DefaultTargetIs1024x1024()
        {
            using var input = CreatePng(2000, 2000);
            using var result = CloudImage.Resize(input);

            var (w, h) = DecodeDimensions(result);
            Assert.Equal(1024, w);
            Assert.Equal(1024, h);
        }

        // ── CloudImage.CreateThumbnail ───────────────────────────────────────────

        [Fact]
        public void CreateThumbnail_ReturnsNonEmptyStreamPositionedAtZero()
        {
            using var input = CreatePng(1200, 800);
            using var result = CloudImage.CreateThumbnail(input);

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
            Assert.Equal(0, result.Position);
        }

        [Fact]
        public void CreateThumbnail_OutputIsDecodableAsPng()
        {
            using var input = CreatePng(1200, 800);
            using var result = CloudImage.CreateThumbnail(input);

            var (w, h) = DecodeDimensions(result);
            Assert.True(w > 0);
            Assert.True(h > 0);
        }

        [Theory]
        [InlineData(1200,  800, 256, 256, 256, 170)]   // landscape → capped pela largura
        [InlineData( 800, 1200, 256, 256, 170, 256)]   // portrait  → capped pela altura
        [InlineData( 512,  512, 256, 256, 256, 256)]   // quadrado  → downscale exato
        public void CreateThumbnail_ScalesDown_PreservingAspectRatio(
            int srcW, int srcH, int thumbW, int thumbH, int expectedW, int expectedH)
        {
            using var input = CreatePng(srcW, srcH);
            using var result = CloudImage.CreateThumbnail(input, thumbW, thumbH);

            var (w, h) = DecodeDimensions(result);
            output.WriteLine($"{srcW}x{srcH} → Thumbnail({thumbW},{thumbH}) → {w}x{h}  (expected {expectedW}x{expectedH})");

            Assert.Equal(expectedW, w);
            Assert.Equal(expectedH, h);
        }

        /// <summary>
        /// Images that already fit within the thumbnail box must NOT be upscaled.
        /// </summary>
        [Theory]
        [InlineData(100, 100)]
        [InlineData( 50, 200)]
        [InlineData(256, 256)]
        public void CreateThumbnail_DoesNotUpscale_WhenImageFitsTarget(int srcW, int srcH)
        {
            using var input = CreatePng(srcW, srcH);
            using var result = CloudImage.CreateThumbnail(input, 256, 256);

            var (w, h) = DecodeDimensions(result);
            output.WriteLine($"{srcW}x{srcH} → Thumbnail(256,256) → {w}x{h}  (sem upscale esperado)");

            Assert.Equal(srcW, w);
            Assert.Equal(srcH, h);
        }

        [Fact]
        public void CreateThumbnail_DefaultTargetIs256x256()
        {
            using var input = CreatePng(1000, 1000);
            using var result = CloudImage.CreateThumbnail(input);

            var (w, h) = DecodeDimensions(result);
            Assert.Equal(256, w);
            Assert.Equal(256, h);
        }

        // ── Inspeção manual (sem assertions) ─────────────────────────────────────

        [Fact]
        public void Resize_LogVariousDimensionsForManualInspection()
        {
            (int W, int H)[] inputs = [(1920, 1080), (1080, 1920), (800, 600), (300, 300), (100, 50)];
            foreach (var (srcW, srcH) in inputs)
            {
                using var input = CreatePng(srcW, srcH);
                using var resized = CloudImage.Resize(input, 1024, 1024);
                using var thumb = CloudImage.CreateThumbnail(CreatePng(srcW, srcH), 256, 256);

                var (rW, rH) = DecodeDimensions(resized);
                var (tW, tH) = DecodeDimensions(thumb);
                output.WriteLine($"{srcW,5}x{srcH,-5} → Resize={rW}x{rH}   Thumbnail={tW}x{tH}");
            }
        }
    }
}
