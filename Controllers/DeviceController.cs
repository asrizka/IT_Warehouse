using IT_Inventory.Data;
using IT_Inventory.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Formats.Asn1;
using System.Data;

namespace IT_Inventory.Controllers
{
    public class DeviceController : Controller
    {
        private readonly ApplicationDbContext context;

        public DeviceController(ApplicationDbContext context)
        {
            this.context = context;
        }

        //Device & Category (Read) 
        public IActionResult Device(List<int> selectedCategories = null)
        {
            var categories = context.ITW_Category.ToList();
            var devices = new List<Models.ITW_Device>();
            if (selectedCategories != null && selectedCategories.Any())
            {
                devices = context.ITW_Device
                    .Where(d => selectedCategories.Contains(d.id_category))
                    .ToList();
            }
            else
            {
                devices = context.ITW_Device.ToList();
            }
            ViewBag.Device = devices;
            ViewBag.Category = categories;
            ViewBag.SelectedCategories = selectedCategories;
            return View();
        }

        //AddCategory
        [HttpGet]
        public IActionResult AddCategory(int id = 0)
        {
            Models.ITW_Category category = new Models.ITW_Category();
            if (id > 0) category = context.ITW_Category.Find(id);
            return View(category);
        }

        [HttpPost]
        public IActionResult AddCategory(int id = 0, Models.ITW_Category data = null)
        {
            if (data != null)
            {
                if (id > 0)
                {
                    var existingCategory = context.ITW_Category.Find(id);
                    if (existingCategory != null)
                    {
                        existingCategory.category_name = data.category_name;
                        existingCategory.category_code = data.category_code;

                        context.Update(existingCategory);
                        context.SaveChanges();
                    }
                }
                else
                {
                    context.Add(data);
                    context.SaveChanges();
                }
            }
            return RedirectToAction("Device", "Device");
        }

        //DeleteCategory
        public IActionResult DeleteCategory(int id)
        {
            var category = context.ITW_Category.Find(id);
            context.ITW_Category.Remove(category);
            context.SaveChanges();

            return RedirectToAction("Device");
        }

        //AddDevice
        [HttpGet]
        public IActionResult AddDevice(int id = 0)
        {
            var device = id > 0 ? context.ITW_Device.Find(id) : new Models.ITW_Device();
            //bikin select option cateogy
            var categories = context.ITW_Category.ToList();
            ViewBag.Categories = new SelectList(categories, "id_category", "category_name");
            ViewBag.category_codes = categories
                //.Where(c => c.id_category)
                .ToDictionary(c => c.id_category, c => c.category_code);
            return View(device);
        }


        [HttpPost] 
        public IActionResult AddDevice(int id = 0, Models.ITW_Device data = null)
        {
            if (data != null)
            {
                if (id == 0)
                {
                    data.created_at = DateTime.Now;
                    data.updated_at = DateTime.Now;

                    context.Add(data);
                    context.SaveChanges();
                }
                else
                {
                    var existingDevice = context.ITW_Device.Find(id);

                    if (existingDevice != null)
                    {
                        existingDevice.device_type = data.device_type;
                        existingDevice.device_name = data.device_name;
                        existingDevice.device_eligibility = data.device_eligibility;
                        existingDevice.id_category = data.id_category;
                        existingDevice.updated_by = data.updated_by;
                        existingDevice.updated_at = DateTime.Now;

                        context.SaveChanges();
                    }
                    else
                    {
                        return NotFound();
                    }
                }
            }
            return RedirectToAction("Device");
        }

        //DeleteDevice
        public IActionResult Delete(int id)
        {
            var device = context.ITW_Device.Find(id);
            context.ITW_Device.Remove(device);
            context.SaveChanges();

            return RedirectToAction("Device", "Device");
        }

