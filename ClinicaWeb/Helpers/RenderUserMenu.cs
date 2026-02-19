using Microsoft.AspNetCore.Http;

namespace ClinicaWeb.Helpers
{
    public static class HttpContextHelper
    {
        public static IHttpContextAccessor Accessor { get; set; }

        // Equivalente a HttpContext.Current
        public static HttpContext Current => Accessor?.HttpContext;
    }
}
