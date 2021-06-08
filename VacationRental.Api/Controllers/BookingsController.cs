using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using VacationRental.Api.Models;
using System.Linq;

namespace VacationRental.Api.Controllers
{
    [Route("api/v1/bookings")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly IDictionary<int, RentalViewModel> _rentals;
        private readonly IDictionary<int, BookingViewModel> _bookings;

        public BookingsController(
            IDictionary<int, RentalViewModel> rentals,
            IDictionary<int, BookingViewModel> bookings)
        {
            _rentals = rentals;
            _bookings = bookings;
        }

        [HttpGet]
        [Route("{bookingId:int}")]
        public BookingViewModel Get(int bookingId)
        {
            if (!_bookings.ContainsKey(bookingId))
                throw new ApplicationException("Booking not found");

            return _bookings[bookingId];
        }

        [HttpPost]
        public ResourceIdViewModel Post(BookingBindingModel model)
        {
            if (model.Nights <= 0)
                throw new ApplicationException("Nigts must be positive");
            if (!_rentals.ContainsKey(model.RentalId))
                throw new ApplicationException("Rental not found");

            int preparationTimeInDays = _rentals[model.RentalId].PreparationTimeInDays;

            for (var i = 0; i < model.Nights; i++)
            {
                var count = 0;
                List<BookingViewModel> bookings = _bookings.Values.Where(a => a.RentalId == model.RentalId).ToList();

                foreach (var booking in bookings)
                {
                    if ((booking.Start <= model.Start.Date && booking.Start.AddDays(booking.Nights).AddDays(preparationTimeInDays) > model.Start.Date)
                        || (booking.Start < model.Start.AddDays(model.Nights).AddDays(preparationTimeInDays) && booking.Start.AddDays(booking.Nights).AddDays(preparationTimeInDays) >= model.Start.AddDays(model.Nights).AddDays(preparationTimeInDays))
                        || (booking.Start > model.Start && booking.Start.AddDays(booking.Nights).AddDays(preparationTimeInDays) < model.Start.AddDays(model.Nights).AddDays(preparationTimeInDays)))
                    {
                        count++;
                    }
                }
                if (count >= _rentals[model.RentalId].Units)
                    throw new ApplicationException("Not available");
            }


            var key = new ResourceIdViewModel { Id = _bookings.Keys.Count + 1 };

            _bookings.Add(key.Id, new BookingViewModel
            {
                Id = key.Id,
                Nights = model.Nights,
                RentalId = model.RentalId,
                Start = model.Start.Date
            });

            return key;
        }
    }
}
