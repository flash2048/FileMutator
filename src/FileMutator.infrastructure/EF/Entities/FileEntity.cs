using System.ComponentModel.DataAnnotations;

namespace FileMutator.infrastructure.EF.Entities
{
    public class FileEntity
    {
        [Required]
        public Guid Id { get; set; }

        public long Size { get; set; }
        public string Name { get; set; } = null!;
        public string ContentType { get; set; } = null!;

        [Required]
        public byte[] FileData { get; set; } = Array.Empty<byte>();

        public bool IsMutated { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
