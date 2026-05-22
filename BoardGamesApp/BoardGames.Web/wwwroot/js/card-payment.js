$(function () {
    var $form = $("#cardPaymentForm");
    var $submit = $("#submitBtn");
    var $terms = $("#termsCheck");

    function toggleSubmit() {
        var isValid = !$form.valid || $form.valid();
        $submit.prop("disabled", !isValid || !$terms.is(":checked"));
    }

    $terms.on("change", toggleSubmit);

    $form.on("input change", "input, select, textarea", function () {
        toggleSubmit();
    });

    $form.on("submit", function () {
        if ($form.valid && $form.valid() && $terms.is(":checked")) {
            $submit.prop("disabled", true).text("Processing...");
        }
    });

    toggleSubmit();
});
