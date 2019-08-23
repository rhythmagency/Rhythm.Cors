using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Rhythm.Cors
{
    /// <summary>
    /// Monitors incoming requests and adds CORS headers as appropriate.
    /// </summary>
    /// <remarks>Refer here for more information: https://developer.mozilla.org/en-US/docs/Web/HTTP/CORS</remarks>
    public class CorsModule : IHttpModule
    {
        /// <summary>
        /// Initializes a module and prepares it to handle requests. 
        /// </summary>
        /// <param name="context">
        /// An System.Web.HttpApplication that provides access to the methods, properties, and events common to all
        /// application objects within an ASP.NET application.
        /// </param>
        public void Init(HttpApplication context)
        {
            context.BeginRequest += CorsModule_BeginRequest;
        }

        /// <summary>
        /// Iterates through the list of configured CORS rules and applies the first one which matches.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An object that contains event data.</param>
        private void CorsModule_BeginRequest(object sender, EventArgs e)
        {
            var context = HttpContext.Current;

            var matchedRule = CorsConfiguration.Current.Rules.FirstOrDefault(r => r.IsMatch(context));
            if (matchedRule != null)
            {
                matchedRule.Apply(context);
            }
        }

        /// <summary>
        /// Disposes of the resources used by this module.
        /// </summary>
        public void Dispose() { }
    }
}
