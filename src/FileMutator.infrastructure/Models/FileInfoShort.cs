using System.ComponentModel.DataAnnotations;

namespace FileMutator.infrastructure.Models
{
    public class FileInfoShort
    {
        [Required]
        public Guid Id { get; set; }
        public long Size { get; set; }
        public string Name { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public bool IsMutated { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