        //DeviceDetail
        public IActionResult DeviceDetail()
        {
            // Join ITW_DeviceDetail, ITW_Device, and ITW_Category to fetch the required information
            var deviceDetails = context.ITW_DeviceDetail
                .Join(context.ITW_Device,
                      detail => detail.id_device,
                      device => device.id_device,
                      (detail, device) => new { detail, device })
                .Join(context.ITW_Category,
                      joined => joined.device.id_category,
                      category => category.id_category,
                      (joined, category) => new DeviceDetailView {
                          Detail = joined.detail,
                          DeviceName = joined.device.device_name,
                          DeviceType = joined.device.device_type,
                          CategoryName = category.category_name
                      })
                .ToList();

            return View(deviceDetails);
        }

        [HttpGet]
        public IActionResult AddDeviceDetail(int id = 0)
        {
            // Fetch detail for editing, or initialize a new instance for adding
            var detail = id > 0 ? context.ITW_DeviceDetail.Find(id) : new ITW_DeviceDetail();

            // Populate the category dropdown
            var categories = context.ITW_Category.ToList();
            ViewBag.Categories = new SelectList(categories, "id_category", "category_name");

            // Perform a join to fetch device data based on selected category
            var devices = context.ITW_Device
                .Join(context.ITW_Category,
                      device => device.id_category,
                      category => category.id_category,
                      (device, category) => new
                      {
                          device.id_device,
                          device.device_name,
                          device.device_type,
                          device.id_category,
                          category.category_name
                      })
                .ToList();

            var categoryDevices = devices
                .GroupBy(d => d.id_category)
                .ToDictionary(g => g.Key, g => g.ToList());

            ViewBag.CategoryDevices = categoryDevices;

            // Set the selected category, type, and device if editing an existing detail
            if (id > 0 && detail.id_device != null)
            {
                var selectedDevice = context.ITW_Device.FirstOrDefault(d => d.id_device == detail.id_device);
                if (selectedDevice != null)
                {
                    //var cat = context.ITW_Category.FirstOrDefault(d => d.id_category == selectedDevice.id_category);
                    ViewBag.SelectedCategory = selectedDevice.id_category;
                    ViewBag.SelectedType = selectedDevice.device_type;
                    ViewBag.SelectedDevice = detail.id_device;
                }
            }

            return View(detail);
        }

        [HttpPost]
        //public IActionResult AddDeviceDetail(int id_detail = 0, Models.ITW_DeviceDetail detail = null)
        public IActionResult AddDeviceDetail(Models.ITW_DeviceDetail detail)
        {
            if (!ModelState.IsValid)
            {
                PopulateViewBag();
                return View(detail);
            }

            ////unique serial number and tag validation
            var unique_sn_tag = context.ITW_DeviceDetail.Any(d =>
        (d.detail_sn == detail.detail_sn && d.id_detail != detail.id_detail) ||
        (!string.IsNullOrEmpty(detail.detail_tag) && d.detail_tag == detail.detail_tag && d.id_detail != detail.id_detail)
    );

            if (unique_sn_tag)
            {
                ModelState.AddModelError("", "Serial Number or Tag is not unique.");
                PopulateViewBag();
                return View(detail);
            }

            //if detail tag null, status: unregistere, otherwsei availbale
            detail.detail_status = string.IsNullOrEmpty(detail.detail_tag) ? "Unregistered" : "Available";

            if (detail.id_detail == 0) //add
            {
                detail.created_at = DateTime.Now;
                detail.updated_at = DateTime.Now;
                context.ITW_DeviceDetail.Add(detail);
            }
            else//edit
            {
                var existingDetail = context.ITW_DeviceDetail.Find(detail.id_detail);
                if (existingDetail != null)
                {
                    existingDetail.id_device = detail.id_device;
                    existingDetail.detail_tag = detail.detail_tag;
                    existingDetail.detail_sn = detail.detail_sn;
                    existingDetail.detail_status = detail.detail_status;
                    existingDetail.detail_location = detail.detail_location;
                    existingDetail.note = detail.note;
                    existingDetail.updated_at = DateTime.Now;
                    existingDetail.updated_by = detail.updated_by;

                    context.ITW_DeviceDetail.Update(existingDetail);
                }
                else
                {
                    return NotFound();
                }
            }

            context.SaveChanges();
            return RedirectToAction("DeviceDetail");
        }

