// Booking form JavaScript

$(document).ready(function () {
    // Calculate total price based on dates
    function calculatePrice() {
        var startDate = $('input[name="StartDate"]').val();
        var endDate = $('input[name="EndDate"]').val();
        var pricePerDay = parseFloat($('#pricePerDay').data('price')) || 0;

        if (startDate && endDate && pricePerDay > 0) {
            var start = new Date(startDate);
            var end = new Date(endDate);

            if (end > start) {
                var diffTime = Math.abs(end - start);
                var diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
                var days = diffDays === 0 ? 1 : diffDays; // Minimum 1 day
                var total = days * pricePerDay;

                $('#totalPrice').text('$' + total.toFixed(2));
                $('#daysCount').text(days + ' days');
            } else {
                $('#totalPrice').text('$0.00');
                $('#daysCount').text('0 days');
            }
        }
    }

    // Date change handlers
    $('input[name="StartDate"], input[name="EndDate"]').on('change', function () {
        calculatePrice();

        // Validate end date is after start date
        var startDateStr = $('input[name="StartDate"]').val();
        var endDateStr = $('input[name="EndDate"]').val();

        if (startDateStr && endDateStr) {
            var start = new Date(startDateStr);
            var end = new Date(endDateStr);

            if (end <= start) {
                showToast('Return date must be after pick-up date.', 'error');
                $('input[name="EndDate"]').val('');
            }
        }
    });

    // Initial calculation if dates are pre-filled
    calculatePrice();

    // Form validation
    $('#bookingForm').on('submit', function (e) {
        var startDate = $('input[name="StartDate"]').val();
        var endDate = $('input[name="EndDate"]').val();

        if (!startDate || !endDate) {
            e.preventDefault();
            showToast('Please select both pick-up and return dates.', 'error');
            return false;
        }

        var start = new Date(startDate);
        var end = new Date(endDate);

        if (end <= start) {
            e.preventDefault();
            showToast('Return date must be after pick-up date.', 'error');
            return false;
        }

        return true;
    });
});
