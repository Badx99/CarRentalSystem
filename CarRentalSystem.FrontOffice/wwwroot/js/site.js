// Car Rental System Client Logic

$(document).ready(function () {
    // 1. Initialize Date Pickers and Validation
    initDateValidation();

    // 2. Confirmation Dialogs
    $('form[data-confirm], button[data-confirm]').on('click submit', function (e) {
        var message = $(this).data('confirm') || 'Are you sure you want to proceed?';
        if (!confirm(message)) {
            e.preventDefault();
            return false;
        }
    });

    // 3. Auto-dismiss Toast/Alerts
    setTimeout(function () {
        $('.alert-dismissible').fadeOut('slow');
    }, 5000);

    // 4. Smooth Scrolling
    $('a[href^="#"]').on('click', function (e) {
        var target = $(this.hash);
        if (target.length) {
            e.preventDefault();
            $('html, body').animate({
                scrollTop: target.offset().top - 70
            }, 800);
        }
    });
});

// --- Availability Check Logic ---
function initDateValidation() {
    var startDateInput = document.getElementById('startDate');
    var endDateInput = document.getElementById('endDate');

    if (startDateInput && endDateInput) {
        // Enforce min dates
        var today = new Date().toISOString().split('T')[0];
        if (!startDateInput.value) startDateInput.value = today;
        startDateInput.setAttribute('min', today);
        
        // Update end date min when start date changes
        startDateInput.addEventListener('change', function () {
            endDateInput.setAttribute('min', this.value);
            if (endDateInput.value && endDateInput.value <= this.value) {
                // Determine next day
                var start = new Date(this.value);
                var nextDay = new Date(start);
                nextDay.setDate(start.getDate() + 1);
                endDateInput.value = nextDay.toISOString().split('T')[0];
            }
        });
    }
}

function checkAvailability() {
    var vehicleId = $('#vehicleId').val();
    var startDate = $('#startDate').val();
    var endDate = $('#endDate').val();
    var resultDiv = $('#availabilityResult');
    var errorDiv = $('#availabilityError');
    var bookBtn = $('#bookButton');

    // Reset UI
    resultDiv.addClass('d-none');
    errorDiv.addClass('d-none');
    
    if (!startDate || !endDate) {
        alert("Please select both start and end dates.");
        return;
    }

    if (startDate >= endDate) {
        alert("Return date must be after pick-up date.");
        return;
    }

    // Show loading state
    bookBtn.addClass('disabled').text('Checking...');

    $.post('/Vehicles/CheckAvailability', {
        vehicleId: vehicleId,
        startDate: startDate,
        endDate: endDate
    })
    .done(function (result) {
        if (result.available) {
            // Success
            $('#resultDays').text(result.days + ' days');
            $('#resultPrice').text('$' + result.totalPrice.toFixed(2));
            
            // Build booking URL with params
            var baseUrl = '/Reservations/Book'; // Or use @Url.Action in view to set a data attribute
            var bookingUrl = `${baseUrl}?vehicleId=${vehicleId}&startDate=${startDate}&endDate=${endDate}`;
            
            bookBtn.attr('href', bookingUrl)
                   .removeClass('disabled btn-secondary')
                   .addClass('btn-success')
                   .text('Book Now');
            
            resultDiv.removeClass('d-none');
        } else {
            // Unavailable
            errorDiv.text(result.message || "Vehicle is not available for these dates.")
                    .removeClass('d-none');
            bookBtn.addClass('disabled').text('Book Now');
        }
    })
    .fail(function () {
        errorDiv.text("Error checking availability. Please try again.").removeClass('d-none');
        bookBtn.removeClass('disabled').text('Check Again');
    })
    .always(function() {
        if($('#availabilityResult').hasClass('d-none')) {
             // If failed or unavailable, ensure button resets text if not processed above
             if(!errorDiv.hasClass('d-none')) bookBtn.text('Check Again');
        }
    });
}
