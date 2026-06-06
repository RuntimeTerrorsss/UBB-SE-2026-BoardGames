(function () {
    function parseDateOnly(value) {
        const datePart = String(value).substring(0, 10);
        const parts = datePart.split('-').map(Number);
        if (parts.length === 3 && !Number.isNaN(parts[0])) {
            return new Date(parts[0], parts[1] - 1, parts[2]);
        }

        const date = new Date(value);
        date.setHours(0, 0, 0, 0);
        return date;
    }

    function formatInputDate(date) {
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        return `${year}-${month}-${day}`;
    }

    function isSameDay(a, b) {
        return a.getFullYear() === b.getFullYear()
            && a.getMonth() === b.getMonth()
            && a.getDate() === b.getDate();
    }

    function getRangeBounds(range) {
        const startValue = range.StartDate ?? range.startDate;
        const endValue = range.EndDate ?? range.endDate;
        return {
            start: parseDateOnly(startValue),
            end: parseDateOnly(endValue),
        };
    }

    function isDateInRanges(date, ranges) {
        for (const range of ranges) {
            const { start, end } = getRangeBounds(range);
            if (date >= start && date <= end) {
                return true;
            }
        }

        return false;
    }

    function isDateBooked(date, bookedRanges) {
        return isDateInRanges(date, bookedRanges);
    }

    function isDatePendingRequest(date, pendingRanges) {
        return isDateInRanges(date, pendingRanges);
    }

    function rangesOverlap(start, end, ranges) {
        if (!ranges || ranges.length === 0) {
            return false;
        }

        for (const range of ranges) {
            const { start: rangeStart, end: rangeEnd } = getRangeBounds(range);
            if (start <= rangeEnd && end >= rangeStart) {
                return true;
            }
        }

        return false;
    }

    function isDateUnavailable(date, bookedRanges) {
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        if (date < today) {
            return true;
        }

        return isDateBooked(date, bookedRanges);
    }

    function isInRange(date, start, end) {
        if (!start || !end) {
            return false;
        }

        const from = start < end ? start : end;
        const to = start < end ? end : start;
        return date >= from && date <= to;
    }

    class RentalDatePicker {
        constructor(container, options) {
            this.container = container;
            this.bookedRanges = options.bookedRanges || [];
            this.pendingRequestRanges = options.pendingRequestRanges || [];
            this.startInput = options.startInput;
            this.endInput = options.endInput;
            this.onChange = options.onChange;
            this.viewMonth = new Date();
            this.viewMonth.setDate(1);
            this.viewMonth.setHours(0, 0, 0, 0);
            this.startDate = null;
            this.endDate = null;
            this.render();
        }

        setBookedRanges(ranges) {
            this.bookedRanges = ranges || [];
            this.render();
        }

        setPendingRequestRanges(ranges) {
            this.pendingRequestRanges = ranges || [];
            this.render();
        }

        render() {
            this.container.innerHTML = '';
            this.container.classList.add('rental-date-picker');

            const header = document.createElement('div');
            header.className = 'rental-date-picker-header';

            const prev = document.createElement('button');
            prev.type = 'button';
            prev.setAttribute('aria-label', 'Previous month');
            prev.textContent = '\u2039';
            prev.addEventListener('click', () => this.changeMonth(-1));

            const title = document.createElement('div');
            title.className = 'rental-date-picker-month';
            title.textContent = this.viewMonth.toLocaleDateString(undefined, { month: 'long', year: 'numeric' });

            const next = document.createElement('button');
            next.type = 'button';
            next.setAttribute('aria-label', 'Next month');
            next.textContent = '\u203A';
            next.addEventListener('click', () => this.changeMonth(1));

            header.append(prev, title, next);

            const weekdays = document.createElement('div');
            weekdays.className = 'rental-date-picker-weekdays';
            ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'].forEach(day => {
                const label = document.createElement('span');
                label.textContent = day;
                weekdays.append(label);
            });

            const grid = document.createElement('div');
            grid.className = 'rental-date-picker-grid';
            this.renderDays(grid);

            const legend = document.createElement('div');
            legend.className = 'rental-date-picker-legend';
            legend.innerHTML =
                '<span><i class="swatch swatch-unavailable"></i> Unavailable</span>' +
                '<span><i class="swatch swatch-pending"></i> Your request</span>' +
                '<span><i class="swatch swatch-selected"></i> Selected</span>';

            this.selectionLabel = document.createElement('div');
            this.selectionLabel.className = 'rental-date-picker-selection';
            this.updateSelectionLabel();

            this.container.append(header, weekdays, grid, legend, this.selectionLabel);
        }

        renderDays(grid) {
            const year = this.viewMonth.getFullYear();
            const month = this.viewMonth.getMonth();
            const firstDay = new Date(year, month, 1);
            const startOffset = firstDay.getDay();
            const daysInMonth = new Date(year, month + 1, 0).getDate();
            const today = new Date();
            today.setHours(0, 0, 0, 0);

            for (let i = 0; i < startOffset; i++) {
                const empty = document.createElement('button');
                empty.type = 'button';
                empty.className = 'rental-date-picker-day is-empty';
                empty.disabled = true;
                grid.append(empty);
            }

            for (let day = 1; day <= daysInMonth; day++) {
                const date = new Date(year, month, day);
                date.setHours(0, 0, 0, 0);

                const button = document.createElement('button');
                button.type = 'button';
                button.className = 'rental-date-picker-day';
                button.textContent = String(day);

                const unavailable = isDateUnavailable(date, this.bookedRanges);
                const pending = isDatePendingRequest(date, this.pendingRequestRanges);
                if (date < today) {
                    button.classList.add('is-past');
                    button.disabled = true;
                } else if (unavailable) {
                    button.classList.add('is-unavailable');
                    button.disabled = true;
                } else if (pending) {
                    button.classList.add('is-pending-request');
                    button.disabled = true;
                } else {
                    if (this.startDate && isSameDay(date, this.startDate)) {
                        button.classList.add('is-selected');
                    }
                    if (this.endDate && isSameDay(date, this.endDate)) {
                        button.classList.add('is-selected');
                    }
                    if (isInRange(date, this.startDate, this.endDate)) {
                        button.classList.add('is-in-range');
                    }

                    button.addEventListener('click', () => this.selectDate(date));
                }

                grid.append(button);
            }
        }

        selectDate(date) {
            const hasRange = this.startDate && this.endDate
                && !isSameDay(this.startDate, this.endDate);

            if (!this.startDate || hasRange) {
                this.startDate = date;
                this.endDate = date;
            } else if (date < this.startDate) {
                this.endDate = this.startDate;
                this.startDate = date;
            } else {
                this.endDate = date;
            }

            this.syncInputs();
            this.render();
            if (this.onChange) {
                this.onChange(this.startDate, this.endDate);
            }
        }

        syncInputs() {
            if (this.startInput) {
                this.startInput.value = this.startDate ? formatInputDate(this.startDate) : '';
            }
            if (this.endInput) {
                this.endInput.value = this.endDate ? formatInputDate(this.endDate) : '';
            }
        }

        updateSelectionLabel() {
            if (!this.selectionLabel) {
                return;
            }

            if (this.startDate && this.endDate) {
                if (isSameDay(this.startDate, this.endDate)) {
                    this.selectionLabel.textContent = `Selected: ${formatInputDate(this.startDate)}`;
                } else {
                    this.selectionLabel.textContent =
                        `Selected: ${formatInputDate(this.startDate)} to ${formatInputDate(this.endDate)}`;
                }
            } else {
                this.selectionLabel.textContent = 'Choose a date on the calendar';
            }
        }

        changeMonth(delta) {
            this.viewMonth = new Date(this.viewMonth.getFullYear(), this.viewMonth.getMonth() + delta, 1);
            this.render();
        }
    }

    window.RentalDatePicker = RentalDatePicker;
    window.RentalDatePickerHelpers = {
        isDateUnavailable,
        isDateBooked,
        isDatePendingRequest,
        rangesOverlap,
    };
})();
