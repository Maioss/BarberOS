namespace BarberOS.Api.Services
{
    public class ProfilePhotoStorage
    {
        public const long MaxBytes = 5 * 1024 * 1024;
        private const string Folder = "photos";

        private readonly IWebHostEnvironment _env;

        public ProfilePhotoStorage(IWebHostEnvironment env) => _env = env;

        public record Result(bool Ok, string? RelativeUrl, string? Error);

        public async Task<Result> SaveAsync(IFormFile? file, Guid userId, CancellationToken ct)
        {
            if (file is null || file.Length == 0)
                return new Result(false, null, "No se adjuntó ninguna imagen.");

            if (file.Length > MaxBytes)
                return new Result(false, null, "La imagen no puede superar 5 MB.");

            await using var upload = file.OpenReadStream();

            var header = new byte[12];
            var read = await upload.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, ct);
            var extension = DetectExtension(header.AsSpan(0, read));

            if (extension is null)
                return new Result(false, null, "Solo se permiten imágenes JPEG, PNG o WebP.");

            var directory = Path.Combine(WebRoot, Folder);
            Directory.CreateDirectory(directory);

            // El sufijo por subida evita que el navegador sirva la foto anterior desde cache.
            var fileName = $"{userId:N}-{DateTime.UtcNow.Ticks:x}{extension}";

            await using (var destination = File.Create(Path.Combine(directory, fileName)))
            {
                upload.Position = 0;
                await upload.CopyToAsync(destination, ct);
            }

            RemovePrevious(directory, userId, fileName);

            return new Result(true, $"/{Folder}/{fileName}", null);
        }

        private static void RemovePrevious(string directory, Guid userId, string keep)
        {
            foreach (var path in Directory.EnumerateFiles(directory, $"{userId:N}-*"))
            {
                if (Path.GetFileName(path) == keep) continue;
                try { File.Delete(path); } catch (IOException) { }
            }
        }

        private string WebRoot =>
            _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");

        /// <summary>El Content-Type lo pone el cliente y puede mentir; esto mira el contenido.</summary>
        private static string? DetectExtension(ReadOnlySpan<byte> header)
        {
            if (header.Length >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
                return ".jpg";

            if (header.Length >= 8 &&
                header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
                return ".png";

            if (header.Length >= 12 &&
                header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
                header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50)
                return ".webp";

            return null;
        }
    }
}
