using BookingSystem.API.Helpers;
using BookingSystem.API.Models;
using BookingSystem.API.Models.DTO;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Z.EntityFramework.Plus;

namespace BookingSystem.API.Controllers
{
    [RoutePrefix("api/routes")]
    [Authorize]
    public class RoutesController : BaseController
    {
        [HttpGet]
        [Route("bus/{id}")]
        public IEnumerable<RouteInfo> GetForBus(long id, [FromUri]QueryOptions options)
        {
            return Map<IList<RouteInfo>>(ApplyQueryOptions(DB.Routes.Where(x => x.Bus.Id == id), options).AsNoTracking().ToArray());
        }

        // GET api/<controller>/5
        public IHttpActionResult Get(long id)
        {
            var route = DB.Routes.Find(id);
            if (route == null)
                return NotFound();

            return Ok(Map<RouteInfo>(route));
        }

        [HttpGet]
        [Route("{id}/summary")]
        public IHttpActionResult GetRouteSummary(long id)
        {
            if (DB.Routes.Any(x => x.Id == id))
            {
                RouteSummary summary = new RouteSummary()
                {
                    TotalReservations = DB.Reservations.Where(x => x.Route.Id == id && !x.Cancelled).Count()
                };

                //
                List<int> bookedSeats = new List<int>();
                foreach (var seat in DB.Reservations.Where(x => !x.Cancelled).Select(t => t.Seats))
                    bookedSeats.AddRange(seat.Split(',').Select(x => int.Parse(x)));

                summary.BookedSeats = bookedSeats.ToArray();

                return MapResponse(summary);
            }
            else
            {
                return NotFound();
            }
        }

        // POST api/<controller>
        [Authorize(Roles = UserRoles.Admin)]
        [Route("bus/{id}")]
        public IHttpActionResult Post(long id, [FromBody]CreateRouteInfo model)
        {
            var bus = DB.Buses.IncludeFilter(x => x.Routes.Where(t => !t.IsSoftDeleted)).FirstOrDefault(x => x.Id == id);
            if (bus == null)
                return NotFound();

            if (model.DepartureTime <= DateTime.Now)
            {
                return BadRequest("Invalid departure time. The departure time is not realistic. Date must be greater than the current time!");
            }

            if (model.DepartureTime > model.ArrivalTime)
            {
                return BadRequest("The departure time cannot be greater than the departure time!");
            }

            //  Check whether bus is idle within the time range
            if (bus.Routes.Any(x => DateHelper.IsBetween(x.DepartureTime, x.ArrivalTime, model.DepartureTime)))
            {
                return BadRequest("The route cannot be created for the bus within the specified range. Possible reasons include the bus will be active or busy during the time range given!");
            }

            var route = new BusRoute()
            {
                ArrivalTime = model.ArrivalTime,
                Comments = model.Comments,
                Bus = bus,
                Cost = model.Cost,
                DepartureTime = model.DepartureTime,
                Destination = model.Destination,
                DestinationLat = model.DestinationLat,
                DestinationLng = model.DestinationLng,
                From = model.From,
                FromLat = model.FromLat,
                FromLng = model.FromLng,
            };

            DB.Routes.Add(route);
            DB.SaveChanges();
            return Create<RouteInfo>(route);
        }

        // PUT api/<controller>/5
        public void Put(long id, [FromBody]EditRouteInfo model)
        {
            var route = DB.Routes.FirstOrDefault(x => !x.IsSoftDeleted && x.Id == id);
            if (route != null)
            {
                //  Route Completed ?
                switch (Helpers.RouteHelpers.Categorize(route.DepartureTime, route.ArrivalTime))
                {
                    case BusRouteState.Used:
                    case BusRouteState.Active:

                        if (model.DepartureTime != null || model.ArrivalTime != null ||
                            model.From != null || model.Destination != null ||
                            model.FromLat != null || model.FromLng != null ||
                            model.DestinationLat != null || model.DestinationLng != null)
                            throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Cannot modify the route in its current state!"));

                        break;
                }

                ObjectMapper.CopyPropertiesTo(model, route, ObjectMapper.UpdateFlag.DeferUpdateOnNull);
                DB.SaveChanges();
            }
        }

        [HttpGet]
        [Authorize]
        [Route("{id}/available/seats")]
        public int[] GetFreeSeats(long id)
        {
            var route = DB.Routes.Select(x => new { x.Id, x.Bus.Seats }).FirstOrDefault(x => x.Id == id);
            if (route == null)
                return new int[] { };

            //
            var seats = route.Seats.Split('-');
            int firstSeat = int.Parse(seats[0]);
            int lastSeat = int.Parse(seats[1]);
            var range = Enumerable.Range(firstSeat, (lastSeat - firstSeat) + 1).ToList();
            foreach (var item in DB.Reservations.Where(x => x.RouteId == id && !x.Cancelled).Select(x => new { x.Seats }))
            {
                var values = item.Seats.Split(',').Select(x => int.Parse(x)).ToArray();
                range.RemoveAll(x => values.Contains(x));
            }

            return range.ToArray();
        }

        // DELETE api/<controller>/5
        [Authorize(Roles = UserRoles.Admin)]
        public void Delete(long id)
        {
            var route = DB.Routes.Find(id);
            if (route != null)
            {
                route.IsSoftDeleted = true;
                DB.SaveChanges();
            }
        }

        #region Overidden Implementations

        protected override IQueryable OnSearch(IQueryable query, QueryOptions options)
        {
            string keyword = options.SearchKeyword;
            return ((IQueryable<BusRoute>)query).Where(x => x.From.Contains(keyword) || x.Destination.Contains(keyword));
        }

        protected override IQueryable OnOrder(IQueryable query, QueryOptions options)
        {
            return ((IQueryable<BusRoute>)query).OrderBy(x => x.Id);
        }

        #endregion
    }
}