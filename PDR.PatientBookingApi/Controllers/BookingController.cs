using Microsoft.AspNetCore.Mvc;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PDR.PatientBookingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly PatientBookingContext _context;

        public BookingController(PatientBookingContext context)
        {
            _context = context;
        }

        [HttpGet("doctor/{doctorId}")]
        public IActionResult GetByDoctorId(long doctorId)
        {
            var bookings = _context.Order.ToList();

            var bookings2 = bookings.Where(x => x.DoctorId == doctorId).ToList();

            var bookings3 = new List<MyOrderResult>();
            for (var i = 0; i < bookings2.Count(); i++)
            {
                bookings3.Add(new MyOrderResult());
                bookings3[i].DoctorId = bookings2[i].DoctorId;
                bookings3[i].StartTime = bookings2[i].StartTime;
                bookings3[i].EndTime = bookings2[i].StartTime;
                bookings3[i].PatientId = bookings2[i].PatientId;
                bookings3[i].SurgeryType = (int)bookings2[i].GetSurgeryType();
            }

            var bookings4 = bookings3.OrderBy(x => x.StartTime);

            return Ok(bookings4);
        }



        [HttpGet("doctor/{doctorId}/latest")]
        public IActionResult GetLatestByDoctorId(long doctorId)
        {
            var bookings = _context.Order.ToList();

            var bookings2 = bookings.Where(x => x.DoctorId == doctorId).ToList();

            var latestBooking = new MyOrderResult();
            latestBooking.StartTime = new DateTime(1970, 1, 1);

            for (var i = 0; i < bookings2.Count(); i++)
            {
                if (bookings2[i].StartTime > latestBooking.StartTime)

                    latestBooking = UpdateLatestBooking(bookings2, i);
            }

            return Ok(latestBooking);
        }

        [HttpGet("patient/{identificationNumber}/next")]
        public IActionResult GetPatientNextAppointnemtn(long identificationNumber)
        {
            var bockings = _context.Order.OrderBy(x => x.StartTime).ToList();

            if (bockings.Where(x => x.Patient.Id == identificationNumber).Count() == 0)
            {
                return StatusCode(502);
            }
            else
            {
                var bookings2 = bockings.Where(x => x.PatientId == identificationNumber);
                if (bookings2.Where(x => x.StartTime > DateTime.Now).Count() == 0)
                {
                    return StatusCode(502);
                }
                else
                {
                    var bookings3 = bookings2.Where(x => x.StartTime > DateTime.Now);
                    return Ok(new
                    {
                        bookings3.First().Id,
                        bookings3.First().DoctorId,
                        bookings3.First().StartTime,
                        bookings3.First().EndTime
                    });
                }
            }
        }

        [HttpPost()]
        public IActionResult AddBooking(NewBooking newBooking)
        {
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