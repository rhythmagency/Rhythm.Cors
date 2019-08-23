using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;

namespace Rhythm.Cors
{
    /// <summary>
    /// Provides support and shared functionality for CORS configuration.
    /// </summary>
    public static class CorsConfiguration
    {
        public static CorsConfigurationSection Current => ConfigurationManager.GetSection("corsConfiguration") as CorsConfigurationSection;

        internal static readonly Uri EmptyUri = new Uri("http://example.com");
    }

    /// <summary>
    /// Represents the CORS configuration section within a configuration file.
    /// </summary>
    public class CorsConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("rules")]
        public CorsConfigurationRuleCollection Rules
        {
            get { return (CorsConfigurationRuleCollection)this["rules"]; }
        }
    }

    /// <summary>
    /// Represents a configuration element containing a collection of CORS rules.
    /// </summary>
    [ConfigurationCollection(typeof(CorsConfigurationRule))]
    public class CorsConfigurationRuleCollection : ConfigurationElementCollection, IEnumerable<CorsConfigurationRule>
    {
        /// <summary>
        /// Creates a new <see cref="CorsConfigurationRule"/>.
        /// </summary>
        /// <returns>A newly created <see cref="CorsConfigurationRule"/>.</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new CorsConfigurationRule();
        }

        /// <summary>
        /// Gets the element key for a specified configuration element.
        /// </summary>
        /// <param name="element">The <see cref="ConfigurationElement"/> to return the key for.</param>
        /// <returns>An <see cref="System.Object"/> that acts as the key for the specified <see cref="ConfigurationElement"/>.</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((CorsConfigurationRule)element).Domain;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A <see cref="IEnumerator<CorsConfigurationRule>"/> that can be used to iterate through the collection.</returns>
        IEnumerator<CorsConfigurationRule> IEnumerable<CorsConfigurationRule>.GetEnumerator()
        {
            foreach (var item in (IEnumerable)this)
                yield return item as CorsConfigurationRule;
        }
    }

    /// <summary>
    /// Represents a single CORS rule element within a configuration file.
    /// </summary>
    public class CorsConfigurationRule : ConfigurationElement
    {
        #region rule behavior properties

        /// <summary>
        /// The domain which this rule targets. Allows the value "*" to target all incoming requests.
        /// </summary>
        [ConfigurationProperty("domain", IsRequired = true)]
        public string Domain
        {
            get { return (string)this["domain"]; }
            set { this["domain"] = value; }
        }

        /// <summary>
        /// The policy which this rule applies. "ALLOW" is the only acceptable value.
        /// </summary>
        [ConfigurationProperty("policy", IsRequired = true)]
        public string Policy
        {
            get { return (string)this["policy"]; }
            set { this["policy"] = value; }
        }

        /// <summary>
        /// Indicates whether or not the rule should allow requests from insecure sources.
        /// </summary>
        [ConfigurationProperty("require-https", IsRequired = false)]
        public bool RequireHttps
        {
            get { return (bool)(this["require-https"] ?? false); }
            set { this["require-https"] = value; }
        }

        #endregion

        #region CORS header values

        /// <summary>
        /// A comma-separated list of header names which the browser is permitted to access.
        /// </summary>
        [ConfigurationProperty("expose-headers", IsRequired = false)]
        public string ExposeHeaders
        {
            get { return (string)this["expose-headers"]; }
            set { this["expose-headers"] = value; }
        }

        /// <summary>
        /// Preflight requests only; indicates how long a preflight request may be cached.
        /// </summary>
        [ConfigurationProperty("max-age", IsRequired = false)]
        public int? MaxAge
        {
            get { return (int?)this["max-age"]; }
            set { this["max-age"] = value; }
        }

        /// <summary>
        /// Preflight requests only; indicates whether or not the following resource request may provide credentials.
        /// </summary>
        [ConfigurationProperty("allow-credentials", IsRequired = false)]
        public bool? AllowCredentials
        {
            get { return (bool?)this["allow-credentials"]; }
            set { this["allow-credentials"] = value; }
        }

        /// <summary>
        /// Preflight requests only; indicates which methods (e.g. GET, POST, etc.) may be used for the following resource request.
        /// </summary>
        [ConfigurationProperty("allow-methods", IsRequired = false)]
        public string AllowMethods
        {
            get { return (string)this["allow-methods"]; }
            set { this["allow-methods"] = value; }
        }

        /// <summary>
        /// Preflight requests only; indicates which header names may be provided for the following resource request.
        /// </summary>
        [ConfigurationProperty("allow-headers", IsRequired = false)]
        public string AllowHeaders
        {
            get { return (string)this["allow-headers"]; }
            set { this["allow-headers"] = value; }
        }

        #endregion

        /// <summary>
        /// Indicates whether or not the request for a particular HttpContext matches this rule.
        /// </summary>
        /// <param name="context">The HttpContext to examine.</param>
        /// <returns>True if the context matches this rule; False otherwise.</returns>
        public bool IsMatch(HttpContext context)
        {
            if (context.Items["Rhythm.Cors.OriginUrl"] == null)
            {
                if (Uri.TryCreate(context.Request.Headers["Origin"], UriKind.Absolute, out var uri))
                    context.Items["Rhythm.Cors.OriginUrl"] = uri;
                else
                    context.Items["Rhythm.Cors.OriginUrl"] = CorsConfiguration.EmptyUri;
            }

            var origin = ((Uri)context.Items["Rhythm.Cors.OriginUrl"]);

            if (Domain == "*")
                return true;
            else if (RequireHttps && origin.Scheme != "https")
                return false;
            else if (origin != CorsConfiguration.EmptyUri)
                return String.Equals(this.Domain, ((Uri)context.Items["Rhythm.Cors.OriginUrl"]).Host, StringComparison.InvariantCultureIgnoreCase);
            else
                return false;
        }

        /// <summary>
        /// Applies this rule's properties to the provided HttpContext. If the context indicates that this is a
        /// preflight request, the response will be immediately completed and returned.
        /// </summary>
        /// <param name="context">The HttpContext to alter.</param>
        public void Apply(HttpContext context)
        {
            var origin = ((Uri)context.Items["Rhythm.Cors.OriginUrl"]);
            if (this.Policy == "ALLOW")
            {
                // set the Allow-Origin header appropriately

                if (Domain == "*")
                {
                    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                }
                else
                {
                    context.Response.Headers.Add("Access-Control-Allow-Origin", origin.OriginalString);
                    var varyHeaderValue = context.Response.Headers["Vary"];
                    if (!String.IsNullOrWhiteSpace(varyHeaderValue))
                        varyHeaderValue += ", ";
                    else
                        varyHeaderValue = "";
                    varyHeaderValue += "Origin";
                    context.Response.Headers["Vary"] = varyHeaderValue;
                }

                // add additional CORS header properties

                if (!String.IsNullOrWhiteSpace(ExposeHeaders))
                    context.Response.Headers["Access-Control-Expose-Headers"] = ExposeHeaders;
                
                    if (AllowCredentials.HasValue)
                        context.Response.Headers["Access-Control-Allow-Credentials"] = AllowCredentials.Value.ToString().ToLower();

                // if this is a preflight request, return the response immediately

                if (context.Request.HttpMethod == "OPTIONS")
                {
                    var requestMethod = context.Request.Headers["Access-Control-Request-Method"];
                    var requestHeaders = context.Request.Headers["Access-Control-Request-Headers"];

                    if (!String.IsNullOrWhiteSpace(AllowMethods))
                        context.Response.Headers["Access-Control-Allow-Methods"] = AllowMethods;

                    if (!String.IsNullOrWhiteSpace(AllowHeaders))
                        context.Response.Headers["Access-Control-Allow-Headers"] = AllowHeaders;

                    if (MaxAge.HasValue)
                        context.Response.Headers["Access-Control-Max-Age"] = MaxAge.ToString();

                    context.Response.StatusCode = 204;
                    context.Response.StatusDescription = "NO CONTENT";
                    context.Response.End();
                }
            }
        }
    }
}
