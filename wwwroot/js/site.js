/**
 * Itihas 360 - Shared Site Interactions
 * Global UI interactions for all pages
 */

$(document).ready(function () {

    // ═════════════════════════════════════════════════════════════
    // 1. SCROLL REVEAL ANIMATION
    // ═════════════════════════════════════════════════════════════

    const reveal = () => {
        $('.reveal').each(function () {

            const windowHeight = window.innerHeight;
            const elementTop = this.getBoundingClientRect().top;
            const elementVisible = 100;

            if (elementTop < windowHeight - elementVisible) {
                $(this).addClass('active');
            }
        });
    };

    window.addEventListener('scroll', reveal);
    reveal();



    // ═════════════════════════════════════════════════════════════
    // 2. HERO DATE (Home page only)
    // ═════════════════════════════════════════════════════════════

    if ($('#heroDate').length) {

        const options = {
            weekday: 'long',
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        };

        $('#heroDate').text(
            new Date()
                .toLocaleDateString('en-US', options)
                .toUpperCase()
        );
    }



    // ═════════════════════════════════════════════════════════════
    // 3. SEARCH OVERLAY
    // ═════════════════════════════════════════════════════════════

    $('#searchOpenTrigger, .nav-search-btn').on('click', function (e) {

        e.preventDefault();

        $('#searchOverlay').addClass('active');

        $('body').css('overflow', 'hidden');

        setTimeout(() => {
            $('#searchInput').focus();
        }, 300);
    });

    $('#searchClose').on('click', function () {

        $('#searchOverlay').removeClass('active');

        $('body').css('overflow', 'auto');
    });



    // ═════════════════════════════════════════════════════════════
    // 4. NOTIFICATION PANEL
    // ═════════════════════════════════════════════════════════════

    const $notifDot = $('#notifDot');
    const $notifToggle = $('#notifToggle');

    // Read values cleanly 
    const currentNotifCount = parseInt($notifToggle.attr('data-count'), 10) || 0;
    const lastSeenCount = parseInt(localStorage.getItem('it360_seen_notif_count'), 10) || 0;

    // RULE 1: If count is 0, hide it right away
    // RULE 2: If the backend count is less than or equal to what they've already seen, hide it
    if (currentNotifCount === 0 || currentNotifCount <= lastSeenCount) {
        $notifDot.removeClass('visible');
        $notifDot.attr('style', 'display: none !important;'); // Hard override to block layout flashing
    }

    $notifToggle.on('click', function (e) {
        e.preventDefault();
        e.stopPropagation();

        $('#notifPanel').toggleClass('open');

        // Hide it visually right now
        $notifDot.removeClass('visible');
        $notifDot.attr('style', 'display: none !important;');

        // Save the count integer value to memory
        localStorage.setItem('it360_seen_notif_count', currentNotifCount);
    });

    $('#notifClose').on('click', function (e) {
        e.preventDefault();
        $('#notifPanel').removeClass('open');
    });

    // Close notification panel on outside click
    $(document).on('click', function (e) {
        const panel = $('#notifPanel');
        const toggle = $('#notifToggle');

        if (
            !panel.is(e.target) &&
            panel.has(e.target).length === 0 &&
            !toggle.is(e.target) &&
            toggle.has(e.target).length === 0
        ) {
            panel.removeClass('open');
        }
    });



    // ═════════════════════════════════════════════════════════════
    // 5. CATEGORY FILTERING (Home page only)
    // ═════════════════════════════════════════════════════════════

    $('[data-filter]').on('click', function () {

        const filterValue = $(this).attr('data-filter');

        $('[data-filter]')
            .removeClass('active')
            .addClass('parch');

        $(this)
            .addClass('active')
            .removeClass('parch');

        if (filterValue === 'all') {

            $('#articleGrid > div').fadeIn(400);

        } else {

            $('#articleGrid > div').hide();

            $(`#articleGrid > div[data-category="${filterValue}"]`)
                .fadeIn(400);
        }
    });



    // ═════════════════════════════════════════════════════════════
    // 6. SCROLL TO TOP
    // ═════════════════════════════════════════════════════════════

    $(window).scroll(function () {

        if ($(this).scrollTop() > 400) {

            $('#scrollTop').addClass('visible');

        } else {

            $('#scrollTop').removeClass('visible');
        }
    });

    $('#scrollTop').click(function () {

        window.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    });



    // ═════════════════════════════════════════════════════════════
    // 7. TICKER SPEED (Home page only)
    // ═════════════════════════════════════════════════════════════

    if ($('.ticker-track').length) {

        const tickerWidth = $('.ticker-track').width();

        $('.ticker-track').css(
            'animation-duration',
            (tickerWidth / 50) + 's'
        );
    }

});



// ═════════════════════════════════════════════════════════════
// GLOBAL CATEGORY REDIRECT
// ═════════════════════════════════════════════════════════════

function redirectToCategory(slug) {

    window.location.href =
        '/Articles?category=' + encodeURIComponent(slug);
}