

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskAspNet.Data.EntityIdentity;

namespace TaskAspNet.Data.Context;

public class UserDbContext(DbContextOptions<UserDbContext> options) : IdentityDbContext<AppUser>(options)
{
    
}