        private void PopulateViewBag()
        {
            var categories = context.ITW_Category.ToList();
            ViewBag.Categories = new SelectList(categories, "id_category", "category_name");

            var devices = context.ITW_Device
                .Join(context.ITW_Category,
                      device => device.id_category,
                      category => category.id_category,
                      (device, category) => new {
                          device.id_device,
                          device.device_name,
                          device.id_category,
                          category.category_code
                      })
                .ToList();

            ViewBag.Devices = new SelectList(devices, "id_device", "device_name");
        }

        // CheckUniqueDevice
        [HttpGet]
        public JsonResult CheckUniqueDevice(string serialNumber, string device_tag, int? id_detail = null)
        {
            bool sn_exist = context.ITW_DeviceDetail.Any(d => d.detail_sn == serialNumber && d.id_detail != id_detail);
            bool tag_exist = context.ITW_DeviceDetail.Any(d => !string.IsNullOrEmpty(device_tag) && d.detail_tag == device_tag && d.id_detail != id_detail);

            return Json(new { sn_exist, tag_exist });
        }


        //DeleteDevice
        public IActionResult DeleteDetail(int id)
        {
            var detail = context.ITW_DeviceDetail.Find(id);
            context.ITW_DeviceDetail.Remove(detail);
            context.SaveChanges();

            return RedirectToAction("DeviceDetail");
        }


        public IActionResult Upload()
        {
            return View();
        }

        [HttpGet]
        public IActionResult DeviceTemplate()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/templates/DeviceTemplate.xlsx");
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DeviceTemplate.xlsx");
        }

        [HttpPost]
        public IActionResult SubmitAllDevices([FromBody] List<DeviceUploadModel> devices)
        {
            if (devices == null || devices.Count == 0)
            {
                return Json(new { success = false, message = "No devices to submit." });
            }

            var deviceList = new List<ITW_Device>();
            var errors = new List<string>(); // Collect all errors
            var existingDeviceNames = context.ITW_Device.Select(d => d.device_name).ToHashSet(); // Get all existing device names

            foreach (var device in devices)
            {
                // Check if the category exists in the ITW_Category table
                var category = context.ITW_Category.FirstOrDefault(c => c.category_name == device.category_name);

                if (category == null)
                {
                    // If the category doesn't exist, add an error message
                    errors.Add($"Category '{device.category_name}' does not exist.");
                    continue;
                }

                // Check if device name already exists
                if (existingDeviceNames.Contains(device.device_name))
                {
                    errors.Add($"Device name '{device.device_name}' already exists.");
                    continue;
                }

                // Map the data from the upload model to ITW_Device entity
                var newDevice = new ITW_Device
                {
                    device_type = device.device_type,
                    device_name = device.device_name,
                    device_eligibility = device.device_eligibility,
                    id_category = category.id_category,  // Link the category using id_category
                    created_at = DateTime.Now,
                    created_by = "System" // Adjust based on actual user input
                };

                deviceList.Add(newDevice);
            }

            // If there are any errors, return them to the client
            if (errors.Any())
            {
                return Json(new { success = false, errors });
            }

            // If no errors, save the devices
            context.ITW_Device.AddRange(deviceList);
            context.SaveChanges();

            return Json(new { success = true, message = "Devices successfully uploaded." });
        }

        //UploadDetail
        public IActionResult UploadDetail()
        {
            return View();
        }

