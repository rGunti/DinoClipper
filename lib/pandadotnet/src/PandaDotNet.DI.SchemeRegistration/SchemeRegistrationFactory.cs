using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PandaDotNet.DI.Configuration;

namespace PandaDotNet.DI.SchemeRegistration
{
    /// <summary>
    /// A function that is called to register dependencies for a certain scheme.
    /// Used by <see cref="SchemeRegistrationFactory"/>.
    /// </summary>
    public delegate void SchemeRegistrationDelegate(IServiceCollection services, IConfiguration config, string connectionString);

    /// <summary>
    /// A factory object that is used to register dependencies based on a scheme in a connection string.
    /// </summary>
    public abstract class SchemeRegistrationFactory
    {
        private readonly Dictionary<string, SchemeRegistrationDelegate> _factories = new();

        /// <summary>
        /// Add a factory method for a connection string scheme.
        /// </summary>
        /// <param name="scheme">A scheme</param>
        /// <param name="factory">A factory method that adds all dependencies for the given scheme</param>
        protected void AddFactory(string scheme, SchemeRegistrationDelegate factory)
        {
            _factories.Add(scheme, factory);
        }

        /// <summary>
        /// Runs the registration for a given connection string.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <param name="connectionStringName"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void RunRegistration(
            IServiceCollection services,
            IConfiguration config,
            string connectionStringName)
        {
            string connectionString = config.GetConnectionString(connectionStringName, false);
            string scheme = connectionString.GetScheme();
            if (_factories.ContainsKey(scheme))
            {
                _factories[scheme](services, config, connectionString);
            }
            else
            {
                throw new NotImplementedException($"The scheme {scheme} is not supported!");
            }
        }
    }
}