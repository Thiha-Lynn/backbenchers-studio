mergeInto(LibraryManager.library, {
  BBStudioSound: function(kind) { window.dispatchEvent(new CustomEvent('backbenchers-sound', {detail:UTF8ToString(kind)})); },
  BBStudioRoom: function(lights,door) { window.dispatchEvent(new CustomEvent('backbenchers-room', {detail:{lights:!!lights,door:!!door}})); },
  BBStudioPrompt: function(json) { window.dispatchEvent(new CustomEvent('backbenchers-prompt', {detail:JSON.parse(UTF8ToString(json))})); },
  BBStudioObject: function(json) { window.dispatchEvent(new CustomEvent('backbenchers-inspect', {detail:JSON.parse(UTF8ToString(json))})); },
  BBStudioPerformance: function(fps) {
    window.dispatchEvent(new CustomEvent('backbenchers-performance', {detail:{fps:fps}}));
  },
  BBStudioSeat: function(x,y,width,height) {
    window.dispatchEvent(new CustomEvent('backbenchers-seat', {detail:{x:x,y:y,width:width,height:height}}));
  },
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
