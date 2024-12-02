using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace IT_Inventory.Models
{
    public class ITW_Request
    {
        [Key]
        public int id_request { get; set; }
        public string request_npp { get; set; }
        public string request_name { get; set; }
        public DateTime request_dateBorrowed { get; set; }
        public DateTime? request_dateReturned { get; set; }
        public int? request_period { get; set; }
        public string request_status { get; set; }
        public string? note { get; set; }
        public DateTime? created_at { get; set; }
        public string? created_by { get; set; }
        public string? updated_by { get; set; }
        public DateTime? updated_at { get; set; }
        public int id_detail { get; set; }

        //[ForeignKey("id_device")]
        //public Device Device { get; set; }

        //// Navigation property for User
        //[ForeignKey("id_user")]
        //public User User { get; set; }
    }
}
