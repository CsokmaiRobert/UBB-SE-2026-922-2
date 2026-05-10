$(function () {
    $("[data-confirm]").on("click submit", function (event) {
        var message = $(this).data("confirm");

        if (message && !window.confirm(message)) {
            event.preventDefault();
            event.stopImmediatePropagation();
            return false;
        }

        return true;
    });
});
