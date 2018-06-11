using BookingSystem.API.Helpers;
using BookingSystem.API.Models;
using BookingSystem.API.Models.DTO;
using BookingSystem.API.Services.Email;
using BookingSystem.API.Services.Payment;
using BookingSystem.API.Services.SMS;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Z.EntityFramework.Plus;

namespace BookingSystem.API.Controllers
{
    [RoutePrefix("api/account")]
    public class AccountController : BaseController
    {
        static readonly string PreferenceClaimType = "user_preference_claim_type";

        IPaymentService paymentService;
        ISMSService smsService;
        IEmailService emailService;

        protected IQueryable<AppUser> Users
        {
            get
            {
                return DB.Users.Include(t => t.ProfileImage).IncludeOptimized(x => x.Claims);
            }
        }

        public AccountController(IPaymentService payService, ISMSService smsService, IEmailService emailService)
        {
            this.paymentService = payService;
            this.smsService = smsService;
            this.emailService = emailService;
        }

        [HttpGet]
        [Authorize(Roles = UserRoles.Admin)]
        [Route("all")]
        public IEnumerable<UserInfo> GetUsers()
        {
            return Map<IList<UserInfo>>(Users.AsNoTracking().ToArray());
        }

        [Route("refresh/token")]
        [HttpPost]
        public IHttpActionResult RefreshToken([FromBody]RefreshTokenModel model)
        {
            var decoded = JwtHelper.DecodeToken(model.RefreshToken, TokenAudiences.RefreshToken);
            if (decoded == null)
            {
                return BadRequest();
            }

            string userId = decoded.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = DB.Users.IncludeOptimized(x => x.Claims).FirstOrDefault(x => x.Id == userId);
            if (user == null)
                return BadRequest();

            return Ok(new LoginResponse()
            {
                RefreshToken = JwtHelper.SignToken(user, expires: DateTime.UtcNow.AddMonths(1), audience: TokenAudiences.RefreshToken),
                AccessToken = JwtHelper.SignToken(user, audience: TokenAudiences.Universal),
                ExpiresAt = DateTime.UtcNow.AddDays(3),
                IssuedAt = DateTime.UtcNow
            });

        }

        [Authorize]
        [Route("preferences")]
        [HttpGet]
        public IDictionary<string, string> GetPreferences()
        {
            var claim = DB.UserClaims.FirstOrDefault(x => x.User.Id == UserId && x.ClaimType == PreferenceClaimType);

            IDictionary<string, string> values = null;
            if (claim != null)
            {
                values = JsonConvert.DeserializeObject<IDictionary<string, string>>(claim.Value);
            }
            else
            {
                var user = DB.Users.Find(UserId);
                DB.UserClaims.Add(new UserClaim()
                {
                    ClaimType = PreferenceClaimType,
                    User = user,
                    Value = JsonConvert.SerializeObject(values = new Dictionary<string, string>())
                });

                DB.SaveChanges();
            }

            return values;
        }

        [Authorize]
        [Route("preferences")]
        [HttpPost]
        public void SetPreference([FromUri]string key, [FromBody] string value)
        {
            var preferences = GetPreferences();
            if (preferences.ContainsKey(key))
                preferences[key] = value;
            else
                preferences.Add(key, value);

            DB.UserClaims.Where(x => x.ClaimType == PreferenceClaimType && x.User.Id == UserId).Update(x => new UserClaim()
            {
                Value = JsonConvert.SerializeObject(preferences)
            });
        }

        [Authorize]
        [Route("preferences")]
        [HttpDelete]
        public void RemovePreference([FromUri]string key)
        {
            var preferences = GetPreferences();
            if (preferences.Remove(key))
            {
                DB.UserClaims.Where(x => x.ClaimType == PreferenceClaimType && x.User.Id == UserId).Update(x => new UserClaim()
                {
                    Value = JsonConvert.SerializeObject(preferences)
                });
            }

        }

