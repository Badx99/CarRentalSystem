// Custom animations JavaScript

$(document).ready(function () {
    // Fade in on scroll
    function fadeInOnScroll() {
        $('.fade-in-on-scroll').each(function () {
            var elementTop = $(this).offset().top;
            var elementBottom = elementTop + $(this).outerHeight();
            var viewportTop = $(window).scrollTop();
            var viewportBottom = viewportTop + $(window).height();

            if (elementBottom > viewportTop && elementTop < viewportBottom) {
                $(this).addClass('visible');
            }
        });
    }

    // Parallax effect for hero section
    $(window).on('scroll', function () {
        var scrolled = $(window).scrollTop();
        var parallax = $('.hero-parallax');
        if (parallax.length) {
            var speed = scrolled * 0.5;
            parallax.css('transform', 'translateY(' + speed + 'px)');
        }
    });

    // Counter animation
    function animateCounter(element, target, duration) {
        var start = 0;
        var increment = target / (duration / 16);
        var timer = setInterval(function () {
            start += increment;
            if (start >= target) {
                element.text(target);
                clearInterval(timer);
            } else {
                element.text(Math.floor(start));
            }
        }, 16);
    }

    // Trigger counter animation when in viewport
    $(window).on('scroll', function () {
        $('.counter').each(function () {
            if ($(this).hasClass('animated')) return;

            var elementTop = $(this).offset().top;
            var viewportBottom = $(window).scrollTop() + $(window).height();

            if (elementTop < viewportBottom) {
                var target = parseInt($(this).data('target'));
                animateCounter($(this), target, 2000);
                $(this).addClass('animated');
            }
        });
    });

    // Initial fade in on scroll check
    fadeInOnScroll();
    $(window).on('scroll', fadeInOnScroll);
});

// CSS for fade in animation
const style = document.createElement('style');
style.textContent = `
    .fade-in-on-scroll {
        opacity: 0;
        transform: translateY(30px);
        transition: opacity 0.6s ease, transform 0.6s ease;
    }
    .fade-in-on-scroll.visible {
        opacity: 1;
        transform: translateY(0);
    }
`;
document.head.appendChild(style);
