using Microsoft.AspNetCore.Identity;

namespace FocusSpace.Domain.Entities
{
    /// <summary>
    /// Application role — wraps IdentityRole&lt;int&gt; so we can use integer PKs.
    /// </summary>
    public class ApplicationRole : IdentityRole<int>
    {
        public ApplicationRole() { }
        public ApplicationRole(string roleName) : base(roleName) { }
    }
}