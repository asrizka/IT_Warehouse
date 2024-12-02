namespace IT_Inventory.Models
{
    public class DeviceUploadModel
    {
        public string category_name { get; set; }  // This comes from the Excel data
        public string device_type { get; set; }
        public string device_name { get; set; }
        public string device_eligibility { get; set; }
    }
}
