$(document).ready(function () {

    var state = {
        currentPage: window.phState ? window.phState.currentPage : 1,
        totalPages: window.phState ? window.phState.totalPages : 1,
        totalCount: window.phState ? window.phState.totalCount : 0
    };

    $('#hamburgerBtn').on('click', function () {
        $(this).toggleClass('is-open');
        $('#filterPanel').toggleClass('is-open');
    });

    $(document).on('click', function (e) {
        if (!$(e.target).closest('#filterPanel, #hamburgerBtn').length) {
            $('#filterPanel').removeClass('is-open');
            $('#hamburgerBtn').removeClass('is-open');
        }
    });

    $('#methodBtns').on('click', '.ph-method-btn', function () {
        $('.ph-method-btn').removeClass('active');
        $(this).addClass('active');
        $('#selectedMethod').val($(this).data('value'));
    });

    $('#applyFiltersBtn').on('click', function () {
        if (!validateFilters()) return;
        state.currentPage = 1;
        loadPayments();
        $('#filterPanel').removeClass('is-open');
        $('#hamburgerBtn').removeClass('is-open');
    });

    $('#resetFiltersBtn').on('click', function () {
        $('#searchQuery').val('');
        $('#filterType').val('2');
        $('.ph-method-btn').removeClass('active');
        $('.ph-method-btn[data-value="0"]').addClass('active');
        $('#selectedMethod').val('0');
        state.currentPage = 1;
        loadPayments();
    });

    $('#prevBtn').on('click', function () {
        if (state.currentPage > 1) {
            state.currentPage--;
            loadPayments();
        }
    });

    $('#nextBtn').on('click', function () {
        if (state.currentPage < state.totalPages) {
            state.currentPage++;
            loadPayments();
        }
    });

    function validateFilters() {
        var search = $('#searchQuery').val();

        if (search.length > 100) {
            alert('Search query cannot exceed 100 characters.');
            return false;
        }

        return true;
    }

    function loadPayments() {
        var filterType = parseInt($('#filterType').val());
        var paymentMethod = parseInt($('#selectedMethod').val());
        var searchQuery = $('#searchQuery').val().trim();

        $('.ph-table-wrapper').addClass('is-loading');

        $.ajax({
            type: 'POST',
            url: '/PaymentHistory/Filter',
            data: {
                filter: filterType,
                paymentMethod: paymentMethod,
                searchQuery: searchQuery,
                pageNumber: state.currentPage,
                pageSize: 10
            },
            success: function (response) {
                state.totalPages = response.totalPages;
                state.totalCount = response.totalCount;
                state.currentPage = response.pageNumber;

                renderTable(response.items);
                renderPagination();
                updateSummary(response.totalAmount, response.items.length, response.totalCount);
            },
            error: function () {
                alert('Something went wrong loading payments. Please try again.');
            },
            complete: function () {
                $('.ph-table-wrapper').removeClass('is-loading');
            }
        });
    }

    function renderTable(items) {
        var $tbody = $('#paymentsBody');
        $tbody.empty();

        if (!items || items.length === 0) {
            $tbody.append(
                '<tr><td colspan="8" class="ph-empty">No rentals or payments found.</td></tr>'
            );
            return;
        }

        $.each(items, function (i, item) {
            var methodLower = (item.paymentMethod || 'unknown').toLowerCase();
            var badgeClass = 'ph-badge ph-badge--' + methodLower;

            var receiptUrl = item.hasPayment
                ? '/PaymentHistory/DownloadReceipt?paymentId=' + item.paymentId
                : '/PaymentHistory/DownloadReceipt?rentalId=' + item.rentalId;

            var row = '<tr>' +
                '<td>' + escapeHtml(item.dateText || '—') + '</td>' +
                '<td>' + escapeHtml(item.productName || 'Unknown Game') + '</td>' +
                '<td>' + escapeHtml(item.role || '—') + '</td>' +
                '<td>' + escapeHtml(item.period || '—') + '</td>' +
                '<td>' + escapeHtml(item.status || '—') + '</td>' +
                '<td><span class="' + badgeClass + '">' + escapeHtml(item.paymentMethod || '—') + '</span></td>' +
                '<td class="text-end fw-semibold">' + escapeHtml(item.amountText || '0.00 lei') + '</td>' +
                '<td class="text-end">' +
                '<a class="btn btn-sm btn-outline-primary ph-receipt-btn" href="' + receiptUrl + '" title="Download receipt PDF">Receipt</a>' +
                '</td>' +
                '</tr>';

            $tbody.append(row);
        });
    }

    function renderPagination() {
        $('#pageInfo').text('Page ' + state.currentPage + ' of ' + (state.totalPages || 1));

        if (state.currentPage <= 1) {
            $('#prevBtn').attr('disabled', true);
        } else {
            $('#prevBtn').removeAttr('disabled');
        }

        if (state.currentPage >= state.totalPages) {
            $('#nextBtn').attr('disabled', true);
        } else {
            $('#nextBtn').removeAttr('disabled');
        }
    }

    function updateSummary(totalAmount, shownCount, totalCount) {
        $('#totalAmount').text(totalAmount);
        $('#countInfo').text('Showing ' + shownCount + ' of ' + totalCount + ' records');
    }

    function escapeHtml(str) {
        if (str == null) return '';
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }

});