        [Authorize]
        [Route("confirm/phone")]
        [HttpPost]
        public async Task ConfirmPhone([FromUri]string phone)
        {
            var user = DB.Users.Include(x => x.Tokens).FirstOrDefault(x => x.Id == UserId);
            if (!DB.Users.Any(x => x.Phone == phone))
            {
                //  Generate and send confirmation code to the phone number
                string dispatchToken = null;
                var existingToken = user.Tokens.FirstOrDefault(x => x.TokenType == TokenType.ChangePhone);
                if (existingToken == null)
                {
                    //  Generate and send confirmation code to the phone number
                    dispatchToken = UserTokensHelper.Generate();

                    //
                    UserToken dbToken = new UserToken()
                    {
                        TokenType = TokenType.ChangePhone,
                        User = user,
                        Token = dispatchToken,
                    };

                    DB.Tokens.Add(dbToken);

                    //  Save token
                    DB.SaveChanges();
                }
                else
                {
                    dispatchToken = existingToken.Token;
                }

                //  Dispatch token to user via phone
                await smsService.SendAsync(new SendSMSOptions()
                {
                    Destinations = new string[] { phone },
                    Message = $"Here's your confirmation code: {dispatchToken}"
                });

            }

        }

        [Authorize]
        [Route("confirm/email")]
        [HttpPost]
        public async Task ConfirmEmail([FromUri]string email)
        {
            var user = DB.Users.Include(x => x.Tokens).FirstOrDefault(x => x.Id == UserId);
            if (!DB.Users.Any(x => x.Email == email))
            {
                string dispatchToken = null;
                var existingToken = user.Tokens.FirstOrDefault(x => x.TokenType == TokenType.ChangeEmail);
                if (existingToken == null)
                {
                    //  Generate and send confirmation code to the phone number
                    dispatchToken = UserTokensHelper.Generate();

                    UserToken dbToken = new UserToken()
                    {
                        TokenType = TokenType.ChangeEmail,
                        User = user,
                        Token = dispatchToken,
                    };

                    DB.Tokens.Add(dbToken);

                    //  Save token
                    DB.SaveChanges();
                }
                else
                {
                    dispatchToken = existingToken.Token;
                }

                //  Dispatch token to user via Email
                await emailService.SendAsync(new SendMailOptions()
                {
                    Subject = "Confirm Email",
                    Destinations = new string[] { email },
                    Message = $"Here's your confirmation code: {dispatchToken}"
                });

            }
        }

