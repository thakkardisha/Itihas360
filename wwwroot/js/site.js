/**
 * Itihas 360 - Global Shared Interactions Layout Logic
 */
$(document).ready(function () {
    // 1. GLOBAL SEARCH OVERLAY TOGGLES
    $('#searchOpenTrigger').on('click', function (e) {
        e.preventDefault();
        $('#searchOverlay').addClass('active');
        $('body').css('overflow', 'hidden');
        setTimeout(() => $('#searchInput').focus(), 300);
    });

    $('#searchClose').on('click', function () {
        $('#searchOverlay').removeClass('active');
        $('body').css('overflow', 'auto');
    });

    // Close overlays with Escape Key
    $(document).on('keydown', function (e) {
        if (e.key === "Escape") {
            $('#searchOverlay').removeClass('active');
            $('body').css('overflow', 'auto');
        }
    });
});