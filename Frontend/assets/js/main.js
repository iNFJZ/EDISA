/**
 * Main
 */

"use strict";

let isRtl = window.Helpers.isRtl(),
  isDarkStyle = window.Helpers.isDarkStyle(),
  menu,
  animate,
  isHorizontalLayout = false;

if (document.getElementById("layout-menu")) {
  isHorizontalLayout = document
    .getElementById("layout-menu")
    .classList.contains("menu-horizontal");
}

(function () {
  setTimeout(function () {
    window.Helpers.initCustomOptionCheck();
  }, 1000);

  if (typeof Waves !== "undefined") {
    Waves.init();
    Waves.attach(
      ".btn[class*='btn-']:not([class*='btn-outline-']):not([class*='btn-label-'])",
      ["waves-light"],
    );
    Waves.attach("[class*='btn-outline-']");
    Waves.attach("[class*='btn-label-']");
    Waves.attach(".pagination .page-item .page-link");
  }

  // Initialize menu
  //-----------------

  let layoutMenuEl = document.querySelectorAll("#layout-menu");
  layoutMenuEl.forEach(function (element) {
    menu = new Menu(element, {
      orientation: isHorizontalLayout ? "horizontal" : "vertical",
      closeChildren: isHorizontalLayout ? true : false,
      // ? This option only works with Horizontal menu
      showDropdownOnHover: localStorage.getItem(
        "templateCustomizer-" + templateName + "--ShowDropdownOnHover",
      ) // If value(showDropdownOnHover) is set in local storage
        ? localStorage.getItem(
            "templateCustomizer-" + templateName + "--ShowDropdownOnHover",
          ) === "true" // Use the local storage value
        : window.templateCustomizer !== undefined // If value is set in config.js
          ? window.templateCustomizer.settings.defaultShowDropdownOnHover // Use the config.js value
          : true, // Use this if you are not using the config.js and want to set value directly from here
    });
    // Change parameter to true if you want scroll animation
    window.Helpers.scrollToActive((animate = false));
    window.Helpers.mainMenu = menu;
  });

  // Initialize menu togglers and bind click on each
  let menuToggler = document.querySelectorAll(".layout-menu-toggle");
  menuToggler.forEach((item) => {
    item.addEventListener("click", (event) => {
      event.preventDefault();
      window.Helpers.toggleCollapsed();
      // Enable menu state with local storage support if enableMenuLocalStorage = true from config.js
      if (config.enableMenuLocalStorage && !window.Helpers.isSmallScreen()) {
        try {
          localStorage.setItem(
            "templateCustomizer-" + templateName + "--LayoutCollapsed",
            String(window.Helpers.isCollapsed()),
          );
          // Update customizer checkbox state on click of menu toggler
          let layoutCollapsedCustomizerOptions = document.querySelector(
            ".template-customizer-layouts-options",
          );
          if (layoutCollapsedCustomizerOptions) {
            let layoutCollapsedVal = window.Helpers.isCollapsed()
              ? "collapsed"
              : "expanded";
            layoutCollapsedCustomizerOptions
              .querySelector(`input[value="${layoutCollapsedVal}"]`)
              .click();
          }
        } catch (e) {}
      }
    });
  });

  // Menu swipe gesture

  // Detect swipe gesture on the target element and call swipe In
  window.Helpers.swipeIn(".drag-target", function (e) {
    window.Helpers.setCollapsed(false);
  });

  // Detect swipe gesture on the target element and call swipe Out
  window.Helpers.swipeOut("#layout-menu", function (e) {
    if (window.Helpers.isSmallScreen()) window.Helpers.setCollapsed(true);
  });

  // Display in main menu when menu scrolls
  let menuInnerContainer = document.getElementsByClassName("menu-inner"),
    menuInnerShadow = document.getElementsByClassName("menu-inner-shadow")[0];
  if (menuInnerContainer.length > 0 && menuInnerShadow) {
    menuInnerContainer[0].addEventListener("ps-scroll-y", function () {
      if (this.querySelector(".ps__thumb-y").offsetTop) {
        menuInnerShadow.style.display = "block";
      } else {
        menuInnerShadow.style.display = "none";
      }
    });
  }

  // Update light/dark image based on current style
  function switchImage(style) {
    if (style === "system") {
      if (window.matchMedia("(prefers-color-scheme: dark)").matches) {
        style = "dark";
      } else {
        style = "light";
      }
    }
    const switchImagesList = [].slice.call(
      document.querySelectorAll("[data-app-" + style + "-img]"),
    );
    switchImagesList.map(function (imageEl) {
      const setImage = imageEl.getAttribute("data-app-" + style + "-img");
      imageEl.src = assetsPath + "img/" + setImage; // Using window.assetsPath to get the exact relative path
    });
  }

  //Style Switcher (Light/Dark/System Mode)
  let styleSwitcher = document.querySelector(".dropdown-style-switcher");

  // Set style on click of style switcher item if template customizer is enabled
  if (window.templateCustomizer && styleSwitcher) {
    // Get style from local storage or use 'system' as default
    let storedStyle =
      localStorage.getItem("templateCustomizer-" + templateName + "--Style") ||
      window.templateCustomizer.settings.defaultStyle;

    let styleSwitcherItems = [].slice.call(
      styleSwitcher.children[1].querySelectorAll(".dropdown-item"),
    );
    styleSwitcherItems.forEach(function (item) {
      item.addEventListener("click", function () {
        let currentStyle = this.getAttribute("data-theme");
        if (currentStyle === "light") {
          window.templateCustomizer.setStyle("light");
        } else if (currentStyle === "dark") {
          window.templateCustomizer.setStyle("dark");
        } else {
          window.templateCustomizer.setStyle("system");
        }
      });
    });

    // Update style switcher icon based on the stored style

    const styleSwitcherIcon = styleSwitcher.querySelector("i");

    if (storedStyle === "light") {
      styleSwitcherIcon.classList.add("ti-sun");
      new bootstrap.Tooltip(styleSwitcherIcon, {
        title: "Light Mode",
        fallbackPlacements: ["bottom"],
      });
    } else if (storedStyle === "dark") {
      styleSwitcherIcon.classList.add("ti-moon");
      new bootstrap.Tooltip(styleSwitcherIcon, {
        title: "Dark Mode",
        fallbackPlacements: ["bottom"],
      });
    } else {
      styleSwitcherIcon.classList.add("ti-device-desktop");
      new bootstrap.Tooltip(styleSwitcherIcon, {
        title: "System Mode",
        fallbackPlacements: ["bottom"],
      });
    }

    // Run switchImage function based on the stored style
    switchImage(storedStyle);
  }

  // Internationalization (Language Dropdown)
  // ---------------------------------------

  function setLanguage(lang) {
    if (!["en", "vi", "ja"].includes(lang)) lang = "en";
    window.i18next.changeLanguage(lang, (err, t) => {
      if (err) {
        return;
      }
      localize();
      showToastrMessage("languageChanged", "success");
      updateLanguageDropdown(lang);
    });
  }

  if (typeof window.i18next !== "undefined") {
    if (
      typeof window.i18nextHttpBackend === "function" ||
      (window.i18nextHttpBackend &&
        typeof window.i18nextHttpBackend === "object")
    ) {
      if (typeof window.i18next.use === "function") {
        window.i18next.use(window.i18nextHttpBackend);
      }
      window.i18next
        .init({
          lng: localStorage.getItem("i18nextLng") || "vi",
          debug: false,
          fallbackLng: "vi",
          backend: {
            loadPath: assetsPath + "lang/{{lng}}.json",
          },
          returnObjects: true,
        })
        .then(function (t) {
          localize();
          const currentLang = localStorage.getItem("i18nextLng") || "vi";
          updateLanguageDropdown(currentLang);
        });
    } else {
      console.error(
        "[i18n] i18nextHttpBackend is missing! File i18n.js may be incorrect or missing backend plugin. Please check your vendor/i18n/i18n.js build.",
      );
      let i18nList = document.querySelectorAll("[data-i18n]");
      i18nList.forEach(function (item) {
        item.innerHTML = "[i18n backend missing]";
      });
    }
  }

  function bindLanguageDropdownHandlers() {
    document
      .querySelectorAll(".dropdown-language [data-language]")
      .forEach(function (item) {
        item.addEventListener("click", function () {
          const lang = this.getAttribute("data-language");
          if (lang) {
            localStorage.setItem("i18nextLng", lang);
            setLanguage(lang);
          }
        });
      });
    const initialLang = localStorage.getItem("i18nextLng") || "vi";
    updateLanguageDropdown(initialLang);
  }

  document.addEventListener("DOMContentLoaded", function () {
    if (document.querySelector(".dropdown-language")) {
      bindLanguageDropdownHandlers();
    }
  });

  function updateDocumentTitle() {
    var titleEl = document.querySelector("title[data-i18n]");
    if (titleEl && window.i18next) {
      var key = titleEl.getAttribute("data-i18n");
      var translated = window.i18next.t(key);
      document.title = translated || titleEl.textContent;
    }
  }
  if (typeof window.i18next !== "undefined") {
    window.i18next.on("languageChanged", updateDocumentTitle);
    updateDocumentTitle();
  }

  function localize() {
    let i18nList = document.querySelectorAll("[data-i18n]");
    i18nList.forEach(function (item) {
      item.innerHTML = window.i18next.t(item.dataset.i18n);
    });
    let i18nPlaceholderList = document.querySelectorAll(
      "[data-i18n-placeholder]",
    );
    i18nPlaceholderList.forEach(function (item) {
      item.setAttribute(
        "placeholder",
        window.i18next.t(item.dataset.i18nPlaceholder),
      );
    });
    document
      .querySelectorAll(".dropdown-language .dropdown-item")
      .forEach(function (el) {
        el.classList.remove("selected");
        if (el.getAttribute("data-language") === window.i18next.language) {
          el.classList.add("selected");
        }
      });
    updateDocumentTitle();
  }

  function updateLanguageDropdown(lang) {
    const flagMap = { vi: "vn", en: "us", ja: "jp" };
    const nameMap = { vi: "Tiếng Việt", en: "English", ja: "日本語" };
    document.querySelectorAll("#current-lang-flag").forEach(function (flagEl) {
      flagEl.className = "fi fi-" + (flagMap[lang] || "us") + " me-2";
    });
    document.querySelectorAll("#current-lang-name").forEach(function (nameEl) {
      nameEl.textContent = nameMap[lang] || "English";
    });
  }

  // Notification
  // ------------
  const notificationMarkAsReadAll = document.querySelector(
    ".dropdown-notifications-all",
  );
  const notificationMarkAsReadList = document.querySelectorAll(
    ".dropdown-notifications-read",
  );

  // Notification: Mark as all as read
  if (notificationMarkAsReadAll) {
    notificationMarkAsReadAll.addEventListener("click", (event) => {
      notificationMarkAsReadList.forEach((item) => {
        item
          .closest(".dropdown-notifications-item")
          .classList.add("marked-as-read");
      });
    });
  }
  // Notification: Mark as read/unread onclick of dot
  if (notificationMarkAsReadList) {
    notificationMarkAsReadList.forEach((item) => {
      item.addEventListener("click", (event) => {
        item
          .closest(".dropdown-notifications-item")
          .classList.toggle("marked-as-read");
      });
    });
  }

  // Notification: Mark as read/unread onclick of dot
  const notificationArchiveMessageList = document.querySelectorAll(
    ".dropdown-notifications-archive",
  );
  notificationArchiveMessageList.forEach((item) => {
    item.addEventListener("click", (event) => {
      item.closest(".dropdown-notifications-item").remove();
    });
  });

  // Init helpers & misc
  // --------------------

  // Init BS Tooltip
  const tooltipTriggerList = [].slice.call(
    document.querySelectorAll('[data-bs-toggle="tooltip"]'),
  );
  tooltipTriggerList.map(function (tooltipTriggerEl) {
    return new bootstrap.Tooltip(tooltipTriggerEl);
  });

  // Accordion active class
  const accordionActiveFunction = function (e) {
    if (e.type == "show.bs.collapse" || e.type == "show.bs.collapse") {
      e.target.closest(".accordion-item").classList.add("active");
    } else {
      e.target.closest(".accordion-item").classList.remove("active");
    }
  };

  const accordionTriggerList = [].slice.call(
    document.querySelectorAll(".accordion"),
  );
  const accordionList = accordionTriggerList.map(function (accordionTriggerEl) {
    accordionTriggerEl.addEventListener(
      "show.bs.collapse",
      accordionActiveFunction,
    );
    accordionTriggerEl.addEventListener(
      "hide.bs.collapse",
      accordionActiveFunction,
    );
  });

  // If layout is RTL add .dropdown-menu-end class to .dropdown-menu
  // if (isRtl) {
  //   Helpers._addClass('dropdown-menu-end', document.querySelectorAll('#layout-navbar .dropdown-menu'));
  // }

  // Auto update layout based on screen size
  window.Helpers.setAutoUpdate(true);

  // Toggle Password Visibility
  window.Helpers.initPasswordToggle();

  // Speech To Text
  window.Helpers.initSpeechToText();

  // Init PerfectScrollbar in Navbar Dropdown (i.e notification)
  window.Helpers.initNavbarDropdownScrollbar();

  // On window resize listener
  // -------------------------
  window.addEventListener(
    "resize",
    function (event) {
      // Hide open search input and set value blank
      if (window.innerWidth >= window.Helpers.LAYOUT_BREAKPOINT) {
        if (document.querySelector(".search-input-wrapper")) {
          document
            .querySelector(".search-input-wrapper")
            .classList.add("d-none");
          document.querySelector(".search-input").value = "";
        }
      }
      // Horizontal Layout : Update menu based on window size
      let horizontalMenuTemplate = document.querySelector(
        "[data-template^='horizontal-menu']",
      );
      if (horizontalMenuTemplate) {
        setTimeout(function () {
          if (window.innerWidth < window.Helpers.LAYOUT_BREAKPOINT) {
            if (document.getElementById("layout-menu")) {
              if (
                document
                  .getElementById("layout-menu")
                  .classList.contains("menu-horizontal")
              ) {
                menu.switchMenu("vertical");
              }
            }
          } else {
            if (document.getElementById("layout-menu")) {
              if (
                document
                  .getElementById("layout-menu")
                  .classList.contains("menu-vertical")
              ) {
                menu.switchMenu("horizontal");
              }
            }
          }
        }, 100);
      }
    },
    true,
  );

  // Manage menu expanded/collapsed with templateCustomizer & local storage
  //------------------------------------------------------------------

  // If current layout is horizontal OR current window screen is small (overlay menu) than return from here
  if (isHorizontalLayout || window.Helpers.isSmallScreen()) {
    return;
  }

  // If current layout is vertical and current window screen is > small

  // Auto update menu collapsed/expanded based on the themeConfig
  if (typeof TemplateCustomizer !== "undefined") {
    if (window.templateCustomizer.settings.defaultMenuCollapsed) {
      window.Helpers.setCollapsed(true, false);
    }
  }

  // Manage menu expanded/collapsed state with local storage support If enableMenuLocalStorage = true in config.js
  if (typeof config !== "undefined") {
    if (config.enableMenuLocalStorage) {
      try {
        if (
          localStorage.getItem(
            "templateCustomizer-" + templateName + "--LayoutCollapsed",
          ) !== null &&
          localStorage.getItem(
            "templateCustomizer-" + templateName + "--LayoutCollapsed",
          ) !== "false"
        )
          window.Helpers.setCollapsed(
            localStorage.getItem(
              "templateCustomizer-" + templateName + "--LayoutCollapsed",
            ) === "true",
            false,
          );
      } catch (e) {}
    }
  }
})();

