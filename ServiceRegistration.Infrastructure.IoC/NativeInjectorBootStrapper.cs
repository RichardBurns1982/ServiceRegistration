using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ServiceRegistration.Infrastructure.IoC
{
    public static class NativeInjectorBootStrapper
    {
        #region Fields

        private static readonly Lazy<IEnumerable<Assembly>> _assemblies;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes the <see cref="NativeInjectorBootStrapper"/> class.
        /// </summary>
        static NativeInjectorBootStrapper()
        {
            _assemblies = new Lazy<IEnumerable<Assembly>>(GetReferencedAssemblies);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the referenced assemblies.
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<Assembly> GetReferencedAssemblies()
        {
            var assemblies = new List<Assembly>();
            var dependencies = DependencyContext.Default.RuntimeLibraries;
            foreach (var library in dependencies)
            {
                if (library.Name.StartsWith(nameof(ServiceRegistration)))
                {
                    var assembly = Assembly.Load(new AssemblyName(library.Name));
                    assemblies.Add(assembly);
                }
            }
            return assemblies;
        }

        /// <summary>
        /// Registers the services.
        /// </summary>
        /// <param name="services">The services.</param>
        public static void RegisterServices(IServiceCollection services)
        {
            // add components by the IDependency interfaces they expose
            var referencedAssemblies = _assemblies.Value;
            var serviceTypes = referencedAssemblies
                .SelectMany(x => x.ExportedTypes)
                .Where(t => typeof(IInstanceDependency).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);
            foreach (var serviceType in serviceTypes)
            {
                var ss = serviceType.GetInterfaces();
                var interfaceTypes = (from s in serviceType.GetInterfaces()
                                      where s != typeof(IInstanceDependency) &&
                                            s != typeof(ISingletonDependency) &&
                                            s != typeof(IScopedDependency) &&
                                            s != typeof(ITransientDependency) &&
                                            typeof(IInstanceDependency).IsAssignableFrom(s)
                                      select s).ToList();

                if (interfaceTypes.Count == 0)
                {
                    throw new NotSupportedException($"No dependency interfaces found on: {serviceType.Name}");
                }

                var truthList = new List<bool>
                    {
                        interfaceTypes.Any(x => typeof(ISingletonDependency).IsAssignableFrom(x)),
                        interfaceTypes.Any(x => typeof(IScopedDependency).IsAssignableFrom(x)),
                        interfaceTypes.Any(x => typeof(ITransientDependency).IsAssignableFrom(x))
                    };

                if (truthList.Count(x => x) > 1)
                {
                    throw new NotSupportedException($"Cannot have multiple dependency setting on: {serviceType.Name}");
                }

                var firstInterface = interfaceTypes.Skip(0).Take(1).FirstOrDefault();
                var secondInterfaces = interfaceTypes.Skip(1).ToList();

                if (typeof(ISingletonDependency).IsAssignableFrom(firstInterface))
                {
                    services.AddSingleton(firstInterface, serviceType);
                    secondInterfaces.ForEach(secondInterface => services.AddSingleton(secondInterface, x => x.GetService(firstInterface)));
                }
                else if (typeof(IScopedDependency).IsAssignableFrom(firstInterface))
                {
                    services.AddScoped(firstInterface, serviceType);
                    secondInterfaces.ForEach(secondInterface => services.AddScoped(secondInterface, x => x.GetService(firstInterface)));

                }
                else if (typeof(ITransientDependency).IsAssignableFrom(firstInterface))
                {
                    services.AddTransient(firstInterface, serviceType);
                    secondInterfaces.ForEach(secondInterface => services.AddTransient(secondInterface, x => x.GetService(firstInterface)));
                }
                else
                {
                    throw new NotSupportedException($"Unrecognised dependency injection type on: {serviceType.Name}");
                }
            }
        }

        public static IEnumerable<Assembly> GetAssemblies()
        {
            return _assemblies.Value;
        }

        #endregion
    }
}
