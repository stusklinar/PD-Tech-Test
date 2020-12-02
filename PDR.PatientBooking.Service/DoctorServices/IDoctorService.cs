using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.DoctorServices.Requests;
using PDR.PatientBooking.Service.DoctorServices.Responses;
using System;

namespace PDR.PatientBooking.Service.DoctorServices
{
    public interface IDoctorService
    {
        void AddDoctor(AddDoctorRequest request);
        GetAllDoctorsResponse GetAllDoctors();
        bool IsDoctorAvailableDuringRange(Doctor doc, DateTime startTime, DateTime endTime);
    }
}