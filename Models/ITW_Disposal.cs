using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT_Inventory.Models
{
    public class ITW_Disposal
    {
        [Key]
        public int id_disposal { get; set; }
        public string? disposal_certificate { get; set; }
        public string? disposal_documentation { get; set; }
        public string? disposal_status { get; set; }
        public DateTime? disposal_date { get; set; }
        public string? note { get; set; }
        public DateTime? created_at { get; set; }
        public string? created_by { get; set; }
        public DateTime? updated_at { get; set; }
        public string? updated_by { get; set; }
        public int id_detail { get; set; }
        [NotMapped]
        public IFormFile certificateFile { get; set; }
        [NotMapped]
        public IFormFile documentationFile { get; set; }
    }
}
