using System.Reflection;

namespace Bible.Data
{
    public static class ResourceHelper
    {
        private const string Namespace = "Bible.Data";

        public static Stream? GetManifestResourceStream(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(resourceName);
            return stream;
        }

        public static Stream GetStreamFromExtension(string filePath)
        {
            var fileExtension = Path.GetExtension(filePath);
            var fileName = filePath.Replace('\\', '.').Replace('/', '.').Replace('-', '_');
            var resourceName = $"{Namespace}{fileExtension}.{fileName}";
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                fileName = Path.GetFileName(filePath).Replace('-', '_');
                var resources = assembly.GetManifestResourceNames();
                var altNames = resources.Where(n => n.Contains(fileName));
                var alt = altNames.FirstOrDefault();
                if (!string.IsNullOrEmpty(alt))
                {
                    stream = assembly.GetManifestResourceStream(alt);
                    if (stream != null)
                    {
                        throw new FileNotFoundException($"Resource '{resourceName}' found at: {alt}");
                    }
                    alt = string.Join("\n", altNames);
                }
                if (string.IsNullOrEmpty(alt))
                {
                    altNames = resources.Where(n => n.EndsWith(fileExtension));
                    alt = altNames.FirstOrDefault();
                }
                throw new FileNotFoundException($"Resource '{resourceName}' not found. Closest:\n{alt}");
            }

            return stream;
        }

        public static async Task WriteStreamAsync(string fileName, Stream inputStream)
        {
            ArgumentNullException.ThrowIfNull(inputStream);
            ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

            // Define the path where the file will be saved on the server
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save the file to the server's file system
            await using var fileStream = new FileStream(filePath, FileMode.Create);
            await inputStream.CopyToAsync(fileStream);
            inputStream.Position = 0; // Reset the position of the input stream
        }
    }
}