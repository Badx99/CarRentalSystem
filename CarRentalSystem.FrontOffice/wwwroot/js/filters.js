// Vehicle filtering JavaScript

$(document).ready(function () {
    // Price range slider (if implemented)
    if ($('#priceRange').length) {
        $('#priceRange').on('input', function () {
            $('#priceValue').text('$' + $(this).val());
        });
    }

    // Category filter
    $('.category-filter').on('change', function () {
        $('#filterForm').submit();
    });

    // Real-time search
    $('#searchInput').on('keyup', debounce(function () {
        $('#filterForm').submit();
    }, 500));
});

// Debounce function for performance
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}
