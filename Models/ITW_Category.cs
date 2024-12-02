using System.ComponentModel.DataAnnotations;

namespace IT_Inventory.Models
{
    public class ITW_Category
    {
        [Key]
        public int id_category { get; set; }
        public string category_name { get; set; }
        public string? category_code { get; set; }
        public DateTime? created_at { get; set; }
        public string? created_by { get; set; }
        public DateTime? updated_at { get; set; }
        public string? updated_by { get; set; }
    }
}
