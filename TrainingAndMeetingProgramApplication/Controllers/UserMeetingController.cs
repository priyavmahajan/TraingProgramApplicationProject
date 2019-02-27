using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace TrainingAndMeetingProgramApplication.Controllers
{
    public class UserMeetingController : ApiController
    {
        private TrainingAndMeetingEntities db = new TrainingAndMeetingEntities();   //object of database entity
        [HttpGet]
        [ResponseType(typeof(User))]
        public IHttpActionResult GetUsers()
        {
            var list = db.Users.ToList();
            foreach (var item in list)
            {
                var result = db.Users.Select(x => new { x.FirstName, x.UserId });
                return Ok(result);
            }

            return Ok();
        }

    }
}
