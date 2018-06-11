using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using AutoMapper;
using BookingSystem.API.Models.DTO;
using System.Security.Claims;
using BookingSystem.API.Models;

namespace BookingSystem.API
{
    public class AutoMapperConfig
    {
        public static IMapper UserMapper { get; private set; }

        public static IMapper AdminMapper { get; private set; }

        static AccountType ToAccountType(string type)
        {
            switch (type)
            {
                case "Admin":
                    return AccountType.Administrator;
                case "User":
                    return AccountType.User;
            }

            throw new InvalidOperationException();
        }

        static ReservationCategory GetCategory(BookReservation reservation)
        {
            if (reservation.Cancelled)
                return ReservationCategory.Cancelled;

            switch (Helpers.RouteHelpers.Categorize(reservation.Route.DepartureTime, reservation.Route.ArrivalTime))
            {
                case Helpers.BusRouteState.Active:
                    return ReservationCategory.Active;
                case Helpers.BusRouteState.Pending:
                    return ReservationCategory.Pending;
                case Helpers.BusRouteState.Used:
                    return ReservationCategory.Completed;
            }

            throw new InvalidOperationException();
        }

        static PayStatus GetPayStatus(BookReservation reservation)
        {
            if (reservation.Transactions.Any(x => x.Type == TransactionType.Charge && x.DateCompleted == null))
                return PayStatus.InitiatePay;

            if (reservation.Transactions.Any(x => x.Type == TransactionType.Refund && x.DateCompleted == null))
                return PayStatus.InitiateRefund;

            if (reservation.Transactions.Any(x => x.Type == TransactionType.Charge && x.Status == TransactionStatus.Successful))
                return PayStatus.Paid;

            if (reservation.Transactions.Any(x => x.Type == TransactionType.Refund && x.Status == TransactionStatus.Successful))
                return PayStatus.Refunded;

            return PayStatus.Failed;
        }

        public static void Config(HttpConfiguration httpConfig)
        {
            UserMapper = new MapperConfiguration(config => InitMapper(config, false)).CreateMapper();
            AdminMapper = new MapperConfiguration(config => InitMapper(config, true)).CreateMapper();
        }

        static void InitMapper(IMapperConfigurationExpression config, bool detailed)
        {
            config.CreateMap<Models.Bus, BusInfo>().ForMember(x => x.Photo, x => x.MapFrom(t => t.ProfileImage));
            config.CreateMap<Models.AppUser, UserInfo>().ForMember(x => x.AccountType, t => t.ResolveUsing(x => ToAccountType(x.Claims.First(j => j.ClaimType == ClaimTypes.Role).Value)));

            var reservationMap = config.CreateMap<Models.BookReservation, ReservationInfo>()
                    .ForMember(x => x.Category, x => x.ResolveUsing(t => GetCategory(t)))
                    .ForMember(x => x.Bus, x => x.MapFrom(t => t.Route.Bus))
                    .ForMember(x => x.PayStatus, x => x.ResolveUsing(t => GetPayStatus(t)))
                    .ForMember(x => x.Cost, x => x.ResolveUsing(t => t.Seats.Split(',').Length * t.Route.Cost));

            if (detailed)
            {
                reservationMap.ForMember(x => x.UserId, x => x.ResolveUsing(t => t.User.Id))
                    .ForMember(x => x.UserFullName, x => x.ResolveUsing(t => t.User.FullName));
            }


            config.CreateMap<BusRoute, RouteInfo>();
            config.CreateMap<UserFeedBack, FeedbackInfoEx>();
            config.CreateMap<UserFeedBack, FeedbackInfo>();
            config.CreateMap<Transaction, TransactionInfo>();
            config.CreateMap<UserWallet, WalletInfo>();
            config.CreateMap<Media, MediaInfo>()
                .ForMember(x => x.Uri, x => x.ResolveUsing(t => $"image/stream/{t.Name}"))
                .ForMember(x => x.ThumbnailUri, x => x.ResolveUsing(t => $"image/thumbnail/{t.Name}"));
        }
    }
}