        [Authorize(Roles = UserRoles.Admin)]
        [Route("dashboard/admin")]
        [HttpGet]
        public async Task<AdminDashboardModel> GetAdminDashboard()
        {
            AdminDashboardModel dashboard = new AdminDashboardModel()
            {
                Buses = new AdminDashboardModel.BusSpec(),
                Money = new AdminDashboardModel.MoneySpec<double>(),
                Reservations = new AdminDashboardModel.ReservationSpec<long>(),
                Routes = new AdminDashboardModel.RoutesSpec(),
                Users = new AdminDashboardModel.UsersSpec()
            };

            //
            var reservations = DB.Reservations.Select(x => new
            {
                x.DateCreated,
                x.Cancelled,
                x.Route.DepartureTime,
                x.Route.ArrivalTime
            }).AsNoTracking().Future();

            var users = DB.Users.Where(x => x.Claims.FirstOrDefault(t => t.ClaimType == ClaimTypes.Role).Value == UserRoles.User).Select(x => new
            {
                x.DateCreated,
                x.EmailConfirmed,
                x.LockedOut,
            }).AsNoTracking().Future();

            var buses = DB.Buses.Select(x => new
            {
                x.DateCreated,
                Routes = x.Routes.Select(t => new { t.DateCreated, t.ArrivalTime, t.DepartureTime })

            }).AsNoTracking().Future();

            var transactions = DB.Transactions.Select(x => new { x.DateCreated, x.Status, x.Type, x.IdealAmount }).AsNoTracking().Future();

            var routes = DB.Routes.Select(x => new { x.DateCreated, x.ArrivalTime, x.DepartureTime }).AsNoTracking().Future();

            //
            var qReservations = await reservations.ToListAsync();
            dashboard.Reservations.Total = qReservations.Count;
            foreach (var r in qReservations)
            {
                if (DateHelper.IsToday(r.DateCreated))
                {
                    dashboard.Reservations.Today++;
                }
                if (DateHelper.IsYesterday(r.DateCreated))
                {
                    dashboard.Reservations.Yesterday++;
                }
                if (DateHelper.IsThisWeek(r.DateCreated))
                {
                    dashboard.Reservations.Week++;
                }
                if (DateHelper.IsThisMonth(r.DateCreated))
                {
                    dashboard.Reservations.Month++;
                }

                switch (RouteHelpers.Categorize(r.DepartureTime, r.ArrivalTime))
                {
                    case BusRouteState.Active:
                        dashboard.Reservations.Active++;
                        break;
                    case BusRouteState.Used:
                        dashboard.Reservations.Used++;
                        break;
                    case BusRouteState.Pending:
                        dashboard.Reservations.Pending++;
                        break;
                }

            }

            var qUsers = await users.ToListAsync();
            dashboard.Users.TotalUsers = qUsers.Count;
            foreach (var u in qUsers)
            {
                if (u.EmailConfirmed && !u.LockedOut)
                {
                    if (DateHelper.IsToday(u.DateCreated))
                    {
                        dashboard.Users.RegisteredToday++;
                    }

                    if (DateHelper.IsYesterday(u.DateCreated))
                    {
                        dashboard.Users.RegisteredYesterday++;
                    }

                    if (DateHelper.IsThisWeek(u.DateCreated))
                    {
                        dashboard.Users.RegisteredWeek++;
                    }

                    if (DateHelper.IsThisMonth(u.DateCreated))
                    {
                        dashboard.Users.RegisteredMonth++;
                    }
                }

                if (!u.EmailConfirmed)
                    dashboard.Users.PendingActivation++;
            }

            var qBuses = await buses.ToListAsync();
            var activeBuses = qBuses.Where(x => x.Routes.Any(t => RouteHelpers.Categorize(t.DepartureTime, t.ArrivalTime) == BusRouteState.Active));
            dashboard.Buses.Total = qBuses.Count;
            dashboard.Buses.TotalActive = activeBuses.Count();

            foreach (var bus in qBuses)
            {
                var bRoutes = bus.Routes;
                if (bRoutes.Any(x => DateHelper.IsToday(x.DepartureTime)))
                {
                    dashboard.Buses.ActiveToday++;
                }

                if (bRoutes.Any(x => DateHelper.IsYesterday(x.DepartureTime)))
                {
                    dashboard.Buses.ActiveYesterday++;
                }

                if (bRoutes.Any(x => DateHelper.IsThisWeek(x.DepartureTime)))
                {
                    dashboard.Buses.ActiveWeek++;
                }

                if (bRoutes.Any(x => DateHelper.IsToday(x.ArrivalTime)))
                {
                    dashboard.Buses.CompleteToday++;
                }

                if (bRoutes.Any(x => DateHelper.IsYesterday(x.ArrivalTime)))
                {
                    dashboard.Buses.CompleteYesterday++;
                }

                if (bRoutes.Any(x => DateHelper.IsThisWeek(x.ArrivalTime)))
                {
                    dashboard.Buses.CompleteWeek++;
                }

            }

            var qTransactions = await transactions.ToListAsync();
            var refundedTxn = qTransactions.Where(x => x.Type == TransactionType.Refund && x.Status == TransactionStatus.Successful);
            dashboard.Money.Refunded = refundedTxn.Count() == 0 ? 0 : refundedTxn.Sum(x => x.IdealAmount);

            foreach (var t in qTransactions.Where(x => x.Status == TransactionStatus.Successful && x.Type == TransactionType.Charge))
            {
                if (DateHelper.IsToday(t.DateCreated))
                {
                    dashboard.Money.Today += t.IdealAmount;
                }

                if (DateHelper.IsYesterday(t.DateCreated))
                {
                    dashboard.Money.Yesterday += t.IdealAmount;
                }

                if (DateHelper.IsThisWeek(t.DateCreated))
                {
                    dashboard.Money.Week += t.IdealAmount;
                }

                if (DateHelper.IsThisMonth(t.DateCreated))
                {
                    dashboard.Money.Month += t.IdealAmount;
                }
            }

            //
            var qRoutes = await routes.ToListAsync();
            dashboard.Routes.Total = qRoutes.Count;

            return dashboard;
        }

