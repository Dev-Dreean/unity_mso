mergeInto(LibraryManager.library, {
  YT_SetReceiver: function (goPtr) {
    try {
      // UTF8ToString sempre existe no runtime WebGL do Unity
      var goName = UTF8ToString(goPtr);
      window.__ytReceiver = goName;
    } catch (e) {
      // fallback silencioso
      window.__ytReceiver = null;
    }
  },

  YT_Open: function (urlPtr) {
    var url = UTF8ToString(urlPtr);

    // cria overlay se não existir
    var ov = document.getElementById("yt-overlay");
    if (!ov) {
      ov = document.createElement("div");
      ov.id = "yt-overlay";
      ov.style.position = "fixed";
      ov.style.left = "0";
      ov.style.top = "0";
      ov.style.right = "0";
      ov.style.bottom = "0";
      ov.style.zIndex = "999999";
      ov.style.display = "flex";
      ov.style.alignItems = "center";
      ov.style.justifyContent = "center";
      ov.style.background = "rgba(0,0,0,0.7)";
      // backdrop-blur é opcional, alguns browsers antigos não suportam
      ov.style.backdropFilter = "blur(2px)";

      // clique fora fecha
      ov.addEventListener("click", function () {
        // fecha direto sem ccall para evitar dependência de export
        try {
          window.__ytClose && window.__ytClose();
        } catch (e) {}
      });

      var wrap = document.createElement("div");
      wrap.id = "yt-framewrap";
      wrap.style.position = "relative";
      wrap.style.width = "min(92vw, 1200px)";
      // usar técnica 16:9 compatível (sem aspect-ratio)
      wrap.style.maxWidth = "1200px";
      wrap.style.width = "92vw";
      wrap.style.height = "auto";
      wrap.style.borderRadius = "16px";
      wrap.style.overflow = "hidden";
      wrap.style.boxShadow = "0 20px 60px rgba(0,0,0,.6)";
      wrap.style.background = "#000";

      // impedir que clique no vídeo feche
      wrap.addEventListener("click", function (e) {
        e.stopPropagation();
      });

      // container com padding-top para manter 16:9
      var keep = document.createElement("div");
      keep.style.position = "relative";
      keep.style.width = "100%";
      keep.style.paddingTop = "56.25%"; // 16:9

      var iframe = document.createElement("iframe");
      iframe.id = "yt-frame";
      iframe.allow =
        "accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share";
      iframe.referrerPolicy = "strict-origin-when-cross-origin";
      iframe.allowFullscreen = true;
      iframe.style.position = "absolute";
      iframe.style.left = "0";
      iframe.style.top = "0";
      iframe.style.width = "100%";
      iframe.style.height = "100%";
      iframe.style.border = "0";
      iframe.style.display = "block";

      keep.appendChild(iframe);
      wrap.appendChild(keep);
      ov.appendChild(wrap);
      document.body.appendChild(ov);

      // função global para fechar
      window.__ytClose = function () {
        var ov2 = document.getElementById("yt-overlay");
        if (ov2) {
          var ifr = document.getElementById("yt-frame");
          if (ifr) ifr.src = "about:blank";
          ov2.style.display = "none";
        }
        if (window.__ytReceiver) {
          try {
            SendMessage(window.__ytReceiver, "OnYouTubeOverlayClosed", "");
          } catch (e) {}
        }
      };

      // ESC fecha
      window.addEventListener("keydown", function (e) {
        if (e.key === "Escape") {
          try {
            window.__ytClose();
          } catch (err) {}
        }
      });
    }

    // set src (aceita embed completo ou ID)
    var s =
      url.indexOf("youtube.com/embed") >= 0
        ? url
        : "https://www.youtube.com/embed/" + url;
    document.getElementById("yt-frame").src = s;
    ov.style.display = "flex";
  },

  YT_Close: function () {
    if (window.__ytClose) window.__ytClose();
  },
});
