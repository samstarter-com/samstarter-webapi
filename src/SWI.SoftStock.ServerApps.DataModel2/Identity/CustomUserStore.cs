using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SWI.SoftStock.ServerApps.DataModel2.Identity.Models;

namespace SWI.SoftStock.ServerApps.DataModel2.Identity
{
    public class CustomUserStore : UserStore<User, CustomRole, DbContext, Guid>
    {
        public CustomUserStore(DbContext context) : base(context)
        {
        }
    }
}
