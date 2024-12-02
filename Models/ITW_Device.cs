using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT_Inventory.Models
{
    public class ITW_Device
    {
        [Key]
        public int id_device { get; set; }
        public string device_name { get; set; }
        public string device_type { get; set; }
        public string device_eligibility { get; set; }
        public int? qty_total { get; set; }
        public int? qty_available { get; set; }
        public int? qty_borrowed { get; set; }
        public int? qty_used { get; set; }
        public int? qty_disposed { get; set; }
        public string? created_by { get; set; }
        public DateTime? created_at { get; set; }
        public string? updated_by { get; set; }
        public DateTime? updated_at { get; set; }

        public int id_category { get; set; }

        [ForeignKey("id_category")]
        public ITW_Category ITW_Category { get; set; }

        //public bool IsDeleted { get; set; } = false;

        //public ICollection<Request> Request { get; set; }
    }
}
