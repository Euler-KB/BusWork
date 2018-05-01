using BookingSystem.API.Helpers;
using BookingSystem.API.Models;
using BookingSystem.API.Models.DTO;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace BookingSystem.API.Controllers
{
    [RoutePrefix("api/buses")]
    [Authorize]
    public class BusesController : BaseController
    {
        protected IQueryable<Bus> Buses => DB.Buses.Include(x => x.ProfileImage);

        // GET api/<controller>
        public async Task<IEnumerable<BusInfo>> Get([FromUri]QueryOptions queryOptions)
        {
            return Map<IList<BusInfo>>(await ApplyQueryOptions(Buses, queryOptions)
                .AsNoTracking().ToArrayAsync());
        }

        // GET api/<controller>/5
        [Route("{id}")]
        public IHttpActionResult Get(long id)
        {
            var bus = Buses.FirstOrDefault(x => x.Id == id);
            if (bus == null)
                return NotFound();

            return Ok(Map<BusInfo>(bus));
        }

        // POST api/<controller>
        public IHttpActionResult Post([FromBody]CreateBusInfo value)
        {
            if (DB.Buses.Any(x => x.Name == value.Name && x.Model == value.Model))
            {
                return BadRequest("Bus already exists");
            }

            var bus = new Bus()
            {
                Name = value.Name,
                Seats = value.Seats,
                Model = value.Model
            };

            DB.Buses.Add(bus);
            DB.SaveChanges();
            return Create<BusInfo>(bus);
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]EditBusInfo value)
        {
            var bus = DB.Buses.FirstOrDefault(x => x.Id == id);
            if (bus == null)
            {
                ObjectMapper.CopyPropertiesTo(value, bus, ObjectMapper.UpdateFlag.DeferUpdateOnNull | ObjectMapper.UpdateFlag.DenoteEmptyStringsAsNull);
                DB.SaveChanges();
            }
        }

        // DELETE api/<controller>/5
        [Authorize(Roles = UserRoles.Admin)]
        [Route("{id}")]
        [HttpDelete]
        public void Delete([FromUri]long id)
        {
            var bus = DB.Buses.Find(id);
            if (bus != null)
            {
                bus.IsSoftDeleted = true;
                DB.SaveChanges();
            }
        }

        #region Photo Upload

        [AcceptVerbs("PUT", "POST")]
        [Route("{id}/photo")]
        [Authorize]
        public async Task<IHttpActionResult> UploadPhoto(long id)
        {
            var bus = DB.Buses.Include(x => x.ProfileImage).FirstOrDefault(x => x.Id == id);
            if (bus == null)
                return NotFound();

            //
            var stream = await Request.Content.ReadAsStreamAsync();
            string mimeType = Request.Content.Headers.ContentType.MediaType;
            string name = Guid.NewGuid().ToString("N");

            if (bus.ProfileImage == null)
            {
                var response = await DiskMediaStore.Instance.SaveMedia(name, mimeType, stream);

                bus.ProfileImage = new Media()
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
                var profile = bus.ProfileImage;
                var response = await DiskMediaStore.Instance.UpdateMedia(profile.Name, name, mimeType, stream);
                if (profile.MimeType != mimeType)
                {
                    profile.MimeType = mimeType;
                }

                profile.LastUpdated = DateTime.UtcNow;
                profile.Name = name;
                profile.Path = response.Path;
            }

            DB.SaveChanges();

            return Ok(bus.ProfileImage);
        }

        [HttpDelete]
        [Route("{id}/photo")]
        [Authorize]
        public async Task RemovePhoto()
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

        #endregion



        #region Overidden Implementations

        protected override IQueryable OnSearch(IQueryable query, QueryOptions options)
        {
            string keyword = options.SearchKeyword;
            return ((IQueryable<Bus>)query).Where(x => x.Name.Contains(keyword) || x.Model.Contains(keyword));
        }

        protected override IQueryable OnOrder(IQueryable query, QueryOptions options)
        {
            return ((IQueryable<Bus>)query).OrderBy(x => x.Id);
        }

        #endregion

    }
}