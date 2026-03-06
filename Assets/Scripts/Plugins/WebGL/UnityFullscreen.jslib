mergeInto(LibraryManager.library, {
  // ---- API nova (UnityFullscreen_*) ----
  UnityFullscreen_Init: function(){},
  UnityFullscreen_Toggle: function () {
    if (typeof document === "undefined") return;
    var c = (typeof Module !== "undefined") ? Module['canvas'] : null;
    var isFs = !!(document.fullscreenElement || document.webkitFullscreenElement || document.mozFullScreenElement || document.msFullscreenElement);
    try {
      if (!isFs) {
        var req = (c && (c.requestFullscreen || c.webkitRequestFullscreen || c.mozRequestFullScreen || c.msRequestFullscreen));
        if (req) req.call(c);
      } else {
        var exit = (document.exitFullscreen || document.webkitExitFullscreen || document.mozCancelFullScreen || document.msExitFullscreen);
        if (exit) exit.call(document);
      }
    } catch (e) { /* no-op */ }
  },
  UnityFullscreen_IsFullscreen: function () {
    if (typeof document === "undefined") return 0;
    return (document.fullscreenElement || document.webkitFullscreenElement || document.mozFullScreenElement || document.msFullscreenElement) ? 1 : 0;
  },

  // ---- Stubs para plugins antigos (UnityFS_*) ----
  UnityFS_toggle: function () { // compat
    if (typeof document === "undefined") return;
    var c = (typeof Module !== "undefined") ? Module['canvas'] : null;
    var isFs = !!(document.fullscreenElement || document.webkitFullscreenElement || document.mozFullScreenElement || document.msFullscreenElement);
    try {
      if (!isFs) {
        var req = (c && (c.requestFullscreen || c.webkitRequestFullscreen || c.mozRequestFullScreen || c.msRequestFullscreen));
        if (req) req.call(c);
      } else {
        var exit = (document.exitFullscreen || document.webkitExitFullscreen || document.mozCancelFullScreen || document.msExitFullscreen);
        if (exit) exit.call(document);
      }
    } catch (e) { /* no-op */ }
  },
  UnityFS_isFullscreen: function () {
    if (typeof document === "undefined") return 0;
    return (document.fullscreenElement || document.webkitFullscreenElement || document.mozFullScreenElement || document.msFullscreenElement) ? 1 : 0;
  },
  UnityFS_fullFitEnable: function (enable) { /* no-op compat */ }
});
