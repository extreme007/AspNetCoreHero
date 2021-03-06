﻿using AspNetCoreHero.Application.Constants;
using AspNetCoreHero.Application.Interfaces.Contexts;
using AspNetCoreHero.Application.Interfaces.Shared;
using AspNetCoreHero.Domain.Common;
using AspNetCoreHero.Domain.Entities;
using AspNetCoreHero.Infrastructure.Persistence.Helpers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreHero.Infrastructure.Persistence.Contexts
{
    public class ApplicationContext : AuditableContext, IApplicationDbContext
    {
        private readonly IDateTimeService _dateTime;
        private readonly IAuthenticatedUserService _authenticatedUser;

        public ApplicationContext(DbContextOptions<ApplicationContext> options, IDateTimeService dateTime, IAuthenticatedUserService authenticatedUser) : base(options)
        {
            //ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
            _dateTime = dateTime;
            _authenticatedUser = authenticatedUser;
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategory { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<ArticleCategory> ArticleCategory { get; set; }
        public bool HasChanges => ChangeTracker.HasChanges();

        public IDbConnection Connection => Database.GetDbConnection();

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            //foreach (var entry in ChangeTracker.Entries<AuditableEntityBase>().ToList())
            //{               
            //    var entityType = entry.Entity.GetType().Name;
            //    switch (entry.State)
            //    {
            //        case EntityState.Added:
            //            entry.Entity.Created = _dateTime.Now;
            //            entry.Entity.CreatedBy = _authenticatedUser.UserId;                        
            //            try
            //            {
            //                await ActivityLogs.AddAsync(new ActivityLog
            //                {
            //                    Action = EntityActionType.Added,
            //                    DateTime = _dateTime.Now,
            //                    UserId = _authenticatedUser.UserId,
            //                    UserName = _authenticatedUser.Username,                               
            //                    CurrentValue = entry.CurrentValues?.ToObject() == null? null : JsonConvert.SerializeObject(entry.CurrentValues.ToObject()),
            //                    Entity = entityType,
            //                    EntityId = "",
            //                    IpAddress = IPHelper.GetIpAddress()
            //                });
            //            }
            //            catch{}

            //            break;
            //        case EntityState.Modified:                                              
            //            try
            //            { 
            //                if (!entry.Entity.IsDeleted)
            //                {
            //                    entry.Entity.LastModified = _dateTime.Now;
            //                    entry.Entity.LastModifiedBy = _authenticatedUser.UserId;
            //                }                           

            //                await ActivityLogs.AddAsync(new ActivityLog
            //                {
            //                    Action = entry.Entity.IsDeleted ? EntityActionType.Deleted : EntityActionType.Modified,
            //                    DateTime = _dateTime.Now,
            //                    UserId = _authenticatedUser.UserId,
            //                    UserName = _authenticatedUser.Username,
            //                    OriginalValue = entry.OriginalValues?.ToObject() == null ? null : JsonConvert.SerializeObject(entry.OriginalValues.ToObject()),
            //                    CurrentValue = entry.CurrentValues?.ToObject() == null || entry.Entity.IsDeleted ? null : JsonConvert.SerializeObject(entry.CurrentValues.ToObject()),
            //                    Entity = entityType,
            //                    EntityId = entry.Entity.Id.ToString(),
            //                    IpAddress = IPHelper.GetIpAddress()
            //                });

            //            }
            //            catch { }
            //            break;
            //        case EntityState.Deleted:
            //            entry.Entity.DeletionTime = _dateTime.Now;
            //            entry.Entity.DeletedBy = _authenticatedUser.UserId;
            //            entry.Entity.IsDeleted = true;
            //            try
            //            {
            //                await ActivityLogs.AddAsync(new ActivityLog
            //                {
            //                    Action = EntityActionType.Deleted,
            //                    DateTime = _dateTime.Now,
            //                    UserId = _authenticatedUser.UserId,
            //                    UserName = _authenticatedUser.Username,
            //                    OriginalValue = entry.OriginalValues?.ToObject() == null ? null : JsonConvert.SerializeObject(entry.OriginalValues.ToObject()),
            //                    Entity = entityType,
            //                    EntityId = entry.Entity.Id.ToString(),
            //                    IpAddress = IPHelper.GetIpAddress()
            //                });
            //            }
            //            catch { }
            //            break;
            //    }
            //}
            //return await base.SaveChangesAsync(cancellationToken);

            foreach (var entry in ChangeTracker.Entries<AuditableBaseEntity>().ToList())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedOn = _dateTime.Now;
                        entry.Entity.CreatedBy = _authenticatedUser.UserId;
                        break;

                    case EntityState.Modified:
                        entry.Entity.LastModifiedOn = _dateTime.Now;
                        entry.Entity.LastModifiedBy = _authenticatedUser.UserId;
                        break;
                }
            }
            if (_authenticatedUser.UserId == null)
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            else
            {
                return await base.SaveChangesAsync(_authenticatedUser.UserId);
            }
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            //All Decimals will have 18,2 Range
            foreach (var property in builder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18,2)");
            }
            base.OnModelCreating(builder);
        }
    }
}
