using Microsoft.AspNetCore.Mvc;
using ToDoList.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Security.Claims;
namespace ToDoList.Controllers
{
  [Authorize]
  public class ItemsController : Controller
  {
    private readonly ToDoListContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ItemsController(UserManager<ApplicationUser> userManager, ToDoListContext db)
    {
      _userManager = userManager;
      _db = db;
    }

    public async Task<ActionResult> Index()
    {
      var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      var currentUser = await _userManager.FindByIdAsync(userId);
      var userItems = _db.Items.Where(entry => entry.User.Id == currentUser.Id).ToList();
      return View(userItems);
      
    }

    public ActionResult Create()
    {
      ViewBag.CategoryId = new SelectList(_db.Categories, "CategoryId", "Name");
      return View();
    }

    [HttpPost]
    public async Task<ActionResult> Create(Item item, int CategoryId)
    {
      var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      var currentUser = await _userManager.FindByIdAsync(userId);
      item.User = currentUser;
      _db.Items.Add(item);
      if (CategoryId != 0)
      { 
        _db.CategoryItem.Add(new CategoryItem() { CategoryId = CategoryId, ItemId = item.ItemId });
      }
      _db.SaveChanges();
      return RedirectToAction("Index");
    }

    public ActionResult Details(int id)
    {
      var thisItem = _db.Items
            .Include(item => item.Categories) //.Include(item => item.Categories) to load the Categories property of each Item. However, the Categories property on an Item is actually a collection of join entities, each of type ICollection<CategoryItem>. These are not the actual categories related to an Item.
            .ThenInclude(join => join.Category) //ThenInclude() method to load the Category of each CategoryItem. Remember that a CategoryItem is simply a reference to a relationship. Each CategoryItem includes the id of an Item as well as the id of a Category. We are actually returning the associated Category of a CategoryItem here.
            .FirstOrDefault(item => item.ItemId == id); // FirstOrDefault() method specifies which item from the database we're working with.
      return View(thisItem);
    }

    public ActionResult Edit(int id)
    {
      var thisItem = _db.Items.FirstOrDefault(items => items.ItemId == id);
      ViewBag.CategoryId = new SelectList(_db.Categories, "CategoryId", "Name");
      return View(thisItem);
    }

    [HttpPost]
    public ActionResult Edit(Item item, int CategoryId)
    {
      if (CategoryId != 0)
      {
        _db.CategoryItem.Add(new CategoryItem() { CategoryId = CategoryId, ItemId = item.ItemId });
      }
      _db.Entry(item).State = EntityState.Modified;
      _db.SaveChanges();
      return RedirectToAction("Index");
    }
    public ActionResult AddCategory(int id)
    {
    var thisItem = _db.Items.FirstOrDefault(items => items.ItemId == id);
    ViewBag.CategoryId = new SelectList(_db.Categories, "CategoryId", "Name");
    return View(thisItem);
    }
    [HttpPost]
    public ActionResult AddCategory(Item item, int CategoryId)
    {
    if (CategoryId != 0)
    {
    _db.CategoryItem.Add(new CategoryItem() { CategoryId = CategoryId, ItemId = item.ItemId });
    }
    _db.SaveChanges();
    return RedirectToAction("Index");
    }
    public ActionResult Delete(int id)
    {
      var thisItem = _db.Items.FirstOrDefault(items => items.ItemId == id);
      return View(thisItem);
    }

    [HttpPost, ActionName("Delete")]
    public ActionResult DeleteConfirmed(int id)
    {
      var thisItem = _db.Items.FirstOrDefault(items => items.ItemId == id);
      _db.Items.Remove(thisItem);
      _db.SaveChanges();
      return RedirectToAction("Index");
    }
    [HttpPost]
    public ActionResult DeleteCategory(int joinId)
    {
    var joinEntry = _db.CategoryItem.FirstOrDefault(entry => entry.CategoryItemId == joinId);
    _db.CategoryItem.Remove(joinEntry);
    _db.SaveChanges();
    return RedirectToAction("Index");
    }
  }
}