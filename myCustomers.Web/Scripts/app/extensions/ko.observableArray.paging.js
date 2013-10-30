(function (window, ko, undefined) {

  ko.extenders.paging = function (target, pageSize) {
    var _pageSize = ko.observable(pageSize || 0), // default pageSize to 0
        _currentPage = ko.observable(1); // default current page to 1

    target.pageSize = ko.computed({
      read: _pageSize,
      write: function (newValue) {
        if (newValue > 0) {
          _pageSize(newValue);
        }
        else {
          _pageSize(10);
        }
      }
    });

    target.pageCount = ko.computed(function () {
      return Math.ceil(target().length / target.pageSize()) || 1;
    });

    target.currentPage = ko.computed({
      read: _currentPage,
      write: function (newValue) {
        if (newValue <= 0) {
          _currentPage(1);
        }
        else {
          _currentPage(newValue);
        }
      }
    });

    target.currentPageData = ko.computed(function () {
      var endIndex = target().length;
      if (_currentPage() !== Infinity) {
        var pageSize = _pageSize(),
            pageIndex = _currentPage();
        endIndex = pageSize * pageIndex;
      }
      return target().slice(0, endIndex);
    });

    target.viewMore = function () {
      target.currentPage(target.currentPage() + 1);
    };
    
    target.viewAll = function () {
      target.currentPage(target.pageCount());
    };

    return target;
  };

})(window, ko);