        [Authorize]
        [HttpGet]
        [Route("dashboard/user")]
        public async Task<UserDashboardModel> GetUserDashboard()
        {
            var user = DB.Users.Find(UserId);
            UserDashboardModel dashboard = new UserDashboardModel()
            {
                Money = new UserDashboardModel.MoneySpec<double>(),
                Reservations = new UserDashboardModel.ReservationSpec<long>()
            };

            //
            var reservations = await DB.Reservations.AsNoDbSetFilter().Where(x => x.User.Id == UserId).Select(x => new
            {
                x.DateCreated,
                x.Route.DepartureTime,
                x.Route.ArrivalTime,
                x.Cancelled
            }).ToListAsync();
            dashboard.Reservations.Total = reservations.Count;
            foreach (var r in reservations)
            {
                if (DateHelper.IsToday(r.DateCreated))
                {
                    dashboard.Reservations.Today++;
                }

                if (DateHelper.IsYesterday(r.DateCreated))
                {
                    dashboard.Reservations.Yesterday++;
                }

                if (DateHelper.IsThisWeek(r.DateCreated))
                {
                    dashboard.Reservations.Week++;
                }

                if (DateHelper.IsThisMonth(r.DateCreated))
                {
                    dashboard.Reservations.Month++;
                }

                if (!r.Cancelled)
                    switch (RouteHelpers.Categorize(r.DepartureTime, r.ArrivalTime))
                    {
                        case BusRouteState.Active:
                            dashboard.Reservations.Active++;
                            break;
                        case BusRouteState.Used:
                            dashboard.Reservations.Used++;
                            break;
                        case BusRouteState.Pending:
                            dashboard.Reservations.Pending++;
                            break;
                    }
            }

            //
            var transactions = await DB.Transactions.Where(x => x.Wallet.User.Id == UserId).Select(x => new { x.DateCreated, x.IdealAmount, x.Type, x.Status }).AsNoTracking().ToListAsync();
            var successfulTxn = transactions.Where(x => x.Status == TransactionStatus.Successful
             && x.Type == TransactionType.Charge);
            dashboard.Money.Total = successfulTxn.Count() == 0 ? 0 : successfulTxn.Sum(x => x.IdealAmount);

            foreach (var t in successfulTxn)
            {
                if (DateHelper.IsToday(t.DateCreated))
                {
                    dashboard.Money.Today += t.IdealAmount;
                }

                if (DateHelper.IsYesterday(t.DateCreated))
                {
                    dashboard.Money.Yesterday += t.IdealAmount;
                }

                if (DateHelper.IsThisWeek(t.DateCreated))
                {
                    dashboard.Money.Week += t.IdealAmount;
                }

                if (DateHelper.IsThisMonth(t.DateCreated))
                {
                    dashboard.Money.Month += t.IdealAmount;
                }
            }

            var refundedTxn = transactions.Where(x => x.Type == TransactionType.Refund && x.Status == TransactionStatus.Successful);
            dashboard.Money.Refunded = refundedTxn.Count() == 0 ? 0 : refundedTxn.Sum(x => x.IdealAmount);

            return dashboard;
        }

        [Authorize]
        [Route("change/phone")]
        [HttpPut]
        public IHttpActionResult ChangePhone([FromBody]ChangePhoneModel model)
        {
            var user = DB.Users.Find(UserId);
            var token = DB.Tokens.FirstOrDefault(x => x.User.Id == UserId && x.TokenType == TokenType.ChangePhone && x.Token == model.Token);
            if (token == null)
                return BadRequest("Invalid confirmation code. Please try again!");

            //  Remove token
            DB.Tokens.Remove(token);

            //  Change phone
            user.Phone = model.Phone;
            DB.SaveChanges();

            return Ok();
        }

        [Authorize]
        [Route("change/email")]
        [HttpPut]
        public IHttpActionResult ChangeEmail([FromBody]ChangeEmailModel model)
        {
            var user = DB.Users.Find(UserId);
            var token = DB.Tokens.FirstOrDefault(x => x.User.Id == UserId && x.TokenType == TokenType.ChangeEmail && x.Token == model.Token);
            if (token == null)
                return BadRequest("Invalid confirmation code. Please try again!");

            //  Remove token
            DB.Tokens.Remove(token);

            //  Change email
            user.Email = model.Email;
            DB.SaveChanges();

            return Ok();
        }


