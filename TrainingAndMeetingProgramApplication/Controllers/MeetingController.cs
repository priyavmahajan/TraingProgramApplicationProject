using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using DAL;
using Models;

namespace TrainingAndMeetingProgramApplication.Controllers
{
    public class MeetingController : ApiController
    {
        private TrainingAndMeetingEntities db = new TrainingAndMeetingEntities();   //object of database entity
        // Post: api/Meeting
        [Authorize]
        [HttpPost]
        [ResponseType(typeof(Meeting))]   //post data for craete new tarining
        public IHttpActionResult PostMeeting(Models.MeetingM meeting)  //bind training model
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return NotFound();
                }
                Schedule sch = new Schedule();                  //object schedule entity
                sch.StartTime = DateTime.ParseExact(meeting.StartTime, "yyyy-MM-dd HH:mm:ss",
                                       System.Globalization.CultureInfo.InvariantCulture);
                sch.EndTime = DateTime.ParseExact(meeting.EndTime, "yyyy-MM-dd HH:mm:ss",
                                       System.Globalization.CultureInfo.InvariantCulture);
                var obj = db.Schedules.Add(sch);
                sch.RoomId = meeting.RoomId;
                db.SaveChanges();                               //save data in schedule table
                Meeting met = new Meeting();                  //object of Training entity
                met.MeetingName = meeting.MeetingName;
                met.Agenda = meeting.Agenda;
                met.ScheduleId = obj.ScheduleId;             //getting Scheduleid from schedule table
                met.CreatedAt = DateTime.Now;
                met.UpdatedAt = DateTime.Now;
                met.UserId = meeting.UserId;
                var obj1 = db.Meetings.Add(met);
                db.Meetings.Add(met);               //save data in meeting db
                db.SaveChanges();
                foreach (int userid in meeting.MeetingAttendeeId)
                {
                    MeetingsAttendee mList = new MeetingsAttendee();    //object of model class
                    mList.UserId = userid;
                    mList.MeetingId = obj1.MeetingId;
                    db.MeetingsAttendees.Add(mList);
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            return CreatedAtRoute("DefaultApi", new { id = meeting.MeetingId }, meeting);
        }
        // GET: api/Meeting
        [Authorize]
        [ResponseType(typeof(Meeting))]
        [HttpGet]
        public List<Models.MeetingListM> GetMeeting()  //bind Training list model
        {
            List<Models.MeetingListM> list = new List<Models.MeetingListM>();
            try
            {    
                var data = db.Meetings.ToList();    //all list of meetings
                foreach (var item in data)
                {
                    MeetingListM mList = new MeetingListM();
                    if (item.DeletedAt == null)
                    {
                        mList.MeetingId = item.MeetingId;
                        mList.MeetingName = item.MeetingName;
                        mList.OrganizerName = item.User.FirstName;
                        mList.Agenda = item.Agenda;
                        mList.StartTime = item.Schedule.StartTime.Value;
                        mList.EndTime = item.Schedule.EndTime.Value;
                        mList.RoomName = item.Schedule.RoomDetail.RoomName;
                        List<Attendee> users = new List<Attendee>();
                        foreach (var one in item.MeetingsAttendees)
                        {
                            Attendee attend = new Attendee();
                            attend.AttendeeName= one.User.FirstName;
                            attend.AttendeeID = one.User.UserId;
                            users.Add(attend);
                        }
                        mList.MeetingAttendeesName = users;
                        list.Add(mList);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return list;
        }

        // GET: api/Meeting/5
        [Authorize]
        [HttpGet]
        [ResponseType(typeof(Meeting))]
        public IHttpActionResult GetMeeting(int id)
        {
            try
            {
                List<MeetingListM> list = new List<MeetingListM>();
                var data = new MeetingController();
                var data1 = data.GetMeeting().Where(a => a.MeetingId == id).FirstOrDefault();
                //var result = data1.;
                return Ok(data1);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // PUT: api/Meeting/5
        [Authorize]
        [HttpPut]
        [ResponseType(typeof(Meeting))]
        public IHttpActionResult PutMeeting(int id, [FromBody]Models.MeetingM meeting)
        {
            if (!ModelState.IsValid)
            {
                return NotFound();
            }
            try
            {
                using (var ctx = new TrainingAndMeetingEntities())
                {
                    Meeting tra = new Meeting();
                    Schedule sch = new Schedule();
                    RoomDetail rm = new RoomDetail();
                    MeetingListM mList = new MeetingListM();    //object of model class
                    var data = db.Meetings.ToList();    //all list of meetings
                    var existingmeeting = ctx.Meetings.Where(s => s.MeetingId == id)
                                                            .FirstOrDefault<Meeting>();   //get existing meeting details
                    var existingschedule = ctx.Schedules.Where(s => s.ScheduleId == existingmeeting.ScheduleId)
                                                             .FirstOrDefault<Schedule>();                    // get existing schedule
                    var existingmeetingattendee=ctx.MeetingsAttendees.Where(s=>s.MeetingId== existingmeeting.MeetingId)
                                                             .FirstOrDefault<MeetingsAttendee>(); //get existing meeting attendee list  
                    if (existingmeeting != null)
                    {
                        existingschedule.StartTime = DateTime.ParseExact(meeting.StartTime, "yyyy-MM-dd HH:mm:ss",
                                       System.Globalization.CultureInfo.InvariantCulture);
                        existingschedule.EndTime = DateTime.ParseExact(meeting.EndTime, "yyyy-MM-dd HH:mm:ss",
                                       System.Globalization.CultureInfo.InvariantCulture);
                        existingschedule.RoomId = meeting.RoomId;
                        var obj = db.Schedules.Add(sch);
                        ctx.SaveChanges();

                        existingmeeting.MeetingName = meeting.MeetingName;    //add value in training table
                        existingmeeting.Agenda = meeting.Agenda;
                       // existingmeetingattendee.MeetingId = meeting.MeetingId;
                        var obj1 = db.Meetings.Add(tra);
                        foreach (int userid in meeting.MeetingAttendeeId)
                        {
                            MeetingsAttendee list = new MeetingsAttendee();    //object of model class
                            list.UserId = userid;
                            list.MeetingId = existingmeeting.MeetingId;
                            ctx.MeetingsAttendees.Add(list);
                            ctx.SaveChanges();
                        }
                        ctx.SaveChanges();
                    }
                    else
                    {
                        return NotFound();
                    }
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            return Ok(meeting);

        }
        // DELETE: api/Meeting/5
        [Authorize]
        [HttpDelete]
        [ResponseType(typeof(Meeting))]
        public IHttpActionResult DeleteMeeting(int id)
        {
            MeetingsAttendee meetingsAttendee = db.MeetingsAttendees.Where(a => a.MeetingId == id).FirstOrDefault();
            Meeting meeting = db.Meetings.Find(id);
            Schedule schedule = db.Schedules.Where(a => a.ScheduleId == meeting.ScheduleId).FirstOrDefault();
            try
            {
                if (meeting == null)
                {
                    return NotFound();
                }
                db.Meetings.Remove(meeting);
                db.Schedules.Remove(schedule);
                db.MeetingsAttendees.Remove(meetingsAttendee);
                db.SaveChanges();

                return Ok(meeting);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool MeetingExists(int id)
        {
            return db.Meetings.Count(e => e.MeetingId == id) > 0;
        }
    }
}