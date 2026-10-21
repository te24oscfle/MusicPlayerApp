using System.Security.Cryptography;
using ATL;

namespace MusicPlayerApp
{
    public class AlbumCover
    {
        public int? AlbumCoverId { get; private set; }
        public byte[] ImageData { get; private set; }
        public string MimeType { get; private set; }
        public string Hash { get; private set; }

        public AlbumCover(int albumCoverId, byte[] imageData, string mimeType, string hash)
        {
            AlbumCoverId = albumCoverId;
            ImageData = imageData;
            MimeType = mimeType;
            Hash = hash;
        }

        public AlbumCover(PictureInfo pictureInfo)
        {
            ImageData = pictureInfo.PictureData;
            MimeType = pictureInfo.MimeType;
            Hash = GetHashFromImageData(pictureInfo.PictureData);
        }

        private static string GetHashFromImageData(byte[] imageData)
        {
            return Convert.ToHexString(SHA256.HashData(imageData));
        }
        public void InitilizeAlbumCoverId(int albumCoverId)
        {   
            if(AlbumCoverId.HasValue)
                throw new InvalidOperationException("The AlbumCoverId has already been initilized");
            
            if (albumCoverId <= 0)
                throw new ArgumentOutOfRangeException($"AlbumCoverId must be greater than 0: {nameof(albumCoverId)}");
            
            AlbumCoverId = albumCoverId;
        }
    }
}