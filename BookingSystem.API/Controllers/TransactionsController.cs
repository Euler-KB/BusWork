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
    [RoutePrefix("api/transactions")]
    public class TransactionsController : BaseController
    {
        [Authorize(Roles = UserRoles.Admin)]
        public IEnumerable<TransactionInfo> Get([FromUri]string type, [FromUri] string status)
        {
            TransactionType[] transactionTypes = type.Split(',').Select(x => (TransactionType)Enum.Parse(typeof(TransactionType), x)).ToArray();
            TransactionStatus[] statuses = status.Split(',').Select(x => (TransactionStatus)Enum.Parse(typeof(TransactionStatus), x)).ToArray();
            return Map<List<TransactionInfo>>(DB.Transactions.Where(x => transactionTypes.Contains(x.Type) && statuses.Contains(x.Status)).AsNoTracking().ToArray());
        }

        [Authorize]
        [Route("{id}/wallet")]
        public IEnumerable<TransactionInfo> GetForWallet(long walletId)
        {
            return Map<IList<TransactionInfo>>(DB.Transactions.Where(x => x.Wallet.User.Id == UserId && x.Wallet.Id == walletId));
        }

        [Authorize]
        [Route("{id}/reservation")]
        public IEnumerable<TransactionInfo> GetForReservation(long id)
        {
            return Map<IList<TransactionInfo>>(DB.Transactions.Where(x => x.Wallet.User.Id == UserId && x.Reservation.Id == id).AsNoTracking());
        }

        [Authorize(Roles = UserRoles.User)]
        [Route("wallet/summary")]
        public IList<WalletSummaryBinding> GetWalletSummary([FromUri]QueryOptions query)
        {
            List<WalletSummaryBinding> response = new List<WalletSummaryBinding>();
            foreach (var wallet in DB.Wallet.Where(x => x.User.Id == UserId))
            {
                TransactionSummary summary = new TransactionSummary()
                {
                    TotalAmountReceived = 0,
                    TotalRefundedAmount = 0
                };

                foreach (var txn in ApplyQueryOptions(DB.Transactions.Where(x => x.Wallet.Id == wallet.Id), query))
                {
                    if (txn.Status == TransactionStatus.Successful && txn.Type == TransactionType.Charge)
                        summary.TotalAmountReceived += txn.IdealAmount;

                    if (txn.Status == TransactionStatus.Successful && txn.Type == TransactionType.Refund)
                        summary.TotalRefundedAmount += txn.IdealAmount;
                }

                response.Add(new WalletSummaryBinding()
                {
                    Summary = summary,
                    Wallet = Map<WalletInfo>(wallet)
                });
            }

            return response;
        }

        [Authorize]
        [Route("summary")]
        public TransactionSummary GetSummary([FromUri]QueryOptions query)
        {
            TransactionSummary summary = new TransactionSummary()
            {
                TotalAmountReceived = 0,
                TotalRefundedAmount = 0
            };

            foreach (var txn in ApplyQueryOptions(DB.Transactions, query))
            {
                if (txn.Status == TransactionStatus.Successful && txn.Type == TransactionType.Charge)
                    summary.TotalAmountReceived += txn.IdealAmount;

                if (txn.Status == TransactionStatus.Successful && txn.Type == TransactionType.Refund)
                    summary.TotalRefundedAmount += txn.IdealAmount;
            }

            return summary;
        }

        #region Overidden Implementations

        protected override IQueryable OnSearch(IQueryable query, QueryOptions options)
        {
            string keyword = options.SearchKeyword;
            return ((IQueryable<Transaction>)query).Where(x => x.RefLocal.Contains(keyword) || x.IdealAmount.ToString().Contains(keyword));
        }

        protected override IQueryable OnOrder(IQueryable query, QueryOptions options)
        {
            return ((IQueryable<Transaction>)query).OrderBy(x => x.Id);
        }

        #endregion
    }
}