mergeInto(LibraryManager.library, {
  PlansulBridge_Init: function () {
    if (typeof window === "undefined") return;

    function dispatchToUnity(payload) {
      try {
        var data = payload;
        if (typeof data !== "string") {
          data = JSON.stringify(data);
        }

        if (typeof SendMessage === "function") {
          SendMessage("RadioSignal", "OnJsMessage", data);
        }
      } catch (e) {
        if (typeof console !== "undefined" && console.error) {
          console.error("[PlansulBridge] Erro ao despachar para Unity:", e);
        }
      }
    }

    if (!window.__plansulBridgeReady) {
      window.__plansulBridgeReady = true;
      window.__plansulBridgeDispatch = dispatchToUnity;

      window.addEventListener("message", function (evt) {
        dispatchToUnity(evt.data);
      }, false);

      // Callback direto para wrappers nativos que nao usam postMessage padrao
      window.PlansulBridge_Deliver = function (payload) {
        dispatchToUnity(payload);
      };
    }

    try {
      if (typeof window.PlansulBridge_OnUnityReady === "function") {
        window.PlansulBridge_OnUnityReady();
      }
    } catch (e) {}
  },

  PlansulBridge_Send: function (jsonPtr) {
    if (typeof window === "undefined") return;

    var json = UTF8ToString(jsonPtr);

    try {
      // Handler dedicado do host (iframe/app)
      if (typeof window.PlansulBridge_Send === "function") {
        window.PlansulBridge_Send(json);
        return;
      }

      // Fallback web padrao
      if (window.parent && window.parent !== window) {
        window.parent.postMessage(json, "*");
      }

      // Fallback React Native WebView
      if (window.ReactNativeWebView && typeof window.ReactNativeWebView.postMessage === "function") {
        window.ReactNativeWebView.postMessage(json);
      }
    } catch (e) {
      if (typeof console !== "undefined" && console.error) {
        console.error("[PlansulBridge] Erro ao enviar:", e);
      }
    }
  },

  // Compatibilidade com externs legados
  SendNivelSelecionado: function (level) {
    if (typeof window === "undefined") return;

    try {
      var payload = JSON.stringify({
        type: "level_click",
        level: level | 0,
        from: "game",
        ts: new Date().toISOString()
      });

      if (typeof window.PlansulBridge_Send === "function") {
        window.PlansulBridge_Send(payload);
        return;
      }

      if (window.parent && window.parent !== window) {
        window.parent.postMessage(payload, "*");
      }

      if (window.ReactNativeWebView && typeof window.ReactNativeWebView.postMessage === "function") {
        window.ReactNativeWebView.postMessage(payload);
      }
    } catch (e) {}
  },

  InitNivelSelecao: function () { /* no-op compat */ }
});
