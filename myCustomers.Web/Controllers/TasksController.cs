using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web.Mvc;
using myCustomers.Globalization;
using myCustomers.Web.Models;
using myCustomers.Web.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Quartet.Entities;
using Quartet.Entities.Views;
using Quartet.Services.Contracts;

namespace myCustomers.Web.Controllers
{
    [AuthorizeConsultant]
    public class TasksController : Controller
    {
        readonly IQuartetClientFactory _clientFactory;
        readonly IMappingService<TaskListItem, TaskViewModel> _taskMapping;

        public TasksController
        (
            IQuartetClientFactory clientFactory,
            IMappingService<TaskListItem, TaskViewModel> taskMapping
        )
        {
            _clientFactory = clientFactory;
            _taskMapping = taskMapping;
        }

        public ActionResult Index()
        {
            return View();
        }

        // TODO: Move to Web API
        [HttpPost]
        public ActionResult Search(TaskTypeParameters query, bool excludeWithoutDueDate = false)
        {
            var taskSearch = _clientFactory.GetTaskQueryServiceClient();            
            query.SortOptions = new[] { new SortOption { Direction = SortDirection.Ascending, Name = "DueDateUtc" } };
            var results = taskSearch.QueryTaskListByType(query).ToList();

            if (excludeWithoutDueDate)
            {
                results = results.Where(t => t.DueDateUtc != null).ToList();
            }

            var resultList = new List<dynamic>();
            foreach (var result in results)
            {
                resultList.AddRange(new[] { _taskMapping.Map(result) });
            }
            string resultIso = JsonConvert.SerializeObject(resultList, new IsoDateTimeConverter());
            return Content(resultIso);
        }

        // TODO: Move to Web API
        [HttpPost]
        public ActionResult Save(Quartet.Entities.Commands.UpdateTask updateTask)
        {
            var commandServiceClient = _clientFactory.GetCommandServiceClient();

            var taskClient = _clientFactory.GetTaskQueryServiceClient();
            var task = taskClient.GetTaskById(updateTask.TaskId);

            if (task == null) throw ApiHelpers.ServerError("Task for which an update was requested does not exist");

            if (updateTask.DueDateUtc != null)
            {
                updateTask.DueDateUtc = updateTask.DueDateUtc.Value.ToUniversalTime();
            }
            if (updateTask.Title == null)
            {
                var result = new
                {
                    Result = "Error",
                    Message = Resources.GetString("REMINDERLIST_ACTION_TITLEISREQUIRED")
                };
                var jsonResult = JsonConvert.SerializeObject(result);

                return Json(jsonResult, JsonRequestBehavior.AllowGet);
            }
            if (updateTask.TaskType != TaskType.Consultant && updateTask.TaskType != TaskType.Customer && !String.IsNullOrEmpty(task.Title))
            {
                updateTask.Title = task.Title;
            }
            try
            {
                commandServiceClient.Execute(updateTask);

            }
            catch (FaultException<ValidationResult> faultException)
            {
                var result = new
                {
                    Result = "Error",
                    Message = faultException.Detail.Errors.FirstOrDefault()
                };

                var jsonResult = JsonConvert.SerializeObject(result);

                return Json(jsonResult, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception)
            {
                var result = new
                {
                    Result = "Error",
                    Message = exception.Message
                };

                var jsonResult = JsonConvert.SerializeObject(result);

                return Json(jsonResult, JsonRequestBehavior.AllowGet);
            }
            var successResult = new
            {
                Result = "Success",
                Message = Resources.GetString("REMINDERLIST_UPDATESUCCESS"),
            };
            var successJsonResult = JsonConvert.SerializeObject(successResult);

            return Json(successJsonResult, JsonRequestBehavior.AllowGet);
        }

        // TODO: Move to Web API
        [HttpPost]
        public JsonResult Add(Quartet.Entities.Commands.AddTask command)
        {
            var commandServiceClient = _clientFactory.GetCommandServiceClient();
            command.TaskId = Guid.NewGuid();
            command.TaskType = TaskType.Consultant;

            if (command.CustomerId.HasValue)
            {
                command.TaskType = TaskType.Customer;
            }

            if (command.DueDateUtc != null)
            {
                command.DueDateUtc = command.DueDateUtc.Value.ToUniversalTime();
            }

            if (String.IsNullOrEmpty(command.Title))
            {
                var result = new
                {
                    Result = "Error",
                    Message = Resources.GetString("REMINDERLIST_ACTION_TITLEISREQUIRED")
                };
                var jsonResult = JsonConvert.SerializeObject(result);

                return Json(jsonResult, JsonRequestBehavior.AllowGet);

            }

            try
            {
                commandServiceClient.Execute(command);
            }

            catch (Exception exception)
            {
                var result = new
                {
                    Result = "Error",
                    Message = exception.Message
                };
                var jsonResult = JsonConvert.SerializeObject(result);

                return Json(jsonResult, JsonRequestBehavior.AllowGet);
            }


            var successResult = new
            {
                Result = "Success",
                Message = Resources.GetString("REMINDERLIST_ADDSUCCESS"),
                TaskId = command.TaskId,
                TaskType = command.TaskType.ToString()
            };
            var resultJson = JsonConvert.SerializeObject(successResult);
            return Json(resultJson, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateTaskStatus(Guid? taskId, bool status)
        {
            var commandServiceClient = _clientFactory.GetCommandServiceClient();
            if (taskId != null && status)
            {
                var command = new Quartet.Entities.Commands.MarkTaskComplete
                    {
                        TaskId = (Guid)taskId
                    };

                commandServiceClient.Execute(command);

                return Json(new { TaskId = command.TaskId, CommandResult = new CommandResult() });
            }

            if (taskId != null)
            {
                var command = new Quartet.Entities.Commands.MarkTaskIncomplete
                {
                    TaskId = (Guid)taskId
                };

                commandServiceClient.Execute(command);

                return Json(new { TaskId = command.TaskId, CommandResult = new CommandResult() });
            }

            return Json(new { TaskId = Guid.Empty, CommandResult = null as CommandResult });
        }

        [HttpPost]
        public ActionResult UpdateMassTaskStatus(Guid?[] tasks)
        {
            foreach (var task in tasks)
            {
                var commandServiceClient = _clientFactory.GetCommandServiceClient();
                if (task != null)
                {
                    var command = new Quartet.Entities.Commands.MarkTaskComplete
                    {
                        TaskId = (Guid)task
                    };

                    commandServiceClient.Execute(command);
                }
   
            }

            var successResult = new
            {
                Result = "Success",
                Message = Resources.GetString("REMINDERLIST_MASSUPDATESUCCESS"),
            };
            var resultJson = JsonConvert.SerializeObject(successResult);
            return Json(resultJson, JsonRequestBehavior.AllowGet);
        }
    }
}
