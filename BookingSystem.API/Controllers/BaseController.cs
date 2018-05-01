using AutoMapper;
using BookingSystem.API.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace BookingSystem.API.Controllers
{
    public class BaseController : ApiController
    {
        private BookingContext dbContext;

        protected BookingContext DB
        {
            get
            {
                if (dbContext == null)
                    dbContext = new BookingContext();

                return dbContext;
            }
        }

        protected virtual IMapper DefaultMapper
        {
            get
            {
                return AutoMapperConfig.UserMapper;
            }
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

#if DEBUG
            DB.Database.Log = (str) =>
            {
                System.Diagnostics.Debug.WriteLine(str);
            };
#endif

        }

        protected T Map<T>(object content)
        {
            if (User.IsInRole(UserRoles.Admin))
                return AutoMapperConfig.AdminMapper.Map<T>(content);
            else if (User.IsInRole(UserRoles.User))
                return AutoMapperConfig.UserMapper.Map<T>(content);

            //  Use fallback mapper
            return DefaultMapper.Map<T>(content);
        }

        protected IHttpActionResult MapResponse<T>(T content)
        {
            return Ok(Map<T>(content));
        }

        protected string UserId
        {
            get
            {
                return ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
        }

        protected virtual IQueryable OnOrder(IQueryable query, QueryOptions options)
        {
            return query;
        }

        protected virtual IQueryable OnSearch(IQueryable query, QueryOptions options)
        {
            return query;
        }

        protected IHttpActionResult Forbidden(string message = null)
        {
            if (message == null)
                return StatusCode(System.Net.HttpStatusCode.Forbidden);

            return ResponseMessage(Request.CreateErrorResponse(System.Net.HttpStatusCode.Forbidden, message));
        }

        protected IHttpActionResult Create<TMap>(object payload)
        {
            return ResponseMessage(Request.CreateResponse(System.Net.HttpStatusCode.Created, Map<TMap>(payload)));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                dbContext?.Dispose();
            }

            base.Dispose(disposing);
        }

        protected IHttpActionResult Empty()
        {
            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }

        protected virtual IQueryable<T> ApplyQueryOptions<T>(IQueryable<T> query, QueryOptions options) where T : class, ITimestamp
        {
            if (options != null)
            {
                if (options.CreatedAfter != null)
                {
                    query = query.Where(x => x.DateCreated > options.CreatedAfter);
                }

                if (options.CreatedBefore != null)
                {
                    query = query.Where(x => x.DateCreated < options.CreatedBefore);
                }

                if (options.FromRange != null && options.ToRange != null)
                {
                    query = query.Where(x => options.FromRange >= x.DateCreated && options.ToRange <= x.DateCreated);
                }

                if (options.SearchKeyword != null)
                {
                    query = (IQueryable<T>)OnSearch(query, options);
                }

                if (options.Limit != null)
                {
                    query = (IQueryable<T>)OnOrder(query, options);
                }

                //
                if (options.Offset != null)
                {
                    query = query.Skip(options.Offset.Value);
                }

                if (options.Limit != null)
                {
                    query = query.Take(options.Limit.Value);
                }
            }

            return query;
        }

    }
}