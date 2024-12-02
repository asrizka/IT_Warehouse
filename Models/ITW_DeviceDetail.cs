using System.ComponentModel.DataAnnotations;

namespace IT_Inventory.Models
{
    public class ITW_DeviceDetail
    {
        [Key]
        public int id_detail { get; set; }
        public string? detail_tag { get; set; }
        public string detail_sn { get; set; }
        public string? detail_status { get; set; }
        public string? detail_location { get; set; }
        public string? note { get; set; }
        public string? created_by { get; set; }
        public DateTime? created_at { get; set; }
        public string? updated_by { get; set; }
        public DateTime? updated_at { get; set; }
        public int id_device { get; set; }
    }
}
