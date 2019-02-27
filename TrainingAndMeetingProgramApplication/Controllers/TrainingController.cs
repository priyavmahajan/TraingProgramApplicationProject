using DAL;
using Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using System.Web.Http.Results;

namespace TrainingAndMeetingProgramApplication.Controllers
{
    public class TrainingController : ApiController
    {
        private TrainingAndMeetingEntities db = new TrainingAndMeetingEntities();    //object of database entity
        // POST: api/Training
        [HttpPost]
        [Authorize]
        [ResponseType(typeof(Training))]   //post data for craete new tarining
        public IHttpActionResult PostTraining(Models.TrainingModel training)  //bind training model
        {
          
            try
            {
                Schedule sch = new Schedule();                  //object schedule entity
                sch.StartTime = DateTime.ParseExact(training.StartTime, "yyyy-MM-dd HH:mm:ss",
                                       System.Globalization.CultureInfo.InvariantCulture);
                sch.EndTime = DateTime.ParseExact(training.EndTime, "yyyy-MM-dd HH:mm:ss",
                                       System.Globalization.CultureInfo.InvariantCulture);
                var obj = db.Schedules.Add(sch);
                sch.RoomId = training.RoomId;

                db.SaveChanges();                               //save data in schedule table
                Training tra = new Training();                  //object of Training entity
                tra.Topic = training.Topic;
                tra.Description = training.Description;
                tra.ScheduleId = obj.ScheduleId;             //getting Scheduleid from schedule table
                tra.CreatedAt = DateTime.Now;
                tra.UpdatedAt = DateTime.Now;
                tra.UserId = training.UserId;
                db.Trainings.Add(tra);               //save data in training db
                db.SaveChanges();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            return CreatedAtRoute("DefaultApi", new { id = training.TrainingId }, training);

        }

        // GET: api/Training
        [Authorize]
        [ResponseType(typeof(Training))]
        [HttpGet]
        public List<Models.TrainingListM> GetTraining()  //bind Training list model
        {
            List<Models.TrainingListM> list = new List<Models.TrainingListM>();
            try
            {
                var data = db.Trainings.ToList();    //all list of training
                foreach (var item in data)
                {
                    TrainingListM tList = new TrainingListM();    //object of medel class
                    if (item.DeletedAt == null)
                    {

                        tList.TrainingId = item.TrainingId;
                        tList.TrainerName = item.User.FirstName;
                        tList.Topic = item.Topic;
                        tList.Description = item.Description;
                        tList.StartTime = item.Schedule.StartTime.Value;
                        tList.EndTime = item.Schedule.EndTime.Value;
                        tList.RoomName = item.Schedule.RoomDetail.RoomName;
                        list.Add(tList);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return list;
        }

        // GET: api/Training/5
        [Authorize]
        [HttpGet]
        [ResponseType(typeof(Training))]
        public IHttpActionResult GetTraining(int id)
        {
            TrainingListM trainingListM = null;
            try
            {
                using (var context = new TrainingAndMeetingEntities())
                {
                    trainingListM = context.Trainings.Include("TrainingId")   //to get list of training by comapring id
                        .Where(s => s.TrainingId == id)
                        .Select(s => new TrainingListM()
                        {
                            Topic = s.Topic,
                            TrainerName = s.User.FirstName,
                            StartTime = s.Schedule.StartTime.Value,
                            EndTime = s.Schedule.EndTime.Value,
                            RoomName = s.Schedule.RoomDetail.RoomName,
                            Description = s.Description
                        }).FirstOrDefault<TrainingListM>();
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            if (trainingListM == null)
            {
                return NotFound();
            }
            return Ok(trainingListM);
        }

        // DELETE: api/Training/5
        [Authorize]
        [HttpDelete]
        [ResponseType(typeof(Training))]
        public IHttpActionResult DeleteTraining(int id)
        {
            Training training = db.Trainings.Find(id);
            Schedule schedule = db.Schedules.Where(a => a.ScheduleId == training.ScheduleId).FirstOrDefault();
            try
            {
                if (training == null)
                {
                    return NotFound();
                }
                db.Trainings.Remove(training);    //delete training
                db.Schedules.Remove(schedule);   //delete schedule of training
                db.SaveChanges();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            return Ok(training);
        }

        // PUT: api/Training/5
        [Authorize]
        [HttpPut]
        [ResponseType(typeof(Training))]
        public IHttpActionResult PutTraining(int id, Models.TrainingModel training)
        {

            if (!ModelState.IsValid)
            {
                return NotFound();
            }
            try
            {
                using (var ctx = new TrainingAndMeetingEntities())
                {
                    Training tra = new Training();
                    Schedule sch = new Schedule();
                    RoomDetail rm = new RoomDetail();
                    var existingtraining = ctx.Trainings.Where(s => s.TrainingId == id)
                                                            .FirstOrDefault<Training>();
                    var existingschedule = ctx.Schedules.Where(s => s.ScheduleId == existingtraining.ScheduleId)
                                                             .FirstOrDefault<Schedule>();
                    if (existingtraining != null)
                    {
                        existingschedule.StartTime = DateTime.ParseExact(training.StartTime, "yyyy-MM-dd HH:mm:ss",
                                       System.Globalization.CultureInfo.InvariantCulture);
                        existingschedule.EndTime = DateTime.ParseExact(training.EndTime, "yyyy-MM-dd HH:mm:ss",
                                       System.Globalization.CultureInfo.InvariantCulture);
                        existingschedule.RoomId = training.RoomId;
                        var obj = db.Schedules.Add(sch);
                        ctx.SaveChanges();

                        existingtraining.Topic = training.Topic;        //add value in training table
                        existingtraining.Description = training.Description;
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
            return Ok(training);

        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool TrainingExists(int id)
        {
            return db.Trainings.Count(e => e.TrainingId == id) > 0;
        }
    }
}
