using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Models
{
    public partial class FaceProfile
    {
        [Key]
        public int FaceProfileId { get; set; }

        public int EmployeeId { get; set; }

        [MaxLength(500)]
        public string? ReferenceImagePath { get; set; }

        public string? FaceEmbedding { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        public DateTime CreatedDate { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int? ModifiedBy { get; set; }

        [ForeignKey(nameof(EmployeeId))]
        public virtual Employee? Employee { get; set; }
    }
}
