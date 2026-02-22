using System;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using System.Diagnostics;

namespace CobaltCord.Classes
{
    public static class AvatarHelper
    {
        private static readonly string AvatarFolderName = "Avatars";

        public static async Task SetAvatarFromHash(Image targetImage, string uid, string hash, string imageUrl)
        {
            if (targetImage == null || string.IsNullOrWhiteSpace(uid) || string.IsNullOrWhiteSpace(hash))
                return;

            // Create the folders needed for caching avatars
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFolder avatarFolder = await localFolder.CreateFolderAsync(AvatarFolderName, CreationCollisionOption.OpenIfExists);

            // How the file name should look like when it is cached so we can easily find it in the end.
            string fileName = $"{uid}-{hash}.png";

            StorageFile avatarFile = null;

            // A try/catch is used here because if we don't, the thing will end up slowing down heavily due to the amount of exceptions.
            try
            {
                avatarFile = await avatarFolder.GetFileAsync(fileName);
            }
            catch
            {
                avatarFile = null; // File doesn't exist, we'll handle it later
            }

            if (avatarFile != null)
            {
                // Open the cached avatar image safely
                try
                {
                    using (var stream = await avatarFile.OpenAsync(FileAccessMode.Read))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        await bitmap.SetSourceAsync(stream);
                        targetImage.Source = bitmap;
                    }
                }
                catch
                {
                    // Failed to load cached file, ignore and fallback to download
                    avatarFile = null;
                }
            }

            if (avatarFile == null)
            {
                // Use await to ensure the async method completes safely
                await DownloadAndStoreImage(targetImage, imageUrl, avatarFolder, fileName);
            }
        }

        private static async Task DownloadAndStoreImage(Image targetImage, string url, StorageFolder folder, string fileName)
        {
            if (string.IsNullOrWhiteSpace(url) || targetImage == null)
                return;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    byte[] bytes = await client.GetByteArrayAsync(url);

                    // Create or replace the target file directly
                    StorageFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteBytesAsync(file, bytes);

                    // Load image into Image control
                    using (var stream = new InMemoryRandomAccessStream())
                    {
                        using (var writer = new DataWriter(stream))
                        {
                            writer.WriteBytes(bytes);
                            await writer.StoreAsync();
                            writer.DetachStream();
                        }

                        stream.Seek(0);
                        BitmapImage bitmap = new BitmapImage();
                        await bitmap.SetSourceAsync(stream);
                        targetImage.Source = bitmap;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Avatar download has failed: {ex.Message}");
            }
        }

        public static async Task<BitmapImage> DownloadImage(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    byte[] bytes = await client.GetByteArrayAsync(url);
                    using (var stream = new InMemoryRandomAccessStream())
                    {
                        using (var writer = new DataWriter(stream))
                        {
                            writer.WriteBytes(bytes);
                            await writer.StoreAsync();
                            writer.DetachStream();
                        }

                        stream.Seek(0);
                        BitmapImage bitmap = new BitmapImage();
                        await bitmap.SetSourceAsync(stream);
                        return bitmap;
                    }
                }
            }
            catch
            {
                return null;
            }
        }
    }
}