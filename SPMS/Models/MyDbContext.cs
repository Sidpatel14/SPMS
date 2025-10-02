using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;

namespace SPMS.Models;

public  class MyDbContext : DbContext
{

    public MyDbContext(DbContextOptions<MyDbContext> options): base(options){}
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Application> Applications { get; set; }
    public virtual DbSet<Document> Documents { get; set; }
    public virtual DbSet<Country> Country { get; set; }
    public virtual DbSet<State> State { get; set; }
}