        [Authorize]
        [Route("change/password")]
        [HttpPut]
        public IHttpActionResult ChangePassword([FromBody]ChangePasswordModel model)
        {
            var user = DB.Users.Find(UserId);
            if (user == null)
            {
                return NotFound();
            }

            if (user.HasPassword(model.OldPassword))
            {
                user.SetPassword(model.NewPassword);
                DB.SaveChanges();
                return Ok("Password changed successfully");
            }
            else
            {
                return BadRequest("Invalid password. Please check you entered the correct original password");
            }
        }

        [Route("resetpassword")]
        [HttpPost]
        public IHttpActionResult ResetPassword([FromBody]ResetPasswordModel model)
        {
            var user = JwtHelper.DecodeToken(model.UserIdentity, TokenAudiences.ResetPassword, false);
            if (user == null)
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }

            //
            var id = user.FindFirst(ClaimTypes.NameIdentifier).Value;
            var dbUser = DB.Users.FirstOrDefault(x => x.Id == id && x.EmailConfirmed && !x.LockedOut);
            if (dbUser == null)
            {
                return BadRequest(MessageResources.ERR_INVALID_ACCOUNT_STATE);
            }

            //
            var token = DB.Tokens.FirstOrDefault(x => x.Token == model.Token && x.User.Id == id && x.TokenType == TokenType.PasswordReset);
            if (token != null)
            {
                DB.Tokens.Remove(token);
                dbUser.SetPassword(model.NewPassword);
                DB.SaveChanges();
                return StatusCode(HttpStatusCode.NoContent);
            }

            return BadRequest("Invalid reset code. Please try again!");
        }

