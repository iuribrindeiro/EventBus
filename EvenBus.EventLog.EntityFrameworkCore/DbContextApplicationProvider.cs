﻿using Microsoft.EntityFrameworkCore;

namespace EventBus.EventLog.EntityFrameworkCore
{
    internal class DbContextApplicationProvider : IDbContextApplicationProvider
    {
        public DbContextApplicationProvider(DbContext dbContext) 
            => DbContext = dbContext;

        public DbContext DbContext { get; }
    }
}