// Main JavaScript file for Car Rental Frontend

$(document).ready(function () {
    // Auto-dismiss alerts after 5 seconds
    setTimeout(function () {
        $('.alert').fadeOut('slow', function () {
            $(this).remove();
        });
    }, 5000);

    // Smooth scroll for anchor links
    $('a[href^="#"]').on('click', function (event) {
        var target = $(this.getAttribute('href'));
        if (target.length) {
            event.preventDefault();
            $('html, body').stop().animate({
                scrollTop: target.offset().top - 70
            }, 1000);
        }
    });

    // Form validation enhancement
    $('form').on('submit', function () {
        var form = $(this);
        if (form[0].checkValidity() === false) {
            event.preventDefault();
            event.stopPropagation();
        }
        form.addClass('was-validated');
    });

    // Date picker minimum date validation
    $('input[type="date"]').on('change', function () {
        var startDate = $('input[name="startDate"]');
        var endDate = $('input[name="endDate"]');

        if ($(this).attr('name') === 'startDate' && endDate.length) {
            var minEndDate = new Date($(this).val());
            minEndDate.setDate(minEndDate.getDate() + 1);
            endDate.attr('min', minEndDate.toISOString().split('T')[0]);
        }
    });

    // Loading spinner for form submissions
    $('form').on('submit', function () {
        var submitBtn = $(this).find('button[type="submit"]');
        if (submitBtn.length) {
            submitBtn.prop('disabled', true);
            submitBtn.html('<i class="fas fa-spinner fa-spin me-2"></i>Processing...');
        }
    });

    // Image error handler
    $('img').on('error', function () {
        if (!$(this).hasClass('no-placeholder')) {
            $(this).attr('src', '/images/placeholder-car.jpg');
        }
    });
});

// Utility Functions
function showLoading() {
    if ($('#loadingOverlay').length === 0) {
        $('body').append('<div id="loadingOverlay" class="position-fixed top-0 start-0 w-100 h-100 bg-dark bg-opacity-50 d-flex align-items-center justify-content-center" style="z-index: 9999;"><div class="spinner-border text-purple" role="status"><span class="visually-hidden">Loading...</span></div></div>');
    }
}

function hideLoading() {
    $('#loadingOverlay').remove();
}

// Toast notification function
function showToast(message, type = 'info') {
    var bgColor = type === 'success' ? 'bg-success' : type === 'error' ? 'bg-danger' : 'bg-info';
    var toast = $('<div class="toast align-items-center text-white ' + bgColor + ' border-0 position-fixed top-0 end-0 m-3" role="alert" style="z-index: 9999;"><div class="d-flex"><div class="toast-body">' + message + '</div><button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button></div></div>');
    $('body').append(toast);
    var bsToast = new bootstrap.Toast(toast[0]);
    bsToast.show();
    toast.on('hidden.bs.toast', function () {
        $(this).remove();
    });
}

// File download utility
function downloadBlob(blob, filename) {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);
}

// Download PDF Receipt
function downloadReceipt(reservationId) {
    showLoading();
    fetch(`/Reservations/DownloadReceipt/${reservationId}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('Receipt not available');
            }
            return response.blob();
        })
        .then(blob => {
            hideLoading();
            downloadBlob(blob, `Reservation_${reservationId}.pdf`);
            showToast('Receipt downloaded successfully!', 'success');
        })
        .catch(error => {
            hideLoading();
            console.error('Download error:', error);
            showToast('Failed to download receipt. Please try again.', 'error');
        });
}

// Show QR Code Modal
function showQRModal(qrCodeBase64, reservationId) {
    // Remove existing modal if any
    $('#qrModal').remove();

    var modal = `
        <div class="modal fade" id="qrModal" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content border-0 shadow-lg rounded-4">
                    <div class="modal-header border-0 bg-purple text-white rounded-top-4">
                        <h5 class="modal-title"><i class="fas fa-qrcode me-2"></i>Check-in QR Code</h5>
                        <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body text-center p-4">
                        <img src="data:image/png;base64,${qrCodeBase64}" alt="QR Code" class="img-fluid mb-3" style="max-width: 250px;">
                        <p class="text-muted small">Present this QR code at the rental desk for quick check-in.</p>
                        <p class="fw-bold">Reservation #${reservationId.substring(0, 8).toUpperCase()}</p>
                    </div>
                    <div class="modal-footer border-0 justify-content-center">
                        <button type="button" class="btn btn-outline-purple rounded-pill" data-bs-dismiss="modal">Close</button>
                    </div>
                </div>
            </div>
        </div>
    `;

    $('body').append(modal);
    var modalElement = new bootstrap.Modal(document.getElementById('qrModal'));
    modalElement.show();
}
