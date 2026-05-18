/**
 * Itihas 360 - Shared Site Interactions (navbar, footer, overlays)
 * Formerly home.js — now included on every page via _Layout.cshtml
 */
$(document).ready(function () {

    // 1. SCROLL REVEAL ANIMATION
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
    reveal(); // initial check on load

    // 2. HERO DATE (only runs when the element exists, i.e. on the Home page)
    if ($('#heroDate').length) {
        const options = { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' };
        $('#heroDate').text(new Date().toLocaleDateString('en-US', options).toUpperCase());
    }

    // 3. SEARCH OVERLAY TOGGLES
    $('#searchOpen, .nav-search-btn').on('click', function (e) {
        e.preventDefault();
        $('#searchOverlay').addClass('active');
        $('body').css('overflow', 'hidden');
        setTimeout(() => $('#searchInput').focus(), 300);
    });
    $('#searchClose').on('click', function () {
        $('#searchOverlay').removeClass('active');
        $('body').css('overflow', 'auto');
    });

    // 4. NOTIFICATION PANEL
    $('#notifToggle').on('click', function () {
        $('#notifPanel').addClass('active');
        $('#notifDot').removeClass('visible');
    });
    $('#notifClose').on('click', function () {
        $('#notifPanel').removeClass('active');
    });

    // 5. CATEGORY FILTERING (LATEST ARTICLES SECTION — Home page only)
    $('[data-filter]').on('click', function () {
        const filterValue = $(this).attr('data-filter');
        $('[data-filter]').removeClass('active').addClass('parch');
        $(this).addClass('active').removeClass('parch');
        if (filterValue === 'all') {
            $('#articleGrid > div').fadeIn(400);
        } else {
            $('#articleGrid > div').hide();
            $(`#articleGrid > div[data-category="${filterValue}"]`).fadeIn(400);
        }
    });

    // 6. SCROLL TO TOP
    $(window).scroll(function () {
        if ($(this).scrollTop() > 400) {
            $('#scrollTop').addClass('visible');
        } else {
            $('#scrollTop').removeClass('visible');
        }
    });
    $('#scrollTop').click(function () {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    });

    // 7. TICKER ANIMATION SPEED ADJUSTMENT (Home page only)
    if ($('.ticker-track').length) {
        const tickerWidth = $('.ticker-track').width();
        $('.ticker-track').css('animation-duration', (tickerWidth / 50) + 's');
    }

});