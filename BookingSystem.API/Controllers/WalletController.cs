using BookingSystem.API.Models;
using BookingSystem.API.Models.DTO;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BookingSystem.API.Controllers
{
    [RoutePrefix("api/wallet")]
    [Authorize]
    public class WalletController : BaseController
    {
        // GET api/<controller>
        public IEnumerable<WalletInfo> Get([FromUri]QueryOptions query)
        {
            return Map<IEnumerable<WalletInfo>>(ApplyQueryOptions(DB.Wallet.Where(x => x.User.Id == UserId), query).AsNoTracking().ToArray());
        }

        public IHttpActionResult Put(long id, [FromBody]EditWalletInfo model)
        {
            var wallet = DB.Wallet.FirstOrDefault(x => x.User.Id == UserId && x.Id == id);
            if (wallet == null)
                return NotFound();

            Helpers.ObjectMapper.CopyPropertiesTo(model, wallet, Helpers.ObjectMapper.UpdateFlag.DeferUpdateOnNull | Helpers.ObjectMapper.UpdateFlag.DenoteEmptyStringsAsNull);
            DB.SaveChanges();
            return Ok(Map<WalletInfo>(wallet));
        }

        public IHttpActionResult Post([FromBody]CreateWalletInfo model)
        {
            if (DB.Wallet.Any(x => x.User.Id == UserId && x.Provider == model.Provider && x.Value == model.Value))
            {
                return BadRequest("Wallet already exists");
            }

            var user = DB.Users.Find(UserId);
            var wallet = new UserWallet()
            {
                Provider = model.Provider,
                Value = model.Value,
                User = user
            };

            DB.Wallet.Add(wallet);
            DB.SaveChanges();

            return Create<WalletInfo>(wallet);
        }

        public void Delete(long id)
        {
            var wallet = DB.Wallet.FirstOrDefault(x => x.User.Id == UserId && x.Id == id);
            if (wallet != null)
            {
                wallet.IsSoftDeleted = true;
                DB.SaveChanges();
            }
        }

        [Authorize(Roles = UserRoles.Admin)]
        [Route("user/{id}")]
        public IEnumerable<WalletInfo> Get(string id)
        {
            return Map<IEnumerable<WalletInfo>>(DB.Wallet.Where(x => x.User.Id == id).ToArray());
        }

        [Authorize(Roles = UserRoles.Admin)]
        public IHttpActionResult Get(long id)
        {
            var wallet = DB.Wallet.FirstOrDefault(x => x.Id == id);
            if (wallet == null)
                return NotFound();

            return Ok(Map<WalletInfo>(wallet));
        }

        #region Overidden Implementations

        protected override IQueryable OnSearch(IQueryable query, QueryOptions options)
        {
            string keyword = options.SearchKeyword;
            return ((IQueryable<UserWallet>)query).Where(x => x.Provider.Contains(keyword) || x.Value.Contains(keyword));
        }

        protected override IQueryable OnOrder(IQueryable query, QueryOptions options)
        {
            return ((IQueryable<UserWallet>)query).OrderBy(x => x.Id);
        }

        #endregion
    }
}