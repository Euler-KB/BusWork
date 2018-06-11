using BookingSystem.API.Models;
using BookingSystem.API.Models.DTO;
using BookingSystem.API.Services.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Data.Entity;
using BookingSystem.API.Hubs;

namespace BookingSystem.API.Controllers
{
    [RoutePrefix("api/reservations")]
    [Authorize]
    public class ReservationController : BaseController
    {
        private IPaymentService payment;

        protected IQueryable<BookReservation> Reservations
        {
            get
            {
                var query = DB.Reservations.Include(x => x.Route).Include(x => x.Route.Bus).Include(x => x.Transactions);
                if (User.IsInRole(UserRoles.Admin))
                    return query.Include(x => x.User);

                return query;
            }
        }

        public ReservationController(IPaymentService paymentService)
        {
            payment = paymentService;
        }

        [Authorize(Roles = UserRoles.Admin)]
        [Route("all")]
        public IEnumerable<ReservationInfo> Get()
        {
            return Map<IList<ReservationInfo>>(Reservations.AsNoTracking().ToArray());
        }

        [Authorize(Roles = UserRoles.Admin)]
        [Route("{id}/user")]
        public UserInfo GetUser(long id)
        {
            var user = DB.Reservations.Where(x => x.Id == id).Select(x => x.User).Include(x => x.ProfileImage).Include(x => x.Claims).FirstOrDefault();
            if (user == null)
                return null;

            return Map<UserInfo>(user);
        }

        [Authorize]
        [Route("category")]
        [HttpGet]
        public IEnumerable<ReservationCategoryBinding> GetReservationCategories([FromUri]long[] id)
        {
            List<ReservationCategoryBinding> result = new List<ReservationCategoryBinding>();
            foreach (var reservation in DB.Reservations.Where(x => x.User.Id == UserId && id.Contains(x.Id)).AsNoTracking().Select(x => new
            {
                x.Id,
                x.Route.DepartureTime,
                x.Route.ArrivalTime,
                x.Cancelled
            }))
            {
                var bind = new ReservationCategoryBinding()
                {
                    ReservationId = reservation.Id
                };

                if (reservation.Cancelled)
                {
                    bind.Category = ReservationCategory.Cancelled;
                }
                else
                {
                    switch (Helpers.RouteHelpers.Categorize(reservation.DepartureTime, reservation.ArrivalTime))
                    {
                        case Helpers.BusRouteState.Active:
                            bind.Category = ReservationCategory.Active;
                            break;
                        case Helpers.BusRouteState.Pending:
                            bind.Category = ReservationCategory.Pending;
                            break;
                        case Helpers.BusRouteState.Used:
                            bind.Category = ReservationCategory.Completed;
                            break;
                    }
                }

                result.Add(bind);
            }

            return result;
        }

        [Route("")]
        public IEnumerable<ReservationInfo> Get([FromUri]QueryOptions options)
        {
            return Map<IList<ReservationInfo>>(ApplyQueryOptions(Reservations.Where(x => x.User.Id == UserId), options)
                .AsNoTracking().ToArray());
        }

        [Route("{id}/bus")]
        public IEnumerable<ReservationInfo> GetForBus(long id)
        {
            return Map<IList<ReservationInfo>>(Reservations.Where(x => x.Route.Bus.Id == id).AsNoTracking().ToArray());
        }

        [Authorize]
        [Route("{id}/charge")]
        [HttpPost]
        public async Task<IHttpActionResult> Charge(long id, [FromBody]PaymentDetails payDetails)
        {
            var user = DB.Users.Find(UserId);
            var reservation = DB.Reservations.FirstOrDefault(x => x.Id == id);
            if (reservation == null)
                return NotFound();

            //
            var wallet = DB.Wallet.FirstOrDefault(x => x.User.Id == UserId && payDetails.WalletId == payDetails.WalletId);
            if (wallet == null)
            {
                return NotFound();
            }

            //
            if (reservation.Transactions.Any(x => x.Status == TransactionStatus.Successful && (x.Type == TransactionType.Charge || x.Type == TransactionType.Refund)))
            {
                return BadRequest("Reservation ticket cannot be charged in its current state!");
            }

            var route = reservation.Route;
            var seats = reservation.Seats.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            //
            var finalCost = route.Cost * seats.Length;
            var gatewayCharges = (await payment.CalculateCharges(finalCost, wallet, TransactionType.Charge));

            //  Charge for reservation here
            var txn = await payment.Charge(new ChargeOptions()
            {
                Amount = finalCost + gatewayCharges,

                FeesOnCustomer = true,
                AdditionalToken = payDetails.AdditionalToken,
                Email = user.Email,
                Name = user.FullName,
                RefLocal = $"BKS-{Guid.NewGuid().ToString("N").Substring(0, 12)}",

                //
                TotalSeats = seats.Length,
                UnitSeatCost = route.Cost,
                GatewayCharges = gatewayCharges

            }, wallet);

            //  Set reservation
            txn.Reservation = reservation;
            reservation.Transactions.Add(txn);

            DB.SaveChanges();
            return Empty();
        }

