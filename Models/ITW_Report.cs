using Microsoft.EntityFrameworkCore;
    
namespace IT_Inventory.Models
{
    [Keyless]
    public class ITW_Report
    {
        public string DetailSN { get; set; }
        public string? DetailTag { get; set; }
        public string DeviceName { get; set; }
        public DateTime? DateAndTime { get; set; }
        public string Status { get; set; }
    }
}
