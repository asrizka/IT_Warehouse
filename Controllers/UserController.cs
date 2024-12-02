using IT_Inventory.Data;
using Microsoft.AspNetCore.Mvc;

namespace IT_Inventory.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext context;

        public UserController(ApplicationDbContext context)
        {
            this.context = context;
        }
        public IActionResult User()
        {
            var users = context.ITW_User.ToList();
            return View(users);
        }

        //AddUser
        [HttpGet]
        public IActionResult AddUser(int id = 0)
        {
            Models.ITW_User user = new Models.ITW_User();
            if (id > 0) user = context.ITW_User.Find(id);
            return View(user);
        }

        [HttpPost]
        public IActionResult AddUser(int id = 0, Models.ITW_User data = null)
        {
            if (data != null)
            {
                if (id > 0)
                {
                    var existingUser = context.ITW_User.Find(id);

                    if (existingUser != null)
                    {
                        existingUser.user_name = data.user_name;
                        existingUser.user_role = data.user_role;
                        existingUser.updated_at = DateTime.Now;

                        context.SaveChanges();
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                else
                {
                    context.Add(data);
                    context.SaveChanges();
                }
                return RedirectToAction("User");
            }
            return BadRequest("Data is null or invalid.");
        }
        //ini belum ada alertnya, jadi misal hapus yg ada fknyanya, malah error bukannya ngasih tau gabisa ini teh datanya lagi dipake di tabel lain
        //DeleteUser
        public IActionResult DeleteUser(int id)
        {
            var user = context.ITW_User.Find(id);
            context.ITW_User.Remove(user);
            context.SaveChanges();

            return RedirectToAction("User", "User");
        }
    }
}
