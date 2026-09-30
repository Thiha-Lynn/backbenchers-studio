mergeInto(LibraryManager.library, {
  BBStudioReady: function(renderer) {
    var detail = { renderer: UTF8ToString(renderer) };
    window.dispatchEvent(new CustomEvent('backbenchers-ready', { detail: detail }));
  },
  BBStudioView: function(index) {
    window.dispatchEvent(new CustomEvent('backbenchers-view', { detail: { index: index } }));
  }
});
