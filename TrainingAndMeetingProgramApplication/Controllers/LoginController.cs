﻿using DAL;
using Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;

namespace TrainingAndMeetingProgramApplication.Controllers
{
    public class LoginController : ApiController
    {
        private TrainingAndMeetingEntities db = new TrainingAndMeetingEntities();  //object of database entity
        [HttpPost]
        public IHttpActionResult Authenticate([FromBody] LoginRequest login)

            if (login != null)
            {
                var result = db.Users.Where(a => a.Email == login.Email && a.Password == login.Password).FirstOrDefault();  // to check data with database
                string token = createToken(loginrequest.Email,result.UserId);
                return Ok(new { token, result.FirstName ,result.UserId});
                // if credentials are not valid send unauthorized status code in response
                loginResponse.responseMsg.StatusCode = HttpStatusCode.Unauthorized;
            //Set issued at date
            DateTime issuedAt = DateTime.UtcNow;
            //set the time when it expires
            DateTime expires = DateTime.UtcNow.AddDays(1);
            var tokenHandler = new JwtSecurityTokenHandler();

            //create a identity and add claims to the user which we want to log in
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(new[]
            {
              new Claim(ClaimTypes.NameIdentifier,UserId.ToString())
        });
            //create the jwt
            var token = (JwtSecurityToken)
tokenHandler.CreateJwtSecurityToken(issuer: "http://localhost:61574", audience: "http://localhost:61574",
subject: claimsIdentity, notBefore: issuedAt, expires: expires, signingCredentials: signingCredentials);

       
    }
}