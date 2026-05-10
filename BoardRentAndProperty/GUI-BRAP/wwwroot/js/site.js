$(function () {
    function confirmAction(element, event) {
        var message = element.data("confirm");

        if (message && !window.confirm(message)) {
            event.preventDefault();
            event.stopImmediatePropagation();
            return false;
        }

        return true;
    }

    $("form[data-confirm]").on("submit", function (event) {
        return confirmAction($(this), event);
    });

    $("a[data-confirm], button[data-confirm]").on("click", function (event) {
        if ($(this).closest("form[data-confirm]").length > 0) {
            return true;
        }

        return confirmAction($(this), event);
    });
});
