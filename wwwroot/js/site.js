(function () {
  "use strict";

  // ---- Mobile navigation -------------------------------------------------
  var header = document.querySelector(".site-header");
  var toggle = document.querySelector(".nav-toggle");
  if (header && toggle) {
    toggle.addEventListener("click", function () {
      var open = header.classList.toggle("nav-open");
      toggle.setAttribute("aria-expanded", open ? "true" : "false");
    });
    document.addEventListener("keydown", function (e) {
      if (e.key === "Escape" && header.classList.contains("nav-open")) {
        header.classList.remove("nav-open");
        toggle.setAttribute("aria-expanded", "false");
        toggle.focus();
      }
    });
  }

  // ---- Scroll reveal -----------------------------------------------------
  var revealEls = document.querySelectorAll(".reveal");
  if ("IntersectionObserver" in window && revealEls.length) {
    var io = new IntersectionObserver(function (entries) {
      entries.forEach(function (entry) {
        if (entry.isIntersecting) {
          entry.target.classList.add("in");
          io.unobserve(entry.target);
        }
      });
    }, { threshold: 0.12 });
    revealEls.forEach(function (el) { io.observe(el); });
  } else {
    revealEls.forEach(function (el) { el.classList.add("in"); });
  }

  // ---- Lightbox ----------------------------------------------------------
  var box = document.getElementById("lightbox");
  if (box && typeof box.showModal === "function") {
    var boxImg = box.querySelector("img");
    var boxCap = box.querySelector(".lb-cap");
    document.addEventListener("click", function (e) {
      var trigger = e.target.closest("[data-lightbox]");
      if (trigger) {
        boxImg.src = trigger.getAttribute("data-lightbox");
        boxImg.alt = trigger.getAttribute("data-caption") || "";
        boxCap.textContent = trigger.getAttribute("data-caption") || "";
        var boxLink = box.querySelector(".lb-link");
        var linkHref = trigger.getAttribute("data-link");
        if (boxLink) {
          if (linkHref) { boxLink.href = linkHref; boxLink.hidden = false; }
          else { boxLink.removeAttribute("href"); boxLink.hidden = true; }
        }
        box.showModal();
        return;
      }
      if (e.target === box || e.target.closest("[data-lightbox-close]")) { box.close(); }
    });
    box.addEventListener("close", function () { boxImg.removeAttribute("src"); });
  }

  // ---- Confirm before destructive admin actions --------------------------
  document.addEventListener("submit", function (e) {
    var msg = e.target.getAttribute("data-confirm");
    if (msg && !window.confirm(msg)) { e.preventDefault(); }
  });

  // ---- Admin item form: show fields that matter for the chosen type ------
  var typeSelect = document.querySelector("[data-type-select]");
  if (typeSelect) {
    var labels = { "0": "Category", "1": "Medium", "2": "Style" };
    var sync = function () {
      var t = typeSelect.value;
      document.querySelectorAll("[data-only-type]").forEach(function (el) {
        el.hidden = el.getAttribute("data-only-type").split(" ").indexOf(t) === -1;
      });
      var catLabel = document.querySelector("[data-category-label]");
      if (catLabel) { catLabel.textContent = labels[t] || "Category"; }
    };
    typeSelect.addEventListener("change", sync);
    sync();
  }

  // ---- Bring the contact form into view when it has errors ---------------
  var contact = document.getElementById("contact");
  if (contact && contact.querySelector(".field-error:not(:empty), .validation-summary-errors")) {
    contact.scrollIntoView({ block: "start" });
  }
})();
