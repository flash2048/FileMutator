using System.ComponentModel.DataAnnotations;
namespace FileMutator.infrastructure.Models
{
    public class FileInfoFull : FileInfoShort
    {
        [Required]
        public string FileText { get; set; } = null!;
    }
}