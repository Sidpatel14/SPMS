using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SPMS.Models;

public  class MyDbContext : DbContext
{


    public MyDbContext(DbContextOptions<MyDbContext> options): base(options){}

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Application> Applications { get; set; }
}
