using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Utilities.IOC
{
	public interface IInjectConfig
    {
		IServiceProvider GetContainer(IServiceCollection services);
        void ConfigureServiceContainer();
		TInterface ConfigureServiceContainer<TInterface>();
        void ConfigureServiceCollection(IServiceCollection services);
		void AddSingletonService<TInterface, TImplementationInstance>(TImplementationInstance singletonObject) where TImplementationInstance : class,TInterface;
		void AddScopedService<TInterface, TImplementation>(object singletonObject);
		void AddTransientService<TInterface, TImplementation>(object singletonObject);
		IServiceProvider BuildServiceProvider();
		TInterface Resolve<TInterface>();
		IEnumerable<TInterface> Resolves<TInterface>();
		TInterface ResolveKeyed<TInterface>(string name);
        void Dispose();
    }
}