// ! Removed following code if you do't wish to use jQuery. Remember that navbar search functionality will stop working on removal.
if (typeof $ !== "undefined") {
  $(function () {
    // ! TODO: Required to load after DOM is ready, did this now with jQuery ready.
    window.Helpers.initSidebarToggle();
    // Toggle Universal Sidebar

    // Navbar Search with autosuggest (typeahead)
    // ? You can remove the following JS if you don't want to use search functionality.
    //----------------------------------------------------------------------------------

    var searchToggler = $(".search-toggler"),
      searchInputWrapper = $(".search-input-wrapper"),
      searchInput = $(".search-input"),
      contentBackdrop = $(".content-backdrop");

    // Open search input on click of search icon
    if (searchToggler.length) {
      searchToggler.on("click", function () {
        if (searchInputWrapper.length) {
          searchInputWrapper.toggleClass("d-none");
          searchInput.focus();
        }
      });
    }
    // Open search on 'CTRL+/'
    $(document).on("keydown", function (event) {
      let ctrlKey = event.ctrlKey,
        slashKey = event.which === 191;

      if (ctrlKey && slashKey) {
        if (searchInputWrapper.length) {
          searchInputWrapper.toggleClass("d-none");
          searchInput.focus();
        }
      }
    });
    // Note: Following code is required to update container class of typeahead dropdown width on focus of search input. setTimeout is required to allow time to initiate Typeahead UI.
    setTimeout(function () {
      var twitterTypeahead = $(".twitter-typeahead");
      searchInput.on("focus", function () {
        if (searchInputWrapper.hasClass("container-xxl")) {
          searchInputWrapper.find(twitterTypeahead).addClass("container-xxl");
          twitterTypeahead.removeClass("container-fluid");
        } else if (searchInputWrapper.hasClass("container-fluid")) {
          searchInputWrapper.find(twitterTypeahead).addClass("container-fluid");
          twitterTypeahead.removeClass("container-xxl");
        }
      });
    }, 10);

    if (searchInput.length) {
      // Filter config
      var filterConfig = function (data) {
        return function findMatches(q, cb) {
          let matches;
          matches = [];
          data.filter(function (i) {
            if (i.name.toLowerCase().startsWith(q.toLowerCase())) {
              matches.push(i);
            } else if (
              !i.name.toLowerCase().startsWith(q.toLowerCase()) &&
              i.name.toLowerCase().includes(q.toLowerCase())
            ) {
              matches.push(i);
              matches.sort(function (a, b) {
                return b.name < a.name ? 1 : -1;
              });
            } else {
              return [];
            }
          });
          cb(matches);
        };
      };

      // Search JSON
      var searchJson = "search-vertical.json"; // For vertical layout
      if ($("#layout-menu").hasClass("menu-horizontal")) {
        var searchJson = "search-horizontal.json"; // For vertical layout
      }
      // Search API AJAX call
      var searchData = $.ajax({
        url: assetsPath + "json/" + searchJson, //? Use your own search api instead
        dataType: "json",
        async: false,
      }).responseJSON;
      // Init typeahead on searchInput
      searchInput.each(function () {
        var $this = $(this);
        if (typeof $.fn.typeahead === 'function') {
          searchInput
            .typeahead(
              {
                hint: false,
                classNames: {
                  menu: "tt-menu navbar-search-suggestion",
                  cursor: "active",
                  suggestion:
                    "suggestion d-flex justify-content-between px-3 py-2 w-100",
                },
              },
              {
                name: "pages",
                display: "name",
                limit: 5,
                source: filterConfig(
                  searchData && searchData.pages ? searchData.pages : [],
                ),
                templates: {
                  header:
                    '<h6 class="suggestions-header text-primary mb-0 mx-3 mt-3 pb-2">Pages</h6>',
                  suggestion: function ({ url, icon, name }) {
                    return (
                      '<a href="' +
                      url +
                      '">' +
                      "<div>" +
                      '<i class="ti ' +
                      icon +
                      ' me-2"></i>' +
                      '<span class="align-middle">' +
                      name +
                      "</span>" +
                      "</div>" +
                      "</a>"
                    );
                  },
                  notFound:
                    '<div class="not-found px-3 py-2">' +
                    '<h6 class="suggestions-header text-primary mb-2">Pages</h6>' +
                    '<p class="py-2 mb-0"><i class="ti ti-alert-circle ti-xs me-2"></i> No Results Found</p>' +
                    "</div>",
                },
              },
              // Files
              {
                name: "files",
                display: "name",
                limit: 4,
                source: filterConfig(
                  searchData && searchData.files ? searchData.files : [],
                ),
                templates: {
                  header:
                    '<h6 class="suggestions-header text-primary mb-0 mx-3 mt-3 pb-2">Files</h6>',
                  suggestion: function ({ src, name, subtitle, meta }) {
                    return (
                      '<a href="javascript:;">' +
                      '<div class="d-flex w-50">' +
                      '<img class="me-3" src="' +
                      assetsPath +
                      src +
                      '" alt="' +
                      name +
                      '" height="32">' +
                      '<div class="w-75">' +
                      '<h6 class="mb-0">' +
                      name +
                      "</h6>" +
                      '<small class="text-muted">' +
                      subtitle +
                      "</small>" +
                      "</div>" +
                      "</div>" +
                      '<small class="text-muted">' +
                      meta +
                      "</small>" +
                      "</a>"
                    );
                  },
                  notFound:
                    '<div class="not-found px-3 py-2">' +
                    '<h6 class="suggestions-header text-primary mb-2">Files</h6>' +
                    '<p class="py-2 mb-0"><i class="ti ti-alert-circle ti-xs me-2"></i> No Results Found</p>' +
                    "</div>",
                },
              },
              // Members
              {
                name: "members",
                display: "name",
                limit: 4,
                source: filterConfig(
                  searchData && searchData.members ? searchData.members : [],
                ),
                templates: {
                  header:
                    '<h6 class="suggestions-header text-primary mb-0 mx-3 mt-3 pb-2">Members</h6>',
                  suggestion: function ({ name, src, subtitle }) {
                    return (
                      '<a href="app-user-view-account.html">' +
                      '<div class="d-flex align-items-center">' +
                      '<img class="rounded-circle me-3" src="' +
                      assetsPath +
                      src +
                      '" alt="' +
                      name +
                      '" height="32">' +
                      '<div class="user-info">' +
                      '<h6 class="mb-0">' +
                      name +
                      "</h6>" +
                      '<small class="text-muted">' +
                      subtitle +
                      "</small>" +
                      "</div>" +
                      "</div>" +
                      "</a>"
                    );
                  },
                  notFound:
                    '<div class="not-found px-3 py-2">' +
                    '<h6 class="suggestions-header text-primary mb-2">Members</h6>' +
                    '<p class="py-2 mb-0"><i class="ti ti-alert-circle ti-xs me-2"></i> No Results Found</p>' +
                    "</div>",
                },
              },
            )
            .bind("typeahead:render", function () {
              contentBackdrop.addClass("show").removeClass("fade");
            })
            .bind("typeahead:select", function (ev, suggestion) {
              if (suggestion.url) {
                window.location = suggestion.url;
              }
            })
            .bind("typeahead:close", function () {
              searchInput.val("");
              $this.typeahead("val", "");
              searchInputWrapper.addClass("d-none");
              contentBackdrop.addClass("fade").removeClass("show");
            });
          searchInput.on("keyup", function () {
            if (searchInput.val() == "") {
              contentBackdrop.addClass("fade").removeClass("show");
            }
          });
        } else {
          console.warn('typeahead is not loaded or not a function');
        }
      });

      // Init PerfectScrollbar in search result
      var psSearch;
      $(".navbar-search-suggestion").each(function () {
        psSearch = new PerfectScrollbar($(this)[0], {
          wheelPropagation: false,
          suppressScrollX: true,
        });
      });

      searchInput.on("keyup", function () {
        psSearch.update();
      });
    }
  });
}

function showToastrMessage(msg, type = "success") {
  if (typeof window.i18next !== "undefined" && typeof window.i18next.t === "function") {
    if (typeof msg === "string" && (!msg.trim().includes(" ") || msg === msg.toUpperCase())) {
      msg = window.i18next.t(msg);
    }
  }
  if (typeof window.showToastr === "function") {
    window.showToastr(msg, type);
  } else if (typeof toastr !== "undefined") {
    toastr.clear();
    toastr[type](msg);
  }
}
