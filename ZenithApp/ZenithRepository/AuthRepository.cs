using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TimeZoneConverter;
using ZenithApp.Settings;
using ZenithApp.ZenithEntities;
using ZenithApp.ZenithMessage;
using ZenithApp.ZenithRepository;
using LoginRequest = ZenithApp.ZenithMessage.LoginRequest;

namespace gmkRepositories
{
    public class AuthRepository : BaseRepository
    {
        private readonly IHttpContextAccessor _acc;
        private readonly IConfiguration _configuration;
        private readonly IMongoCollection<tbl_user> _user;
        private readonly IMongoCollection<tbl_User_Role> _roles;

        public AuthRepository(IOptions<MongoDbSettings> settings, IHttpContextAccessor accessor, IConfiguration configuration)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _user = database.GetCollection<tbl_user>("tbl_user");
            _roles = database.GetCollection<tbl_User_Role>("tbl_User_Role");

            // _jobs = database.GetCollection<Job>("Jobs");
            _acc = accessor;
            _configuration = configuration;
        }

        public LoginResponse Login(LoginRequest request)
        {
            var response = new LoginResponse();
            try
            {
                if (request.loginType == "Customer")
                {
                    if (string.IsNullOrWhiteSpace(request.email) && string.IsNullOrWhiteSpace(request.mobileNo))
                    {
                        return CreateErrorResponse<LoginResponse>("Please enter email or mobile.", 400);
                    }
                    string? userid = "0";

                    var user = _user.Find(x =>
                        (x.EmailId == request.email || x.ContactNo == request.mobileNo)
                        && x.IsDelete == 0).FirstOrDefault();

                    if (user == null)
                    {
                        // Register new Reviewer
                        user = new tbl_user
                        {
                            FullName = "customer",
                            EmailId = !string.IsNullOrWhiteSpace(request.email) && request.email.Contains("@") ? request.email : null,
                            ContactNo = !string.IsNullOrWhiteSpace(request.mobileNo) ? request.mobileNo : null,

                            Password = "123456", // Default Password (never used)
                            Fk_RoleID = "686fc53af41f7edee9b89cd7",
                            CreatedAt = GetLocalDateTime(),
                            UpdatedAt = GetLocalDateTime(),
                            CreatedBy = "System",
                            UpdatedBy = "System",
                            IsDelete = 0
                        };
                        _user.InsertOne(user);

                         userid = user.Id;
                    }
                    else
                    {
                        // Check role
                        if (user.Fk_RoleID != "686fc53af41f7edee9b89cd7")
                            return CreateErrorResponse<LoginResponse>("User is not a Reviewer.", 400);
                    }

                    // Send static OTP
                    userid = user.Id;
                    var otp = "0000";
                    if (!string.IsNullOrWhiteSpace(user.EmailId))
                        SendEmailOtp(user.EmailId, otp);
                    else if (!string.IsNullOrWhiteSpace(user.ContactNo))
                        SendSmsOtp(user.ContactNo, otp);

                    response.Message = "Otp send successfully.";
                    response.Success = true;
                    response.HttpStatusCode = System.Net.HttpStatusCode.OK;
                    response.ResponseCode = 0;
                    response.data = new LoginData
                    {
                        userId = userid,
                    };
                }
                else 
                {
                if (string.IsNullOrWhiteSpace(request.email))
                    return CreateErrorResponse("Please enter email.", 400);

                    var user = _user.Find(x => x.UserName == request.email && x.IsDelete == 0).FirstOrDefault();

                if (user == null)
                    return CreateErrorResponse("Invalid username.", 400);

                if (user.Password != request.password)
                    return CreateErrorResponse("Invalid password.", 400);

                var role = _roles.Find(x => x.Id == user.Fk_RoleID).FirstOrDefault();
                var userRole = role.roleName;
                var token = GenerateJwtToken(user.Id, user.ContactNo, userRole);

                _acc.HttpContext.Session.Set("Login_ID", Encoding.UTF8.GetBytes(user.Id));

                response.code = 200;
                response.HttpStatusCode = HttpStatusCode.OK;
                response.Success = true;
                response.Message = "Login successful.";
                response.data = new LoginData
                {
                    token = token,
                    userId = user.Id,
                    fullName = user.FullName,
                    Role = userRole,
                    email = user.EmailId,
                    mobileNo = user.ContactNo,
                    Department = user.ContactNo,
                };

                return response;
            }
                return response;
            }
            catch (Exception ex)
            {
                return CreateErrorResponse($"An error occurred: {ex.Message}", 500);
            }
        }


        // Generic Error Response Helper
        private T CreateErrorResponse<T>(string message, int code) where T : BaseResponse, new()
        {
            return new T
            {
                HttpStatusCode = HttpStatusCode.BadRequest,
                Success = false,
                Message = message,
                ResponseCode = code
            };
        }

