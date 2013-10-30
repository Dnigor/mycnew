var myc = myc || {};

myc.TaskList = function (config) {
  var self = this;
  
  var mapping = {
	  'DueDateUtc': {
      create: function (options) {
	      if (options.data != null) {
	        return ko.observable(new Date(moment(options.data)));
	      }
	      return ko.observable(null);
	    }
	  }
	};
	
	self.searchService = new myc.SearchService({ url: config.actions.search });
	self.ajaxService   = new myc.AjaxService();
  
	self.searchIsBusy = ko.computed(function () { return self.searchService.isBusy(); });
		
	//Search results
  self.searchResults = ko.mapping.fromJS([]);
	
	// Paging
  self.pageSize         = ko.observable(500);
  self.currentWeekPage  = ko.observable(0); //it's used for the week paging
	self.overDueTasksPage = ko.observable(1); //it's used to load More overdue Tasks
	  
	// Search Criteria
  self.today      = ko.computed(function () { return moment().startOf('day').add('w', self.currentWeekPage()); });
  self.firstDay   = ko.computed(function () { return moment().startOf('day').add('w', self.currentWeekPage()).add('d', 1); });
  self.secondDay  = ko.computed(function () { return moment().startOf('day').add('w', self.currentWeekPage()).add('d', 2); });
  self.thirdDay   = ko.computed(function () { return moment().startOf('day').add('w', self.currentWeekPage()).add('d', 3); });
  self.fourthDay  = ko.computed(function () { return moment().startOf('day').add('w', self.currentWeekPage()).add('d', 4); });
  self.fifthDay   = ko.computed(function () { return moment().startOf('day').add('w', self.currentWeekPage()).add('d', 5); });
  self.sixthDay   = ko.computed(function () { return moment().startOf('day').add('w', self.currentWeekPage()).add('d', 6); });
	self.filterType = ko.observable(null);
	 
  self.selectedTask             = ko.observable();
  self.newTask                  = ko.observable();
  self.selectedView             = ko.observable('task-template');
  self.remindersCount           = config.data.remindersCount;
  self.editTaskTitleInput       = ko.observable();
	self.editTaskDescriptionInput = ko.observable();
  self.editTaskDueDateUtcInput  = ko.observable();
  self.editTaskTitleHasContent  = ko.computed(function () { return self.editTaskTitleInput(); });
  self.newTaskTitleInput        = ko.observable();
  self.newTaskDescriptionInput  = ko.observable();
  self.newTaskDueDateUtcInput   = ko.observable();
  self.newTaskTitleHasContent   = ko.computed(function () { return self.newTaskTitleInput(); });
  
  self.todayText = ko.computed(function () {
    return Globalize.format(new Date(self.today()), 'dddd') + " - " + Globalize.format(new Date(self.today()), 'MMMM') + " " + Globalize.format(new Date(self.today()), 'dd');
  });
		
	self.firstDayText = ko.computed(function () {
    return Globalize.format(new Date(self.firstDay()), 'dddd') + " - " + Globalize.format(new Date(self.firstDay()), 'MMMM') + " " + Globalize.format(new Date(self.firstDay()), 'dd');
  });
		
	self.secondDayText = ko.computed(function () {
    return Globalize.format(new Date(self.secondDay()), 'dddd') + " - " + Globalize.format(new Date(self.secondDay()), 'MMMM') + " " + Globalize.format(new Date(self.secondDay()), 'dd');
  });
		
	self.thirdDayText = ko.computed(function () {
    return Globalize.format(new Date(self.thirdDay()), 'dddd') + " - " + Globalize.format(new Date(self.thirdDay()), 'MMMM') + " " + Globalize.format(new Date(self.thirdDay()), 'dd');
  });
		
	self.fourthDayText = ko.computed(function () {
    return Globalize.format(new Date(self.fourthDay()), 'dddd') + " - " + Globalize.format(new Date(self.fourthDay()), 'MMMM') + " " + Globalize.format(new Date(self.fourthDay()), 'dd');
  });
		
	self.fifthDayText = ko.computed(function () {
    return Globalize.format(new Date(self.fifthDay()), 'dddd') + " - " + Globalize.format(new Date(self.fifthDay()), 'MMMM') + " " + Globalize.format(new Date(self.fifthDay()), 'dd');
  });
		
	self.sixthDayText = ko.computed(function () {
    return Globalize.format(new Date(self.sixthDay()), 'dddd') + " - " + Globalize.format(new Date(self.sixthDay()), 'MMMM') + " " + Globalize.format(new Date(self.sixthDay()), 'dd');
  });
		
	self.weekText = ko.computed(function () {
	  return Globalize.format(new Date(self.today()), 'MMMM') + " " + Globalize.format(new Date(self.today()), 'dd') + " - " +
			Globalize.format(new Date(self.sixthDay()), 'MMMM') + " " + Globalize.format(new Date(self.sixthDay()), 'dd') + ", " + Globalize.format(new Date(self.sixthDay()), 'yyyy');
	});

  var typeAheadCriteria = {
    d: false,
    s: 10,
    i: 1,
    qf: ["FirstAndLastName", "FirstMiddleAndLastName"],
    rf: ["FirstMiddleAndLastName"],
    sf: "LastName.Sort|FirstName.Sort",
  };

  function _getNames(data) {
    var names = {};
      $.each(data.hits.hits, function (index, value) {
      var name = app.safeTrim(value.fields.FirstMiddleAndLastName);
      names[value._id] = name;
    });

    return names;
  };

  self.typeAheadSource = function(term, cb) {
    typeAheadCriteria.q = term;

    $.getJSON(config.actions.getCustomerList, typeAheadCriteria)
      .then(_getNames)
      .done(cb);
  };

	// Filtered Search Results
	self.todayTasks = ko.computed(function () {
		return ko.utils.arrayFilter(self.searchResults(), function (searchResult) {
		  if (searchResult.DueDateUtc()) {
			  return (self.today().isSame(searchResult.DueDateUtc(), 'day'));
			}
			return null;
		});
	}, self);

	self.firstDayTasks = ko.computed(function () {
		return ko.utils.arrayFilter(self.searchResults(), function (searchResult) {
			if (searchResult.DueDateUtc()) {
				return (self.firstDay().isSame(searchResult.DueDateUtc(), 'day'));
			}
			return null;
		});
	}, self).extend({ paging: self.remindersCount });

	self.secondDayTasks = ko.computed(function () {
		return ko.utils.arrayFilter(self.searchResults(), function (searchResult) {
			if (searchResult.DueDateUtc()) {
				return (self.secondDay().isSame(searchResult.DueDateUtc(), 'day'));
			}
			return null;
		});
	}, self).extend({ paging: self.remindersCount });

  self.thirdDayTasks = ko.computed(function () {
		return ko.utils.arrayFilter(self.searchResults(), function (searchResult) {
			if (searchResult.DueDateUtc()) {
				return (self.thirdDay().isSame(searchResult.DueDateUtc(), 'day'));
			}
			return null;
		});
	}, self).extend({ paging: self.remindersCount });

	self.fourthDayTasks = ko.computed(function () {
		return ko.utils.arrayFilter(self.searchResults(), function (searchResult) {
			if (searchResult.DueDateUtc()) {
				return (self.fourthDay().isSame(searchResult.DueDateUtc(), 'day'));
			}
			return null;
		});
	}, self).extend({ paging: self.remindersCount });

  self.fifthDayTasks = ko.computed(function () {
		return ko.utils.arrayFilter(self.searchResults(), function (searchResult) {
			if (searchResult.DueDateUtc()) {
				return (self.fifthDay().isSame(searchResult.DueDateUtc(), 'day'));
			}
			return null;
		});
	}, self).extend({ paging: self.remindersCount });

  self.sixthDayTasks = ko.computed(function () {
		return ko.utils.arrayFilter(self.searchResults(), function (searchResult) {
			if (searchResult.DueDateUtc()) {
				return (self.sixthDay().isSame(searchResult.DueDateUtc(), 'day'));
			}
			return null;
		});
	}, self).extend({ paging: self.remindersCount });
	  
	self.withoutDueDateTasks = ko.computed(function () {
		return ko.utils.arrayFilter(self.searchResults(), function (searchResult) {
		  return (!searchResult.DueDateUtc());
		});
	}, self).extend({ paging: self.remindersCount });

  self.overDueTasks = ko.computed(function() {
    return ko.utils.arrayFilter(self.searchResults(), function(searchResult) {
      if (searchResult.DueDateUtc()) {
        return (moment().startOf('day').isAfter(searchResult.DueDateUtc(), 'day'));
      }
      return null;
    }).sort(function (left, right) {
            return left.DueDateUtc() == right.DueDateUtc() ? 0 : (left.DueDateUtc() < right.DueDateUtc() ? -1 : 1);
      });
  
  }, self).extend({ paging: 0 });
  
	function _getCriteria() {
		var criteria = {
      TaskTypes: self.filterType() == "" ? null : self.filterType().split(","),
			MaxDueDateUtc: new Date(self.sixthDay().add('d', 1).add('seconds', -1)),
			MinDueDateUtc: new Date(moment().startOf('day').add('M', -3)),
      IsCompleted: false
		};
		return criteria;
	};

	function _doSearch(cb, currentWeekPage) {
		self.currentWeekPage(currentWeekPage);
		var criteria = _getCriteria();
		self.searchService.criteria(criteria);
		self.searchService.search({
			always: cb,
			success: function (data) {
				self.currentWeekPage(currentWeekPage);
				ko.mapping.fromJS(data, mapping, self.searchResults);
				}
		});
	}

	function _doSave(cb, data) {
		if (data.DueDateUtc)
		  data.DueDateUtc = app.toISO8601(data.DueDateUtc);

		self
      .ajaxService
      .postJSON(config.actions.saveTask, data)
      .done(function (result) {
        // REVIEW: should not have to JSON parse this. The result should already be a JSON object
			  result = JSON.parse(result);
			  switch (result.Result) {
				  case "Success":
					  app.notifySuccess(result.Message);
				    self.selectedTask(null);
					  break;
				  case "Error":
					  app.notifyError(result.Message, app.localize('ERROR_NOTIFY_TITLE'));
					  break;
				  }
			})
      .always(cb);
	}
		
	function _addTask(cb, data) {
	  if (data.DueDateUtc)
		  data.DueDateUtc = app.toISO8601(data.DueDateUtc);

		self
      .ajaxService
      .postJSON(config.actions.addTask, data)
      .done(function (result) {
        // REVIEW: should not have to JSON parse this. The result should already be a JSON object
			  result = JSON.parse(result);
			  switch (result.Result) {
				  case "Success":
				    app.notifySuccess(result.Message);
				    _addNewTaskSuccess(result);
				    break;
				  case "Error":
				    app.notifyError(result.Message, app.localize('ERROR_NOTIFY_TITLE'));
				    break;
			  }
			})
		  .always(cb);
	}
  
	function _updateTaskStatus(data, cb) {
    self.ajaxService.postJSON(config.actions.updateTaskStatus, data).always(cb);
	}

  function _updateMassTaskStatus(data, cb) {
		self.ajaxService.postJSON(config.actions.updateMassTaskStatus, data).always(cb);
  }

  function _addNewTaskSuccess(task) {
    self.newTask().TaskId(task.TaskId);
    self.newTask().TaskType(task.TaskType);
    if (self.newTask().CustomerId() != null) {
      self.newTask().ProfileUrl(config.data.profileUrl + self.newTask().CustomerId());
    }
    else {
      self.newTask().PrefixText("");
      self.newTask().Name("");
    }
    self.searchResults.push(self.newTask());
    self.newTask(null);
  }

  //TODO: refactor this
  function _resetCurrentPage() {
    self.firstDayTasks.currentPage(1);
    self.secondDayTasks.currentPage(1);
    self.thirdDayTasks.currentPage(1);
    self.fourthDayTasks.currentPage(1);
    self.fifthDayTasks.currentPage(1);
    self.sixthDayTasks.currentPage(1);
    self.withoutDueDateTasks.currentPage(1);
  };
  
	//Commands
	self.searchCommand = ko.asyncCommand({
    execute: function (cb) { _doSearch(cb, self.currentWeekPage()); },
    canExecute: function () { return !self.searchService.isBusy(); }
	});

	self.nextPageCommand = ko.asyncCommand({
		execute: function (cb) { _doSearch(cb, self.currentWeekPage() + 1); self.hideOverDueTasks(); _resetCurrentPage(); },
		canExecute: function () { return !self.searchService.isBusy() && self.currentWeekPage() < 4; } //allow review one month future only
	});

	self.prevPageCommand = ko.asyncCommand({
		execute: function (cb) { _doSearch(cb, self.currentWeekPage() - 1); self.hideOverDueTasks(); _resetCurrentPage(); },
		canExecute: function () { return !self.searchService.isBusy() && self.currentWeekPage() > 0; } //can't review overdue tasks
	});

	self.saveCommand = ko.asyncCommand({
		execute: function (cb) { _doSave(cb, ko.mapping.toJS(self.selectedTask)); },
		canExecute: function () { return !self.ajaxService.isBusy(); }
	});

	self.addCommand = ko.asyncCommand({
		execute: function (cb) { _addTask(cb, ko.mapping.toJS(self.newTask)); },
		canExecute: function () { return !self.ajaxService.isBusy(); }
	});
  
	self.updateMassStatusCommand = ko.asyncCommand({
	  execute: function (cb, data) { _updateMassTaskStatus(cb, data); },
	  canExecute: function () { return !self.ajaxService.isBusy(); }
	});
  
	self.updateStatusCommand = ko.asyncCommand({
	  execute: function (cb, data) { _updateTaskStatus(cb, data); },
	  canExecute: function () { return !self.ajaxService.isBusy(); }
	});
		
	// fire off a search when filterType changes
	self.filterType.subscribe(function () {
		self.searchCommand.execute();
	});
		
	//Additional functions
	self.hideOverDueTasks = function () {
	  self.overDueTasks.currentPage(1);
	  self.searchCommand.execute();
	};

	self.editTask = function (task) {
	  self.selectedTask(task);
	  self.editTaskTitleInput(task.Title());
	  self.editTaskDescriptionInput(task.Description());
	  self.editTaskDueDateUtcInput(task.DueDateUtc());
	};
	  
	self.cancelEditTask = function () {
		self.selectedTask(null);
	};
		
	self.completeTask = function (task) {
		 var data = { TaskId: task.TaskId(), Status: task.IsComplete() };
		 self.updateStatusCommand.execute(data);
	};
	  
	self.addTask = function () {
	  self.newTask(new myc.Task());
	  self.newTaskTitleInput(self.newTask().Title());
	  self.newTaskDescriptionInput(self.newTask().Description());
	  self.newTaskDueDateUtcInput(self.newTask().DueDateUtc());
	};

	self.template = function (item) {
		if (item === self.selectedTask() && (item.TaskType() === 'Consultant' || item.TaskType() === 'Customer')) {
		  return 'taskEdit-template';
		}
    else if (item === self.selectedTask()) {
		  return 'taskEdit-template-automat';
		}
		return self.selectedView();
	};
  
	self.overDueTemplate = function (item) {
	  if (item === self.selectedTask() && (item.TaskType() === 'Consultant' || item.TaskType() === 'Customer')) {
	    return 'taskEdit-overDueTemplate';
	  }
	  else if (item === self.selectedTask()) {
	    return 'taskEdit-overDueTemplate-automat';
	  }
	  return 'task-overDueTemplate';
	};
	  
	self.saveTask = function () {
	  self.selectedTask().Title(self.editTaskTitleInput());
	  self.selectedTask().Description(self.editTaskDescriptionInput());
	  self.selectedTask().DueDateUtc(self.editTaskDueDateUtcInput());
	  self.saveCommand.execute();
	 };

  self.addNewTask = function () {
	  self.newTask().Title(self.newTaskTitleInput());
	  self.newTask().Description(self.newTaskDescriptionInput());
	  self.newTask().DueDateUtc(self.newTaskDueDateUtcInput());
	  self.addCommand.execute();
	};

	self.selectAll = function () {
	  var tasks = [];
		ko.utils.arrayForEach(self.overDueTasks(), function (task) {
		  task.IsComplete(true);
	    tasks.push(task.TaskId);
		});
		self.updateMassStatusCommand.execute(tasks);
	};

  self.resetToToday = function () {
    self.currentWeekPage(0);
    self.searchCommand.execute();
    self.hideOverDueTasks();
  };

  //#region Validation

  self.validator = ko.validation.applyTo(self, {
    global: {
      editTaskTitleInput: { required: { message: 'VALIDATION_TASKTITLE_REQUIRED' } },
      newTaskTitleInput: { required: { message: 'VALIDATION_TASKTITLE_REQUIRED' } },
      editTaskDueDateUtcInput: { validDate: true },
      newTaskDueDateUtcInput: {validDate: true}
    }
  });

  //#endregion
};