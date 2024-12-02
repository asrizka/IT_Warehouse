using IT_Inventory.Data;
using IT_Inventory.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;


namespace IT_Inventory.Controllers
{
    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IWebHostEnvironment environment;

        public TransactionController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            this.context = context;
            this.environment = environment;
        }

        //Request
        public IActionResult Request()
        {
            var req = context.ITW_Request.ToList();
            var devices = (from detail in context.ITW_DeviceDetail
                           join device in context.ITW_Device
                           on detail.id_device equals device.id_device
                           select new
                           {
                               detail.id_detail,
                               detail.detail_tag,
                               device.device_name
                           }).ToList();
            //var devices = context.ITW_DeviceDetail.ToList();
            ViewBag.Devices = devices;
            //ViewBag.Users = context.ITW_User.ToList();
            return View(req);
            //var req = context.Request
            //    .Include(r => r.Device)
            //    .Include(r => r.User)
            //    .ToList();
            //return View(req);
        }


        //AddRequest
        [HttpGet]
        public IActionResult AddRequest(int id = 0)
        {
            var request = id > 0 ? context.ITW_Request.Find(id) : new Models.ITW_Request();
            PopulateDropdowns(request.id_detail);
            return View(request);
        }

        [HttpPost]
        public IActionResult AddRequest(int id, Models.ITW_Request data)
        {
            if (ModelState.IsValid)
            {
                var deviceDetail = context.ITW_DeviceDetail.FirstOrDefault(d => d.id_detail == data.id_detail);
                //pastiin devicenya ada
                if (deviceDetail == null)
                {
                    ModelState.AddModelError("id_detail", "The selected device does not exist.");
                    PopulateDropdowns();
                    return View(data);
                }

                var device = context.ITW_Device.FirstOrDefault(d => d.id_device == deviceDetail.id_device);

                if (id > 0) //edit
                {   
                    var existingRequest = context.ITW_Request.Find(id);
                    if (existingRequest != null)
                    {
                        existingRequest.request_npp = data.request_npp;
                        existingRequest.request_name = data.request_name;
                        existingRequest.request_dateBorrowed = data.request_dateBorrowed;
                        existingRequest.request_dateReturned = data.request_dateReturned;
                        existingRequest.request_period = data.request_period;
                        existingRequest.note = data.note;
                        existingRequest.id_detail = data.id_detail;
                        existingRequest.updated_at = DateTime.Now;

                        //cek klo status sblmnya "used"
                        if (existingRequest.request_status == "used" && data.request_dateReturned.HasValue)
                        {
                            existingRequest.request_status = "surplus";
                            deviceDetail.detail_status = "Available";
                            deviceDetail.detail_location = "Warehouse";
                        } //klo bukan "used" maka:
                        else if (data.request_dateReturned.HasValue) {
                            existingRequest.request_status = "returned";
                            deviceDetail.detail_status = "Available";
                            deviceDetail.detail_location = "Warehouse";

                        }
                        context.SaveChanges();
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                else //add request
                {
                    data.request_dateBorrowed = DateTime.Now;
                    data.created_at = DateTime.Now;
                    context.ITW_Request.Add(data);
                    //update device status to unavailable
                    deviceDetail.detail_status = "Unavailable";
                    deviceDetail.detail_location = $"{data.request_npp} - {data.request_name}";
                    context.SaveChanges();
                }
                // Send notification email with the combined NPP and Name as well as device and request details
                var emailSubject = $"Device Request Notification for {data.request_npp} - {data.request_name}";
                var emailBody = $@"
            <p>The following device request has been made:</p>
            <ul>
                <li><b>Device:</b> {deviceDetail.detail_tag} - {device.device_name}</li>
                <li><b>Requested By:</b> {data.request_npp} - {data.request_name}</li>
                <li><b>Status:</b> {data.request_status}</li>
                <li><b>Request Time:</b> {data.request_dateBorrowed}</li>";

                if (!string.IsNullOrEmpty(data.note))
                    emailBody += $"<li><b>Note:</b> {data.note}</li>";

                emailBody += $"<li><b>Responsible Admin:</b> {data.created_by}</li></ul>";

                // Call email function
                SendEmailToRecipients(emailSubject, emailBody);

                return RedirectToAction("Request");
            }
            PopulateDropdowns();
            return View(data);
        }
        
        [HttpPost]
        public IActionResult UpdateReturnDate(int id_request, DateTime request_dateReturned)
        {
            var existingRequest = context.ITW_Request.FirstOrDefault(r => r.id_request == id_request);
            if (existingRequest != null)
            {
                // Update request details
                existingRequest.request_dateReturned = request_dateReturned;
                existingRequest.updated_at = DateTime.Now;

                // Update the device status and location if applicable
                var deviceDetail = context.ITW_DeviceDetail.FirstOrDefault(d => d.id_detail == existingRequest.id_detail);
                if (deviceDetail != null)
                {
                    deviceDetail.detail_status = "Available";
                    deviceDetail.detail_location = "Warehouse";
                }

                // Set the status
                if (existingRequest.request_status == "used")
                {
                    existingRequest.request_status = "surplus";
                }
                else
                {
                    existingRequest.request_status = "returned";
                }

                context.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Request not found" });
        }

        private void SendEmailToRecipients(string subject, string body)
        {
            // Configure recipients
            var recipients = new List<string> { "asrizka23@gmail.com", "asri.10121299@mahasiswa.unikom.ac.id"};

            foreach (var recipient in recipients)
            {
                // Implement the email sending logic here
            }
        }
        //private void PopulateDropdowns(int? selectedDeviceId = null)
        //{
        //    //ViewBag.Devices = new SelectList(available_devices, "id_detail", "Display", selectedDeviceId);
        //    //
        //    var available_devices = (from detail in context.ITW_DeviceDetail
        //                             join device in context.ITW_Device
        //                             on detail.id_device equals device.id_device
        //                             where detail.detail_status == "Available" || detail.id_detail == selectedDeviceId
        //                             select new
        //                             {
        //                                 detail.id_detail,
        //                                 Display = detail.detail_tag + " - " + device.device_name
        //                             }).ToList();
        //    ViewBag.Devices = new SelectList(available_devices, "id_detail", "Display", selectedDeviceId);

        //    // Retrieve distinct device types
        //    var deviceTypes = context.ITW_Device
        //        .Select(d => d.device_type)
        //        .Distinct()
        //        .ToList();

        //    if (deviceTypes.Count == 0) // Debugging
        //    {
        //        Console.WriteLine("No device types found.");
        //    }
        //    else
        //    {
        //        Console.WriteLine($"Device Types Found: {string.Join(", ", deviceTypes)}"); // Debugging
        //    }

        //    ViewBag.DeviceType = new SelectList(deviceTypes);
        //}
        private void PopulateDropdowns(int? selectedDeviceId = null, string selectedDeviceType = null)
        {
            // Populate devices
            var available_devices = (from detail in context.ITW_DeviceDetail
                                     join device in context.ITW_Device
                                     on detail.id_device equals device.id_device
                                     where detail.detail_status == "Available" || detail.id_detail == selectedDeviceId
                                     select new
                                     {
                                         detail.id_detail,
                                         Display = detail.detail_tag + " - " + device.device_name
                                     }).ToList();
            ViewBag.Devices = new SelectList(available_devices, "id_detail", "Display", selectedDeviceId);

            // Populate device types
            var deviceTypes = context.ITW_Device
                .Select(d => d.device_type)
                .Distinct()
                .ToList();

            ViewBag.DeviceType = new SelectList(deviceTypes, selectedDeviceType);
        }


        public JsonResult GetDevicesByType(string deviceTypes)
        {
            var devices = (from detail in context.ITW_DeviceDetail
                           join device in context.ITW_Device on detail.id_device equals device.id_device
                           where device.device_type == deviceTypes && detail.detail_status == "Available"
                           select new
                           {
                               detail.id_detail,
                               Display = detail.detail_tag + " - " + device.device_name
                           }).ToList();
            return Json(devices);
        }

        //Delete Request
        public IActionResult DeleteReq(int id)
        {
            var request = context.ITW_Request.Find(id);
            if (request != null)
            {
                //device mana yg di request
                var device = context.ITW_DeviceDetail.Find(request.id_detail);
                if (device != null)
                {
                    //balikin availablitynya
                    device.detail_status = "Available";
                    context.SaveChanges();
                }

                context.ITW_Request.Remove(request);
                context.SaveChanges();

                return RedirectToAction("Request");
            }

            return NotFound();
        }

        //Disposed_Device
        public IActionResult Disposed()
        {
            var item = context.ITW_Disposal.ToList();
            var devices = (from detail in context.ITW_DeviceDetail
                           join device in context.ITW_Device
                           on detail.id_device equals device.id_device
                           select new
                           {
                               detail.id_detail,
                               detail.detail_tag,
                               device.device_name
                           }).ToList();
            ViewBag.Devices = devices;
            //var deviceDetails = context.ITW_DeviceDetail.ToList(); // Fetch ITW_DeviceDetail instead of ITW_Device
            return View(item);
        }

        // AddDisposal action method with GET and POST
        [HttpGet]
        public IActionResult AddDisposed(int id = 0)
        {
            var disposal = id > 0 ? context.ITW_Disposal.Find(id) : new Models.ITW_Disposal();

            //PopulateDropdowns(disposal);
            //return View(disposal);
            // Find the selected device type and id_detail
            string selectedDeviceType = null;
            int? selectedDeviceId = null;
            if (id > 0 && disposal != null)
            {
                var deviceDetail = context.ITW_DeviceDetail
                    .Join(context.ITW_Device,
                          detail => detail.id_device,
                          device => device.id_device,
                          (detail, device) => new { detail, device })
                    .FirstOrDefault(x => x.detail.id_detail == disposal.id_detail);

                if (deviceDetail != null)
                {
                    selectedDeviceType = deviceDetail.device.device_type;
                    selectedDeviceId = deviceDetail.detail.id_detail;
                }
            }

            // Pass the selected values to PopulateDropdowns
            PopulateDropdowns(selectedDeviceId, selectedDeviceType);
            return View(disposal);
        }

        [HttpPost]
        public IActionResult AddDisposed(int? id_disposal, ITW_Disposal data)
        {
            if (data == null)
            {
                ModelState.AddModelError("", "Form data is invalid.");
                PopulateDropdowns(); // Populate dropdowns for re-rendering the view
                return View(data);
            }

            // Validate device exists (by id_detail)
            var deviceDetail = context.ITW_DeviceDetail.Find(data.id_detail);
            if (deviceDetail == null)
            {
                ModelState.AddModelError("id_detail", "The selected device does not exist.");
                PopulateDropdowns();
                return View(data);
            }

            // Handle file uploads (if provided)
            if (data.certificateFile != null)
                data.disposal_certificate = UploadFile(data.certificateFile, "certificate");
            if (data.documentationFile != null)
                data.disposal_documentation = UploadFile(data.documentationFile, "documentation");

            // Determine the status based on the certificate file
            data.disposal_status = string.IsNullOrEmpty(data.disposal_certificate) ? "On Hold" : "Disposed";

            if (id_disposal.HasValue && id_disposal > 0)
            {
                // Update existing record
                var existingDisposal = context.ITW_Disposal.Find(id_disposal.Value);
                if (existingDisposal == null)
                {
                    return NotFound("Disposal record not found.");
                }

                existingDisposal.id_detail = data.id_detail;
                existingDisposal.disposal_date = data.disposal_date;
                existingDisposal.disposal_certificate = data.disposal_certificate ?? existingDisposal.disposal_certificate;
                existingDisposal.disposal_documentation = data.disposal_documentation ?? existingDisposal.disposal_documentation;
                existingDisposal.note = data.note;
                existingDisposal.disposal_status = data.disposal_status;
                existingDisposal.updated_at = DateTime.Now;
                existingDisposal.updated_by = data.updated_by;

                context.SaveChanges();
            }
            else
            {
                // Add new record
                data.created_at = DateTime.Now;
                context.ITW_Disposal.Add(data);
                context.SaveChanges();
            }

            // Update device status in ITW_DeviceDetail table
            if (data.disposal_status == "Disposed")
            {
                deviceDetail.detail_status = "Disposed";
            }
            else if (data.disposal_status == "On Hold")
            {
                deviceDetail.detail_status = "On Hold"; // Device can be borrowed again
            }
            context.SaveChanges();

            return RedirectToAction("Disposed");
        }


        //Delete Dipsosed
        public IActionResult DeleteDisposal(int id)
        {
            var disposal = context.ITW_Disposal.Find(id);
            if (disposal != null)
            {
                var deviceDetail = context.ITW_DeviceDetail.FirstOrDefault(d => d.id_detail == disposal.id_detail);
                if (deviceDetail != null)
                {
                    deviceDetail.detail_status = "Available";
                }

                context.ITW_Disposal.Remove(disposal);
                context.SaveChanges();
            }
            else
            {
                return NotFound();
            }

            return RedirectToAction("Disposal", "Transaction");
        }

        //UploadFile method
        private string UploadFile(IFormFile file, string folderName)
        {
            if (file != null && file.Length > 0)
            {
                //generate file name
                string newFileName = DateTime.Now.ToString("yyyMMddHHmmssfff") + Path.GetExtension(file.FileName);
                string fullPath = Path.Combine(environment.WebRootPath, folderName, newFileName);
                //save ke server
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                return newFileName; //save naemfile ke db
            }

            return null;
        }
    }
}