        [HttpGet]
        public IActionResult DetailTemplate()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/templates/DetailTemplate.xlsx");
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DetailTemplate.xlsx");
        }

        [HttpPost]
        public IActionResult SubmitAllDeviceDetails([FromBody] List<DeviceDetailUploadModel> deviceDetails)
        {
            if (deviceDetails == null || deviceDetails.Count == 0)
            {
                return Json(new { success = false, message = "No device details to submit." });
            }

            var detailList = new List<ITW_DeviceDetail>();
            var errors = new List<string>(); // Collect all errors

            // Get existing tags and SNs to check for duplicates in the database
            var existingTags = context.ITW_DeviceDetail
                                    .Where(d => d.detail_tag != null)
                                    .Select(d => d.detail_tag).ToHashSet();
            var existingSNs = context.ITW_DeviceDetail
                                    .Where(d => d.detail_sn != null)
                                    .Select(d => d.detail_sn).ToHashSet();
            var existingDeviceNames = context.ITW_Device
                                    .Select(d => new { d.device_name, d.id_device }).ToList();

            // Track tags and SNs in the current upload for uniqueness within this batch
            var currentTags = new HashSet<string>();
            var currentSNs = new HashSet<string>();

            foreach (var deviceDetail in deviceDetails)
            {
                // Collect errors for each row to ensure all issues are reported
                var rowErrors = new List<string>();

                // 1. Check if device name or SN is empty
                if (string.IsNullOrWhiteSpace(deviceDetail.device_name))
                {
                    rowErrors.Add("Device name cannot be empty.");
                }
                if (string.IsNullOrWhiteSpace(deviceDetail.detail_sn))
                {
                    rowErrors.Add($"SN cannot be empty for device '{deviceDetail.device_name}'.");
                }

                // 2. Check if the device name exists in the database
                var device = existingDeviceNames.FirstOrDefault(d => d.device_name == deviceDetail.device_name);
                if (device == null)
                {
                    rowErrors.Add($"Device '{deviceDetail.device_name}' does not exist.");
                }

                // 3. Check if tag is unique within both the uploaded data and the database
                if (!string.IsNullOrWhiteSpace(deviceDetail.detail_tag))
                {
                    if (existingTags.Contains(deviceDetail.detail_tag) || currentTags.Contains(deviceDetail.detail_tag))
                    {
                        rowErrors.Add($"Tag '{deviceDetail.detail_tag}' already exists in the database or in this upload.");
                    }
                    else
                    {
                        currentTags.Add(deviceDetail.detail_tag);
                    }
                }

                // 4. Check if SN is unique within both the uploaded data and the database
                if (existingSNs.Contains(deviceDetail.detail_sn) || currentSNs.Contains(deviceDetail.detail_sn))
                {
                    rowErrors.Add($"Serial number '{deviceDetail.detail_sn}' already exists in the database or in this upload.");
                }
                else
                {
                    currentSNs.Add(deviceDetail.detail_sn);
                }

                // If there are row-specific errors, add them to the main errors list
                if (rowErrors.Any())
                {
                    errors.Add($"Row {deviceDetails.IndexOf(deviceDetail) + 1}: " + string.Join("; ", rowErrors));
                    continue;
                }

                // Map to the ITW_DeviceDetail entity
                var newDeviceDetail = new ITW_DeviceDetail
                {
                    id_device = device.id_device,
                    detail_tag = deviceDetail.detail_tag,
                    detail_sn = deviceDetail.detail_sn,
                    detail_location = deviceDetail.detail_location,
                    note = deviceDetail.note,
                    detail_status = string.IsNullOrWhiteSpace(deviceDetail.detail_tag) ? "Unregistered" : "Available", //if tag is null, unregistered (no matter what device)
                    created_at = DateTime.Now,
                    created_by = "tes",
                };

                detailList.Add(newDeviceDetail);
            }

            // If there are any errors, return them to the client
            if (errors.Any())
            {
                return Json(new { success = false, errors });
            }

            // If no errors, save the device details
            context.ITW_DeviceDetail.AddRange(detailList);
            context.SaveChanges();

            return Json(new { success = true, message = "Device details successfully uploaded." });
        }

        public IActionResult Report()
        {

            var reportData = context.ITW_Report.OrderBy(r => r.DateAndTime).ToList();
            return View(reportData);
        }

    }
}
