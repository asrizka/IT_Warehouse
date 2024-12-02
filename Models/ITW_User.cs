using System.ComponentModel.DataAnnotations;

namespace IT_Inventory.Models
{
    public class ITW_User
    {
        [Key]
        public int id_user { get; set; }
        public string user_name { get; set; }
        public string user_role { get; set; }
        public string? user_email { get; set; }
        public string? user_pass { get; set; }
        public DateTime? created_at { get; set; }
        public string? created_by { get; set; }
        public DateTime? updated_at { get; set; }
        public string? updated_by { get; set; }

        //public ICollection<Request> Request { get; set; }
    }
}
