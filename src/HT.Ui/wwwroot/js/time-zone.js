// HP-016, research R-12: sends the browser's time zone with a form, with no visible field. App.razor loads this once,
// after blazor.web.js, so it works however the page was reached (a full load, enhanced navigation or Back). One
// document-level submit listener, in the capture phase, fills every input marked data-ht-browser-time-zone of the
// form being submitted. A browser that gives no time zone leaves the input empty, and the account gets the
// installation's default (FR-025).
(function () {
    "use strict";

    function browserTimeZone() {
        try {
            return Intl.DateTimeFormat().resolvedOptions().timeZone || "";
        } catch (e) {
            return "";
        }
    }

    document.addEventListener("submit", function (event) {
        var form = event.target;
        if (!(form instanceof HTMLFormElement)) {
            return;
        }

        var inputs = form.querySelectorAll("input[data-ht-browser-time-zone]");
        if (inputs.length === 0) {
            return;
        }

        var zone = browserTimeZone();
        inputs.forEach(function (input) {
            input.value = zone;
        });
    }, true);
})();
