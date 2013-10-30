var myc = myc || {}

myc.ProfileQuestionGroup = function(model) {
  var self = this;

  self.key       = ko.observable();
  self.text      = ko.observable();
  self.questions = ko.observableArray([]);

  //#region Mapping

  var _mapping = {
    questions: {
      create: function(ctx) { 
        return new myc.ProfileQuestion(ctx.data); 
      }
    }
  };

  model = $.extend({}, self, model);
  ko.mapping.fromJS(model, _mapping, self);

  //#endregion
};

myc.ProfileQuestion = function(model) {
  var self = this;

  self.key     = ko.observable();
  self.type    = ko.observable();
  self.text    = ko.observable();
  self.answer  = ko.observable();
  self.answers = ko.observableArray([]);
  self.options = ko.observableArray([]);

  //#region Mapping

  var _mapping = {
    options: {
      create: function(ctx) { 
        return new myc.ProfileQuestionOption(ctx.data);
      }
    }
  }

  model = $.extend({}, self, model);
  ko.mapping.fromJS(model, _mapping, self);

  //#endregion

  self.id = 'question' + app.newId();

  if (!self.answers())
    self.answers([]);
};

myc.ProfileQuestionOption = function(model) {
  var self = this;

  self.key           = ko.observable();
  self.text          = ko.observable();
  self.imageUrl      = ko.observable();
  self.allowFreeText = ko.observable();

  //#region Mapping

  model = $.extend({}, self, model);
  ko.mapping.fromJS(model, {}, self);

  //#endregion

  self.id = 'option' + app.newId();
};