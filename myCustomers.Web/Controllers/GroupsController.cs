using System;
using System.Web.Mvc;
using myCustomers.Web.Models;
using myCustomers.Web.Services;

namespace myCustomers.Web.Controllers
{
    [AuthorizeConsultant]
    public class GroupsController : Controller
    {
        private readonly IMappingService<Group, GroupDetailViewModel> _groupDetailMappingService;

        public GroupsController(IMappingService<Group, GroupDetailViewModel> groupDetailMappingService)
        {
            _groupDetailMappingService = groupDetailMappingService;
        }

        public ActionResult Detail(Guid groupId, string groupName)
        {
            var source = new Group
                             {                                 
                                 GroupId = groupId, 
                                 GroupName = groupName                           
                             };

            var model = _groupDetailMappingService.Map(source);

            return View(model);
        }
    }
}