        [Route("forgotpassword")]
        [HttpPost]
        public async Task<IHttpActionResult> ForgotPassword([FromBody]ForgotPasswordModel model)
        {
            var user = DB.Users.Include(x => x.Claims).FirstOrDefault(x => x.Email == model.Email);
            if (user != null)
            {
                if (!user.EmailConfirmed || user.LockedOut)
                {
                    return BadRequest(MessageResources.ERR_INVALID_ACCOUNT_STATE);
                }

                //  generate token for user
                string token;
                var userToken = DB.Tokens.FirstOrDefault(x => x.TokenType == TokenType.PasswordReset && x.User.Id == user.Id);
                if (userToken != null)
                {
                    token = userToken.Token;
                }
                else
                {
                    token = UserTokensHelper.Generate();

                    UserToken dbToken = new UserToken()
                    {
                        TokenType = TokenType.PasswordReset,
                        User = user,
                        Token = token
                    };

                    DB.Tokens.Add(dbToken);

                    //  Save token
                    await DB.SaveChangesAsync();
                }

                //  send token to client via email
                await emailService.SendAsync(new SendMailOptions()
                {
                    Destinations = new string[] { user.Email },
                    Subject = "Reset Password",
                    IsHtml = false,
                    Message = $"Hello {user.FullName.Split(' ').First()}, here's your password reset code"
                });

                return Ok(new
                {
                    ResetToken = JwtHelper.SignToken(user, audience: TokenAudiences.ResetPassword)
                });
            }

            return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Forbidden, MessageResources.MSG_RESET_PWD_INVALID_USER));
        }

        [Route("feedback")]
        [Authorize]
        public void PostFeedBack([FromBody]CreateFeedback feedback)
        {
            var user = DB.Users.Find(UserId);
            user.Feedback.Add(new UserFeedBack()
            {
                Message = feedback.Message,
            });

            DB.SaveChanges();
        }

        [Route("feedback")]
        [Authorize(Roles = UserRoles.Admin)]
        public IEnumerable<FeedbackInfo> GetFeedbacks()
        {
            return Map<IList<FeedbackInfoEx>>(DB.Feedback.IncludeOptimized(x => x.User).AsNoTracking().ToArray());
        }

        [Route("feedback/{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public IEnumerable<FeedbackInfo> GetUserFeedbacks(string id)
        {
            return Map<IList<FeedbackInfo>>(DB.Feedback.IncludeOptimized(x => x.User).Where(x => x.User.Id == id).ToArray());
        }

        [Route("myself")]
        [Authorize]
        [HttpGet]
        public UserInfo GetMySelf()
        {
            return Map<UserInfo>(Users.FirstOrDefault(x => x.Id == UserId));
        }

        [Authorize]
        [HttpPut]
        [Route("")]
        public IHttpActionResult UpdateProfile([FromBody]UpdateUserInfo model)
        {
            var user = DB.Users.Find(UserId);
            if (user == null)
                return NotFound();

            ObjectMapper.CopyPropertiesTo(model, user, ObjectMapper.UpdateFlag.DeferUpdateOnNull, new string[]{
                nameof(user.Username),
                nameof(user.FullName)
            });

            DB.SaveChanges();
            return Empty();
        }

        [AcceptVerbs("PUT", "POST")]
        [Route("photo")]
        [Authorize]
        public async Task<IHttpActionResult> UploadProfilePhotoAsync()
        {
            var user = DB.Users.Include(x => x.ProfileImage).FirstOrDefault(x => x.Id == UserId);
            if (user == null)
                return NotFound();

            //
            var stream = await Request.Content.ReadAsStreamAsync();
            string mimeType = Request.Content.Headers.ContentType.MediaType;
            string name = Guid.NewGuid().ToString("N");

            if (user.ProfileImage == null)
            {
                var response = await DiskMediaStore.Instance.SaveMedia(name, mimeType, stream);

                user.ProfileImage = new Media()
                {
                    Tag = "default-profile",
                    Name = name,
                    Path = response.Path,
                    MimeType = mimeType
                };

            }
            else
            {
                //  update
                var profile = user.ProfileImage;
                var response = await DiskMediaStore.Instance.UpdateMedia(profile.Name, name, mimeType, stream);
                if (profile.MimeType != mimeType)
                {
                    profile.MimeType = mimeType;
                }

                profile.LastUpdated = DateTime.UtcNow;
                profile.Name = response.Name;
                profile.Path = response.Path;
            }

            DB.SaveChanges();

            return Ok(user.ProfileImage);
        }

        [HttpDelete]
        [Route("photo")]
        [Authorize]
        public async Task RemoveProfilePhotoAsync()
        {
            var user = DB.Users.Find(UserId);
            if (user == null)
                return;

            if (user.ProfileImage != null)
            {
                await DiskMediaStore.Instance.DeleteMedia(user.ProfileImage.Name);
                user.ProfileImage = null;
                await DB.SaveChangesAsync();
            }

        }

        [Route("login")]
        [HttpPost]
        public async Task<IHttpActionResult> Login([FromBody]LoginModel model)
        {
            var user = await DB.Users.Include(x => x.Claims).FirstOrDefaultAsync(x => (x.Username == model.UserId || x.Email == model.UserId));

            if (user == null)
                return BadRequest(MessageResources.MSG_LOGIN_INVALID_CRED);

            if (user.LockedOut)
            {
                return BadRequest(MessageResources.MSG_LOGIN_LOCKED_ACC);
            }

            if (!user.EmailConfirmed)
            {
                var response = Request.CreateErrorResponse(HttpStatusCode.BadRequest, MessageResources.MSG_LOGIN_REQUIRE_EMAIL_CONFIRM);
                response.Headers.Add("UserToken", JwtHelper.SignToken(user, audience: TokenAudiences.ActivateAccount));
                return ResponseMessage(response);
            }

            if (!user.HasPassword(model.Password))
            {
                return BadRequest(MessageResources.MSG_LOGIN_INVALID_CRED);
            }

            //  return jwt for user
            return Ok(new LoginResponse()
            {
                AccessToken = JwtHelper.SignToken(user, audience: TokenAudiences.Universal),
                RefreshToken = JwtHelper.SignToken(user, expires: DateTime.UtcNow.AddMonths(1), audience: TokenAudiences.RefreshToken),
                ExpiresAt = DateTime.UtcNow.AddDays(3),
                IssuedAt = DateTime.UtcNow
            });
        }

        [HttpDelete]
        [Authorize]
        [Route("")]
        public void DeleteAccount()
        {
            var user = DB.Users.Find(UserId);
            if (user != null)
            {
                user.IsSoftDeleted = true;
                DB.SaveChanges();
            }
        }

        [Route("beginactivate")]
        [HttpPost]
        public async Task<IHttpActionResult> BeginActivateAsync([FromUri]string token)
        {
            var principal = JwtHelper.DecodeToken(token, TokenAudiences.ActivateAccount, false);
            if (principal == null)
                return Forbidden();

            //
            var user = DB.Users.Find(principal.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (user == null)
                return Forbidden();

            //
            var activateToken = user.Tokens.FirstOrDefault(x => x.TokenType == TokenType.ActivateAccount);
            if (activateToken == null)
                return Forbidden();

            //  send confirmation email
            await emailService.SendAsync(new SendMailOptions()
            {
                Subject = "Activate Account",
                Destinations = new string[] { user.Email },
                IsHtml = false,
                Message = $"Hello {user.FullName.Split(' ')}, here's your account activation code {activateToken.Token}"
            });

            return Empty();
        }

        [Route("activate")]
        [HttpPut]
        public IHttpActionResult Activate([FromBody]ActivateAccountModel model)
        {
            var principal = JwtHelper.DecodeToken(model.Token, TokenAudiences.ActivateAccount, false);
            if (principal == null)
                return Forbidden();

            var id = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = DB.Users.Find(id);
            if (user == null)
                return Forbidden();

            var token = DB.Tokens.FirstOrDefault(x => x.User.Id == id && x.TokenType == TokenType.ActivateAccount && x.Token == model.ActivationCode);
            if (token != null)
            {
                DB.Tokens.Remove(token);
                user.EmailConfirmed = true;
                DB.SaveChanges();
                return Empty();
            }

            return BadRequest("Invalid activation code entered");
        }

        [Route("register")]
        [HttpPost]
        public async Task<IHttpActionResult> Register([FromUri]string accountType, [FromBody] RegisterModel model)
        {

            if (!(string.Equals(accountType, UserRoles.Admin, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(accountType, UserRoles.User, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("accountType", "Invalid account type");
                return BadRequest(ModelState);
            }

            if (DB.Users.Any(x => x.Email == model.Email))
            {
                return BadRequest("Email already used!");
            }

            if (DB.Users.Any(x => x.Username == model.Username))
            {
                return BadRequest("Username already exists!");
            }

            if (DB.Users.Any(x => x.Phone == model.Phone))
            {
                return BadRequest("Phone No. already exists!");
            }

            //
            var user = AppUser.New(model.Password);
            user.FullName = $"{model.FirstName} {model.LastName}".Trim();
            user.Email = model.Email;
            user.Phone = model.Phone;
            user.Username = model.Username;
            user.EmailConfirmed = false;
            user.LockedOut = false;

            IEnumerable<Claim> claims = null;
            if (accountType.Equals(UserRoles.Admin, StringComparison.OrdinalIgnoreCase))
            {
                claims = AppUser.GenerateAdminClaims(user);
            }
            else if (accountType.Equals(UserRoles.User, StringComparison.OrdinalIgnoreCase))
            {
                claims = AppUser.GenerateUserClaims(user);
            }

            foreach (var claim in claims)
            {
                user.Claims.Add(new UserClaim()
                {
                    ClaimType = claim.Type,
                    Value = claim.Value,
                    User = user
                });
            }

            var activationToken = new UserToken()
            {
                TokenType = TokenType.ActivateAccount,
                User = user,
                Token = UserTokensHelper.Generate(),
            };

            user.Tokens.Add(activationToken);

            //
            DB.Users.Add(user);

            await DB.SaveChangesAsync();

            //  send confirmation email
            await emailService.SendAsync(new SendMailOptions()
            {
                Destinations = new string[] { user.Email },
                IsHtml = false,
                Subject = "Activate Account",
                Message = $"Hello {model.FirstName.Trim()}, here's your account activation code {activationToken.Token}"
            });

            //
            var response = Request.CreateResponse(HttpStatusCode.Created, Map<UserInfo>(user));
            response.Headers.Add("UserToken", JwtHelper.SignToken(user, audience: TokenAudiences.ActivateAccount));
            return ResponseMessage(response);
        }
    }
}