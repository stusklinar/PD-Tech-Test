﻿using Microsoft.AspNetCore.Mvc;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.DoctorServices;
using PDR.PatientBooking.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace PDR.PatientBookingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly PatientBookingContext _context;
        private readonly IDoctorService _doctorService;
        private readonly IDateTimeProvider _dateTimeProvider;

        public BookingController(PatientBookingContext context, IDoctorService doctorService, IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _doctorService = doctorService;
            _dateTimeProvider = dateTimeProvider;
        }

        [HttpGet("patient/{identificationNumber}/next")]
        public IActionResult GetPatientNextAppointment(long identificationNumber)
        {
            var availableBookings = _context
                .Order
                .Where(x => !x.IsCancelled
                && x.Patient.Id == identificationNumber
                && x.StartTime > DateTime.Now)
                .OrderBy(x => x.StartTime).ToList();

            if (!availableBookings.Any())
            {
                return StatusCode(502);
            }

            return Ok(new
            {
                availableBookings.First().Id,
                availableBookings.First().DoctorId,
                availableBookings.First().StartTime,
                availableBookings.First().EndTime
            });
        }

        [Route("{bookingId}")]
        [HttpDelete]
        public ActionResult CancelAppointment([FromRoute] Guid bookingId)
        {
            var order = _context.Order.Find(bookingId);

            if (order == null)
            {
                return BadRequest("Unable to find appointment");
            }

            try
            {
                order.IsCancelled = true;
                _context.Order.Update(order);
                _context.SaveChanges();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(502, "Unable to delete booking");
            }

        }

        [HttpPost()]
        public IActionResult AddBooking(NewBooking newBooking)
        {
            if (newBooking.StartTime < _dateTimeProvider.UtcNow)
            {
                return BadRequest("Unable to add a booking in the past");
            }

            var bookingId = new Guid();
            var bookingStartTime = newBooking.StartTime;
            var bookingEndTime = newBooking.EndTime;
            var bookingPatientId = newBooking.PatientId;
            var bookingPatient = _context.Patient.FirstOrDefault(x => x.Id == newBooking.PatientId);
            var bookingDoctorId = newBooking.DoctorId;
            var bookingDoctor = _context.Doctor.FirstOrDefault(x => x.Id == newBooking.DoctorId);
            var bookingSurgeryType = _context.Patient.FirstOrDefault(x => x.Id == bookingPatientId).Clinic.SurgeryType;

            var myBooking = new Order
            {
                Id = bookingId,
                StartTime = bookingStartTime,
                EndTime = bookingEndTime,
                PatientId = bookingPatientId,
                DoctorId = bookingDoctorId,
                Patient = bookingPatient,
                Doctor = bookingDoctor,
                SurgeryType = (int)bookingSurgeryType
            };

            var isDoctorAvailableDuringRange = _doctorService.IsDoctorAvailableDuringRange(bookingDoctor, newBooking.StartTime, newBooking.EndTime);

            if (isDoctorAvailableDuringRange)
            {
                return BadRequest("This timeslot is not currently available");
            }

            _context.Order.AddRange(new List<Order> { myBooking });
            _context.SaveChanges();

            return StatusCode(200);
        }

        public class NewBooking
        {
            public Guid Id { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public long PatientId { get; set; }
            public long DoctorId { get; set; }
        }

        private static MyOrderResult UpdateLatestBooking(List<Order> bookings2, int i)
        {
            MyOrderResult latestBooking;
            latestBooking = new MyOrderResult();
            latestBooking.Id = bookings2[i].Id;
            latestBooking.DoctorId = bookings2[i].DoctorId;
            latestBooking.StartTime = bookings2[i].StartTime;
            latestBooking.EndTime = bookings2[i].EndTime;
            latestBooking.PatientId = bookings2[i].PatientId;
            latestBooking.SurgeryType = (int)bookings2[i].GetSurgeryType();

            return latestBooking;
        }

        private class MyOrderResult
        {
            public Guid Id { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public long PatientId { get; set; }
            public long DoctorId { get; set; }
            public int SurgeryType { get; set; }
        }
    }
}