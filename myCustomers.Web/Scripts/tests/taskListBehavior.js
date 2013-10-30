describe("Task list", function () {
  var config = {
    actions:
    {
      search: '/search',
      saveTask: '/save',
      addTask: '/add',
      getCustomerList: '/getcustomerlist',
      updateTaskStatus: '/updatetaskstatus',
      updateMassTaskStatus: '/updatemasstaskstatus'
    },
    data:
    {
      remindersCount: 2
    }
  };

  it("should use search url from config object", function () {
    var startingSubsidiaryCode = app.subsidiary;
    app.subsidiary = 'xx';
    var model = new myc.TaskList(config);

    expect(model.searchService.url()).toBe(config.actions.search);

    app.subsidiary = startingSubsidiaryCode;
  });

  it("should disable searchCommand when search service  is busy", function () {
    var startingSubsidiaryCode = app.subsidiary;
    app.subsidiary = 'xx';
    var model = new myc.TaskList(config);

    model.searchService.isBusy(false);
    expect(model.searchCommand.canExecute()).toBe(true);
    model.searchService.isBusy(true);
    expect(model.searchCommand.canExecute()).toBe(false);

    app.subsidiary = startingSubsidiaryCode;
  });

  it("should disable prevPageCommand when currentWeekPage is 0", function () {
    var startingSubsidiaryCode = app.subsidiary;
    app.subsidiary = 'xx';
    var model = new myc.TaskList(config);

    model.currentWeekPage(0);
    expect(model.prevPageCommand.canExecute()).toBe(false);

    app.subsidiary = startingSubsidiaryCode;
  });

  it("should disable nextPageCommand when currentWeekPage is 4( ~ 1 month)", function () {
    var startingSubsidiaryCode = app.subsidiary;
    app.subsidiary = 'xx';
    var model = new myc.TaskList(config);

    model.currentWeekPage(4);
    expect(model.nextPageCommand.canExecute()).toBe(false);

    app.subsidiary = startingSubsidiaryCode;
  });

  it("should call an Ajax post when do save Task", function () {
    var startingSubsidiaryCode = app.subsidiary;
    app.subsidiary = 'xx';
    var model = new myc.TaskList(config);

    model.selectedTask(new myc.Task());
    model.ajaxService.postJSON = jasmine.createSpy('Post JSON')
      .andCallFake(function () { return $.Deferred(function () { }) });
    var cb = jasmine.createSpy('cb');
    model.saveCommand.execute(cb);

    expect(model.ajaxService.postJSON).wasCalled();

    app.subsidiary = startingSubsidiaryCode;
  });


  it("should make an Ajax request to the correct URL when do save Task command", function () {
    var startingSubsidiaryCode = app.subsidiary;
    app.subsidiary = 'xx';
    var model = new myc.TaskList(config);
    model.ajaxService.postJSON = jasmine.createSpy('postJSON')
      .andCallFake(function () { return $.Deferred(function () { }) });

    model.selectedTask(new myc.Task());
    model.saveCommand.execute();
    expect(model.ajaxService.postJSON.mostRecentCall.args[0]).toEqual(config.actions.saveTask);

    app.subsidiary = startingSubsidiaryCode;
  });

  it("should call an Ajax post when do add Task", function () {
    var startingSubsidiaryCode = app.subsidiary;
    app.subsidiary = 'xx';
    var model = new myc.TaskList(config);

    model.newTask(new myc.Task());
    model.ajaxService.postJSON = jasmine.createSpy('postJSON')
      .andCallFake(function () { return $.Deferred(function () { }) });

    model.addCommand.execute();
    expect(model.ajaxService.postJSON).wasCalled();

    app.subsidiary = startingSubsidiaryCode;
  });


  it("should make an Ajax request to the correct URL when do add Task command", function () {
    var startingSubsidiaryCode = app.subsidiary;
    app.subsidiary = 'xx';
    var model = new myc.TaskList(config);
    model.ajaxService.postJSON = jasmine.createSpy('postJSON')
      .andCallFake(function () { return $.Deferred(function () { }) });

    model.newTask(new myc.Task());
    model.addCommand.execute();
    expect(model.ajaxService.postJSON.mostRecentCall.args[0]).toEqual(config.actions.addTask);

    app.subsidiary = startingSubsidiaryCode;
  });

  it("should make an Ajax request to the correct URL when do search Task command", function () {
    var startingSubsidiaryCode = app.subsidiary;
    app.subsidiary = 'xx';
    var model = new myc.TaskList(config);

    spyOn($, "ajax");
    model.filterType("");
    var cb = jasmine.createSpy('cb');
    model.searchCommand.execute(cb);
    expect($.ajax.mostRecentCall.args[0]['url']).toEqual(config.actions.search);

    app.subsidiary = startingSubsidiaryCode;
  });

  it("should call search search when do search Task", function () {
    var startingSubsidiaryCode = app.subsidiary;
    app.subsidiary = 'xx';
    var model = new myc.TaskList(config);

    model.searchService.search = jasmine.createSpy();
    model.filterType("");
    var cb = jasmine.createSpy('cb');
    model.searchCommand.execute(cb);
    expect(model.searchService.search).wasCalled();

    app.subsidiary = startingSubsidiaryCode;
  });
});