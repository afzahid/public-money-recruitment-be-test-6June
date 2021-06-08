using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using VacationRental.Api.Models;

namespace VacationRental.Api.Controllers
{
    [Route("api/v1/calendar")]
    [ApiController]
    public class CalendarController : ControllerBase
    {
        private readonly IDictionary<int, RentalViewModel> _rentals;
        private readonly IDictionary<int, BookingViewModel> _bookings;

        public CalendarController(
            IDictionary<int, RentalViewModel> rentals,
            IDictionary<int, BookingViewModel> bookings)
        {
            _rentals = rentals;
            _bookings = bookings;
        }

        [HttpGet]
        public CalendarViewModel Get(int rentalId, DateTime start, int nights)
        {
            if (nights < 0)
                throw new ApplicationException("Nights must be positive");
            if (!_rentals.ContainsKey(rentalId))
                throw new ApplicationException("Rental not found");

            var result = new CalendarViewModel
            {
                RentalId = rentalId,
                Dates = new List<CalendarDateViewModel>()
            };
            for (var i = 0; i < nights; i++)
            {
                var date = new CalendarDateViewModel
                {
                    Date = start.Date.AddDays(i),
                    Bookings = new List<CalendarBookingViewModel>(),
                    PreparationTimes = new List<CalendarPreparationViewModel>()
                };

                List<BookingViewModel> bookings = _bookings.Values.Where(a => a.RentalId == rentalId).ToList();

                foreach (var booking in bookings)
                {
                    int preparationTimeInDays = _rentals[rentalId].PreparationTimeInDays;

                    if (booking.Start <= date.Date &&
                        booking.Start.AddDays(booking.Nights - 1).Date >= date.Date)
                    {
                        int unitCount = bookings.Where(a => date.Date >= a.Start && date.Date <= a.Start.AddDays(a.Nights - 1).Date).Count();

                        date.Bookings.Add(new CalendarBookingViewModel
                        {
                            Id = booking.Id,
                            Unit = unitCount
                        });
                    }
                    else
                    {
                        CalendarBookingViewModel calendarBooking = new CalendarBookingViewModel
                        {
                            Id = booking.Id,
                            Unit = 0
                        };

                        if (preparationTimeInDays > 0 &&
                            booking.Start.AddDays(booking.Nights - 1).AddDays(preparationTimeInDays).Date == date.Date)
                        {
                            int unitCount = bookings.Where(a => date.Date == a.Start.AddDays(a.Nights - 1).AddDays(preparationTimeInDays)).Count();
                            date.PreparationTimes.Add(new CalendarPreparationViewModel
                            {
                                Unit = unitCount
                            });
                        }
                    }
                }

                result.Dates.Add(date);
            }

            return result;
        }
    }
}