        //public LoginResponse Login(LoginRequest request)
        //{
        //    var response = new LoginResponse();
        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(request.email))
        //            return CreateErrorResponse("Please enter email.", 400);

        //        var user = _user.Find(x => x.EmailId == request.email && x.IsDelete == 0).FirstOrDefault();

        //        if (user == null)
        //            return CreateErrorResponse("Invalid username.", 400);

        //        if (user.Password != request.password)
        //            return CreateErrorResponse("Invalid password.", 400);

        //        var role = _roles.Find(x => x.Id == user.Fk_RoleID).FirstOrDefault();
        //        var userRole = role.roleName;
        //        var token = GenerateJwtToken(user.Id, user.ContactNo, userRole);

        //        _acc.HttpContext.Session.Set("Login_ID", Encoding.UTF8.GetBytes(user.Id));

        //        response.code = 200;
        //        response.HttpStatusCode = HttpStatusCode.OK;
        //        response.Success = true;
        //        response.Message = "Login successful.";
        //        response.data = new LoginData
        //        {
        //            token = token,
        //            userId = user.Id,
        //            fullName = user.FullName,
        //            Role = userRole,
        //            email = user.EmailId,
        //            mobileNo = user.ContactNo,
        //            Department = user.ContactNo,
        //        };

        //        return response;
        //    }
        //    catch (Exception ex)
        //    {
        //        return CreateErrorResponse($"An error occurred: {ex.Message}", 500);
        //    }
        //}

        

       
        //public BaseResponse SendOtp(SendOtpRequest request)
        //{
        //    if (string.IsNullOrWhiteSpace(request.EmailOrMobile))
        //        return CreateErrorResponse("Please enter email or mobile number.", 400);

        //    tbl_user user = null;

        //    if (request.EmailOrMobile.Contains("@"))
        //    {
        //        user = _user.Find(x => x.EmailId == request.EmailOrMobile && x.IsDelete == 0).FirstOrDefault();
        //    }
        //    else
        //    {
        //        user = _user.Find(x => x.ContactNo == request.EmailOrMobile && x.IsDelete == 0).FirstOrDefault();
        //    }

        //    if (user == null)
        //    {
        //        user = new tbl_user
        //        {
        //            FullName = "Reviewer",
        //            EmailId = request.EmailOrMobile.Contains("@") ? request.EmailOrMobile : null,
        //            ContactNo = !request.EmailOrMobile.Contains("@") ? request.EmailOrMobile : null,
        //            Password = "123456",
        //            Fk_RoleID = "686fc53af41f7edee9b89cd7", // Reviewer RoleId
        //            CreatedAt = GetLocalDateTime(),
        //            UpdatedAt = GetLocalDateTime(),
        //            CreatedBy = "System",
        //            UpdatedBy = "System",
        //            IsDelete = 0
        //        };
        //        _user.InsertOne(user);
        //    }

        //     ✅ Static OTP Logic
        //    var otp = "0000";

        //    if (request.EmailOrMobile.Contains("@"))
        //    {
        //        SendEmailOtp(request.EmailOrMobile, otp);
        //    }
        //    else
        //    {
        //        SendSmsOtp(request.EmailOrMobile, otp);
        //    }

        //    return CreateSuccessResponse("OTP sent successfully.");
        //}

        private void SendEmailOtp(string email, string otp)
        {
            // Your Email Sending Logic Here
        }

        private void SendSmsOtp(string mobileNo, string otp)
        {
            // Your SMS Sending Logic Here
        }
        public LoginResponse VerifyOtp(VerifyOtpRequest request )
        {
            if (string.IsNullOrWhiteSpace(request.userId))
                return CreateErrorResponse<LoginResponse>("Please enter email or mobile number.", 400);

            if (string.IsNullOrWhiteSpace(request.otp))
                return CreateErrorResponse<LoginResponse>("Please enter OTP.", 400);

            var user = _user.Find(x =>
                x.Id == request.userId  && x.IsDelete == 0).FirstOrDefault();

            if (user == null)
                return CreateErrorResponse<LoginResponse>("User not found.", 400);

            if (user.Fk_RoleID != "686fc53af41f7edee9b89cd7")
                return CreateErrorResponse<LoginResponse>("User is not a Reviewer.", 400);

            if (request.otp != "0000")
                return CreateErrorResponse<LoginResponse>("Invalid OTP.", 400);


            var role = "customer"; // Assuming Reviewer role is static for this example
            var token = GenerateJwtToken(user.Id, user.ContactNo, role);

            _acc.HttpContext.Session.Set("Login_ID", Encoding.UTF8.GetBytes(user.Id));

            return new LoginResponse
            {
                code = 200,
                HttpStatusCode = HttpStatusCode.OK,
                Success = true,
                Message = "Login successful.",
                data = new LoginData
                {
                    token = token,
                    userId = user.Id,
                    fullName = user.FullName,
                    Role = "customer",
                    email = user.EmailId,
                    mobileNo = user.ContactNo,
                    Department = user.ContactNo,
                }
            };
        }
        private string GenerateJwtToken(string userId, string mobileNumber, string role)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, jwtSettings["Subject"]),
            new Claim("UserId", userId),
            new Claim("MobileNumber", mobileNumber),
            new Claim("UserRole", role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private LoginResponse CreateErrorResponse(string message, int code)
        {
            return new LoginResponse
            {
                HttpStatusCode = HttpStatusCode.BadRequest,
                Success = false,
                Message = message,
                code = code
            };
        }

        

        public DateTime GetLocalDateTime()
        {
            DateTime utcNow = DateTime.UtcNow;
            return TimeZoneInfo.ConvertTimeFromUtc(utcNow, TZConvert.GetTimeZoneInfo("India Standard Time"));
        }

        protected override void Disposing()
        {
        }
    }

}
