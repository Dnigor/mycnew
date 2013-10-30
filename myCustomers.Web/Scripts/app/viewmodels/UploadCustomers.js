var myc = myc || {};
var app = app || {};

myc.UploadCustomers = function (config) {
  var self = this;

  self.csvfilename = ko.observable('').extend({
    required: {
      message: 'ERROR_CSV_FILE_REQUIRED',
      params: true
    },
    pattern: {
      message: "ERROR_CSV_FILE_EXPECTED",
      params: /^.+\.csv$/gi
    }
  });

  self.updateCsvFileName = function (data, event) {
    self.csvfilename(event.target.value.replace(/^.*[\\\/]/, ''));
  };

  self.uploadCsvFileCommand = ko.asyncCommand({
    execute: function (cb) {
      self.uploadCsvFile(cb); 
    },
    canExecute: function () {
      return self.csvfilename.isValid()
    }
  });

  self.uploadCsvFile = function (cb) {
    $('#uploadCsvForm').ajaxSubmit({
      url: config.csvFileUploadUrl,
      type: "POST",
      dataType: "text",
      headers: { "Content-Disposition": "attachment; filename=" + $('#uploadCsvForm' + 'input[name=csvinputfile]').val() },

      success: function (responseJson) {
        var response = JSON.parse(responseJson);
        if (!response.success) {
          app.notifyError(response.message);
        }
        else location.href = config.csvFileUploadRedirectUrl;
      },

      error: function (jqXHR, textStatus, errorThrown) {
        app.notifyError(errorThrown);
      },

      complete: cb
    });
   
  };


};