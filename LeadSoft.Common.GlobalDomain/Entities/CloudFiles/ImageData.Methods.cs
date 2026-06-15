using Amazon.S3;

using LeadSoft.Adapter.AWS;
using LeadSoft.Common.Library.Extensions;

using SkiaSharp;

namespace LeadSoft.Common.GlobalDomain.Entities.CloudFiles
{
    /// <summary>
    /// Image Data methods
    ///
    /// TODO: Extending a future Interface feature, methods as delete, resize, download, and other stuff must be developed here
    /// </summary>
    public partial class ImageData
    {
        /// <summary>
        /// base constructor
        /// </summary>
        public ImageData()
        {
        }

        /// <summary>
        /// base constructor
        /// </summary>
        public ImageData(string aTitle, string aImageUrl)
        {
            Title = aTitle.IsSomething() ? aTitle : Id.GetString();
            Description = aTitle;
            Key = aImageUrl;
            Url = new(aImageUrl);
            Bytes = 0;
            ThumbnailKey = aImageUrl;
            ThumbnailUrl = new(aImageUrl);
        }

        /// <summary>
        /// Async Method to Upload Image and set Url and Thumbnail Url for ImageData
        /// </summary>
        /// <param name="aAmazonS3">AmazonS3 instance</param>
        /// <param name="aFileStream">Image File Stream</param>
        /// <param name="aFullFileName">Full File Name</param>
        /// <param name="aAccessType">S3 Access Type</param>
        /// <param name="aStorageType">S3 storage Type</param>
        /// <param name="aCreateThumbnail">Optional Create or not Thumbnail flag. Default: true.</param>
        public async Task<ImageData> UploadAsync(AmazonS3 aAmazonS3, Stream aFileStream, string aFullFileName, S3CannedACL aAccessType, S3StorageClass aStorageType, string aTitle = "", bool aCreateThumbnail = true, int aThumbX = 256, int aThumbY = 256)
        {
            Title = aTitle.IsSomething() ? aTitle : Id.GetString();
            Description = aFullFileName;
            Key = aFullFileName;
            Bytes = (int)aFileStream.Length;

            if (aCreateThumbnail)
            {
                string thumbnailFileName = aFullFileName.Replace(".", "_thumbnail.");

                Stream aThumbCopy = new MemoryStream();
                await aFileStream.Reset().CopyToAsync(aThumbCopy);

                ThumbnailKey = thumbnailFileName;
                ThumbnailUrl = await aAmazonS3.StoreFileAsync(CreateThumbnail(aThumbCopy.Reset(), aThumbX, aThumbY),
                                                              thumbnailFileName, aAccessType, aStorageType);
            }

            Url = await aAmazonS3.StoreFileAsync(aFileStream.Reset(), aFullFileName, aAccessType, aStorageType);

            return this;
        }

        /// <summary>
        /// Sets a new Id to image
        /// </summary>
        /// <param name="aId"></param>
        /// <returns></returns>
        public ImageData SetId(Guid aId)
        {
            Id = aId;
            return this;
        }

        public ImageData ProvideUrl(AmazonS3 aAmazonS3)
        {
            if (Key.IsSomething())
                Url = aAmazonS3.GetUrl(Key);

            if (ThumbnailKey.IsSomething())
                ThumbnailUrl = aAmazonS3.GetUrl(ThumbnailKey);

            return this;
        }

        public async Task<ImageData> DeleteContentAsync(AmazonS3 aAmazonS3)
        {
            if (Key.IsSomething())
            {
                await aAmazonS3.DeleteAsync(Key);
                Key = string.Empty;
            }

            if (ThumbnailKey.IsSomething())
            {
                await aAmazonS3.DeleteAsync(ThumbnailKey);
                ThumbnailKey = string.Empty;
            }

            return this;
        }

        /// <summary>
        /// Method to create resized Thumbnail from image stream
        /// </summary>
        /// <param name="aImageStream">Image stream</param>
        /// <param name="aX">Image thumbnail X axis pixels</param>
        /// <param name="aY">Image thumbnail Y axis pixels</param>
        /// <returns>Stream</returns>
        public static Stream CreateThumbnail(Stream aImageStream, int aX = 256, int aY = 256)
            => ResizeInternal(aImageStream, aX, aY);

        /// <summary>
        /// Method to create resized image stream
        /// </summary>
        /// <param name="aFile">Image Stream</param>
        /// <param name="aX">Image thumbnail X axis pixels</param>
        /// <param name="aY">Image thumbnail Y axis pixels</param>
        /// <returns>Stream</returns>
        public static Stream Resize(Stream aFile, int aX = 1024, int aY = 1024)
            => ResizeInternal(aFile, aX, aY);

        private static Stream ResizeInternal(Stream aImageStream, int aMaxWidth, int aMaxHeight)
        {
            using var original = SKBitmap.Decode(aImageStream);
            double ratio = Math.Min((double)aMaxWidth / original.Width, (double)aMaxHeight / original.Height);
            int newWidth = ratio < 1 ? (int)(original.Width * ratio) : original.Width;
            int newHeight = ratio < 1 ? (int)(original.Height * ratio) : original.Height;
            using var resized = original.Resize(new SKSizeI(newWidth, newHeight), SKSamplingOptions.Default);
            using var image = SKImage.FromBitmap(resized);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            var result = new MemoryStream();
            data.SaveTo(result);
            result.Position = 0;
            return result;
        }
    }
}
