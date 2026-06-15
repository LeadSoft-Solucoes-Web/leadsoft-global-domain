using LeadSoft.Adapter.AWS;
using LeadSoft.Common.Library.Extensions;

using SkiaSharp;

using static LeadSoft.Common.Library.Enumerators.Enums;

namespace LeadSoft.Common.GlobalDomain.Entities.CloudFiles
{
    public partial class CloudImage
    {
        /// <summary>
        /// Empty Constructor
        /// </summary>
        public CloudImage() : base()
        {
        }

        /// <summary>
        /// Main Constructor
        /// </summary>
        /// <remarks>
        /// Use File extension to set the mime type on file. To get that, there is a function to find the correct enum.
        /// If the file extension doesn't exist, create a new enum! Or request it on developers e-mail.
        ///     Enums.GetByDescription FileExtension (fileType)
        /// </remarks>
        /// <param name="aBucket">Bucket name</param>
        /// <param name="aFileExtension">File mime type in FileExtension.</param>
        /// <param name="aKey">cloud key</param>
        /// <param name="aSize">file size in bytes</param>
        /// <param name="aTitle">file size in bytes</param>
        /// <param name="aDescription">file size in bytes</param>
        /// <param name="aUrl">file size in bytes</param>
        /// <param name="aThumbnailUrl">file size in bytes</param>
        /// <param name="aThumbnailKey">file size in bytes</param>
        public CloudImage(
            string aBucket,
            FileExtension aFileExtension,
            string aKey,
            long aSize,
            string aTitle = null,
            string aDescription = null,
            Uri? aUrl = null,
            Uri? aThumbnailUrl = null,
            string? aThumbnailKey = "")
            : base(aBucket, aFileExtension, aKey, aSize)
        {
            Title = aTitle;
            Description = aDescription;
            Url = aUrl ?? null;
            ThumbnailUrl = aThumbnailUrl ?? null;
            ThumbnailKey = aThumbnailKey ?? string.Empty;
        }

        /// <summary>
        /// Method that provides the permanent URL from a image file and it's thumbnail on Amazon S3 and sets it into class
        /// </summary>
        /// <param name="aAmazonS3">Amazon S3 instance</param>
        /// <returns>Self for chain call.</returns>
        public new CloudImage ProvideUrl(AmazonS3 aAmazonS3)
        {
            base.ProvideUrl(aAmazonS3);

            if (ThumbnailKey.IsSomething())
                ThumbnailUrl = aAmazonS3.GetUrl(ThumbnailKey);

            return this;
        }

        /// <summary>
        /// Method that provides the temporary URL from a image file and it's thumbnail on Amazon S3 and sets it into class
        /// </summary>
        /// <param name="aAmazonS3">Amazon S3 instance</param>
        /// <param name="aSeconds">Seconds to expire</param>
        /// <returns>Self for chain call.</returns>
        public new CloudImage ProvideTemporaryUrl(AmazonS3 aAmazonS3, int aSeconds)
        {
            base.ProvideTemporaryUrl(aAmazonS3, aSeconds);

            if (ThumbnailKey.IsSomething())
                ThumbnailUrl = aAmazonS3.GetTemporaryUrl(ThumbnailKey, aSeconds);

            return this;
        }

        /// <summary>
        /// Method that creates a thumbnail from a image stream
        /// </summary>
        /// <param name="aImageStream">Image stream</param>
        /// <param name="aWidth">Width</param>
        /// <param name="aHeight">Height</param>
        /// <returns>Thumbnail image stream</returns>
        public static Stream CreateThumbnail(Stream aImageStream, int aWidth = 256, int aHeight = 256)
            => ResizeInternal(aImageStream, aWidth, aHeight);

        /// <summary>
        /// Method that resizes a image stream
        /// </summary>
        /// <param name="aImageStream">Image stream</param>
        /// <param name="aWidth">Width</param>
        /// <param name="aHeight">Height</param>
        /// <returns>Resized image stream</returns>
        public static Stream Resize(Stream aImageStream, int aWidth = 1024, int aHeight = 1024)
            => ResizeInternal(aImageStream, aWidth, aHeight);

        private static Stream ResizeInternal(Stream aImageStream, int aMaxWidth, int aMaxHeight)
        {
            using var original = SKBitmap.Decode(aImageStream);
            var (width, height) = FitWithinBounds(original.Width, original.Height, aMaxWidth, aMaxHeight);
            using var resized = original.Resize(new SKSizeI(width, height), SKSamplingOptions.Default);
            using var image = SKImage.FromBitmap(resized);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            var result = new MemoryStream();
            data.SaveTo(result);
            result.Position = 0;
            return result;
        }

        private static (int width, int height) FitWithinBounds(int srcWidth, int srcHeight, int maxWidth, int maxHeight)
        {
            if (srcWidth <= maxWidth && srcHeight <= maxHeight)
                return (srcWidth, srcHeight);

            var ratio = Math.Min((double)maxWidth / srcWidth, (double)maxHeight / srcHeight);
            return ((int)(srcWidth * ratio), (int)(srcHeight * ratio));
        }

        /// <summary>
        /// Method that deletes on S3 the image and it's thumbnail and clear class
        /// </summary>
        /// <param name="aAmazonS3">Amazon S3 instance</param>
        /// <returns>Self for chain call.</returns>
        public new async Task<CloudImage> DeleteContentAsync(AmazonS3 aAmazonS3)
        {
            await base.DeleteContentAsync(aAmazonS3);

            if (ThumbnailKey.IsSomething())
            {
                await aAmazonS3.DeleteAsync(ThumbnailKey);
                ThumbnailKey = string.Empty;
            }

            return this;
        }

        /// <summary>
        /// Clear all properties
        /// </summary>
        /// <returns>Self for chain call.</returns>
        public new CloudImage Clear()
        {
            base.Clear();
            Title = string.Empty;
            Description = string.Empty;
            ThumbnailUrl = null;
            ThumbnailKey = string.Empty;

            return this;
        }
    }
}