mergeInto(LibraryManager.library, {
  BBStudioReady: function(renderer) {
    var detail = { renderer: UTF8ToString(renderer) };
    window.dispatchEvent(new CustomEvent('backbenchers-ready', { detail: detail }));
  },
  BBStudioDevice: function(id,power,page) {
    window.dispatchEvent(new CustomEvent('backbenchers-device', {detail:{id:UTF8ToString(id),power:!!power,page:page}}));
  },
  BBStudioInspect: function(topic) {
    window.dispatchEvent(new CustomEvent('backbenchers-inspect', {detail:{topic:UTF8ToString(topic)}}));
  },
  BBStudioView: function(index) {
    window.dispatchEvent(new CustomEvent('backbenchers-view', { detail: { index: index } }));
  }
});