        [HttpGet]
        [Route("cost/{routeId}/{walletId}")]
        [Authorize]
        public async Task<IHttpActionResult> CalculateCost(long routeId, long walletId, [FromUri]string seats)
        {
            ReservationCostInfo costInfo = new ReservationCostInfo();
            var route = DB.Routes.Find(routeId);
            var wallet = DB.Wallet.Find(walletId);
            if (route == null || wallet == null)
                return NotFound();

            //
            var seatParts = seats.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (seatParts.Length == 0)
                return BadRequest("No seats booked!");


            var cost = seatParts.Length * route.Cost;
            costInfo.Charges = await payment.CalculateCharges(cost, wallet, TransactionType.Charge);
            costInfo.ReservationCost = cost;

            return Ok(costInfo);
        }

        // POST api/<controller>
        [Route("")]
        public async Task<IHttpActionResult> Post([FromBody]CreateReservationInfo model)
        {
            var route = DB.Routes.Find(model.RouteId);
            var wallet = DB.Wallet.Find(model.WalletId);

            if (route == null || wallet == null)
                return NotFound();

            if (Helpers.RouteHelpers.Categorize(route.DepartureTime, route.ArrivalTime) != Helpers.BusRouteState.Pending)
            {
                return BadRequest("Reservation cannot be booked at this time!");
            }

            //  Check seat has been booked already
            var seats = model.Seats.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (seats.Length == 0)
            {
                return BadRequest("No seats booked!");
            }

            //  Check for already booked seats
            List<int> bookedSeats = new List<int>();
            foreach (var seatBooking in DB.Reservations.Where(x => !x.Cancelled && x.RouteId == model.RouteId).Select(x => x.Seats).ToArray())
                bookedSeats.AddRange(seatBooking.Split(',').Select(x => int.Parse(x)).ToArray());

            //
            var intersecting = seats.Select(x => int.Parse(x)).Intersect(bookedSeats);
            if (intersecting.Count() > 0)
            {
                return BadRequest($"The seat(s) {string.Join(",", intersecting)} {(intersecting.Count() == 1 ? "has" : "have")} already been booked!");
            }

            var user = DB.Users.Find(UserId);
            var rCount = DB.Reservations.Count().ToString();
            var reservation = new BookReservation()
            {
                RouteId = model.RouteId,
                Route = route,
                Seats = model.Seats,
                ReferenceNo = $"{Guid.NewGuid().ToString("N").Substring(0, 3)}{ (rCount.Length < 3 ? rCount.PadLeft(3, '0') : rCount.Substring(rCount.Length - 3))}",
                PickupLocation = model.PickupLocation,
                User = user,
            };

            user.Reservations.Add(reservation);


            var finalCost = route.Cost * seats.Length;
            var gatewayCharges = (await payment.CalculateCharges(finalCost, wallet, TransactionType.Charge));

            //  Charge for reservation here
            var txn = await payment.Charge(new ChargeOptions()
            {
                Amount = finalCost + gatewayCharges,
                FeesOnCustomer = true,
                AdditionalToken = model.AdditionalToken,
                Email = user.Email,
                Name = user.FullName,
                RefLocal = $"BKS-{Guid.NewGuid().ToString("N").Substring(0, 12)}",

                //
                UnitSeatCost = route.Cost,
                TotalSeats = seats.Length,
                GatewayCharges = gatewayCharges

            }, wallet);

            //  Set reservation
            txn.Reservation = reservation;
            reservation.Transactions.Add(txn);

            DB.SaveChanges();

            //
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.Created, Map<ReservationInfo>(reservation)));
        }

        [HttpPut]
        [Route("cancel/{id}")]
        public async Task<IHttpActionResult> CancelReservation(long id)
        {
            var reservation = DB.Reservations.FirstOrDefault(x => !x.Cancelled && x.Id == id);
            if (reservation != null)
            {
                if (Helpers.RouteHelpers.Categorize(reservation.Route.DepartureTime, reservation.Route.ArrivalTime) == Helpers.BusRouteState.Pending)
                {
                    bool refunded = false;
                    reservation.Cancelled = true;

                    //  Check reservation has been successfully paid for
                    var payTxn = reservation.Transactions.FirstOrDefault(x => x.Status == TransactionStatus.Successful && x.Type == TransactionType.Charge);
                    if (payTxn != null && payment.CanRefund)
                    {
                        //  Refund reservation here
                        var txn = await payment.Refund(payTxn);
                        txn.Reservation = reservation;
                        reservation.Transactions.Add(txn);
                        refunded = true;
                    }

                    await DB.SaveChangesAsync();

                    if (refunded)
                    {
                        Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<CoreHub>().Clients.All.OnReservationRefunded(id);
                    }
                }
                else
                {
                    return BadRequest("Reservation cannot be cancelled at this time!");
                }

            }

            return Empty();

        }

        #region Overidden Implementations

        protected override IQueryable OnSearch(IQueryable query, QueryOptions options)
        {
            string keyword = options.SearchKeyword;
            return ((IQueryable<BookReservation>)query).Where(x => x.ReferenceNo.Contains(keyword));
        }

        protected override IQueryable OnOrder(IQueryable query, QueryOptions options)
        {
            return ((IQueryable<BookReservation>)query).OrderBy(x => x.Id);
        }

        #endregion

    }
}