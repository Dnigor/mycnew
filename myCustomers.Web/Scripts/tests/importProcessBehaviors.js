describe('ImportProcess', function () {
  var config = {
    csvFileUploadUrl: '@Url.Action("uploadcsvfile", "customers")',
    csvFileUploadRedirectUrl: '@Url.Action("importlist", "customers")'
  };

  it('should start uploading csv file', function () {
    var uploadService = new myc.UploadCustomers(config);
    spyOn(uploadService, 'uploadCsvFile');
    uploadService.csvfilename('test.csv');
    uploadService.uploadCsvFileCommand.execute();
    expect(uploadService.uploadCsvFile).toHaveBeenCalled();
  });

  it('should not start uploading file other than csv', function () {
    var uploadService = new myc.UploadCustomers(config);
    spyOn(uploadService, 'uploadCsvFile');
    uploadService.csvfilename('test.jpg');
    uploadService.uploadCsvFileCommand.execute();
    expect(uploadService.uploadCsvFile).wasNotCalled();
  });


});