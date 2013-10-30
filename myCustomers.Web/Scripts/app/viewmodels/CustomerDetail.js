var myc = myc || {};

myc.CustomerDetail = function(config) {
  var self = this;

  self.productsData                          = config.productsData;
  self.notes                                 = ko.observableArray([]);
  self.tasks                                 = ko.observableArray([]);
  self.newNoteInput                          = ko.observable(null).extend({ maxLength: { max: 1000 } });
  self.hasNotes                              = ko.computed(function() { return self.notes().length > 0; });
  self.hasTasks                              = ko.computed(function() { return self.tasks().length > 0; });
  self.hasNewNoteInputContent                = ko.computed(function() { return self.newNoteInput() && $.trim(self.newNoteInput()).length > 0; });
  self.customerOrders                        = ko.observableArray([]);
  self.hasNoCustomerOrders                   = ko.computed(function() { return self.customerOrders().length === 0; });
  self.hasCustomerOrders                     = ko.computed(function() { return self.customerOrders().length > 0; });
  self.currentTab                            = ko.observable(config.startingTab);
  self.customerIsDeleted                     = ko.observable(config.customerIsDeleted === 'True');
  self.customerIsNotDeleted                  = ko.computed(function() { return !self.customerIsDeleted(); });
  self.customerIsArchived                    = ko.observable(config.customerIsArchived === 'True');
  self.customerIsNotArchived                 = ko.computed(function() { return !self.customerIsArchived(); });
  self.questionnaireAnswers                  = ko.observableArray([]);
  self.hasQuestionnaireAnswers               = ko.computed(function() { return self.questionnaireAnswers() !== null && self.questionnaireAnswers().length > 0; });
  self.hasLoadedQuestionnaireAnswers         = ko.observable(false);
  self.hasLoadedCustomerOrders               = ko.observable(false);
  self.hasLoadedCustomerNotes                = ko.observable(false);
  self.hasLoadedCustomerTasks                = ko.observable(false);
  self.newTaskTitleInput                     = ko.observable(null);
  self.newTaskDescriptionInput               = ko.observable(null);
  self.newTaskDueDateUtcInput                = ko.observable(null);
  self.totalTasks                            = ko.observable(0);
  self.moreTasksHasBeenClicked               = ko.observable(false);
  self.showMoreTasksLinks                    = ko.computed(function() { return self.totalTasks() > parseInt(config.taskPageSize, 10) && !self.moreTasksHasBeenClicked(); });
  self.isInAddTaskMode                       = ko.observable(false);
  self.initialTaskLoadIsExecuting            = ko.observable(false);
  self.activities                            = ko.observableArray([]);
  self.activityPageSizeTriggeredIsLoading    = ko.observable(false);
  self.activityTabSelectedTriggeredIsLoading = ko.observable(false);
  self.hasNewTaskInputContent                = ko.computed(function() { return self.newTaskTitleInput() !== null && self.newTaskTitleInput().length > 0; });
  self.orderSearchInProgress                 = ko.observable(false);
  self.customerPrrView                       = new myc.CustomerPrrView({ url: config.prrApiUrl, ajaxService: config.ajaxService });

  self.navigate = function(url) {
    location.href = url;
  };

  self.navigateNewWindow = function(url) {
    window.open(url, '_blank');
  };

  //#region Groups

  self.assignedGroups        = ko.observableArray([]);
  self.availableGroups       = ko.observableArray([]);
  self.hasGroupsToSelectFrom = ko.computed(function() { return self.availableGroups().length > 0; });
  self.groupEntryIsVisible   = ko.observable(false);
  self.newGroupName          = ko.observable();
  self.hasNewGroupName       = ko.computed(function () { return self.newGroupName() && $.trim(self.newGroupName()).length > 0;});

  self.groupSelectorIsVisible = ko.computed(function() {
    return self.hasGroupsToSelectFrom() && !self.groupEntryIsVisible();
  });

  self.showGroupEntry = function() {
    self.groupEntryIsVisible(true);
  };

  self.hideGroupEntry = function() {
    self.groupEntryIsVisible(false);
  };

  // Sort groups array by group name
  function SortGroupsByName(group1, group2) {
    return ((group1.name().toLowerCase() < group2.name().toLowerCase()) ? -1 :
      ((group1.name().toLowerCase() > group2.name().toLowerCase()) ? 1 : 0));
  }

  // Load assigned groups.
  function _getCustomerGroups() {
    config.ajaxService.getJSON(config.customerGroupsApiUrl).done(function(data) { _customerGroupsLoaded(data); });
  }

  var _assignedGroups = [];
  function _customerGroupsLoaded(data) {
    $.each(data, function(index, value) {
      _assignedGroups.push(new myc.CustomerGroup(value));
    });
    _assignedGroups.sort(SortGroupsByName);
    self.assignedGroups(_assignedGroups); //Sort by group name
    _getAvailableGroups();
  }

  // Get available groups. 
  // Get all groups and remove assigned groups
  function _getAvailableGroups() {
    return config.ajaxService.getJSON(config.groupsApiUrl).done(function(data) { _availableGroupsLoaded(data); });
  }

  var _availableGroups = [];
  function _availableGroupsLoaded(data) {
    $.each(data, function(index, value) {
      if(!_groupNameExists(_assignedGroups, value.Name))
      {
        _availableGroups.push(new myc.CustomerGroup({ name: value.Name, groupId: value.GroupId }));
      }
    });
    _availableGroups.sort(SortGroupsByName);
    self.availableGroups(_availableGroups);
  }
  
  // Check if the groups array contains a group with specified group name
  function _groupNameExists(groupArray, groupName) {
    var isInGroup = false;
    $.each(groupArray, function(index, group) {
      if(group.name().toLowerCase() === groupName.toLowerCase())
      {
        isInGroup = true;
      }
    });
    return isInGroup;
  }

  self.removeFromGroupCommand = ko.asyncCommand({
    execute: function(data, cb) {
      app.confirm('CONFIRM_REMOVEFROMGROUP_TITLE', 'CONFIRM_REMOVEFROMGROUP_BODY')
      .then(function() {
          return config.ajaxService.del(config.customerGroupsApiUrl + "/" + data.groupId());
      })
      .done(function() {
            self.availableGroups.push(new myc.CustomerGroup({ name: data.name(), groupId: data.groupId() }));
            self.assignedGroups.remove(data);
            self.availableGroups.sort(SortGroupsByName);
      })
      .always(cb);
    },
    canExecute: function() { return !config.ajaxService.isBusy(); }
  });

  self.addToGroupCommand = ko.asyncCommand({
    execute: function(data, cb) {
      config.ajaxService.postJSON(config.customerGroupsApiUrl + "/" + data.groupId())
        .done(function() {
          self.assignedGroups.push(new myc.CustomerGroup({ name: data.name(), groupId: data.groupId() }));
          self.availableGroups.remove(data);
          self.assignedGroups.sort(SortGroupsByName);
        })
      .always(cb);
    },
    canExecute: function() { return !config.ajaxService.isBusy(); }
  });

  self.addGroupCommand = ko.asyncCommand({
    execute: function(cb) {
      if(self.validationState.isValid())
      {
        config.ajaxService.postJSON(config.customerGroupsApiUrl, { groupName: $.trim(self.newGroupName()) })
          .done(function(data) {
            self.assignedGroups.push(new myc.CustomerGroup(data));
            self.newGroupName(null);
            self.groupEntryIsVisible(false);
            self.assignedGroups.sort(SortGroupsByName);
          })
          .always(cb);
      }
      else
        cb();
    },
    canExecute: function () { return !config.ajaxService.isBusy() && self.hasNewGroupName() && self.validationState.isValid(); }
  });

  //#endregion groups

  //#region Wishlist

  self.addWishlistProductCommand = ko.asyncCommand({
    execute: function(data, cb) {
      var content = { productId: data.id() };
      config.ajaxService.postJSON(config.wishListUrl, content).done(function(success) { _addWishlistProductSuccess(success, data); }).always(cb);
    },
    canExecute: function() { return !config.ajaxService.isBusy(); }
  });

  self.removeWishlistProductCommand = ko.asyncCommand({
    execute: function(data, cb) {
      var content = { productId: data.id() };
      config.ajaxService.del(config.wishListUrl, content).done(function(success) { _removeWishlistProductSuccess(success, data); }).always(cb);
    },
    canExecute: function() { return !config.ajaxService.isBusy(); }
  });

  self.closeAddToWishListForm = function() {
    //default implementation does nothing.  This is implementated in the markup; 
  };

  function _addWishlistProductSuccess(success, data) {
    if(success) {
      config.productsData.wishListProducts.products.unshift(data);
      self.closeAddToWishListForm();
    }
  }

  function _removeWishlistProductSuccess(success, data) {
    if(success) {
      config.productsData.wishListProducts.products.remove(data);
      app.notifySuccess(app.localize('REMOVEWISHLIST_SUCCESS'));
    }
  }

  self.addWishlistProductCommand.isExecuting(false);

  self.wishListActivityIsExecuting = ko.computed(function() {
    return self.productsData.productCatalog.pageCommandIsExecuting()
      || self.addWishlistProductCommand.isExecuting();
  });

  //#endregion 

  //#region Activity summary

  var _isInitialActivityLoad = true;

  function _getActivitySummarySuccess(data) {
    if(self.activityCurrentPage() > self.activityPageCount())
    {
      self.activityCurrentPage(self.activityPageCount());
      _doActivitySearch(self.activityCurrentPage());
    }
    else
    {
      self.activities(data.activities);
    }
  }

  function _doActivitySearch(page, cb) {
    var criteria = _getCriteria();
    criteria.ai = page;

    var content = { page: criteria.ai, pageSize: criteria.as, customerId: config.customerId };

    //REVIEW why are we losing state on page on delete and undelete scenario
    if(criteria.ai === 0)
      criteria.ai = 1;

    config.ajaxService.getJSON(config.activitySummaryUrl, content)
      .done(function(data) {
        self.activityItemCount(data.count);
        self.activityCurrentPage(page);
        _getActivitySummarySuccess(data);
        _saveState(criteria);
      })
      .always(_activitySearchComplete)
      .always(cb);
  }

  function _activitySearchComplete(cb) {
    self.activityPageSizeTriggeredIsLoading(false);
    self.activityTabSelectedTriggeredIsLoading(false);
    _isInitialActivityLoad = false;
  }

  //#endregion

  //#region Tasks

  self.updateTaskCommand = ko.asyncCommand({
    execute: function(data, cb) {
      var content = {
        taskId: data.taskId(),
        title: data.editTaskTitleInput(),
        description: data.editTaskDescriptionInput(),
        dueDateUtc: data.editTaskDueDateUtcInput()
      };
      config.ajaxService.postJSON(config.updateTaskUrl, content).done(function() { _updateTaskCommandComplete(data, content); }).always(cb);
    },
    canExecute: function() { return !config.ajaxService.isBusy(); }
  });

  function _updateTaskCommandComplete(data, content) {
    data.title(content.title);
    data.description(content.description);
    data.dueDateUtc(content.dueDateUtc);
    data.editTaskTitleInput(null);
    data.editTaskDescriptionInput(null);
    data.editTaskDueDateUtcInput(null);
    data.isEditing(false);
  }

  self.updateTaskCompleteStatusCommand = ko.asyncCommand({
    execute: function(data, cb) {
      if(data.isComplete()) {
        config.ajaxService.postJSON(config.markTaskCompleteUrl, { id: data.taskId() }).always(cb);
      }
      else {
        config.ajaxService.postJSON(config.markTaskNotCompleteUrl, { id: data.taskId() }).always(cb);
      }
    },
    canExecute: function() { return !config.ajaxService.isBusy(); }
  });

  self.getAllTasksCommand = ko.asyncCommand({
    execute: function(cb) {
      var criteria = {
        page: 1,
        pageSize: 500,
        isComplete: false
      };
      config.ajaxService.getJSON(config.tasksUrl, criteria).done(function(data) { _tasksLoaded(data); self.moreTasksHasBeenClicked(true); }).always(cb);
    },
    canExecute: function() { return !config.ajaxService.isBusy(); }
  });

  function _doLoadTasks(cb) {
    self.initialTaskLoadIsExecuting(true);
    var criteria = {
      page: 1,
      pageSize: config.taskPageSize,
      isComplete: false
    };
    config.ajaxService.getJSON(config.tasksUrl, criteria).done(function(data) { _tasksLoaded(data); }).always(cb);
  }

  function _tasksLoaded(data) {
    var tasks = [];
    $.each(data.tasks, function(index, value) {
      tasks.push(new myc.CustomerTask(value));
    });
    self.tasks(tasks);
    self.totalTasks(data.count);

    if(!self.hasLoadedCustomerTasks())
      self.hasLoadedCustomerTasks(true);

    //TODO: move to finnaly or always executed
    self.initialTaskLoadIsExecuting(false);
  }

  self.addTaskCommand = ko.asyncCommand({
    execute: function(data, cb) {
      var content = {
        title: self.newTaskTitleInput(),
        dueDateUtc: self.newTaskDueDateUtcInput(),
        description: self.newTaskDescriptionInput(),
        firstName: config.firstName,
        lastName: config.lastName
      };

      config.ajaxService.postJSON(config.tasksUrl, content).done(_taskAdded).always(cb);
    },
    canExecute: function() { return !config.ajaxService.isBusy(); }
  });

  function _taskAdded(data) {
    self.tasks.unshift(new myc.CustomerTask(data));
    self.newTaskTitleInput(null);
    self.newTaskDueDateUtcInput(null);
    self.newTaskDescriptionInput(null);
    self.isInAddTaskMode(false);
  }

  self.cancelAddTaskClicked = function() {
    self.newTaskTitleInput(null);
    self.newTaskDueDateUtcInput(null);
    self.newTaskDescriptionInput(null);
    self.isInAddTaskMode(false);
  };

  self.addTaskClicked = function() {
    self.isInAddTaskMode(true);
  };

  //Defensively setting isExecuting to false on startup
  self.getAllTasksCommand.isExecuting(false);
  self.addTaskCommand.isExecuting(false);
  self.updateTaskCompleteStatusCommand.isExecuting(false);
  self.updateTaskCommand.isExecuting(false);

  self.taskIOIsExecuting = ko.computed(function() {
    return self.initialTaskLoadIsExecuting()
      || self.getAllTasksCommand.isExecuting()
      || self.addTaskCommand.isExecuting()
      || self.updateTaskCompleteStatusCommand.isExecuting()
      || self.updateTaskCommand.isExecuting();
  });

  //#endregion

  //#region Questionnaire

  self.getQuestionnaireAnswersCommand = ko.asyncCommand({
    execute: function(cb) {
      config.ajaxService.getJSON(config.questionnaireAnswersUrl).done(function(data) { _questionnaireAnswersLoaded(data); }).always(cb);
    },
    canExecute: function() { return !config.ajaxService.isBusy(); }
  });

  function _questionnaireAnswersLoaded(data) {
    self.questionnaireAnswers(data);
    self.hasLoadedQuestionnaireAnswers(true);
  }

  //#endregion

  //#region Orders

  self.customerOrderClicked = function(data) {
    if(data._source.OrderStatus === "Submitted" || data._source.OrderStatus === "UnderReview")
      self.navigate(config.editOrderUrl.replace('_0_', data._source.OrderId));
    else 
      self.navigate(config.orderDetailUrl.replace('_0_', data._source.OrderId));
  };

  self.customerOrderClickedActivity = function(data) {
    if(data.orderStatus === "Submitted" || data.orderStatus === "UnderReview")
      self.navigate(config.editOrderUrl.replace('_0_', data.orderId));
    else
      self.navigate(config.orderDetailUrl.replace('_0_', data.orderId));
  };

  function _ordersLoaded(data) {
    self.customerOrders(data);
    self.hasLoadedCustomerOrders(true);
  }

  self.orderSearchCommand = ko.asyncCommand({
    execute: function(cb) { _doOrderSearch(1, cb); },
    canExecute: function() { return !config.ajaxService.isBusy() && self.customerIsNotDeleted(); }
  });

  function _resetOrderSearch() {
    self.customerOrders([]);
    self.orderPageSize(config.orderPageSize);
    _doOrderSearch(null, 1);
  }

  function _doOrderSearch(page, cb) {
    var criteria = _getCriteria();
    criteria.i = page;

    // REVIEW why are we losing state on oage on delete and undelete scenario
    if(criteria.i === 0)
      criteria.i = 1;

    config.ajaxService.getJSON(config.customerOrdersApiUrl, criteria)
      .done(function(data) {
        self.orderItemCount(data.hits.total);
        self.orderCurrentPage(page);

        if(self.orderCurrentPage() > self.orderPageCount())
          self.orderCurrentPage(self.orderPageCount());

        _ordersLoaded(data.hits.hits);

        _saveState(criteria);
      })
      .always(_orderSearchComplete)
      .always(cb);
  }

  // REVIEW: should use config.ajaxService.isBusy rather than self.orderSearchInProgress
  function _orderSearchComplete(cb) {
    self.orderSearchInProgress(false);
  }

  //#endregion

  //#region Activity paging

  self.activityPageSize = ko.observable(config.activityPageSize);
  self.activityCurrentPage = ko.observable(1);
  self.activityItemCount = ko.observable(0);
  self.activityPageCount = ko.computed(function() { return Math.ceil(self.activityItemCount() / self.activityPageSize()); });

  self.prevActivityPageCommand = ko.asyncCommand({
    execute: function(cb) { _doActivitySearch(self.activityCurrentPage() - 1, cb); },
    canExecute: function() { return !config.ajaxService.isBusy() && self.activityCurrentPage() > 1; }
  });

  self.nextActivityPageCommand = ko.asyncCommand({
    execute: function(cb) { _doActivitySearch(self.activityCurrentPage() + 1, cb); },
    canExecute: function() { return !config.ajaxService.isBusy() && self.activityCurrentPage() < self.activityPageCount(); }
  });

  // trigger a search when activity page size changes
  ko.computed(function() {
    self.activityPageSize();
  }).subscribe(function() {
    if(!_isInitialActivityLoad)
    {
      self.activityPageSizeTriggeredIsLoading(true);
      _doActivitySearch(1);
    }
  });

  //#endregion

  //#region Order paging

  self.orderPageSize = ko.observable(config.orderPageSize);
  self.orderCurrentPage = ko.observable(1);
  self.orderItemCount = ko.observable(0);
  self.orderPageCount = ko.computed(function() { return Math.ceil(self.orderItemCount() / self.orderPageSize()); });

  self.prevOrderPageCommand = ko.asyncCommand({
    execute: function(cb) { _doOrderSearch(self.orderCurrentPage() - 1, cb); },
    canExecute: function() { return !config.ajaxService.isBusy() && self.orderCurrentPage() > 1; }
  });

  self.nextOrderPageCommand = ko.asyncCommand({
    execute: function(cb) { _doOrderSearch(self.orderCurrentPage() + 1, cb); },
    canExecute: function() { return !config.ajaxService.isBusy() && self.orderCurrentPage() < self.orderPageCount(); }
  });

  self.nextOrderPageCommand.isExecuting(false);
  self.prevOrderPageCommand.isExecuting(false);

  self.orderSearchIsExecuting = ko.computed(function() {
    return self.nextOrderPageCommand.isExecuting() ||
      self.prevOrderPageCommand.isExecuting() ||
      self.orderSearchInProgress();
  });

  // fire off a search when order pageSize changes
  self.orderPageSize.subscribe(function() {
    self.orderSearchInProgress(true);
    self.orderSearchCommand.execute();
  });

  //#endregion

  //#region Customer Delete / Undelete

  self.deleteCustomerCommand = ko.asyncCommand({
    execute: function(cb) {
      app.confirm('CONFIRM_DELETECUSTOMER_TITLE', 'CONFIRM_DELETECUSTOMER_BODY')
         .then(function () {
            app.showProgressModal('PROGRESS_DELETECUSTOMERS');
         })
         .then(function () { return config.ajaxService.postJSON(config.deleteCustomerUrl, { id: config.customerId }); })
         .then(function () {
           return $.ajaxPoll({
             url: config.customersApiUrl,
             type: 'GET',
             data: { q: config.customerId, qf: ['CustomerId'], rf: ['IsDeleted'] },
             dataType: 'json',
             pollingType: 'interval',
             interval: 500,
             maxInterval: 5000,
             expireAfter: 10,
             expire: function () {
               // TODO: show message indicating the operation will take longer
             },
             successCondition: function (res) {
               var isSuccess;

               if (res && res.hits && res.hits.hits) {
                 $.each(res.hits.hits, function (index, ival) {

                   if (config.customerId === ival._id && ival.fields && ival.fields.IsDeleted === true) {
                     app.logTrace("Customer deleted", config.customerId);
                     isSuccess = true;
                   }
                 });
               }

               return isSuccess;
             }
           });
         })
         .done(function () {
           app.closeProgressModal();
           app.notifySuccess(app.localize('DELETECUSTOMERS_SUCCESS'));
           document.location = config.customersListUrl;
         })
         .always(cb);
    },
    canExecute: function() { return !config.ajaxService.isBusy(); }
  });

  self.undeleteCustomerCommand = ko.asyncCommand({
    execute: function(cb) {
      app
        .confirm('CONFIRM_UNDELETECUSTOMER_TITLE', 'CONFIRM_UNDELETECUSTOMER_BODY')
        .then(function() { return config.ajaxService.postJSON(config.undeleteCustomerUrl, { id: config.customerId }); })
        .done(function() { self.customerIsDeleted(false); _resetOrderSearch(); })
        .always(cb);
    },
    canExecute: function() { return !config.ajaxService.isBusy(); }
  });

  //#endregion

  //#region Customer Archive / Unarchive

  self.archiveCustomerCommand = ko.asyncCommand({
    execute: function(cb) {
      app
        .confirm('CONFIRM_ARCHIVECUSTOMER_TITLE', 'CONFIRM_ARCHIVECUSTOMER_BODY', [config.firstName, config.lastName])
        .then(function() { return config.ajaxService.postJSON(config.archiveCustomerUrl, { id: config.customerId }); })
        .done(function() { self.customerIsArchived(true); })
        .always(cb);
    },
    canExecute: function() { return !config.ajaxService.isBusy(); }
  });

  self.unarchiveCustomerCommand = ko.asyncCommand({
    execute: function(cb) {
      app
        .confirm('CONFIRM_UNARCHIVECUSTOMER_TITLE', 'CONFIRM_UNARCHIVECUSTOMER_BODY')
        .then(function() { return config.ajaxService.postJSON(config.unarchiveCustomerUrl, { id: config.customerId }); })
        .done(function() { self.customerIsArchived(false); })
        .always(cb);
    },
    canExecute: function() { return !config.ajaxService.isBusy(); }
  });

  //#endregion

  //#region Ecards

  self.sendeCardCommand = ko.asyncCommand({
    execute: function(cb) {
      var message = {
        ContentId:      config.eCards.contentId,
        SubsidiaryCode: config.eCards.subsidiaryCode,
        ReturnUrl:      config.eCards.returnUrl,
        ContentMode:    config.eCards.contentMode,
        Recipients:     [{
          RecipientKey: config.customerId,
          EmailAddress: config.eCards.email,
          Attributes: {
            RecipientKey:   config.customerId,
            CustomerId:     config.customerId,
            SubsidaryCode:  config.eCards.subsidiaryCode,
            FirstName:      config.eCards.firstName,
            LastName:       config.eCards.lastName
          }
        }]
      };

      app.showProgressModal('SUBMITTING_ECARD');
      config.eCards.ecardService.submit(message, config.eCards.consultantId)
        .then(function(res) {
          // redirect to ecards application
          window.location.href = res.continueUrl;
        })
        .fail(app.closeProgressModal) // only close the modal on fail because the success case will redirect to a new page
        .always(cb);
    }
  });

  //#endregion

  //#region Notes
  function _getNotes() {
    config.ajaxService.getJSON(config.notesApiUrl).done(function(data) { _notesLoaded(data); });
  }

  function _notesLoaded(data) {
    var notes = [];
    $.each(data, function(index, value) {
      notes.push(new myc.CustomerNote(value));
    });
    self.notes(notes);
    self.hasLoadedCustomerNotes(true);
  }

  self.addNewNoteCommand = ko.asyncCommand({
    execute: function(cb) {
      config.ajaxService.postJSON(config.notesApiUrl, { content: self.newNoteInput() }).done(function(data) {
        self.notes.unshift(new myc.CustomerNote(data));
        self.newNoteInput(null);
      }).always(cb);
    },
    canExecute: function() { return self.hasNewNoteInputContent() && !config.ajaxService.isBusy(); }
  });

  self.updateNoteCommand = ko.asyncCommand({
    execute: function(ctx, cb) {
      var data = { content: ctx.editModeContent(), customerNoteKey: ctx.customerNoteKey(), dateCreatedUtc: ctx.dateCreatedUtc() };
      // REVIEW: don't concat urls. Use .replace('_id_')
      config
        .ajaxService
        .postJSON(config.notesApiUrl + "/" + data.customerNoteKey, data)
        .done(_noteUpdatedSuccess(ctx))
        .always(cb);
    },
    canExecute: function() { return !config.ajaxService.isBusy(); }
  });

  function _noteUpdatedSuccess(data) {
    data.content(data.editModeContent());
    data.isEditing(false);
  }

  self.deleteNoteCommand = ko.asyncCommand({
    execute: function(data, cb) {
      // REVIEW: don't concat urls. Use .replace('_id_')
      app
        .confirm('DELETE_NOTES_CONFIRMATION_TITLE', 'DELETE_NOTES_CONFIRMATION_BODY')
        .then(function() {
          return config.ajaxService.del(config.notesApiUrl + "/" + data.customerNoteKey(), data)
          .done(function() { self.notes.remove(data); })
          .always(cb);
        });
    },
    canExecute: function() { return !config.ajaxService.isBusy(); }
  });

  //#endregion

  //#region State

  self.setCurrentTab = function(tab) {
    self.currentTab(tab);
    var criteria = _getCriteria();
    _saveState(criteria);
    if(!self.hasLoadedQuestionnaireAnswers() && self.currentTab() === 'customerinfotab')
      self.getQuestionnaireAnswersCommand.execute();

    if(!self.hasLoadedCustomerOrders() && self.currentTab() === 'orderstab')
    {
      self.orderSearchInProgress(true);
      _doOrderSearch(self.orderCurrentPage());
    }

    if(!self.hasLoadedCustomerNotes() && self.currentTab() === 'notestab')
      _getNotes();

    if(!self.hasLoadedCustomerTasks())
      _doLoadTasks();

    if(self.currentTab() === 'historytab')
    {
      self.activityTabSelectedTriggeredIsLoading(true);
      _doActivitySearch(self.activityCurrentPage());
    }
    if (self.currentTab() === 'prrTab') {
      self.customerPrrView.show();
    }
  };

  function _loadState() {
    var jsonState = app.session.getItem('myc.customerDetail.state' + config.customerId);
    if(jsonState !== undefined && jsonState !== null)
    {
      var state = $.parseJSON(jsonState);

      if(state.t !== 'undefined')
        self.currentTab(state.t);

      if(!isNaN(state.s))
        self.orderPageSize(state.s);

      if(!isNaN(state.i))
        self.orderCurrentPage(state.i);

      if(!isNaN(state.as))
        self.activityPageSize(state.as);

      if(!isNaN(state.ai))
        self.activityCurrentPage(state.ai);
    }
  }

  function _saveState(state) {
    var jsonState = ko.toJSON(state);
    app.session.setItem('myc.customerDetail.state' + config.customerId, jsonState);
  }

  function _getCriteria() {
    var criteria = {
      s: self.orderPageSize(),
      i: self.orderCurrentPage(),
      as: self.activityPageSize(),
      ai: self.activityCurrentPage(),
      sd: true,
      sf: 'OrderDateUtc',
      d: false
    };
    return criteria;
  }

  //#endregion

  //#region Validation

  self.validationState = ko.validation.applyTo(self, {
    global: {
      newGroupName: {
        validation: {
          message: 'EDIT_CUSTOMER_INVALID_DUPLICATE_GROUP_NAME',
          validator: function(val) {
            if(!val) return true;

            var isValidFormat = /^[\w\s]{1,256}$/gi.test(val);
            var isDuplicate = _groupNameExists(_assignedGroups, self.newGroupName()) || _groupNameExists(_availableGroups, self.newGroupName());

            return isValidFormat && (!isDuplicate);
          }
        }
      }
    }
  });

  //#endregion

  //VMO
  self.vmoLinkClicked = function() {
    self.navigateNewWindow(config.vmoUrl);
  };

  // Load groups for this customer
  _getCustomerGroups();

  //init
  _loadState();
};
